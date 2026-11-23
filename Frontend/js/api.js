// =========================================================
// Comunicación con la Web API (fetch)
// Todas las llamadas HTTP del frontend pasan por acá.
// =========================================================

async function pedirApi(ruta, { metodo = "GET", cuerpo } = {}) {
    const opciones = { method: metodo, headers: {} };

    if (cuerpo !== undefined) {
        opciones.headers["Content-Type"] = "application/json";
        opciones.body = JSON.stringify(cuerpo);
    }

    let respuesta;
    try {
        respuesta = await fetch(`${API_URL}${ruta}`, opciones);
    } catch {
        // fetch solo falla así cuando no hay conexión (API apagada, URL incorrecta, CORS)
        actualizarEstadoApi(false);
        throw new Error(`No se pudo conectar con la API (${API_URL}). Verificá que el backend esté corriendo.`);
    }

    actualizarEstadoApi(true);

    // 204 No Content: PUT y DELETE exitosos no devuelven cuerpo
    if (respuesta.status === 204) return null;

    const texto = await respuesta.text();
    let datos = null;
    if (texto) {
        try {
            datos = JSON.parse(texto);
        } catch {
            datos = null;
        }
    }

    if (!respuesta.ok) {
        throw new Error(obtenerMensajeError(datos, respuesta.status));
    }

    return datos;
}

// La API devuelve { mensaje: "..." } en sus validaciones (BadRequest / NotFound).
// Si el error lo genera ASP.NET (validación automática), viene como { title, errors }.
function obtenerMensajeError(datos, status) {
    if (datos?.mensaje) return datos.mensaje;

    if (datos?.errors) {
        const primerError = Object.values(datos.errors).flat()[0];
        if (primerError) return primerError;
    }

    if (datos?.title) return datos.title;
    if (status === 404) return "El recurso solicitado no existe.";
    if (status >= 500) return "Error interno del servidor. Revisá la consola del backend.";

    return `Error ${status} al comunicarse con la API.`;
}

// ---------------------- Endpoints por entidad ----------------------

const api = {
    productos: {
        listar: () => pedirApi("/producto"),
        obtener: id => pedirApi(`/producto/${id}`),
        crear: producto => pedirApi("/producto", { metodo: "POST", cuerpo: producto }),
        actualizar: (id, producto) => pedirApi(`/producto/${id}`, { metodo: "PUT", cuerpo: producto }),
        eliminar: id => pedirApi(`/producto/${id}`, { metodo: "DELETE" })
    },

    cajeros: {
        listar: () => pedirApi("/cajero"),
        obtener: id => pedirApi(`/cajero/${id}`),
        crear: cajero => pedirApi("/cajero", { metodo: "POST", cuerpo: cajero }),
        actualizar: (id, cajero) => pedirApi(`/cajero/${id}`, { metodo: "PUT", cuerpo: cajero }),
        eliminar: id => pedirApi(`/cajero/${id}`, { metodo: "DELETE" })
    },

    ventas: {
        listar: () => pedirApi("/venta"),
        obtener: id => pedirApi(`/venta/${id}`),
        ultimas: cantidad => pedirApi(`/venta/ultimas?cantidad=${cantidad}`),
        hoy: () => pedirApi("/venta/hoy"),
        semana: () => pedirApi("/venta/semana"),
        // Body: { cajeroId, productoId, cantidadKg }
        crear: venta => pedirApi("/venta", { metodo: "POST", cuerpo: venta }),
        // Body: { cajeroId, cantidadKg } (el producto de una venta no se cambia)
        actualizar: (id, venta) => pedirApi(`/venta/${id}`, { metodo: "PUT", cuerpo: venta }),
        eliminar: id => pedirApi(`/venta/${id}`, { metodo: "DELETE" })
    }
};
