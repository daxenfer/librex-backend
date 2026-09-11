using System.Text;
using Librex.Application.UseCases.Auth;
using Librex.Application.UseCases.Customers;
using Librex.Application.UseCases.Products;
using Librex.Application.UseCases.Suppliers;
using Librex.Application.UseCases.Remissions;
using Librex.Application.UseCases.ReturnNotes;
using Librex.Application.UseCases.Payments;
using Librex.Application.UseCases.Reports;
using Librex.Application.UseCases.Settings;
using Librex.Application.UseCases.Deletion;
using Librex.Application.UseCases.Users;
using Librex.API.Middleware;
using Librex.API.OpenApi;
using Librex.API.Security;
using Librex.Domain.Constants;
using Librex.Domain.Interfaces;
using Librex.Infrastructure.Data;
using Librex.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// El reloj se inyecta en vez de leerse de DateTime.UtcNow: así la ventana de bloqueo del login
// se puede adelantar en una prueba sin esperar quince minutos de verdad.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<LibrexDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IRemissionRepository, RemissionRepository>();
builder.Services.AddScoped<IReturnNoteRepository, ReturnNoteRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
builder.Services.AddScoped<ICompanySettingsRepository, CompanySettingsRepository>();
builder.Services.AddScoped<IDeletionRepository, DeletionRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IRemissionService, RemissionService>();
builder.Services.AddScoped<IReturnNoteService, ReturnNoteService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<IDeletionService, DeletionService>();
builder.Services.AddScoped<IUserService, UserService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
    };

    options.Events = new JwtBearerEvents
    {
        // Un token firmado y vigente solo prueba cómo era el usuario cuando entró. Aquí se
        // confirma contra la base que siga siendo el mismo: sin esto, dar de baja a alguien o
        // quitarle un rol no surte efecto hasta que su token expire, o sea hasta 8 horas después.
        //
        // Cuesta una consulta por petición. Para un sistema con este número de usuarios es un
        // precio razonable a cambio de que "desactivar usuario" signifique algo de verdad.
        OnTokenValidated = async context =>
        {
            var principal = context.Principal;
            var rawId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? principal?.FindFirst("sub")?.Value;
            var stamp = principal?.FindFirst(SecurityClaims.Stamp)?.Value;

            // Tokens emitidos antes de que existiera el sello: no se pueden verificar, así que
            // no se aceptan. El usuario vuelve a iniciar sesión y obtiene uno completo.
            if (!int.TryParse(rawId, out var userId) || string.IsNullOrEmpty(stamp))
            {
                context.Fail("El token no trae una identidad verificable.");
                return;
            }

            var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await users.GetByIdAsync(userId);

            if (user is null || !user.IsActive || user.SecurityStamp != stamp)
                context.Fail("La sesión ya no es válida.");
        },
    };
});

// Límite de peticiones al login. Es la otra mitad del anti-fuerza-bruta: el bloqueo por cuenta
// (LockoutPolicy) frena a quien insiste contra un usuario, y esto frena a quien barre muchos
// usuarios desde una misma IP, que el bloqueo por cuenta no ve.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(RateLimitPolicies.Login, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Sin IP conocida todos caen en la misma partición: es más restrictivo, que es el
            // lado correcto para equivocarse.
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,   // se rechaza de inmediato, no se encola
            }));
});

// Autorización por permiso. Una policy por cada permiso de la matriz, con el permiso como nombre
// de la policy: por eso [Authorize(Policy = Permissions.ProductsDelete)] en un controlador no
// necesita ninguna traducción. El claim lo emitió AuthService al firmar el token.
var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var permission in Permissions.All)
{
    authorization.AddPolicy(permission, policy =>
        policy.RequireClaim(Permissions.ClaimType, permission));
}

// CORS for React frontend (Vite on port 5173 or 5174)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
        policy.WithOrigins("https://s-svc-2fb9eac6-81ec-47cb-9c21-a393aacdee45.static.kubiy.site", "http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Documentación de la API: el documento OpenAPI lo genera el propio framework; Scalar solo lo
// pinta. El esquema Bearer lo agrega BearerSecuritySchemeTransformer, porque OpenAPI describe
// los endpoints pero no sabe cómo se autentican.
builder.Services.AddOpenApi(options =>
{
    // Sin esto el documento se titula con el nombre del ensamblado ("Librex.API").
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Librex API";
        document.Info.Description = "Sistema de distribución de libros.";
        return Task.CompletedTask;
    });

    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibrexDbContext>();
    var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    await DatabaseInitializer.SeedAsync(context, clock, builder.Configuration["Seed:AdminPassword"]);
}

// La documentación publica el mapa completo de la API, incluidos los endpoints de usuarios.
// Fuera de desarrollo queda apagada salvo que se prenda a propósito con ApiDocs:Enabled = true.
var apiDocsEnabled = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("ApiDocs:Enabled");

if (apiDocsEnabled)
{
    app.MapOpenApi();                       // el documento, en /openapi/v1.json
    app.MapScalarApiReference("/scalar");   // la interfaz para leerlo y probarlo
}

if (!app.Environment.IsDevelopment())
{
    // Solo fuera de desarrollo: en local la API se sirve por http y redirigir a https rompería
    // el proxy de Vite.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<ErrorLoggingMiddleware>();

app.UseCors("ReactPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", (TimeProvider clock) =>
       Results.Ok(new { status = "ok", timestamp = clock.GetUtcNow().UtcDateTime }))
   .WithTags("System");

app.Run();
