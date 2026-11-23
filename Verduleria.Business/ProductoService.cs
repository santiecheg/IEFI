using Microsoft.EntityFrameworkCore;
using Verduleria.Data;
using Verduleria.Entities;

namespace Verduleria.Business;

public class ProductoService
{
    private readonly VerduleriaDbContext _context;

    public ProductoService(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Ok, string Error, Producto? Producto)> CrearAsync(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return (false, "El nombre del producto es obligatorio.", null);

        if (producto.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.", null);

        if (producto.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.", null);

        bool existeNombre = await _context.Productos
            .AnyAsync(p => p.Nombre.ToLower() == producto.Nombre.Trim().ToLower());

        if (existeNombre)
            return (false, "Ya existe un producto con ese nombre.", null);

        producto.Id = 0;
        producto.Nombre = producto.Nombre.Trim();

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return (true, string.Empty, producto);
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, Producto productoActualizado)
    {
        var productoDb = await _context.Productos.FindAsync(id);
        if (productoDb is null)
            return (false, "Producto no encontrado.");

        if (string.IsNullOrWhiteSpace(productoActualizado.Nombre))
            return (false, "El nombre del producto es obligatorio.");

        if (productoActualizado.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.");

        if (productoActualizado.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.");

        bool duplicado = await _context.Productos
            .AnyAsync(p => p.Id != id && p.Nombre.ToLower() == productoActualizado.Nombre.Trim().ToLower());

        if (duplicado)
            return (false, "Ya existe otro producto con ese nombre.");

        productoDb.Nombre = productoActualizado.Nombre.Trim();
        productoDb.PrecioPorKg = productoActualizado.PrecioPorKg;
        productoDb.StockKg = productoActualizado.StockKg;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var productoDb = await _context.Productos
            .Include(p => p.Ventas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productoDb is null)
            return (false, "Producto no encontrado.");

        if (productoDb.Ventas.Any())
            return (false, "No se puede eliminar un producto que ya tiene ventas registradas.");

        _context.Productos.Remove(productoDb);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}