using Verduleria.Data.Repositories;
using Verduleria.Entities;

namespace Verduleria.Business;

public class CajeroService
{
    private readonly CajeroRepository _cajeroRepository;

    public CajeroService(CajeroRepository cajeroRepository)
    {
        _cajeroRepository = cajeroRepository;
    }

    public async Task<List<Cajero>> ObtenerTodosAsync()
    {
        return await _cajeroRepository.ObtenerTodosAsync();
    }

    public async Task<Cajero?> ObtenerPorIdAsync(int id)
    {
        return await _cajeroRepository.ObtenerPorIdAsync(id);
    }

    public async Task<(bool Ok, string Error, Cajero? Cajero)> CrearAsync(Cajero cajero)
    {
        if (string.IsNullOrWhiteSpace(cajero.Nombre))
            return (false, "El nombre del cajero es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(cajero.Legajo))
            return (false, "El legajo es obligatorio.", null);

        string legajoNormalizado = cajero.Legajo.Trim().ToUpper();

        bool existeLegajo = await _cajeroRepository.ExisteLegajoAsync(legajoNormalizado);

        if (existeLegajo)
            return (false, "Ya existe un cajero con ese legajo.", null);

        cajero.Id = 0;
        cajero.Nombre = cajero.Nombre.Trim();
        cajero.Legajo = legajoNormalizado;

        var creado = await _cajeroRepository.AgregarAsync(cajero);

        return (true, string.Empty, creado);
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, Cajero cajeroActualizado)
    {
        var cajeroDb = await _cajeroRepository.ObtenerPorIdAsync(id);
        if (cajeroDb is null)
            return (false, "Cajero no encontrado.");

        if (string.IsNullOrWhiteSpace(cajeroActualizado.Nombre))
            return (false, "El nombre del cajero es obligatorio.");

        if (string.IsNullOrWhiteSpace(cajeroActualizado.Legajo))
            return (false, "El legajo es obligatorio.");

        string legajoNormalizado = cajeroActualizado.Legajo.Trim().ToUpper();

        bool legajoDuplicado = await _cajeroRepository.ExisteLegajoAsync(legajoNormalizado, excluirId: id);

        if (legajoDuplicado)
            return (false, "Ya existe otro cajero con ese legajo.");

        cajeroDb.Nombre = cajeroActualizado.Nombre.Trim();
        cajeroDb.Legajo = legajoNormalizado;
        cajeroDb.Activo = cajeroActualizado.Activo;

        await _cajeroRepository.ActualizarAsync(cajeroDb);
        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var cajeroDb = await _cajeroRepository.ObtenerPorIdAsync(id);
        if (cajeroDb is null)
            return (false, "Cajero no encontrado.");

        bool tieneVentas = await _cajeroRepository.TieneVentasAsync(id);
        if (tieneVentas)
            return (false, "No se puede eliminar un cajero que tiene ventas registradas.");

        await _cajeroRepository.EliminarAsync(cajeroDb);

        return (true, string.Empty);
    }
}