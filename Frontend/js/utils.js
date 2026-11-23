// =========================================================
// Funciones auxiliares: formatos, mensajes y validaciones
// =========================================================

const formatoMoneda = new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS",
    minimumFractionDigits: 2
});

const formatoNumero = new Intl.NumberFormat("es-AR", {
    maximumFractionDigits: 2
});

// 1900 -> "$ 1.900,00"
function formatearMoneda(valor) {
    return formatoMoneda.format(Number(valor) || 0);
}

// 2.5 -> "2,5 kg"
function formatearKg(valor) {
    return `${formatoNumero.format(Number(valor) || 0)} kg`;
}

// "2026-11-23T10:30:00" -> "23/11/26, 10:30"
function formatearFecha(fechaIso) {
    const fecha = new Date(fechaIso);
    if (isNaN(fecha)) return "—";
    return fecha.toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" });
}

// Evita que un texto cargado por el usuario se interprete como HTML
function escaparHtml(texto) {
    const div = document.createElement("div");
    div.textContent = texto ?? "";
    return div.innerHTML;
}

// Fila de tabla para "sin datos", "cargando" o errores
function filaMensaje(columnas, mensaje, esError = false) {
    const clase = esError ? "vacio is-error" : "vacio";
    return `<tr><td colspan="${columnas}" class="${clase}">${escaparHtml(mensaje)}</td></tr>`;
}

// ---------------------- Mensajes flotantes ----------------------

let temporizadorToast;

function mostrarToast(mensaje, tipo = "exito") {
    const toast = document.getElementById("toast");
    toast.textContent = mensaje;
    toast.className = `toast toast--${tipo} is-visible`;

    clearTimeout(temporizadorToast);
    temporizadorToast = setTimeout(() => {
        toast.classList.remove("is-visible");
    }, 3500);
}

// ---------------------- Estado de la conexión ----------------------

function actualizarEstadoApi(conectada) {
    const indicador = document.getElementById("api-status");
    const texto = document.getElementById("api-status-texto");

    indicador.classList.toggle("is-ok", conectada);
    indicador.classList.toggle("is-error", !conectada);
    texto.textContent = conectada ? "API conectada" : "API sin conexión";
}

// ---------------------- Errores de formularios ----------------------

function mostrarErrorCampo(idCampo, mensaje) {
    const campo = document.getElementById(idCampo);
    const error = document.querySelector(`[data-error-de="${idCampo}"]`);

    campo.closest(".field").classList.add("has-error");
    error.textContent = mensaje;
}

// Borra el error de un solo campo (se usa cuando el usuario lo corrige)
function limpiarErrorCampo(idCampo) {
    const error = document.querySelector(`[data-error-de="${idCampo}"]`);
    if (!error) return;

    error.textContent = "";
    document.getElementById(idCampo).closest(".field").classList.remove("has-error");
}

function limpiarErrores(formulario) {
    formulario.querySelectorAll(".field").forEach(field => field.classList.remove("has-error"));
    formulario.querySelectorAll(".field__error").forEach(error => (error.textContent = ""));
}

// Muestra todos los errores de un objeto { idCampo: "mensaje" }.
// Devuelve true si no hubo errores.
function aplicarErrores(formulario, errores) {
    limpiarErrores(formulario);

    const ids = Object.keys(errores);
    ids.forEach(id => mostrarErrorCampo(id, errores[id]));

    if (ids.length > 0) {
        document.getElementById(ids[0]).focus();
        return false;
    }
    return true;
}

// ---------------------- Validaciones de números ----------------------

// Acepta números con hasta 2 decimales (las columnas son decimal(10,2))
function tieneHastaDosDecimales(textoNumero) {
    return /^\d+(\.\d{1,2})?$/.test(textoNumero);
}

// Valida un número leído de un <input type="number">.
// Devuelve un mensaje de error o null si es válido.
function validarNumero(texto, { nombre, minimo, permitirCero = false }) {
    if (texto.trim() === "") return `Ingresá ${nombre}.`;

    const valor = Number(texto);
    if (!Number.isFinite(valor)) return `${capitalizar(nombre)} debe ser un número.`;

    if (permitirCero ? valor < minimo : valor <= minimo) {
        return permitirCero
            ? `${capitalizar(nombre)} no puede ser negativo.`
            : `${capitalizar(nombre)} debe ser mayor a ${minimo}.`;
    }

    if (!tieneHastaDosDecimales(texto)) return "Usá como máximo 2 decimales.";
    if (valor > 99999999) return `${capitalizar(nombre)} es demasiado grande.`;

    return null;
}

function capitalizar(texto) {
    return texto.charAt(0).toUpperCase() + texto.slice(1);
}

// Deshabilita el botón mientras se espera la respuesta de la API
async function conBotonOcupado(boton, textoOcupado, accion) {
    const textoOriginal = boton.textContent;
    boton.disabled = true;
    boton.textContent = textoOcupado;
    try {
        await accion();
    } finally {
        boton.disabled = false;
        // Si la acción ya cambió el texto (por ejemplo al limpiar el formulario), se respeta
        if (boton.textContent === textoOcupado) boton.textContent = textoOriginal;
    }
}
