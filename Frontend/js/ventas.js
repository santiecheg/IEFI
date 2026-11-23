// =========================================================
// Ventas: registrar, listar, editar y eliminar.
// Acá se ve la lógica de negocio del sistema: control de stock,
// cálculo del total y solo cajeros activos pueden vender.
// La validación definitiva la hace VentaService en el backend.
// =========================================================

const formVenta = document.getElementById("form-venta");
const inputVentaId = document.getElementById("venta-id");
const selectVentaCajero = document.getElementById("venta-cajero");
const selectVentaProducto = document.getElementById("venta-producto");
const inputVentaCantidad = document.getElementById("venta-cantidad");
const btnGuardarVenta = document.getElementById("btn-guardar-venta");
const btnCancelarVenta = document.getElementById("btn-cancelar-venta");
const tablaVentas = document.getElementById("tabla-ventas");
const textoResumenVentas = document.getElementById("ventas-resumen");
const filtroVentas = document.getElementById("filtro-ventas");

let filtroActual = "todas";   // "todas" | "hoy" | "semana"
let ventaEditando = null;     // venta que se está modificando (o null)

// ---------------------- Carga inicial de la sección ----------------------

async function cargarSeccionVentas() {
    try {
        const [cajeros, productos] = await Promise.all([api.cajeros.listar(), api.productos.listar()]);
        estado.cajeros = cajeros;
        estado.productos = productos;
        llenarSelectCajeros();
        llenarSelectProductos();
        actualizarPreview();
    } catch (error) {
        mostrarToast(error.message, "error");
    }
    await cargarVentas();
}

// ---------------------- Selects del formulario ----------------------

function llenarSelectCajeros() {
    const seleccionado = selectVentaCajero.value;
    const activos = estado.cajeros.filter(c => c.activo);

    if (activos.length === 0) {
        selectVentaCajero.innerHTML = `<option value="">No hay cajeros activos</option>`;
        return;
    }

    selectVentaCajero.innerHTML =
        `<option value="">Seleccioná un cajero</option>` +
        activos.map(c => `<option value="${c.id}">${escaparHtml(c.nombre)} (${escaparHtml(c.legajo)})</option>`).join("");

    selectVentaCajero.value = activos.some(c => String(c.id) === seleccionado) ? seleccionado : "";
}

function llenarSelectProductos() {
    const seleccionado = selectVentaProducto.value;

    if (estado.productos.length === 0) {
        selectVentaProducto.innerHTML = `<option value="">No hay productos cargados</option>`;
        return;
    }

    selectVentaProducto.innerHTML =
        `<option value="">Seleccioná un producto</option>` +
        estado.productos.map(p => {
            const sinStock = stockDisponible(p) <= 0;
            // Al registrar una venta nueva no se pueden elegir productos sin stock
            const deshabilitado = sinStock && !ventaEditando ? "disabled" : "";
            const detalle = sinStock ? "sin stock" : formatearKg(stockDisponible(p));
            return `<option value="${p.id}" ${deshabilitado}>
                        ${escaparHtml(p.nombre)} · ${formatearMoneda(p.precioPorKg)}/kg · ${detalle}
                    </option>`;
        }).join("");

    selectVentaProducto.value = estado.productos.some(p => String(p.id) === seleccionado) ? seleccionado : "";
}

function productoSeleccionado() {
    const id = Number(selectVentaProducto.value);
    return estado.productos.find(p => p.id === id) ?? null;
}

// Stock que se puede vender. Si se está editando una venta, los kg que ya
// tenía esa venta vuelven al stock (igual que hace ActualizarCantidadAsync).
function stockDisponible(producto) {
    if (ventaEditando && ventaEditando.productoId === producto.id) {
        return redondear2(producto.stockKg + ventaEditando.cantidadKg);
    }
    return producto.stockKg;
}

// Evita resultados como 0.7 + 0.1 = 0.7999999999999999
function redondear2(valor) {
    return Math.round(valor * 100) / 100;
}

// Total = kg x precio, redondeado a 2 decimales igual que Math.Round(decimal, 2)
// de .NET (redondeo "a par": si queda justo en la mitad, va al centavo par).
// Se trabaja en centavos para evitar errores de coma flotante.
function calcularTotal(cantidadKg, precioPorKg) {
    const kgEnCentesimos = Math.round(cantidadKg * 100);
    const precioEnCentavos = Math.round(precioPorKg * 100);
    const totalPor10000 = kgEnCentesimos * precioEnCentavos;

    let centavos = Math.floor(totalPor10000 / 100);
    const resto = totalPor10000 % 100;
    if (resto > 50 || (resto === 50 && centavos % 2 === 1)) centavos++;

    return centavos / 100;
}

// ---------------------- Vista previa (total y stock) ----------------------

function actualizarPreview() {
    const producto = productoSeleccionado();
    const cantidad = Number(inputVentaCantidad.value) || 0;

    const precio = document.getElementById("preview-precio");
    const stock = document.getElementById("preview-stock");
    const restante = document.getElementById("preview-restante");
    const total = document.getElementById("preview-total");

    if (!producto) {
        precio.textContent = "—";
        stock.textContent = "—";
        restante.textContent = "—";
        restante.classList.remove("is-negativo");
        total.textContent = formatearMoneda(0);
        return;
    }

    const disponible = stockDisponible(producto);
    const queda = redondear2(disponible - cantidad);

    precio.textContent = formatearMoneda(producto.precioPorKg);
    stock.textContent = formatearKg(disponible);
    restante.textContent = queda < 0 ? "Stock insuficiente" : formatearKg(queda);
    restante.classList.toggle("is-negativo", queda < 0);
    total.textContent = formatearMoneda(calcularTotal(cantidad, producto.precioPorKg));
}

// ---------------------- Validar ----------------------

function validarVenta() {
    const errores = {};
    const cajero = estado.cajeros.find(c => c.id === Number(selectVentaCajero.value));
    const producto = productoSeleccionado();

    if (!cajero) {
        errores["venta-cajero"] = "Seleccioná un cajero.";
    } else if (!cajero.activo) {
        errores["venta-cajero"] = "El cajero seleccionado no está activo.";
    }

    if (!producto) errores["venta-producto"] = "Seleccioná un producto.";

    const errorCantidad = validarNumero(inputVentaCantidad.value, { nombre: "la cantidad", minimo: 0 });
    if (errorCantidad) {
        errores["venta-cantidad"] = errorCantidad;
    } else if (producto && Number(inputVentaCantidad.value) > stockDisponible(producto)) {
        errores["venta-cantidad"] =
            `Stock insuficiente para ${producto.nombre}. Disponible: ${formatearKg(stockDisponible(producto))}.`;
    }

    return aplicarErrores(formVenta, errores);
}

// ---------------------- Registrar / Modificar ----------------------

formVenta.addEventListener("submit", async evento => {
    evento.preventDefault();
    if (!validarVenta()) return;

    const cajeroId = Number(selectVentaCajero.value);
    const cantidadKg = Number(inputVentaCantidad.value);

    await conBotonOcupado(btnGuardarVenta, "Guardando…", async () => {
        try {
            if (ventaEditando) {
                await api.ventas.actualizar(ventaEditando.id, { cajeroId, cantidadKg });
                mostrarToast("Venta modificada. El stock se recalculó.");
            } else {
                const productoId = Number(selectVentaProducto.value);
                const venta = await api.ventas.crear({ cajeroId, productoId, cantidadKg });
                mostrarToast(`Venta registrada: ${formatearKg(venta.cantidadKg)} por ${formatearMoneda(venta.total)}.`);
            }
            resetearFormVenta();
            await cargarSeccionVentas();   // el stock de los productos cambió
        } catch (error) {
            mostrarToast(error.message, "error");
        }
    });
});

function editarVenta(id) {
    const venta = estado.ventas.find(v => v.id === id);
    if (!venta) return;

    ventaEditando = venta;
    inputVentaId.value = venta.id;

    llenarSelectProductos();
    llenarSelectCajeros();
    selectVentaProducto.value = venta.productoId;
    selectVentaProducto.disabled = true;   // la API no permite cambiar el producto
    selectVentaCajero.value = estado.cajeros.some(c => c.id === venta.cajeroId && c.activo) ? venta.cajeroId : "";
    inputVentaCantidad.value = venta.cantidadKg;

    document.getElementById("form-venta-titulo").textContent = `Modificar venta #${venta.id}`;
    btnGuardarVenta.textContent = "Guardar cambios";
    btnCancelarVenta.hidden = false;
    limpiarErrores(formVenta);
    actualizarPreview();
    renderizarVentas();
    inputVentaCantidad.focus();
}

function resetearFormVenta() {
    ventaEditando = null;
    formVenta.reset();
    inputVentaId.value = "";
    selectVentaProducto.disabled = false;

    document.getElementById("form-venta-titulo").textContent = "Registrar venta";
    btnGuardarVenta.textContent = "Registrar venta";
    btnCancelarVenta.hidden = true;
    limpiarErrores(formVenta);
    llenarSelectProductos();
    llenarSelectCajeros();
    actualizarPreview();
    renderizarVentas();
}

// ---------------------- Eliminar ----------------------

async function eliminarVenta(id) {
    const venta = estado.ventas.find(v => v.id === id);
    if (!venta) return;

    const producto = venta.producto?.nombre ?? "el producto";
    const mensaje = `¿Eliminar la venta #${venta.id} (${formatearKg(venta.cantidadKg)} de ${producto})?\n` +
                    `Los ${formatearKg(venta.cantidadKg)} vuelven al stock.`;
    if (!confirm(mensaje)) return;

    try {
        await api.ventas.eliminar(id);
        mostrarToast(`Venta #${id} eliminada. Se devolvió el stock.`);
        if (ventaEditando?.id === id) resetearFormVenta();
        await cargarSeccionVentas();
    } catch (error) {
        mostrarToast(error.message, "error");
    }
}

// ---------------------- Listado y filtros ----------------------

async function cargarVentas() {
    tablaVentas.innerHTML = filaMensaje(6, "Cargando ventas…");
    textoResumenVentas.innerHTML = "&nbsp;";

    try {
        let ventas;
        let totalRecaudado;

        if (filtroActual === "hoy") {
            const respuesta = await api.ventas.hoy();          // GET api/venta/hoy
            ventas = respuesta.ventas;
            totalRecaudado = respuesta.totalRecaudado;
        } else if (filtroActual === "semana") {
            const respuesta = await api.ventas.semana();       // GET api/venta/semana
            ventas = respuesta.ventas;
            totalRecaudado = respuesta.totalRecaudado;
        } else {
            ventas = await api.ventas.listar();                // GET api/venta
            totalRecaudado = ventas.reduce((suma, v) => suma + v.total, 0);
        }

        estado.ventas = ventas;
        renderizarVentas();

        const etiqueta = { todas: "en total", hoy: "hoy", semana: "esta semana" }[filtroActual];
        const palabra = ventas.length === 1 ? "venta" : "ventas";
        textoResumenVentas.textContent =
            `${ventas.length} ${palabra} ${etiqueta} · Recaudado: ${formatearMoneda(totalRecaudado)}`;
    } catch (error) {
        tablaVentas.innerHTML = filaMensaje(6, error.message, true);
    }
}

function renderizarVentas() {
    if (estado.ventas.length === 0) {
        const mensaje = {
            todas: "Todavía no hay ventas registradas.",
            hoy: "No hay ventas registradas hoy.",
            semana: "No hay ventas registradas esta semana."
        }[filtroActual];
        tablaVentas.innerHTML = filaMensaje(6, mensaje);
        return;
    }

    tablaVentas.innerHTML = estado.ventas.map(v => `
        <tr class="${ventaEditando?.id === v.id ? "is-editando" : ""}">
            <td>${formatearFecha(v.fecha)}</td>
            <td>${escaparHtml(v.producto?.nombre ?? `Producto #${v.productoId}`)}</td>
            <td>${escaparHtml(v.cajero?.nombre ?? `Cajero #${v.cajeroId}`)}</td>
            <td class="num">${formatearKg(v.cantidadKg)}</td>
            <td class="num">${formatearMoneda(v.total)}</td>
            <td class="acciones">
                <button class="btn btn--ghost btn--sm" data-accion="editar" data-id="${v.id}">Editar</button>
                <button class="btn btn--danger btn--sm" data-accion="eliminar" data-id="${v.id}">Eliminar</button>
            </td>
        </tr>
    `).join("");
}

// ---------------------- Eventos ----------------------

selectVentaProducto.addEventListener("change", actualizarPreview);
inputVentaCantidad.addEventListener("input", actualizarPreview);
btnCancelarVenta.addEventListener("click", resetearFormVenta);

tablaVentas.addEventListener("click", evento => {
    const boton = evento.target.closest("button[data-accion]");
    if (!boton) return;

    const id = Number(boton.dataset.id);
    if (boton.dataset.accion === "editar") editarVenta(id);
    if (boton.dataset.accion === "eliminar") eliminarVenta(id);
});

filtroVentas.addEventListener("click", evento => {
    const boton = evento.target.closest("button[data-filtro]");
    if (!boton || boton.dataset.filtro === filtroActual) return;

    filtroActual = boton.dataset.filtro;
    filtroVentas.querySelectorAll("button").forEach(b => b.classList.toggle("is-active", b === boton));
    cargarVentas();
});
