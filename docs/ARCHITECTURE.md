# Arquitectura — Librex

## Contexto del sistema

Librex es un sistema de gestión para una distribuidora de libros. Proveedores son editoriales; clientes son distribuidores, maestros y escuelas (principalmente de gobierno). El sistema está diseñado para iniciar como mono-tenant con posibilidad de evolucionar a multi-tenant (SaaS).

**MVP incluye:** Punto de Venta, Clientes, Productos, Órdenes de Compra, Devoluciones, Reportes de Ventas.

---

## Stack tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Backend | .NET Web API | 9.0 |
| ORM | Entity Framework Core + Npgsql | 9.x |
| Base de datos | PostgreSQL | 16 |
| Frontend | A definir (React recomendado) | — |
| Reverse proxy | Nginx | — |
| Contenedores | Docker + Docker Compose | — |
| Cloud | AWS EC2 (Linux) | — |

---

## Estructura de capas (Clean Architecture liviana)

```
Librex.Domain/
  ├── Entities/          → Clases de dominio (Product, Customer, Order, etc.)
  ├── Interfaces/        → Contratos de repositorios (IProductRepository, etc.)
  └── Enums/             → Enumeradores del dominio

Librex.Application/
  ├── UseCases/          → Lógica de negocio por módulo
  ├── DTOs/              → Request/Response objects
  └── Interfaces/        → Contratos de servicios de aplicación

Librex.Infrastructure/
  ├── Data/              → LibrexDbContext, configuraciones de entidades
  ├── Repositories/      → Implementaciones de interfaces del dominio
  └── Migrations/        → Migraciones de EF Core

Librex.API/
  ├── Controllers/       → Endpoints HTTP por módulo
  ├── Middleware/        → Error handling, auth, logging
  └── Extensions/        → DI registration helpers
```

**Flujo de dependencias:** API → Application → Domain ← Infrastructure

---

## Decisiones de diseño

### Multi-tenant preparado
Todas las entidades principales incluirán `TenantId` desde el inicio, aunque la primera versión sea mono-tenant. Esto evita una migración costosa si se decide convertir a SaaS.

### No RDS en primera fase
PostgreSQL se aloja en la misma EC2 para reducir costo (~$15-25/mes de ahorro). Migrar a RDS es una operación de pocas horas cuando el volumen lo justifique.

### DTOs obligatorios
Nunca se exponen entidades de dominio directamente en la API. Todos los endpoints usan DTOs en request y response. Razón: protege el dominio de cambios de contrato y evita over-posting.

---

## Infraestructura en AWS (bajo costo)

```
Internet → Elastic IP → EC2 (t3.micro / t4g.small, Amazon Linux 2023)
                             ├── Nginx (puerto 80/443, SSL Let's Encrypt)
                             ├── Librex API (.NET 9, systemd, puerto 5000)
                             └── PostgreSQL 16 (local, puerto 5432)
```

**Costo estimado inicial:** $8-15 USD/mes (t3.micro Free Tier si aplica).

---

## Módulos del MVP

| Módulo | Entidades principales |
|--------|-----------------------|
| Productos | Product, Category, Publisher (editorial) |
| Clientes | Customer, CustomerType |
| Punto de Venta | Sale, SaleItem, Payment |
| Órdenes de Compra | PurchaseOrder, PurchaseOrderItem |
| Devoluciones | Return, ReturnItem |
| Reportes | Vistas/queries de lectura, sin entidades propias |
