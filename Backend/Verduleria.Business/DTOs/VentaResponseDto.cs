namespace Verduleria.Business.DTOs;

public record VentaResponseDto(
    int Id,
    DateTime Fecha,
    decimal CantidadKg,
    decimal Total,
    int CajeroId,
    CajeroResponseDto? Cajero,
    int ProductoId,
    ProductoResponseDto? Producto
);