# Documentación de Prompts — Proyecto Integrador: Verdulería

## Contexto general del trabajo
- **Dominio:** Sistema de gestión para una verdulería (`Producto`, `Cajero`, `Venta`).
- **Stack y arquitectura:** .NET 8, ASP.NET Core Web API, Entity Framework Core, SQL Server (`SQLEXPRESS`) y arquitectura en 4 capas (`Verduleria.Entities`, `Verduleria.Data`, `Verduleria.Business`, `Verduleria.Api`), con repositorios concretos en `Verduleria.Data/Repositories`.
- **Control de versiones:** Git y GitHub (`main` / `origin/main`).

---

## 1. Capa de datos y repositorios

### Prompt 1.1 — Repositorios de productos, cajeros y ventas
> **Objetivo:** Definir la capa de acceso a datos mediante clases repositorio sobre `VerduleriaDbContext`.
>
> "Creá las clases `ProductoRepository`, `CajeroRepository` y `VentaRepository` dentro de `Verduleria.Data/Repositories` usando `VerduleriaDbContext` y consultas asíncronas con Entity Framework Core. Incluí operaciones CRUD, consultas con detalle (`Include`), filtros por rango de fechas, comprobación de legajo o nombre duplicado, validación de ventas asociadas y soporte de transacciones para operaciones compuestas."

---

## 2. Lógica de negocio y reglas del dominio

### Prompt 2.1 — Servicio de cajeros
> **Objetivo:** Encapsular las reglas de negocio de `Cajero` utilizando `CajeroRepository`.
>
> "Implementá `CajeroService` en `Verduleria.Business` inyectando `CajeroRepository`. Validá nombre y legajo obligatorios, normalizá el legajo, evitá duplicados al crear o actualizar y bloqueá la eliminación de cajeros que tengan ventas registradas."

### Prompt 2.2 — Servicio de productos
> **Objetivo:** Encapsular las reglas de negocio de `Producto` utilizando `ProductoRepository`.
>
> "Implementá `ProductoService` en `Verduleria.Business` inyectando `ProductoRepository`. Validá nombre obligatorio, precio por kg mayor a cero y stock no negativo, evitá nombres duplicados (respetando el id actual al editar) e impedí eliminar productos con ventas asociadas."

### Prompt 2.3 — Servicio de ventas, control de stock y reportes
> **Objetivo:** Gestionar ventas de forma atómica, actualizar el stock en kg y generar consultas de resumen.
>
> "Implementá `VentaService` en `Verduleria.Business` inyectando `VentaRepository`, `ProductoRepository` y `CajeroRepository`. Verificá que la cantidad en kg sea mayor a cero, que el cajero exista y esté activo, y que haya stock suficiente. Usá transacciones al registrar, actualizar o eliminar ventas para descontar o devolver stock de forma consistente, y agregá los métodos para obtener las últimas ventas, las ventas del día con su total recaudado y las ventas de la semana actual con su total recaudado."

---

## 3. Configuración de la API y control de versiones

### Prompt 3.1 — Registro de dependencias y configuración en `Program.cs`
> **Objetivo:** Configurar `VerduleriaDbContext`, repositorios, servicios, CORS, serialización JSON y Swagger en la capa `Api`.
>
> "Configurá `Verduleria.Api/Program.cs` con `VerduleriaDbContext` usando `DefaultConnection`, registrá `ProductoRepository`, `CajeroRepository`, `VentaRepository` y los servicios de `Verduleria.Business` como `Scoped`, habilitá CORS, configurá `ReferenceHandler.IgnoreCycles` en los controladores y activá Swagger en desarrollo."

### Prompt 3.2 — Compilación y publicación en GitHub
> **Objetivo:** Verificar que toda la solución compile y sincronizar los cambios en la rama principal.
>
> "Proporcioná los comandos de PowerShell para compilar la solución completa (`dotnet build`), verificar el estado del repositorio (`git status`), realizar el commit y subir los cambios a `main` en GitHub."
