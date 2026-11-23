using Microsoft.EntityFrameworkCore;
using Verduleria.Entities;

namespace Verduleria.Data.Repositories;

public class ProductoRepository
{
    private readonly VerduleriaDbContext _context;

    public ProductoRepository(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerTodosAsync() =>
        await _context.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();

    public async Task<Producto?> ObtenerPorIdAsync(int id) =>
        await _context.Productos.FindAsync(id);

    public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null) =>
        await _context.Productos.AnyAsync(p =>
            p.Nombre.ToLower() == nombre.ToLower() &&
            (!excluirId.HasValue || p.Id != excluirId.Value));

    public async Task<Producto> AgregarAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task ActualizarAsync(Producto producto)
    {
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Producto producto)
    {
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TieneVentasAsync(int productoId) =>
        await _context.Ventas.AnyAsync(v => v.ProductoId == productoId);
}