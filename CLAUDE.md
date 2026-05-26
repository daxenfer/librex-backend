# CLAUDE.md — Librex Backend

## Descripción
Sistema de distribución de libros — servicio backend. Módulos: Productos, Editoriales, Clientes, Remisiones, Devoluciones, Pagos, Reportes.

## Stack
- .NET 9 Web API (Clean Architecture liviana)
- ORM: Entity Framework Core 9 + Npgsql 9
- Auth: JWT Bearer tokens
- DB: PostgreSQL 16 (instalado nativamente en Windows, puerto 5433, base `librex_dev`, usuario `daniel`)

## Estructura de capas
```
Librex.Domain/         → Entities, interfaces, enums, value objects
Librex.Application/    → Use cases, DTOs, services, repository interfaces
Librex.Infrastructure/ → EF Core DbContext, repositories, migrations
Librex.API/            → Controllers, middlewares, DI, configuration
Librex.Tests/          → Unit tests (xUnit + Moq + EF InMemory)
```

## Reglas de arquitectura
- Controllers solo reciben y devuelven HTTP — delegan todo a Application
- Lógica de negocio solo en Domain y Application
- Infrastructure no expone EF directamente — solo a través de interfaces definidas en Application
- DTOs para todas las respuestas API (nunca exponer entidades de dominio)
- Entities llevan campo `TenantId` para compatibilidad multi-tenant futura

## Convenciones de código
- **Todo el código en inglés**: clases, métodos, propiedades, variables, comentarios, DTOs
  - Razón: el token `[controller]` de ASP.NET Core solo elimina el sufijo inglés "Controller".
    "Controlador" en español NO se elimina → las rutas fallan (`/api/AuthControlador` en vez de `/api/auth`).
- Siempre usar **atributos de ruta explícitos** — nunca depender del token `[controller]`:
  ```csharp
  [Route("api/auth")]          // correcto
  [Route("api/[controller]")]  // NO USAR
  ```
- Sin lógica en Controllers
- Sin `IQueryable` fuera de Infrastructure

## Comandos frecuentes
```powershell
# Desde la carpeta backend/ (donde está Librex.sln)
dotnet run --project Librex.API --configuration Release        # inicia API (puerto 5176)
dotnet build --configuration Release                           # compilar
dotnet test                                                    # ejecutar tests

dotnet ef migrations add NombreMigracion --project Librex.Infrastructure --startup-project Librex.API --configuration Release
dotnet ef database update --project Librex.Infrastructure --startup-project Librex.API --configuration Release

dotnet user-secrets set "Jwt:Key" "tu-clave-secreta" --project Librex.API
```

## Tests
- Sin Docker — usar `Microsoft.EntityFrameworkCore.InMemory`
- Proyecto: `Librex.Tests` (xUnit + Moq + EF InMemory)
- Nunca mockear DbContext directamente — usar InMemory provider y DbContext real

## Lo que NO hacer
- No commit de connection strings con credenciales reales
- No usar `appsettings.Development.json` con contraseñas — usar `dotnet user-secrets`
- Sin lógica de negocio en Controllers o Infrastructure
- No saltarse migraciones de EF
- No exponer entidades de dominio directamente en la API
- No usar `dotnet build` sin `--configuration Release` cuando VS Code C# extension está abierta (bloquea DLLs Debug)
- No usar token `[controller]` en rutas — siempre usar `[Route("api/...")]` explícito
