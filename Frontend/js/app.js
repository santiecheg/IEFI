// =========================================================
// Navegación entre secciones e inicio de la aplicación
// =========================================================

// Qué datos se cargan al entrar a cada sección
const cargadores = {
    resumen: cargarResumen,
    ventas: cargarSeccionVentas,
    productos: cargarProductos,
    cajeros: cargarCajeros
};

function mostrarSeccion(nombre) {
    document.querySelectorAll(".tab").forEach(tab =>
        tab.classList.toggle("is-active", tab.dataset.seccion === nombre));

    document.querySelectorAll(".section").forEach(seccion =>
        seccion.classList.toggle("is-visible", seccion.id === `seccion-${nombre}`));

    // Se guarda en la URL (#ventas, #productos…) para que F5 no vuelva al inicio
    try {
        history.replaceState(null, "", `#${nombre}`);
    } catch {
        // Algunos navegadores no lo permiten al abrir el archivo con doble clic (file://)
    }

    // Siempre se recargan los datos para ver los cambios hechos en otras secciones
    cargadores[nombre]();
}

document.getElementById("tabs").addEventListener("click", evento => {
    const tab = evento.target.closest(".tab");
    if (tab) mostrarSeccion(tab.dataset.seccion);
});

// Cuando el usuario corrige un campo con error, el mensaje desaparece
document.querySelectorAll("form").forEach(formulario => {
    formulario.addEventListener("input", evento => limpiarErrorCampo(evento.target.id));
    formulario.addEventListener("change", evento => limpiarErrorCampo(evento.target.id));
});

// Al abrir la página se muestra la sección de la URL o el Resumen
const seccionInicial = location.hash.replace("#", "");
mostrarSeccion(cargadores[seccionInicial] ? seccionInicial : "resumen");
