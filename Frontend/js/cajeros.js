// =========================================================
// CRUD de Cajeros
// =========================================================

const formCajero = document.getElementById("form-cajero");
const inputCajeroId = document.getElementById("cajero-id");
const inputCajeroNombre = document.getElementById("cajero-nombre");
const inputCajeroLegajo = document.getElementById("cajero-legajo");
const inputCajeroActivo = document.getElementById("cajero-activo");
const btnGuardarCajero = document.getElementById("btn-guardar-cajero");
const btnCancelarCajero = document.getElementById("btn-cancelar-cajero");
const tablaCajeros = document.getElementById("tabla-cajeros");

// ---------------------- Listar ----------------------

async function cargarCajeros() {
    tablaCajeros.innerHTML = filaMensaje(4, "Cargando cajeros…");
    try {
        estado.cajeros = await api.cajeros.listar();
        renderizarCajeros();
    } catch (error) {
        tablaCajeros.innerHTML = filaMensaje(4, error.message, true);
    }
}

function renderizarCajeros() {
    if (estado.cajeros.length === 0) {
        tablaCajeros.innerHTML = filaMensaje(4, "Todavía no hay cajeros cargados.");
        return;
    }

    const idEditando = Number(inputCajeroId.value);

    tablaCajeros.innerHTML = estado.cajeros.map(c => `
        <tr class="${c.id === idEditando ? "is-editando" : ""}">
            <td>${escaparHtml(c.nombre)}</td>
            <td>${escaparHtml(c.legajo)}</td>
            <td>
                ${c.activo
                    ? `<span class="badge badge--ok">Activo</span>`
                    : `<span class="badge badge--off">Inactivo</span>`}
            </td>
            <td class="acciones">
                <button class="btn btn--ghost btn--sm" data-accion="editar" data-id="${c.id}">Editar</button>
                <button class="btn btn--danger btn--sm" data-accion="eliminar" data-id="${c.id}">Eliminar</button>
            </td>
        </tr>
    `).join("");
}

// ---------------------- Validar ----------------------

// Mismas reglas que CajeroService en el backend
function validarCajero() {
    const errores = {};
    const nombre = inputCajeroNombre.value.trim();
    const legajo = inputCajeroLegajo.value.trim().toUpperCase();

    if (nombre === "") errores["cajero-nombre"] = "El nombre del cajero es obligatorio.";

    if (legajo === "") {
        errores["cajero-legajo"] = "El legajo es obligatorio.";
    } else {
        const idActual = Number(inputCajeroId.value);
        const repetido = estado.cajeros.some(c =>
            c.legajo.toUpperCase() === legajo && c.id !== idActual);
        if (repetido) errores["cajero-legajo"] = "Ya existe un cajero con ese legajo.";
    }

    return aplicarErrores(formCajero, errores);
}

// ---------------------- Crear / Editar ----------------------

formCajero.addEventListener("submit", async evento => {
    evento.preventDefault();
    if (!validarCajero()) return;

    const id = Number(inputCajeroId.value);
    const cajero = {
        nombre: inputCajeroNombre.value.trim(),
        legajo: inputCajeroLegajo.value.trim().toUpperCase(), // la API también lo normaliza
        activo: inputCajeroActivo.checked
    };

    await conBotonOcupado(btnGuardarCajero, "Guardando…", async () => {
        try {
            if (id) {
                await api.cajeros.actualizar(id, { id, ...cajero });
                mostrarToast(`Cajero "${cajero.nombre}" actualizado.`);
            } else {
                await api.cajeros.crear(cajero);
                mostrarToast(`Cajero "${cajero.nombre}" creado.`);
            }
            resetearFormCajero();
            await cargarCajeros();
        } catch (error) {
            mostrarToast(error.message, "error");
        }
    });
});

function editarCajero(id) {
    const cajero = estado.cajeros.find(c => c.id === id);
    if (!cajero) return;

    inputCajeroId.value = cajero.id;
    inputCajeroNombre.value = cajero.nombre;
    inputCajeroLegajo.value = cajero.legajo;
    inputCajeroActivo.checked = cajero.activo;

    document.getElementById("form-cajero-titulo").textContent = "Editar cajero";
    btnGuardarCajero.textContent = "Guardar cambios";
    btnCancelarCajero.hidden = false;
    limpiarErrores(formCajero);
    renderizarCajeros();
    inputCajeroNombre.focus();
}

function resetearFormCajero() {
    formCajero.reset();
    inputCajeroId.value = "";
    inputCajeroActivo.checked = true;
    document.getElementById("form-cajero-titulo").textContent = "Nuevo cajero";
    btnGuardarCajero.textContent = "Guardar cajero";
    btnCancelarCajero.hidden = true;
    limpiarErrores(formCajero);
    renderizarCajeros();
}

// ---------------------- Eliminar ----------------------

async function eliminarCajero(id) {
    const cajero = estado.cajeros.find(c => c.id === id);
    if (!cajero) return;

    if (!confirm(`¿Eliminar al cajero "${cajero.nombre}" (${cajero.legajo})?`)) return;

    try {
        await api.cajeros.eliminar(id);
        mostrarToast(`Cajero "${cajero.nombre}" eliminado.`);
        if (Number(inputCajeroId.value) === id) resetearFormCajero();
        await cargarCajeros();
    } catch (error) {
        // Ej: "No se puede eliminar un cajero que tiene ventas registradas."
        mostrarToast(error.message, "error");
    }
}

// ---------------------- Eventos ----------------------

btnCancelarCajero.addEventListener("click", resetearFormCajero);

tablaCajeros.addEventListener("click", evento => {
    const boton = evento.target.closest("button[data-accion]");
    if (!boton) return;

    const id = Number(boton.dataset.id);
    if (boton.dataset.accion === "editar") editarCajero(id);
    if (boton.dataset.accion === "eliminar") eliminarCajero(id);
});
