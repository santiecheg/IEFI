namespace Verduleria.Business.DTOs;

public record CrearVentaRequestDto(
    int CajeroId,
    int ProductoId,
    decimal CantidadKg
);