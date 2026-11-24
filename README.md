# IEFI – Sistema de gestión para una verdulería

Trabajo Práctico Integrador de Programación II.

Sistema web para administrar los **productos**, **cajeros** y **ventas** de una verdulería. Controla el stock en kilos, calcula el total de cada venta y muestra lo recaudado en el día y en la semana.

---

## Tecnologías

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core, LINQ y SQL Server, con arquitectura en capas.
- **Frontend:** HTML, CSS y JavaScript vainilla (sin frameworks), que consume la API con `fetch`.

## Estructura del proyecto

```
├── Backend/                      → Web API en .NET 8 (VerduleriaApi.sln)
│   ├── Verduleria.Entities       → entidades: Producto, Cajero, Venta
│   ├── Verduleria.Data           → DbContext, repositorios y migraciones (EF Core + SQL Server)
│   ├── Verduleria.Business       → servicios con las reglas de negocio y DTOs
│   └── Verduleria.Api            → controllers REST, configuración y Swagger
├── Frontend/                     → index.html, css/ y js/
├── Prompts.md                    → prompts utilizados con Inteligencia Artificial
└── README.md                     → este archivo
```

---

## Puesta en marcha, paso a paso

Hay que hacer los pasos 1 a 3 una sola vez. Después, para usar el sistema alcanza con los pasos 4 y 5: levantar la API y abrir el front.

### Paso 0 · Instalar lo necesario

| Programa | Para qué | Cómo verificar que está |
| --- | --- | --- |
| **Visual Studio 2022** (17.8 o posterior) con la carga de trabajo *Desarrollo de ASP.NET y web* | Abrir y ejecutar el backend. Incluye el SDK de .NET 8. | En una terminal: `dotnet --list-sdks` muestra una versión `8.x` |
| **SQL Server Express** (instancia `SQLEXPRESS`) | Base de datos | `Win + R` → `services.msc` → **SQL Server (SQLEXPRESS)** figura *En ejecución* |
| **Herramienta `dotnet-ef`** | Crear la base con las migraciones | `dotnet ef --version` |
| **Git** | Descargar el repositorio | `git --version` |
| Un navegador (Chrome, Edge o Firefox) | Usar el sistema | — |

Si falta `dotnet-ef`, se instala una sola vez con:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Después de instalarla, cerrá y volvé a abrir la terminal.

> Opcionales: **SQL Server Management Studio** para ver las tablas, y **VS Code** con la extensión **Live Server** para abrir el front.

### Paso 1 · Descargar el proyecto

```powershell
git clone https://github.com/santiecheg/IEFI.git
cd IEFI
```

### Paso 2 · Revisar la conexión a SQL Server

La cadena de conexión está en `Backend/Verduleria.Api/appsettings.json`:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=VerduleriaDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Usa tu usuario de Windows para conectarse (`Trusted_Connection`), así que no hace falta usuario ni contraseña. Si instalaste SQL Server Express con las opciones por defecto, **no hay que tocar nada**. Si tu SQL Server es otro, cambiá solo la parte `Server=`:

| Tu SQL Server | Valor de `Server=` |
| --- | --- |
| SQL Server Express (instalación por defecto) | `localhost\\SQLEXPRESS` (ya viene así) |
| Developer o Standard, instancia por defecto | `localhost` |
| LocalDB de Visual Studio | `(localdb)\\MSSQLLocalDB` |

> En JSON la barra invertida se escribe doble: `\\`.

### Paso 3 · Crear la base de datos

Abrí una terminal en la carpeta `Backend` del proyecto y ejecutá:

```powershell
dotnet ef database update --project Verduleria.Data --startup-project Verduleria.Api
```

El comando compila la solución, aplica la migración `InitialCreate` y termina con **`Done.`** Se crea la base `VerduleriaDb`, con las tablas `Cajeros`, `Productos` y `Ventas` y estos datos iniciales:

| Cajeros | Productos (precio por kg · stock) |
| --- | --- |
| Lucía Gómez (CAJ-01) | Pimiento ($2.800 · 25 kg) |
| Martín Pérez (CAJ-02) | Tomate ($1.900 · 40 kg) |
| | Pera ($1.600 · 30 kg) |
| | Cebolla ($1.200 · 50 kg) |
| | Banana ($2.100 · 35 kg) |

> **Desde la Consola del Administrador de paquetes de Visual Studio:** `Update-Database` necesita el paquete `Microsoft.EntityFrameworkCore.Tools`, que el proyecto no trae instalado. Por eso conviene usar el comando de arriba.

### Paso 4 · Ejecutar la API

**Opción A: Visual Studio**

1. Abrí `Backend/VerduleriaApi.sln`.
2. En el *Explorador de soluciones*, `Verduleria.Api` tiene que estar en negrita. Si no lo está: clic derecho → **Establecer como proyecto de inicio**.
3. En el desplegable que está al lado del botón verde ▶ elegí el perfil **`http`**.
4. Presioná **F5** (o **Ctrl + F5** para ejecutar sin depurar).

**Opción B: terminal** (en la carpeta `Backend`)

```powershell
dotnet run --project Verduleria.Api --launch-profile http
```

Cuando aparezca `Now listening on: http://localhost:5129`, la API ya está funcionando.

**Comprobar que responde:** entrá a <http://localhost:5129/swagger>. En **Producto**, abrí `GET /api/Producto` → *Try it out* → *Execute*. Tiene que responder `200` con los 5 productos.

> Dejá la API corriendo mientras usás el sistema. Para detenerla: **Shift + F5** en Visual Studio o **Ctrl + C** en la terminal.

### Paso 5 · Abrir el sistema

Abrí `Frontend/index.html` con doble clic. Arriba a la derecha tiene que aparecer **API conectada** en verde.

Si aparece **API sin conexión**, revisá [Problemas frecuentes](#problemas-frecuentes). También podés abrir el front con **Live Server** desde VS Code: clic derecho en `index.html` → *Open with Live Server*.

---

## Cómo usar el sistema

La página tiene cuatro pestañas. Todo lo que se carga se guarda al instante en SQL Server y sigue ahí al cerrar y volver a abrir.

### Resumen

Es la pantalla de inicio y muestra cómo viene el negocio:

- **Recaudado hoy** y **Recaudado esta semana** (desde el lunes), con la cantidad de ventas.
- **Productos con poco stock:** los que tienen menos de 5 kg.
- **Cajeros activos:** los que pueden registrar ventas.
- **Últimas ventas:** con el selector **Mostrar** elegís ver 5, 10 o 20.
- **Stock bajo:** los productos que conviene reponer, del que tiene menos stock al que tiene más.

El botón **Actualizar** vuelve a pedir los datos a la API.

### Productos

**Crear un producto**

1. Completá **Nombre**, **Precio por kg ($)** y **Stock (kg)**.
2. Presioná **Guardar producto**.

**Editar un producto**

1. En el listado, presioná **Editar** en su fila. Los datos pasan al formulario y la fila queda resaltada.
2. Cambiá lo necesario y presioná **Guardar cambios**. Con **Cancelar** se descartan los cambios.

**Eliminar un producto:** presioná **Eliminar** y confirmá.

**Buscar:** escribí en **Buscar producto…** y el listado se filtra mientras escribís.

Reglas:

- El nombre es obligatorio y no puede repetirse, aunque cambien las mayúsculas.
- El precio tiene que ser mayor a 0 y el stock no puede ser negativo. Se aceptan hasta 2 decimales.
- Un producto con ventas no se puede eliminar.
- En el listado, el stock aparece en naranja si es menor a 5 kg y como **Sin stock** si llegó a 0.

### Cajeros

**Crear un cajero**

1. Completá **Nombre y apellido** y **Legajo**, por ejemplo `CAJ-03`.
2. Dejá marcado **Activo (puede registrar ventas)**.
3. Presioná **Guardar cajero**.

**Editar o desactivar un cajero:** presioná **Editar**, hacé los cambios y presioná **Guardar cambios**. Para que un cajero deje de vender, desmarcá **Activo**: no se borra, pero deja de aparecer en el formulario de ventas.

**Eliminar un cajero:** presioná **Eliminar** y confirmá.

Reglas:

- El nombre y el legajo son obligatorios.
- El legajo se guarda en mayúsculas y no puede repetirse: `caj-03` y `CAJ-03` cuentan como el mismo.
- Un cajero con ventas no se puede eliminar. En ese caso, desactivalo.

### Ventas

**Registrar una venta**

1. Elegí el **Cajero**. Solo aparecen los activos.
2. Elegí el **Producto**. El desplegable muestra el precio por kg y el stock; los productos sin stock no se pueden elegir.
3. Ingresá la **Cantidad (kg)**, por ejemplo `1.5`.
4. Revisá la vista previa: precio por kg, stock disponible, **stock después de la venta** y **total**.
5. Presioná **Registrar venta**. El stock del producto se descuenta en el momento.

**Modificar una venta**

1. Presioná **Editar** en la venta. El formulario pasa a decir *Modificar venta #N*.
2. Podés cambiar el **cajero** y la **cantidad**. El producto no se puede cambiar: si te equivocaste de producto, eliminá la venta y cargala de nuevo.
3. Presioná **Guardar cambios**. El stock y el total se recalculan solos.

**Eliminar una venta:** presioná **Eliminar** y confirmá. Los kilos de esa venta vuelven al stock del producto.

**Filtrar:** los botones **Todas**, **Hoy** y **Semana** cambian el listado. Arriba de la tabla se ve cuántas ventas hay y el **total recaudado** del período.

Reglas:

- La cantidad tiene que ser mayor a 0 y no puede superar el stock disponible.
- El total es kilos × precio por kg, redondeado a 2 decimales, y lo calcula el servidor.
- La fecha y hora de la venta se ponen solas al registrarla.

---

## Recorrido de prueba (5 minutos)

Sirve para comprobar que todo funciona o para mostrar el sistema en la defensa. Empieza con la base recién creada en el paso 3.

| # | Qué hacer | Qué tiene que pasar |
| --- | --- | --- |
| 1 | Abrir el sistema en **Resumen** | Recaudado hoy en $ 0,00; 2 cajeros activos; 0 productos con poco stock |
| 2 | **Ventas** → cajero Lucía Gómez, producto Tomate, `2.5` kg | Vista previa: total $ 4.750,00 y stock después de la venta 37,5 kg |
| 3 | Presionar **Registrar venta** | Mensaje "Venta registrada" y la venta en el listado |
| 4 | Cargar otra venta de Tomate por `100` kg | Error de stock insuficiente: la venta no se registra |
| 5 | **Editar** la venta del paso 3 y poner `5` kg | Total $ 9.500,00; el Tomate queda en 35 kg |
| 6 | **Productos** → crear "Zanahoria", $1500, `3` kg | Aparece en el listado con el stock en naranja |
| 7 | **Productos** → **Eliminar** Tomate | Error: no se puede eliminar porque tiene ventas |
| 8 | **Cajeros** → crear "Ana López", legajo `caj-03` | Se guarda con legajo CAJ-03 |
| 9 | **Editar** a Ana López, desmarcar **Activo** y guardar | En **Ventas**, Ana ya no aparece en el desplegable de cajeros |
| 10 | **Ventas** → **Eliminar** la venta de Tomate | El Tomate vuelve a tener 40 kg |
| 11 | Volver a **Resumen** | Zanahoria figura en **Stock bajo**. Como se eliminó la única venta, lo recaudado hoy vuelve a $ 0,00 |

---

## Problemas frecuentes

| Problema | Causa probable | Solución |
| --- | --- | --- |
| El front muestra **API sin conexión** | La API no está corriendo, o se ejecutó con otro perfil o puerto | Hacé el paso 4 con el perfil **`http`** y recargá la página (F5) |
| Las tablas muestran **"Error interno al obtener…"** | La API funciona pero no llega a la base: SQL Server está detenido o la base no existe | Iniciá **SQL Server (SQLEXPRESS)** en `services.msc` y hacé el paso 3 |
| `A network-related or instance-specific error…` al crear la base | El nombre de la instancia no coincide | Revisá el `Server=` del paso 2 |
| `Cannot open database "VerduleriaDb"` | Falta crear la base | Hacé el paso 3 |
| `dotnet ef` no se reconoce como comando | Falta la herramienta o la terminal se abrió antes de instalarla | Instalá `dotnet-ef` (paso 0) y abrí una terminal nueva |
| `Update-Database` no se reconoce en Visual Studio | Falta el paquete `Microsoft.EntityFrameworkCore.Tools` | Usá el comando `dotnet ef` del paso 3 |
| Se ejecutó la API con el perfil **https** y el front no conecta | El front apunta a `http://localhost:5129` y la API redirige a https | Ejecutá con el perfil **`http`**. Si necesitás https, cambiá la URL en `Frontend/js/config.js` por `https://localhost:7036/api` y ejecutá `dotnet dev-certs https --trust` |
| `Failed to bind to address … 5129: address already in use` | Ya hay otra instancia de la API abierta | Cerrá la otra ventana o terminal de la API y volvé a ejecutarla |
| Con doble clic en `index.html` el navegador no conecta y la API sí responde en Swagger | El navegador bloquea pedidos desde archivos locales | Abrí el front con **Live Server** (paso 5) |

---

## Empezar de nuevo con la base limpia

Para borrar todas las ventas y volver a los datos iniciales, por ejemplo antes de la defensa: detené la API y ejecutá en la carpeta `Backend`:

```powershell
dotnet ef database drop --force --project Verduleria.Data --startup-project Verduleria.Api
dotnet ef database update --project Verduleria.Data --startup-project Verduleria.Api
```

---

## Más información

- [`Frontend/README.md`](Frontend/README.md): organización de los archivos del front y endpoints de la API que usa cada pantalla.
- [`Prompts.md`](Prompts.md): prompts usados con Inteligencia Artificial durante el desarrollo.
- Con la API en marcha, <http://localhost:5129/swagger> documenta y permite probar todos los endpoints.
