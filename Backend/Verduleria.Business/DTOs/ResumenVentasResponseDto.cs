namespace Verduleria.Business.DTOs;

public record ResumenVentasResponseDto(
    DateTime? Fecha,
    int CantidadVentas,
    decimal TotalRecaudado,
    List<VentaResponseDto> Ventas
);