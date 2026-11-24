namespace Verduleria.Business.DTOs;

public record ActualizarProductoRequestDto(
    int Id,
    string Nombre,
    decimal PrecioPorKg,
    decimal StockKg
);