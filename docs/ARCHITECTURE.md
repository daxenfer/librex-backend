# Arquitectura — Librex

## Contexto del sistema

Librex es un sistema de gestión para una distribuidora de libros. Los proveedores son editoriales;
los clientes son distribuidores, maestros y escuelas (principalmente de gobierno).

**Módulos en producción:** Productos, Proveedores, Clientes, Remisiones, Devoluciones, Pagos,
Cuentas por cobrar, Reportes, Configuración y Usuarios.

---

## Stack tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Backend | .NET Web API | 10.0 (LTS hasta noviembre de 2028) |
| ORM | Entity Framework Core + Npgsql | 10.x |
| Base de datos | PostgreSQL | 16 (local: puerto 5433, base `librex_dev`) |
| Frontend | React + Vite + TypeScript (repo `librex-frontend`) | React 19 |
| Documentación de la API | OpenAPI 3.1 nativo + Scalar | `/scalar` |
| Despliegue | Kubiy (`api.librex.apps2.kubiy.site`) | — |

---

## Estructura de capas (Clean Architecture liviana)

```
Librex.Domain/
  ├── Entities/          → Clases de dominio y BaseEntity
  ├── Interfaces/        → Contratos de repositorio (IRepository<T> y los específicos)
  ├── Constants/         → Roles, Permissions, LockoutPolicy, SecurityClaims
  ├── Enums/             → DeletableEntity, DependentKind, LoginOutcome
  └── Exceptions/        → BusinessRuleException

Librex.Application/
  ├── UseCases/<Área>/   → Un Service + su IService por módulo
  ├── DTOs/<Área>/       → Request/Response, todos `record`
  └── Validation/        → StrongPasswordAttribute

Librex.Infrastructure/
  ├── Data/              → LibrexDbContext, Configurations/, Migrations/,
  │                        DeletionGraph, DatabaseInitializer, DatabaseErrors
  └── Repositories/      → Repository<T>, DocumentRepository<T> y los concretos

Librex.API/
  ├── Controllers/       → Endpoints HTTP por módulo, ruta `api/<área>` explícita
  ├── Middleware/        → ErrorLoggingMiddleware
  ├── OpenApi/           → BearerSecuritySchemeTransformer
  ├── Security/          → RateLimitPolicies
  └── Program.cs         → DI, JWT, policies, CORS, rate limiting
```

**Flujo de dependencias:** API → Application → Domain ← Infrastructure

La configuración compartida de los cinco proyectos vive en `Directory.Build.props` y
`Directory.Packages.props`, en la raíz.

---

## Decisiones de diseño

### DTOs obligatorios
Nunca se exponen entidades de dominio en la API. Protege el dominio de cambios de contrato y
evita over-posting.

### Borrado lógico en cascada
Nada se destruye. Al eliminar una entidad raíz, ella y sus dependientes se marcan como inactivos
en un solo `SaveChangesAsync`. El grafo de dependientes lo resuelve
`Librex.Infrastructure/Data/DeletionGraph.cs`, que sirve tanto para la previsualización de
impacto como para el borrado real. Los documentos ya emitidos que citan un registro eliminado
conservan su historia intacta: por eso un reporte o un PDF de hace seis meses sigue cuadrando.

Los folios de documentos eliminados quedan quemados y no se reutilizan, para no chocar con el
índice único de `FolioNumber`.

### Autorización por permiso, no por rol
`Librex.Domain/Constants/Permissions.cs` es la fuente única de la matriz rol → permisos. De ahí
salen las policies de `Program.cs`, los claims `perm` del token y la lista que el login devuelve.
El nombre del permiso **es** el nombre de la policy. El frontend nunca razona por rol.

### Revocación de sesiones
`User.SecurityStamp` viaja en el token y se compara contra la base en cada petición. Cuesta una
consulta por petición, a cambio de que dar de baja a un usuario surta efecto al instante en vez
de hasta que expire su token.

### Multi-tenant: planeado y no hecho
Se consideró llevar `TenantId` en todas las entidades desde el inicio. **No se implementó**: no
existe en el código. Si algún día se retoma, es una migración de datos de verdad.

---

## Modelo de dominio

| Entidad | Notas |
|---|---|
| `Customer` | Cliente. |
| `Supplier` | Editorial. En la UI se llama "Editorial"; en el código, `Supplier`. |
| `Product` | Título. Pertenece a un `Supplier`. |
| `Remission` + `RemissionDetail` | Funciona como factura. `Discount` es **monto fijo**, no porcentaje. |
| `ReturnNote` + `ReturnNoteDetail` | Devolución. Puede ir ligada a una remisión o suelta; si es suelta, el motivo es obligatorio. |
| `Payment` + `PaymentAllocation` | Un pago se reparte entre remisiones. El remanente sin asignar es un anticipo. |
| `CompanySettings` | Datos de la empresa (incluido el logo) para los PDFs. |
| `User`, `LoginAttempt` | Usuarios y bitácora de accesos. |
| `ErrorLog` | Errores no controlados, persistidos por el middleware. |

**Cuentas por cobrar**, por remisión: `saldo = total − Σ devoluciones − Σ asignaciones de pago`.

---

## Historial

El backend se migró de .NET 9 a .NET 10 en septiembre de 2026, junto con una limpieza:
configuración centralizada, analizadores con `TreatWarningsAsErrors`, `Repository<T>` base,
`TimeProvider`, `CancellationToken` de punta a punta, y el reemplazo de Swashbuckle por OpenAPI
nativo con Scalar.
