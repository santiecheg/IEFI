# Documentación de Prompts — Proyecto Integrador: Verdulería

Registro de todos los prompts utilizados con Inteligencia Artificial durante el desarrollo del proyecto, según lo pide la consigna del Trabajo Práctico Integrador de Programación II.

## Contexto general del trabajo
- **Dominio:** Sistema de gestión para una verdulería (`Producto`, `Cajero`, `Venta`).
- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core, SQL Server (`SQLEXPRESS`) y arquitectura en 4 capas (`Verduleria.Entities`, `Verduleria.Data`, `Verduleria.Business`, `Verduleria.Api`), con repositorios concretos en `Verduleria.Data/Repositories`.
- **Frontend:** HTML, CSS y JavaScript vainilla (sin frameworks) que consume la API REST con `fetch`.
- **Control de versiones:** Git y GitHub (`main` / `origin/main`).

## Estructura del proyecto
```
IEFI Programación/   (raíz del repositorio)
├── Backend/         → solución VerduleriaApi.sln (Entities, Data, Business, Api)
├── Frontend/        → index.html, css/ y js/ (ver Frontend/README.md)
├── Prompts.md       → este archivo
└── README.md        → descripción del proyecto y cómo ejecutarlo
```

---

## 1. Capa de datos, repositorios y migraciones

### Prompt 1.1 — Repositorios de productos, cajeros y ventas
> **Objetivo:** Definir la capa de acceso a datos mediante clases repositorio sobre `VerduleriaDbContext`.
>
> "Creá las clases `ProductoRepository`, `CajeroRepository` y `VentaRepository` dentro de `Verduleria.Data/Repositories` usando `VerduleriaDbContext` y consultas asíncronas con Entity Framework Core. Incluí operaciones CRUD, consultas con detalle (`Include`), filtros por rango de fechas, comprobación de legajo o nombre duplicado, validación de ventas asociadas y soporte de transacciones para operaciones compuestas."

### Prompt 1.2 — Migraciones y creación de base de datos en SQL Server
> **Objetivo:** Generar y aplicar las migraciones de Entity Framework Core para crear las tablas y relaciones en SQL Server.
>
> "Configurá `VerduleriaDbContext` y la cadena de conexión `DefaultConnection` para SQL Server (`SQLEXPRESS`), generá la migración inicial (`InitialCreate`) en `Verduleria.Data` y proporcioná los comandos para aplicar la migración y actualizar la base de datos (`dotnet ef database update`)."

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

### Prompt 2.4 — DTOs y adaptación de servicios con mapeo y rollback
> **Objetivo:** Separar los contratos de entrada y salida de la API mediante DTOs en `Verduleria.Business/DTOs` y actualizar los servicios para trabajar con ellos y revertir transacciones ante fallos.
>
> "Definí los DTOs de entrada y salida (`RequestDto` y `ResponseDto`, además de `ResumenVentasResponseDto`) en `Verduleria.Business/DTOs`, y actualizá `CajeroService`, `ProductoService` y `VentaService` para recibir DTOs de creación/actualización, devolver DTOs de respuesta/resumen mediante mapeo y hacer `RollbackAsync()` dentro de `try-catch` si falla alguna operación transaccional en ventas."

---

## 3. Controllers, configuración de la API y control de versiones

### Prompt 3.1 — Controllers de la Web API
> **Objetivo:** Exponer los endpoints REST delegando la lógica de negocio a los servicios correspondientes.
>
> "Implementá los controladores de `Verduleria.Api` (`ProductoController`, `CajeroController` y `VentaController`) inyectando `ProductoService`, `CajeroService` y `VentaService`. Exponé las operaciones CRUD y los endpoints específicos de ventas (incluyendo últimas ventas, ventas del día y ventas de la semana), devolviendo los códigos de estado HTTP adecuados y los mensajes de error o validación correspondientes."

### Prompt 3.2 — Registro de dependencias y configuración en `Program.cs`
> **Objetivo:** Configurar `VerduleriaDbContext`, repositorios, servicios, CORS, serialización JSON y Swagger en la capa `Api`.
>
> "Configurá `Verduleria.Api/Program.cs` con `VerduleriaDbContext` usando `DefaultConnection`, registrá `ProductoRepository`, `CajeroRepository`, `VentaRepository` y los servicios de `Verduleria.Business` como `Scoped`, habilitá CORS, configurá `ReferenceHandler.IgnoreCycles` en los controladores y activá Swagger en desarrollo."

### Prompt 3.3 — Uso de DTOs y manejo de excepciones (`try-catch`) en controladores
> **Objetivo:** Adaptar los controladores para trabajar con los DTOs de negocio y capturar excepciones no controladas devolviendo estado `500`.
>
> "Actualizá `CajeroController`, `ProductoController` y `VentaController` para recibir y devolver los DTOs de `Verduleria.Business.DTOs` (eliminando los records locales del controlador de ventas) y envolvé cada acción en bloques `try-catch` que retornen `StatusCode(500, new { mensaje = \"...\" })` ante errores inesperados."

### Prompt 3.4 — Compilación y publicación en GitHub
> **Objetivo:** Verificar que toda la solución compile y sincronizar los cambios en la rama principal.
>
> "Proporcioná los comandos de PowerShell para compilar la solución completa (`dotnet build`), verificar el estado del repositorio (`git status`), realizar el commit y subir los cambios a `main` en GitHub."

---

## 4. Estructura del proyecto y frontend

### Prompt 4.1 — Carpeta del proyecto y frontend que consume la API
> **Objetivo:** Organizar el proyecto en una carpeta con Backend y Frontend, incorporar el backend del repositorio y desarrollar el frontend en HTML, CSS y JavaScript vainilla según la consigna (se adjuntó el PDF del Trabajo Práctico Integrador).
>
> "Quiero que crees una carpeta en el escritorio que se llame IF Programación. Dentro de esa carpeta tiene que haber otra carpeta llamada Backend, otra Frontend y un archivo Markdown donde podamos poner los prompts que utilizamos. Dentro de la carpeta Backend usá lo que está en el GitHub https://github.com/santiecheg/IEFI.git: descargá todo el repositorio, con todas las capas, en la carpeta Backend. En la carpeta Frontend creá todo el front que debe llevar esta página, algo sencillo como lo explica el PDF."

**Resultado:**
- Se creó la carpeta del proyecto con `Backend`, `Frontend` y este archivo de prompts.
- El código del repositorio IEFI se incorporó en la carpeta `Backend`.
- Se desarrolló el frontend a partir de las entidades, rutas y validaciones del backend:
  - `index.html` con 4 secciones: Resumen, Ventas, Productos y Cajeros.
  - CRUD de productos y cajeros con formulario, listado, edición y eliminación.
  - Registro de ventas con vista previa del total y del stock restante, edición, eliminación y filtros Todas / Hoy / Semana (endpoints `api/venta/hoy` y `api/venta/semana`).
  - Resumen con lo recaudado hoy y en la semana, productos con poco stock, cajeros activos y últimas ventas (`api/venta/ultimas`).
  - Validaciones en los formularios iguales a las de los services, manejo de los errores que devuelve la API e indicador de conexión con el backend.

### Prompt 4.2 — README con el paso a paso para usar el sistema
> **Objetivo:** Documentar en el `README.md` de la raíz cómo instalar, configurar y usar el sistema completo.
>
> "Mejorá el README con el paso a paso para poder usar el sistema."

**Resultado:**
- Requisitos (Visual Studio 2022, SDK de .NET 8, SQL Server Express, `dotnet-ef` y Git) y cómo verificar cada uno.
- Pasos para clonar el repositorio, revisar la cadena de conexión, crear la base con `dotnet ef database update`, ejecutar la API con el perfil `http` y abrir el frontend.
- Guía de uso de cada pantalla (Resumen, Productos, Cajeros y Ventas) con sus reglas de negocio.
- Recorrido de prueba con los datos iniciales, tabla de problemas frecuentes y cómo reiniciar la base.
- El `Frontend/README.md` remite al README principal para la puesta en marcha.
