using Microsoft.EntityFrameworkCore;
using Verduleria.Entities;

namespace Verduleria.Data.Repositories;

public class CajeroRepository
{
    private readonly VerduleriaDbContext _context;

    public CajeroRepository(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cajero>> ObtenerTodosAsync() =>
        await _context.Cajeros.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();

    public async Task<Cajero?> ObtenerPorIdAsync(int id) =>
        await _context.Cajeros.FindAsync(id);

    public async Task<bool> ExisteLegajoAsync(string legajo, int? excluirId = null) =>
        await _context.Cajeros.AnyAsync(c =>
            c.Legajo.ToLower() == legajo.ToLower() &&
            (!excluirId.HasValue || c.Id != excluirId.Value));

    public async Task<Cajero> AgregarAsync(Cajero cajero)
    {
        _context.Cajeros.Add(cajero);
        await _context.SaveChangesAsync();
        return cajero;
    }

    public async Task ActualizarAsync(Cajero cajero)
    {
        _context.Cajeros.Update(cajero);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Cajero cajero)
    {
        _context.Cajeros.Remove(cajero);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TieneVentasAsync(int cajeroId) =>
        await _context.Ventas.AnyAsync(v => v.CajeroId == cajeroId);
}