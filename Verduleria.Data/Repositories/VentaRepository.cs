using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Verduleria.Entities;

namespace Verduleria.Data.Repositories;

public class VentaRepository
{
    private readonly VerduleriaDbContext _context;

    public VentaRepository(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venta>> ObtenerTodasConDetalleAsync() =>
        await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

    public async Task<Venta?> ObtenerPorIdConDetalleAsync(int id) =>
        await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Venta?> ObtenerPorIdAsync(int id) =>
        await _context.Ventas.FindAsync(id);

    public async Task<Venta?> ObtenerPorIdConProductoTrackedAsync(int id) =>
        await _context.Ventas
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<List<Venta>> ObtenerUltimasConDetalleAsync(int cantidad) =>
        await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .OrderByDescending(v => v.Fecha)
            .Take(cantidad)
            .ToListAsync();

    public async Task<List<Venta>> ObtenerRangoFechasConDetalleAsync(DateTime desde, DateTime hasta) =>
        await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .Where(v => v.Fecha >= desde && v.Fecha < hasta)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
        await _context.Database.BeginTransactionAsync();

    public async Task AgregarSinGuardarAsync(Venta venta) =>
        await _context.Ventas.AddAsync(venta);

    public void EliminarSinGuardar(Venta venta) =>
        _context.Ventas.Remove(venta);

    public async Task GuardarCambiosAsync() =>
        await _context.SaveChangesAsync();

    public async Task<Venta> AgregarAsync(Venta venta)
    {
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task ActualizarAsync(Venta venta)
    {
        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Venta venta)
    {
        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();
    }
}