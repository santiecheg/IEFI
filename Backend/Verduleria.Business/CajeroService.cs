using Verduleria.Business.DTOs;
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

    public async Task<List<CajeroResponseDto>> ObtenerTodosAsync()
    {
        var cajeros = await _cajeroRepository.ObtenerTodosAsync();
        return cajeros.Select(MapearADto).ToList();
    }

    public async Task<CajeroResponseDto?> ObtenerPorIdAsync(int id)
    {
        var cajero = await _cajeroRepository.ObtenerPorIdAsync(id);
        return cajero is null ? null : MapearADto(cajero);
    }

    public async Task<(bool Ok, string Error, CajeroResponseDto? Cajero)> CrearAsync(CrearCajeroRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return (false, "El nombre del cajero es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(dto.Legajo))
            return (false, "El legajo es obligatorio.", null);

        string legajoNormalizado = dto.Legajo.Trim().ToUpper();

        bool existeLegajo = await _cajeroRepository.ExisteLegajoAsync(legajoNormalizado);
        if (existeLegajo)
            return (false, "Ya existe un cajero con ese legajo.", null);

        var cajero = new Cajero
        {
            Nombre = dto.Nombre.Trim(),
            Legajo = legajoNormalizado,
            Activo = dto.Activo
        };

        var creado = await _cajeroRepository.AgregarAsync(cajero);
        return (true, string.Empty, MapearADto(creado));
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, ActualizarCajeroRequestDto dto)
    {
        var cajeroDb = await _cajeroRepository.ObtenerPorIdAsync(id);
        if (cajeroDb is null)
            return (false, "Cajero no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return (false, "El nombre del cajero es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Legajo))
            return (false, "El legajo es obligatorio.");

        string legajoNormalizado = dto.Legajo.Trim().ToUpper();

        bool legajoDuplicado = await _cajeroRepository.ExisteLegajoAsync(legajoNormalizado, excluirId: id);
        if (legajoDuplicado)
            return (false, "Ya existe otro cajero con ese legajo.");

        cajeroDb.Nombre = dto.Nombre.Trim();
        cajeroDb.Legajo = legajoNormalizado;
        cajeroDb.Activo = dto.Activo;

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

    internal static CajeroResponseDto MapearADto(Cajero cajero) =>
        new(cajero.Id, cajero.Nombre, cajero.Legajo, cajero.Activo);
}