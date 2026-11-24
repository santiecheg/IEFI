using Verduleria.Business.DTOs;
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

    public async Task<List<VentaResponseDto>> ObtenerTodasAsync()
    {
        var ventas = await _ventaRepository.ObtenerTodasConDetalleAsync();
        return ventas.Select(MapearADto).ToList();
    }

    public async Task<VentaResponseDto?> ObtenerPorIdAsync(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetalleAsync(id);
        return venta is null ? null : MapearADto(venta);
    }

    public async Task<(bool Ok, string Error, VentaResponseDto? Venta)> RegistrarVentaAsync(CrearVentaRequestDto dto)
    {
        if (dto.CantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.", null);

        var cajero = await _cajeroRepository.ObtenerPorIdAsync(dto.CajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.", null);

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.", null);

        var producto = await _productoRepository.ObtenerPorIdAsync(dto.ProductoId);
        if (producto is null)
            return (false, "El producto seleccionado no existe.", null);

        if (producto.StockKg < dto.CantidadKg)
            return (false, $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockKg} kg.", null);

        await using var transaction = await _ventaRepository.BeginTransactionAsync();
        try
        {
            producto.StockKg -= dto.CantidadKg;

            var venta = new Venta
            {
                CajeroId = cajero.Id,
                ProductoId = producto.Id,
                CantidadKg = dto.CantidadKg,
                Total = Math.Round(dto.CantidadKg * producto.PrecioPorKg, 2),
                Fecha = DateTime.Now
            };

            await _ventaRepository.AgregarSinGuardarAsync(venta);
            await _ventaRepository.GuardarCambiosAsync();
            await transaction.CommitAsync();

            venta.Cajero = cajero;
            venta.Producto = producto;

            return (true, string.Empty, MapearADto(venta));
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Ocurrió un error al registrar la venta.", null);
        }
    }

    public async Task<(bool Ok, string Error)> ActualizarCantidadAsync(int id, ActualizarVentaRequestDto dto)
    {
        if (dto.CantidadKg <= 0)
            return (false, "La cantidad en kg debe ser mayor a 0.");

        var cajero = await _cajeroRepository.ObtenerPorIdAsync(dto.CajeroId);
        if (cajero is null)
            return (false, "El cajero seleccionado no existe.");

        if (!cajero.Activo)
            return (false, "El cajero seleccionado no está activo.");

        var venta = await _ventaRepository.ObtenerPorIdConProductoTrackedAsync(id);
        if (venta is null || venta.Producto is null)
            return (false, "Venta no encontrada.");

        decimal stockDisponible = venta.Producto.StockKg + venta.CantidadKg;
        if (stockDisponible < dto.CantidadKg)
            return (false, $"Stock insuficiente para {venta.Producto.Nombre}. Disponible: {stockDisponible} kg.");

        await using var transaction = await _ventaRepository.BeginTransactionAsync();
        try
        {
            venta.Producto.StockKg = stockDisponible - dto.CantidadKg;
            venta.CajeroId = dto.CajeroId;
            venta.CantidadKg = dto.CantidadKg;
            venta.Total = Math.Round(dto.CantidadKg * venta.Producto.PrecioPorKg, 2);

            await _ventaRepository.GuardarCambiosAsync();
            await transaction.CommitAsync();

            return (true, string.Empty);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Ocurrió un error al actualizar la venta.");
        }
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdConProductoTrackedAsync(id);
        if (venta is null)
            return (false, "Venta no encontrada.");

        await using var transaction = await _ventaRepository.BeginTransactionAsync();
        try
        {
            if (venta.Producto is not null)
                venta.Producto.StockKg += venta.CantidadKg;

            _ventaRepository.EliminarSinGuardar(venta);
            await _ventaRepository.GuardarCambiosAsync();
            await transaction.CommitAsync();

            return (true, string.Empty);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Ocurrió un error al eliminar la venta.");
        }
    }

    public async Task<List<VentaResponseDto>> ObtenerUltimasVentasAsync(int cantidad = 10)
    {
        if (cantidad <= 0)
            cantidad = 10;

        var ventas = await _ventaRepository.ObtenerUltimasConDetalleAsync(cantidad);
        return ventas.Select(MapearADto).ToList();
    }

    public async Task<ResumenVentasResponseDto> ObtenerVentasHoyAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var ventasHoy = await _ventaRepository.ObtenerRangoFechasConDetalleAsync(hoy, manana);
        decimal totalRecaudado = ventasHoy.Sum(v => v.Total);

        return new ResumenVentasResponseDto(
            hoy,
            ventasHoy.Count,
            totalRecaudado,
            ventasHoy.Select(MapearADto).ToList()
        );
    }

    public async Task<ResumenVentasResponseDto> ObtenerVentasSemanaAsync()
    {
        var hoy = DateTime.Today;
        int diasDesdeLunes = hoy.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)hoy.DayOfWeek - 1;
        var inicioSemana = hoy.AddDays(-diasDesdeLunes);
        var finSemana = inicioSemana.AddDays(7);

        var ventasSemana = await _ventaRepository.ObtenerRangoFechasConDetalleAsync(inicioSemana, finSemana);
        decimal totalRecaudado = ventasSemana.Sum(v => v.Total);

        return new ResumenVentasResponseDto(
            null,
            ventasSemana.Count,
            totalRecaudado,
            ventasSemana.Select(MapearADto).ToList()
        );
    }

    private static VentaResponseDto MapearADto(Venta venta) =>
        new(
            venta.Id,
            venta.Fecha,
            venta.CantidadKg,
            venta.Total,
            venta.CajeroId,
            venta.Cajero is null ? null : CajeroService.MapearADto(venta.Cajero),
            venta.ProductoId,
            venta.Producto is null ? null : ProductoService.MapearADto(venta.Producto)
        );
}