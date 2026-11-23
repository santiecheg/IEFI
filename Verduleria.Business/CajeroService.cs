using Microsoft.EntityFrameworkCore;
using Verduleria.Data;
using Verduleria.Entities;

namespace Verduleria.Business;

public class CajeroService
{
    private readonly VerduleriaDbContext _context;

    public CajeroService(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cajero>> ObtenerTodosAsync()
    {
        return await _context.Cajeros
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Cajero?> ObtenerPorIdAsync(int id)
    {
        return await _context.Cajeros
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(bool Ok, string Error, Cajero? Cajero)> CrearAsync(Cajero cajero)
    {
        if (string.IsNullOrWhiteSpace(cajero.Nombre))
            return (false, "El nombre del cajero es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(cajero.Legajo))
            return (false, "El legajo es obligatorio.", null);

        string legajoNormalizado = cajero.Legajo.Trim().ToUpper();

        bool existeLegajo = await _context.Cajeros
            .AnyAsync(c => c.Legajo.ToUpper() == legajoNormalizado);

        if (existeLegajo)
            return (false, "Ya existe un cajero con ese legajo.", null);

        cajero.Id = 0;
        cajero.Nombre = cajero.Nombre.Trim();
        cajero.Legajo = legajoNormalizado;

        _context.Cajeros.Add(cajero);
        await _context.SaveChangesAsync();

        return (true, string.Empty, cajero);
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, Cajero cajeroActualizado)
    {
        var cajeroDb = await _context.Cajeros.FindAsync(id);
        if (cajeroDb is null)
            return (false, "Cajero no encontrado.");

        if (string.IsNullOrWhiteSpace(cajeroActualizado.Nombre))
            return (false, "El nombre del cajero es obligatorio.");

        if (string.IsNullOrWhiteSpace(cajeroActualizado.Legajo))
            return (false, "El legajo es obligatorio.");

        string legajoNormalizado = cajeroActualizado.Legajo.Trim().ToUpper();

        bool legajoDuplicado = await _context.Cajeros
            .AnyAsync(c => c.Id != id && c.Legajo.ToUpper() == legajoNormalizado);

        if (legajoDuplicado)
            return (false, "Ya existe otro cajero con ese legajo.");

        cajeroDb.Nombre = cajeroActualizado.Nombre.Trim();
        cajeroDb.Legajo = legajoNormalizado;
        cajeroDb.Activo = cajeroActualizado.Activo;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var cajeroDb = await _context.Cajeros
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cajeroDb is null)
            return (false, "Cajero no encontrado.");

        if (cajeroDb.Ventas.Any())
            return (false, "No se puede eliminar un cajero que tiene ventas registradas.");

        _context.Cajeros.Remove(cajeroDb);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}