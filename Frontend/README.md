# Frontend – Verdulería

Frontend del Trabajo Práctico Integrador de Programación II.
Hecho con **HTML, CSS y JavaScript vainilla** (sin frameworks), consume la Web API REST en .NET 8 que está en `../Backend`.

## Cómo levantarlo

El paso a paso completo (requisitos, base de datos, ejecución de la API y uso de cada pantalla) está en el [README principal](../README.md).

En resumen: con la API corriendo con el perfil **http** (`http://localhost:5129`), abrí `index.html` con doble clic o con **Live Server**. Arriba a la derecha tiene que aparecer **API conectada** en verde.

> Si la API se ejecuta con el perfil **https**, cambiá la URL en `js/config.js` por `https://localhost:7036/api`.

## Estructura

```
Frontend/
├── index.html        → página única con 4 secciones (Resumen, Ventas, Productos, Cajeros)
├── css/
│   └── styles.css    → estilos
└── js/
    ├── config.js     → URL de la API, umbral de stock bajo y datos compartidos
    ├── utils.js      → formatos ($, kg, fechas), mensajes, errores de formularios y validación de números
    ├── api.js        → todas las llamadas fetch a la API y el manejo de errores HTTP
    ├── productos.js  → CRUD de productos
    ├── cajeros.js    → CRUD de cajeros
    ├── ventas.js     → CRUD de ventas + lógica de negocio (stock y total)
    ├── resumen.js    → indicadores del día, de la semana y últimas ventas
    └── app.js        → navegación entre secciones e inicio de la app
```

Los scripts se cargan en ese orden al final de `index.html` porque cada archivo usa funciones de los anteriores.

## Endpoints que consume

| Sección   | Método y ruta                                  | Uso                                   |
|-----------|------------------------------------------------|---------------------------------------|
| Productos | `GET/POST /api/producto`                       | Listar y crear                        |
|           | `PUT/DELETE /api/producto/{id}`                | Modificar y eliminar                  |
| Cajeros   | `GET/POST /api/cajero`                         | Listar y crear                        |
|           | `PUT/DELETE /api/cajero/{id}`                  | Modificar y eliminar                  |
| Ventas    | `GET /api/venta`                               | Listado completo                      |
|           | `POST /api/venta`                              | Registrar venta `{ cajeroId, productoId, cantidadKg }` |
|           | `PUT /api/venta/{id}`                          | Modificar venta `{ cajeroId, cantidadKg }` |
|           | `DELETE /api/venta/{id}`                       | Eliminar venta (devuelve el stock)    |
| Reportes  | `GET /api/venta/hoy`                           | Ventas y total recaudado del día      |
|           | `GET /api/venta/semana`                        | Ventas y total recaudado de la semana |
|           | `GET /api/venta/ultimas?cantidad=N`            | Últimas N ventas                      |

## Validaciones del front

Repiten las reglas de los services del backend para avisar antes de enviar. **La validación definitiva siempre la hace la API**: si responde un error, el mensaje (`{ mensaje }`) se muestra tal cual.

- **Producto:** nombre obligatorio y no repetido, precio por kg mayor a 0, stock no negativo, hasta 2 decimales.
- **Cajero:** nombre y legajo obligatorios, legajo no repetido (se guarda en mayúsculas), estado activo/inactivo.
- **Venta:** cajero activo, producto elegido, cantidad mayor a 0 y que no supere el stock disponible.

## Lógica de negocio (ventas)

- Al elegir producto y cantidad se muestra el **total** (kg × precio) y el **stock que queda** antes de confirmar.
- Solo se listan los **cajeros activos** y no se pueden elegir productos **sin stock**.
- Al **modificar** una venta, los kg que tenía vuelven al stock disponible (igual que en `VentaService.ActualizarCantidadAsync`). El producto no se puede cambiar porque la API no lo permite.
- Al **eliminar** una venta, el stock se devuelve al producto.
- No se pueden eliminar productos ni cajeros que ya tengan ventas: la API lo rechaza y el front muestra el motivo.
