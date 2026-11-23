// =========================================================
// Resumen: usa los endpoints de reportes de la API
//   GET api/venta/hoy       -> ventas del día + total recaudado
//   GET api/venta/semana    -> ventas de la semana + total recaudado
//   GET api/venta/ultimas   -> últimas N ventas
// =========================================================

const selectUltimas = document.getElementById("select-ultimas");
const tablaUltimas = document.getElementById("tabla-ultimas");
const tablaStockBajo = document.getElementById("tabla-stock-bajo");

async function cargarResumen() {
    tablaUltimas.innerHTML = filaMensaje(5, "Cargando…");
    tablaStockBajo.innerHTML = filaMensaje(2, "Cargando…");

    try {
        const [hoy, semana, ultimas, productos, cajeros] = await Promise.all([
            api.ventas.hoy(),
            api.ventas.semana(),
            api.ventas.ultimas(selectUltimas.value),
            api.productos.listar(),
            api.cajeros.listar()
        ]);

        estado.productos = productos;
        estado.cajeros = cajeros;

        renderizarIndicadores(hoy, semana, productos, cajeros);
        renderizarUltimas(ultimas);
        renderizarStockBajo(productos);
    } catch (error) {
        tablaUltimas.innerHTML = filaMensaje(5, error.message, true);
        tablaStockBajo.innerHTML = filaMensaje(2, "Sin datos.", true);
        mostrarToast(error.message, "error");
    }
}

function renderizarIndicadores(hoy, semana, productos, cajeros) {
    const plural = n => (n === 1 ? "venta" : "ventas");

    document.getElementById("stat-hoy-total").textContent = formatearMoneda(hoy.totalRecaudado);
    document.getElementById("stat-hoy-cantidad").textContent = `${hoy.cantidadVentas} ${plural(hoy.cantidadVentas)}`;

    document.getElementById("stat-semana-total").textContent = formatearMoneda(semana.totalRecaudado);
    document.getElementById("stat-semana-cantidad").textContent = `${semana.cantidadVentas} ${plural(semana.cantidadVentas)} desde el lunes`;

    const conPocoStock = productos.filter(p => p.stockKg < STOCK_BAJO_KG).length;
    document.getElementById("stat-stock-bajo").textContent = conPocoStock;
    document.getElementById("stat-stock-bajo-hint").textContent = `de ${productos.length} productos (menos de ${STOCK_BAJO_KG} kg)`;

    const activos = cajeros.filter(c => c.activo).length;
    document.getElementById("stat-cajeros").textContent = activos;
    document.getElementById("stat-cajeros-hint").textContent = `de ${cajeros.length} cajeros registrados`;
}

function renderizarUltimas(ventas) {
    if (ventas.length === 0) {
        tablaUltimas.innerHTML = filaMensaje(5, "Todavía no hay ventas registradas.");
        return;
    }

    tablaUltimas.innerHTML = ventas.map(v => `
        <tr>
            <td>${formatearFecha(v.fecha)}</td>
            <td>${escaparHtml(v.producto?.nombre ?? `Producto #${v.productoId}`)}</td>
            <td>${escaparHtml(v.cajero?.nombre ?? `Cajero #${v.cajeroId}`)}</td>
            <td class="num">${formatearKg(v.cantidadKg)}</td>
            <td class="num">${formatearMoneda(v.total)}</td>
        </tr>
    `).join("");
}

function renderizarStockBajo(productos) {
    const bajos = productos
        .filter(p => p.stockKg < STOCK_BAJO_KG)
        .sort((a, b) => a.stockKg - b.stockKg);

    if (bajos.length === 0) {
        tablaStockBajo.innerHTML = filaMensaje(2, "Todos los productos tienen stock suficiente.");
        return;
    }

    tablaStockBajo.innerHTML = bajos.map(p => `
        <tr>
            <td>${escaparHtml(p.nombre)}</td>
            <td class="num">${etiquetaStock(p.stockKg)}</td>
        </tr>
    `).join("");
}

selectUltimas.addEventListener("change", cargarResumen);
document.getElementById("btn-actualizar-resumen").addEventListener("click", cargarResumen);
