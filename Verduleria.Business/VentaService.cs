using Microsoft.EntityFrameworkCore;
using Verduleria.Data;
using Verduleria.Entities;

namespace Verduleria.Business;

public class VentaService
{
    private readonly VerduleriaDbContext _context;

    public VentaService(VerduleriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venta>> ObtenerTodasAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<Venta?> ObtenerPorIdAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<(bool Ok, string Error, Venta? Venta)> RegistrarVentaAsync(int cajeroId, int productoId, decimal cantidadKg)
    {
        if (cantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.", null);

        var cajero = await _context.Cajeros.FindAsync(cajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.", null);

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.", null);

        var producto = await _context.Productos.FindAsync(productoId);
        if (producto is null)
            return (false, "El producto seleccionado no existe.", null);

        if (producto.StockKg < cantidadKg)
            return (false, $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockKg} kg.", null);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        producto.StockKg -= cantidadKg;

        var venta = new Venta
        {
            CajeroId = cajero.Id,
            ProductoId = producto.Id,
            CantidadKg = cantidadKg,
            Total = Math.Round(cantidadKg * producto.PrecioPorKg, 2),
            Fecha = DateTime.Now
        };

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        venta.Cajero = cajero;
        venta.Producto = producto;

        return (true, string.Empty, venta);
    }

    public async Task<(bool Ok, string Error)> ActualizarCantidadAsync(int id, int nuevoCajeroId, decimal nuevaCantidadKg)
    {
        if (nuevaCantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.");

        var cajero = await _context.Cajeros.FindAsync(nuevoCajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.");

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.");

        var venta = await _context.Ventas
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null || venta.Producto is null)
            return (false, "Venta no encontrada.");

        // Devolvemos la cantidad anterior al stock y comprobamos si alcanza para la nueva
        decimal stockDisponible = venta.Producto.StockKg + venta.CantidadKg;
        if (stockDisponible < nuevaCantidadKg)
            return (false, $"Stock insuficiente para {venta.Producto.Nombre}. Disponible: {stockDisponible} kg.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        venta.Producto.StockKg = stockDisponible - nuevaCantidadKg;
        venta.CajeroId = nuevoCajeroId;
        venta.CantidadKg = nuevaCantidadKg;
        venta.Total = Math.Round(nuevaCantidadKg * venta.Producto.PrecioPorKg, 2);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null)
            return (false, "Venta no encontrada.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        if (venta.Producto is not null)
            venta.Producto.StockKg += venta.CantidadKg;

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, string.Empty);
    }

    // 1. Últimas N ventas (por defecto las últimas 10)
    public async Task<List<Venta>> ObtenerUltimasVentasAsync(int cantidad = 10)
    {
        if (cantidad <= 0)
            cantidad = 10;

        return await _context.Ventas
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .OrderByDescending(v => v.Fecha)
            .Take(cantidad)
            .ToListAsync();
    }

    // 2. Ventas realizadas hoy + total recaudado del día
    public async Task<(List<Venta> Ventas, decimal TotalRecaudado)> ObtenerVentasHoyAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var ventasHoy = await _context.Ventas
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .Where(v => v.Fecha >= hoy && v.Fecha < manana)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        decimal totalRecaudado = ventasHoy.Sum(v => v.Total);

        return (ventasHoy, totalRecaudado);
    }

    // 3. Ventas de la semana actual (desde el lunes) + total recaudado
    public async Task<(List<Venta> Ventas, decimal TotalRecaudado)> ObtenerVentasSemanaAsync()
    {
        var hoy = DateTime.Today;
        // En .NET DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
        int diasDesdeLunes = hoy.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)hoy.DayOfWeek - 1;
        var inicioSemana = hoy.AddDays(-diasDesdeLunes);
        var finSemana = inicioSemana.AddDays(7);

        var ventasSemana = await _context.Ventas
            .Include(v => v.Cajero)
            .Include(v => v.Producto)
            .Where(v => v.Fecha >= inicioSemana && v.Fecha < finSemana)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        decimal totalRecaudado = ventasSemana.Sum(v => v.Total);

        return (ventasSemana, totalRecaudado);
    }
}