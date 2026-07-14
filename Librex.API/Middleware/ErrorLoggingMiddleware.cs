using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Librex.Domain.Entities;
using Librex.Infrastructure.Data;

namespace Librex.API.Middleware;

// Captura cualquier excepción no manejada, guarda lo relevante del error y responde un 500
// genérico. La persistencia tiene 3 niveles independientes (BD -> archivo -> ILogger); cada
// catch solo puede caer al siguiente nivel DISTINTO o rendirse en silencio — nunca vuelve a
// invocar el método que arma o guarda el registro, así que un ciclo es estructuralmente
// imposible, no solo improbable.
public class ErrorLoggingMiddleware
{
    private const int MaxBodyReadChars = 8000;   // tope de LECTURA del stream, antes de redactar/truncar
    private const int MaxBodyStoredChars = 2000;
    private const int MaxTraceStoredChars = 4000;
    private const int MaxShortFieldChars = 500;

    private static readonly TimeSpan DbTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan DedupWindow = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DedupEntryMaxAge = TimeSpan.FromMinutes(5);

    private static readonly string[] SensitiveKeyMarkers =
        ["password", "token", "secret", "pwd", "authorization"];

    // Firma de error -> última vez que se persistió. Estático porque debe recordar entre
    // requests distintos; se purga solo (sin timer aparte) cada vez que se usa.
    private static readonly ConcurrentDictionary<string, DateTime> _recentlyPersisted = new();

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _env;

    public ErrorLoggingMiddleware(
        RequestDelegate next,
        ILogger<ErrorLoggingMiddleware> logger,
        IServiceScopeFactory scopeFactory,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Debe ir antes de next(): permite releer el body más abajo, después de que el
        // model binding de MVC ya lo haya consumido.
        context.Request.EnableBuffering();

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión o navegó a otra página: no es un error de la app.
            // Loguearlo ensuciaría la tabla con ruido cada vez que alguien es impaciente.
            throw;
        }
        catch (Exception ex)
        {
            var record = await BuildErrorLogAsync(context, ex);

            if (!IsDuplicate(record))
            {
                if (!await TryWriteToDatabaseAsync(record))
                    if (!await TryWriteToFileAsync(record))
                        TryWriteToLogger(record);
            }

            await TryWriteResponseAsync(context, record);
        }
    }

    // ---- Construcción del registro ----

    private async Task<ErrorLog> BuildErrorLogAsync(HttpContext context, Exception ex)
    {
        try
        {
            var request = context.Request;

            string? body;
            try { body = await ReadAndRedactBodyAsync(request); }
            catch { body = "(no se pudo leer el cuerpo del request)"; }

            string? routeValues = null;
            try
            {
                if (request.RouteValues.Count > 0)
                    routeValues = JsonSerializer.Serialize(request.RouteValues);
            }
            catch { /* se deja en null; no es motivo para perder el resto del registro */ }

            return new ErrorLog
            {
                RequestId = context.TraceIdentifier,
                Method = request.Method,
                Path = Sanitize(request.Path.Value, MaxShortFieldChars) ?? string.Empty,
                QueryString = Sanitize(request.QueryString.HasValue ? request.QueryString.Value : null, MaxShortFieldChars),
                RouteValues = Sanitize(routeValues, MaxShortFieldChars),
                RequestBody = Sanitize(body, MaxBodyStoredChars),
                StatusCode = StatusCodes.Status500InternalServerError,
                ExceptionType = ex.GetType().FullName ?? ex.GetType().Name,
                Message = Sanitize(BuildMessageChain(ex), MaxBodyStoredChars) ?? string.Empty,
                StackTrace = Sanitize(ex.ToString(), MaxTraceStoredChars),
                Username = context.User.FindFirst(ClaimTypes.Name)?.Value,
            };
        }
        catch
        {
            // Ni siquiera se pudo armar el registro completo (caso extremo: una excepción
            // custom cuyo .Message truena al evaluarse). Nunca nos quedamos sin nada que guardar.
            return new ErrorLog
            {
                RequestId = context.TraceIdentifier,
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                StatusCode = StatusCodes.Status500InternalServerError,
                ExceptionType = "(desconocido)",
                Message = "(no se pudo construir el detalle del error)",
            };
        }
    }

    // Mensaje externo + cadena de InnerException: en un DbUpdateException el detalle útil
    // (constraint violada, tabla) viene en la PostgresException interna, no en ex.Message.
    private static string BuildMessageChain(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        var depth = 0;
        while (current is not null && depth < 5)
        {
            if (sb.Length > 0) sb.Append(" ---> ");
            sb.Append(current.GetType().Name).Append(": ").Append(current.Message);
            current = current.InnerException;
            depth++;
        }
        return sb.ToString();
    }

    private static async Task<string?> ReadAndRedactBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanRead || !request.Body.CanSeek || request.ContentLength is 0) return null;

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        var buffer = new char[MaxBodyReadChars];
        var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
        request.Body.Position = 0;

        if (read == 0) return null;
        return RedactJson(new string(buffer, 0, read));
    }

    private static string RedactJson(string raw)
    {
        try
        {
            var node = JsonNode.Parse(raw);
            if (node is not null)
            {
                RedactNode(node);
                return node.ToJsonString();
            }
        }
        catch
        {
            // No era JSON válido (o quedó cortado a la mitad de un token por el tope de
            // lectura) — se guarda tal cual; Sanitize se encarga de truncar/limpiar después.
        }
        return raw;
    }

    private static void RedactNode(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                var lower = key.ToLowerInvariant();
                if (SensitiveKeyMarkers.Any(lower.Contains))
                    obj[key] = "***";
                else
                    RedactNode(obj[key]);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
                RedactNode(item);
        }
    }

    // Quita bytes NUL y demás caracteres de control (Postgres rechaza NUL de plano en columnas
    // text) y trunca a maxLen. Se aplica a todo campo de texto antes de intentar el insert.
    private static string? Sanitize(string? value, int maxLen)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var sb = new StringBuilder(Math.Min(value.Length, maxLen));
        var truncated = false;
        foreach (var c in value)
        {
            var isAllowedControl = c is '\n' or '\r' or '\t';
            var isControl = c < 0x20 || (c >= 0x7F && c <= 0x9F);
            if (isControl && !isAllowedControl) continue;

            if (sb.Length >= maxLen) { truncated = true; break; }
            sb.Append(c);
        }

        return truncated ? sb.Append("...(truncado)").ToString() : sb.ToString();
    }

    // ---- Anti-inundación ----

    private static bool IsDuplicate(ErrorLog record)
    {
        var signature = $"{record.Method} {record.Path} {record.ExceptionType}";
        var now = DateTime.UtcNow;

        foreach (var kv in _recentlyPersisted)
            if (now - kv.Value > DedupEntryMaxAge)
                _recentlyPersisted.TryRemove(kv.Key, out _);

        if (_recentlyPersisted.TryGetValue(signature, out var last) && now - last < DedupWindow)
            return true;

        _recentlyPersisted[signature] = now;
        return false;
    }

    // ---- Tier 1: tabla error_logs ----

    private async Task<bool> TryWriteToDatabaseAsync(ErrorLog record)
    {
        try
        {
            // Scope y DbContext propios — nunca el de la request que falló. Si la excepción
            // original vino de ESE DbContext (p.ej. un DbUpdateException), su ChangeTracker o
            // su conexión pueden estar en mal estado; reusarlo aquí sería la clase de
            // auto-referencia que puede encadenar fallos.
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LibrexDbContext>();

            using var cts = new CancellationTokenSource(DbTimeout);
            db.ErrorLogs.Add(record);
            await db.SaveChangesAsync(cts.Token);
            return true;
        }
        catch
        {
            // catch (Exception) amplio a propósito: un timeout se manifiesta como
            // OperationCanceledException, no como una excepción típica de Postgres — un catch
            // angosto dejaría ese caso sin atrapar. Un solo intento, sin reintentos: si falla,
            // se cae a Tier 2 en vez de insistir contra la misma BD.
            return false;
        }
    }

    // ---- Tier 2: archivo local ----

    private async Task<bool> TryWriteToFileAsync(ErrorLog record)
    {
        try
        {
            var dir = Path.Combine(_env.ContentRootPath, "Logs");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"errors-{DateTime.UtcNow:yyyyMMdd}.log");
            var line = JsonSerializer.Serialize(record) + Environment.NewLine;
            await File.AppendAllTextAsync(file, line, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ---- Tier 3: último recurso ----

    private void TryWriteToLogger(ErrorLog record)
    {
        try
        {
            _logger.LogError(
                "[{RequestId}] {Method} {Path} -> {ExceptionType}: {Message}",
                record.RequestId, record.Method, record.Path, record.ExceptionType, record.Message);
        }
        catch
        {
            // No queda nada más que intentar.
        }
    }

    // ---- Respuesta al cliente ----

    private async Task TryWriteResponseAsync(HttpContext context, ErrorLog record)
    {
        try
        {
            if (context.Response.HasStarted) return;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = _env.IsDevelopment()
                ? JsonSerializer.Serialize(new { error = "Ocurrió un error interno.", requestId = record.RequestId, detail = record.Message })
                : JsonSerializer.Serialize(new { error = "Ocurrió un error interno.", requestId = record.RequestId });

            await context.Response.WriteAsync(payload);
        }
        catch
        {
            // El cliente ya se desconectó (el WriteAsync truena) — no hay a quién responder.
        }
    }
}
