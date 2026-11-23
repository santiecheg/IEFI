// =========================================================
// Configuración general del frontend
// =========================================================

// URL base de la Web API (.NET 8).
// Corresponde al perfil "http" de Verduleria.Api/Properties/launchSettings.json.
// Si levantan la API con el perfil "https", cambiar por: "https://localhost:7036/api"
const API_URL = "http://localhost:5129/api";

// Un producto se considera con "poco stock" por debajo de estos kg
const STOCK_BAJO_KG = 5;

// Datos cargados desde la API y compartidos entre las secciones
const estado = {
    productos: [],
    cajeros: [],
    ventas: []
};
