# CLAUDE.md — Librex Backend

## Descripción
Sistema de distribución de libros — servicio backend. Módulos: Productos, Editoriales, Clientes, Remisiones, Devoluciones, Pagos, Reportes.

## Stack
- .NET 10 Web API (Clean Architecture liviana) — LTS hasta noviembre de 2028
- ORM: Entity Framework Core 10 + Npgsql 10
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
- DTOs para todas las respuestas API (nunca exponer entidades de dominio). Son `record`.
- Los repositorios heredan de `Repository<T>` (`Librex.Infrastructure/Repositories/Repository.cs`),
  que implementa las cinco operaciones de `IRepository<T>`. Un repositorio concreto solo declara
  su `DeletionRoot` y sobrescribe lo que de verdad cambia: qué `Include` lleva y con qué orden
  lista. Los que llevan folio heredan de `DocumentRepository<T>`.
- Toda firma async recibe `CancellationToken`, desde la acción del controlador hasta EF.
- El reloj se inyecta con `TimeProvider`, no se lee de `DateTime.UtcNow`.

> Multi-tenant: se planeó un campo `TenantId` en todas las entidades y **nunca se implementó**.
> No existe en el código. Si algún día se retoma, es una migración de verdad, no un pendiente.

## Autorización
- Roles en `Librex.Domain/Constants/Roles.cs`: `SuperAdmin` (proveedor del sistema, único con
  `users.manage`), `Administrator` (dueño del negocio), `User` (operativo, sin borrar).
- `Librex.Domain/Constants/Permissions.cs` es la **fuente única** de la matriz rol → permisos. De
  ahí salen las policies de `Program.cs`, los claims `perm` que emite `AuthService.BuildToken` y la
  lista que el login devuelve en `LoginResponseDto.Permissions`.
- El nombre del permiso **es** el nombre de la policy — no hay tabla de traducción:
  `[Authorize(Policy = Permissions.ProductsDelete)]`. `Program.cs` registra una policy por permiso
  recorriendo la matriz.
- `[Authorize]` a nivel de clase cubre los GET (leer no lleva permiso); el permiso va en la acción,
  solo en POST/PUT/DELETE.
- Al agregar un módulo: constantes nuevas en `Permissions.cs` y atributos en el controlador. Nada
  más — ni `Program.cs` ni el frontend se tocan.
- `api/users` es solo de `SuperAdmin`. Sus reglas de guarda (anti-escalada de privilegios, no
  modificarse a sí mismo, no dejar el sistema sin un SuperAdmin activo) están en `UserService`, no
  en el controlador.
- Un rol desconocido en la BD se queda **sin permisos**, nunca con los de administrador.

## Seguridad de acceso
Configuración **obligatoria** fuera de desarrollo (variables de entorno; el `__` es el separador
de secciones de .NET). Ninguna vive ya en `appsettings.json`, que está versionado:
```
Jwt__Key                  clave de firma, mínimo 32 caracteres — sin ella la API no arranca
ConnectionStrings__Default cadena de conexión de Postgres
Seed__AdminPassword       opcional: contraseña del usuario semilla en una instalación nueva
ApiDocs__Enabled          opcional: true para publicar la documentación fuera de desarrollo
```
En local van en `dotnet user-secrets` (`--project Librex.API`), no en archivos.

Capas contra fuerza bruta, cada una tapa lo que la otra no ve:
- **Bloqueo por cuenta** (`LockoutPolicy`): 10 intentos fallidos consecutivos bloquean 15 minutos.
  Es temporal a propósito — con bloqueo permanente, cualquiera dejaría al dueño fuera del sistema
  con diez intentos malos. Restablecer la contraseña también levanta el bloqueo.
- **Límite por IP** (`RateLimitPolicies.Login`): 20 peticiones por minuto, solo sobre el login.
  Frena a quien barre muchas cuentas desde una misma IP, que el bloqueo por cuenta no detecta.
- **Bitácora** (`login_attempts`): todo intento queda registrado con su motivo real. Al usuario
  siempre se le responde lo mismo — decirle "cuenta bloqueada" confirmaría que esa cuenta existe.
- **Tiempo constante**: el login verifica un hash aunque el usuario no exista, para que el tiempo
  de respuesta no delate qué nombres están dados de alta.

**Revocación de sesiones** (`User.SecurityStamp`): el sello viaja en el token y se compara contra
la base en cada petición (`OnTokenValidated` en `Program.cs`). Cambia al dar de baja al usuario,
cambiarle el rol, el nombre de usuario o la contraseña — y entonces sus tokens dejan de valer al
instante. Sin esto, un JWT es válido hasta expirar aunque el usuario ya no exista. Cuesta una
consulta por petición, que es el precio de que "desactivar usuario" signifique algo.

**Contraseñas** (`StrongPasswordAttribute`): mínimo 10 caracteres con mayúscula, minúscula, número
y símbolo. Hash con BCrypt, nunca en claro ni reversible.

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
                                                               # documentación en /scalar
dotnet build --configuration Release                           # compilar
dotnet test                                                    # ejecutar tests

dotnet ef migrations add NombreMigracion --project Librex.Infrastructure --startup-project Librex.API --configuration Release
dotnet ef database update --project Librex.Infrastructure --startup-project Librex.API --configuration Release

dotnet user-secrets set "Jwt:Key" "tu-clave-secreta" --project Librex.API
```

## Configuración de los proyectos
- `Directory.Build.props` tiene lo común a los cinco: `TargetFramework`, `Nullable`,
  `ImplicitUsings` y los analizadores. **Cambiar de versión de .NET se hace ahí, en una línea.**
- `Directory.Packages.props` fija la versión de cada paquete una sola vez (Central Package
  Management). Los `.csproj` dicen qué paquete usan, nunca en qué versión.
- `global.json` fija la banda del SDK en 10.0.4xx.
- El build corre con `TreatWarningsAsErrors`: está en cero advertencias y hay que mantenerlo ahí.
  Las tres reglas apagadas están en `.editorconfig`, cada una con su razón escrita.

## Tests
- Sin Docker — usar `Microsoft.EntityFrameworkCore.InMemory`
- Proyecto: `Librex.Tests` (xUnit + Moq + EF InMemory)
- Nunca mockear DbContext directamente — usar InMemory provider y DbContext real

## Lo que NO hacer
- No commit de connection strings, claves JWT ni contraseñas — van en user-secrets o variables de entorno
- No usar `appsettings.Development.json` con contraseñas — usar `dotnet user-secrets`
- Sin lógica de negocio en Controllers o Infrastructure
- No saltarse migraciones de EF
- No exponer entidades de dominio directamente en la API
- No usar `dotnet build` sin `--configuration Release` cuando VS Code C# extension está abierta (bloquea DLLs Debug)
- No usar token `[controller]` en rutas — siempre usar `[Route("api/...")]` explícito
- No poner `Version=` en un `PackageReference` — la versión va en `Directory.Packages.props`
- No apagar una advertencia sin escribir por qué en `.editorconfig`
