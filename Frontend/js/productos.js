// =========================================================
// CRUD de Productos
// =========================================================

const formProducto = document.getElementById("form-producto");
const inputProductoId = document.getElementById("producto-id");
const inputProductoNombre = document.getElementById("producto-nombre");
const inputProductoPrecio = document.getElementById("producto-precio");
const inputProductoStock = document.getElementById("producto-stock");
const btnGuardarProducto = document.getElementById("btn-guardar-producto");
const btnCancelarProducto = document.getElementById("btn-cancelar-producto");
const tablaProductos = document.getElementById("tabla-productos");
const inputBuscarProducto = document.getElementById("buscar-producto");

// ---------------------- Listar ----------------------

async function cargarProductos() {
    tablaProductos.innerHTML = filaMensaje(4, "Cargando productos…");
    try {
        estado.productos = await api.productos.listar();
        renderizarProductos();
    } catch (error) {
        tablaProductos.innerHTML = filaMensaje(4, error.message, true);
    }
}

function renderizarProductos() {
    const texto = inputBuscarProducto.value.trim().toLowerCase();
    const productos = estado.productos.filter(p => p.nombre.toLowerCase().includes(texto));

    if (productos.length === 0) {
        const mensaje = texto ? "Ningún producto coincide con la búsqueda." : "Todavía no hay productos cargados.";
        tablaProductos.innerHTML = filaMensaje(4, mensaje);
        return;
    }

    const idEditando = Number(inputProductoId.value);

    tablaProductos.innerHTML = productos.map(p => `
        <tr class="${p.id === idEditando ? "is-editando" : ""}">
            <td>${escaparHtml(p.nombre)}</td>
            <td class="num">${formatearMoneda(p.precioPorKg)}</td>
            <td class="num">${etiquetaStock(p.stockKg)}</td>
            <td class="acciones">
                <button class="btn btn--ghost btn--sm" data-accion="editar" data-id="${p.id}">Editar</button>
                <button class="btn btn--danger btn--sm" data-accion="eliminar" data-id="${p.id}">Eliminar</button>
            </td>
        </tr>
    `).join("");
}

// Muestra el stock con una etiqueta de color si está bajo o agotado
function etiquetaStock(stockKg) {
    if (stockKg <= 0) return `<span class="badge badge--error">Sin stock</span>`;
    if (stockKg < STOCK_BAJO_KG) return `<span class="badge badge--aviso">${formatearKg(stockKg)}</span>`;
    return formatearKg(stockKg);
}

// ---------------------- Validar ----------------------

// Mismas reglas que ProductoService en el backend
function validarProducto() {
    const errores = {};
    const nombre = inputProductoNombre.value.trim();

    if (nombre === "") {
        errores["producto-nombre"] = "El nombre del producto es obligatorio.";
    } else {
        const idActual = Number(inputProductoId.value);
        const repetido = estado.productos.some(p =>
            p.nombre.toLowerCase() === nombre.toLowerCase() && p.id !== idActual);
        if (repetido) errores["producto-nombre"] = "Ya existe un producto con ese nombre.";
    }

    const errorPrecio = validarNumero(inputProductoPrecio.value, { nombre: "el precio por kg", minimo: 0 });
    if (errorPrecio) errores["producto-precio"] = errorPrecio;

    const errorStock = validarNumero(inputProductoStock.value, { nombre: "el stock", minimo: 0, permitirCero: true });
    if (errorStock) errores["producto-stock"] = errorStock;

    return aplicarErrores(formProducto, errores);
}

// ---------------------- Crear / Editar ----------------------

formProducto.addEventListener("submit", async evento => {
    evento.preventDefault();
    if (!validarProducto()) return;

    const id = Number(inputProductoId.value);
    const producto = {
        nombre: inputProductoNombre.value.trim(),
        precioPorKg: Number(inputProductoPrecio.value),
        stockKg: Number(inputProductoStock.value)
    };

    await conBotonOcupado(btnGuardarProducto, "Guardando…", async () => {
        try {
            if (id) {
                await api.productos.actualizar(id, { id, ...producto });
                mostrarToast(`Producto "${producto.nombre}" actualizado.`);
            } else {
                await api.productos.crear(producto);
                mostrarToast(`Producto "${producto.nombre}" creado.`);
            }
            resetearFormProducto();
            await cargarProductos();
        } catch (error) {
            mostrarToast(error.message, "error");
        }
    });
});

function editarProducto(id) {
    const producto = estado.productos.find(p => p.id === id);
    if (!producto) return;

    inputProductoId.value = producto.id;
    inputProductoNombre.value = producto.nombre;
    inputProductoPrecio.value = producto.precioPorKg;
    inputProductoStock.value = producto.stockKg;

    document.getElementById("form-producto-titulo").textContent = "Editar producto";
    btnGuardarProducto.textContent = "Guardar cambios";
    btnCancelarProducto.hidden = false;
    limpiarErrores(formProducto);
    renderizarProductos();
    inputProductoNombre.focus();
}

function resetearFormProducto() {
    formProducto.reset();
    inputProductoId.value = "";
    document.getElementById("form-producto-titulo").textContent = "Nuevo producto";
    btnGuardarProducto.textContent = "Guardar producto";
    btnCancelarProducto.hidden = true;
    limpiarErrores(formProducto);
    renderizarProductos();
}

// ---------------------- Eliminar ----------------------

async function eliminarProducto(id) {
    const producto = estado.productos.find(p => p.id === id);
    if (!producto) return;

    if (!confirm(`¿Eliminar el producto "${producto.nombre}"?`)) return;

    try {
        await api.productos.eliminar(id);
        mostrarToast(`Producto "${producto.nombre}" eliminado.`);
        if (Number(inputProductoId.value) === id) resetearFormProducto();
        await cargarProductos();
    } catch (error) {
        // Ej: "No se puede eliminar un producto que ya tiene ventas registradas."
        mostrarToast(error.message, "error");
    }
}

// ---------------------- Eventos ----------------------

btnCancelarProducto.addEventListener("click", resetearFormProducto);
inputBuscarProducto.addEventListener("input", renderizarProductos);

// Un solo listener para todos los botones de la tabla (delegación de eventos)
tablaProductos.addEventListener("click", evento => {
    const boton = evento.target.closest("button[data-accion]");
    if (!boton) return;

    const id = Number(boton.dataset.id);
    if (boton.dataset.accion === "editar") editarProducto(id);
    if (boton.dataset.accion === "eliminar") eliminarProducto(id);
});
