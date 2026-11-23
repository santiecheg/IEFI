using Verduleria.Data.Repositories;
using Verduleria.Entities;

namespace Verduleria.Business;

public class VentaService
{
    private readonly VentaRepository _ventaRepository;
    private readonly ProductoRepository _productoRepository;
    private readonly CajeroRepository _cajeroRepository;

    public VentaService(
        VentaRepository ventaRepository,
        ProductoRepository productoRepository,
        CajeroRepository cajeroRepository)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _cajeroRepository = cajeroRepository;
    }

    public async Task<List<Venta>> ObtenerTodasAsync()
    {
        return await _ventaRepository.ObtenerTodasConDetalleAsync();
    }

    public async Task<Venta?> ObtenerPorIdAsync(int id)
    {
        return await _ventaRepository.ObtenerPorIdConDetalleAsync(id);
    }

    public async Task<(bool Ok, string Error, Venta? Venta)> RegistrarVentaAsync(int cajeroId, int productoId, decimal cantidadKg)
    {
        if (cantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.", null);

        var cajero = await _cajeroRepository.ObtenerPorIdAsync(cajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.", null);

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.", null);

        var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
        if (producto is null)
            return (false, "El producto seleccionado no existe.", null);

        if (producto.StockKg < cantidadKg)
            return (false, $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockKg} kg.", null);

        await using var transaction = await _ventaRepository.BeginTransactionAsync();

        producto.StockKg -= cantidadKg;

        var venta = new Venta
        {
            CajeroId = cajero.Id,
            ProductoId = producto.Id,
            CantidadKg = cantidadKg,
            Total = Math.Round(cantidadKg * producto.PrecioPorKg, 2),
            Fecha = DateTime.Now
        };

        await _ventaRepository.AgregarSinGuardarAsync(venta);
        await _ventaRepository.GuardarCambiosAsync();
        await transaction.CommitAsync();

        venta.Cajero = cajero;
        venta.Producto = producto;

        return (true, string.Empty, venta);
    }

    public async Task<(bool Ok, string Error)> ActualizarCantidadAsync(int id, int nuevoCajeroId, decimal nuevaCantidadKg)
    {
        if (nuevaCantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.");

        var cajero = await _cajeroRepository.ObtenerPorIdAsync(nuevoCajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.");

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.");

        var venta = await _ventaRepository.ObtenerPorIdConProductoTrackedAsync(id);

        if (venta is null || venta.Producto is null)
            return (false, "Venta no encontrada.");

        // Devolvemos la cantidad anterior al stock y comprobamos si alcanza para la nueva
        decimal stockDisponible = venta.Producto.StockKg + venta.CantidadKg;
        if (stockDisponible < nuevaCantidadKg)
            return (false, $"Stock insuficiente para {venta.Producto.Nombre}. Disponible: {stockDisponible} kg.");

        await using var transaction = await _ventaRepository.BeginTransactionAsync();

        venta.Producto.StockKg = stockDisponible - nuevaCantidadKg;
        venta.CajeroId = nuevoCajeroId;
        venta.CantidadKg = nuevaCantidadKg;
        venta.Total = Math.Round(nuevaCantidadKg * venta.Producto.PrecioPorKg, 2);

        await _ventaRepository.GuardarCambiosAsync();
        await transaction.CommitAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdConProductoTrackedAsync(id);

        if (venta is null)
            return (false, "Venta no encontrada.");

        await using var transaction = await _ventaRepository.BeginTransactionAsync();

        if (venta.Producto is not null)
            venta.Producto.StockKg += venta.CantidadKg;

        _ventaRepository.EliminarSinGuardar(venta);
        await _ventaRepository.GuardarCambiosAsync();
        await transaction.CommitAsync();

        return (true, string.Empty);
    }

    // 1. Últimas N ventas (por defecto las últimas 10)
    public async Task<List<Venta>> ObtenerUltimasVentasAsync(int cantidad = 10)
    {
        if (cantidad <= 0)
            cantidad = 10;

        return await _ventaRepository.ObtenerUltimasConDetalleAsync(cantidad);
    }

    // 2. Ventas realizadas hoy + total recaudado del día
    public async Task<(List<Venta> Ventas, decimal TotalRecaudado)> ObtenerVentasHoyAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var ventasHoy = await _ventaRepository.ObtenerRangoFechasConDetalleAsync(hoy, manana);

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

        var ventasSemana = await _ventaRepository.ObtenerRangoFechasConDetalleAsync(inicioSemana, finSemana);

        decimal totalRecaudado = ventasSemana.Sum(v => v.Total);

        return (ventasSemana, totalRecaudado);
    }
}