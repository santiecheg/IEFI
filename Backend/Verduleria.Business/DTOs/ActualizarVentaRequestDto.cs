namespace Verduleria.Business.DTOs;

public record ActualizarVentaRequestDto(
    int CajeroId,
    decimal CantidadKg
);