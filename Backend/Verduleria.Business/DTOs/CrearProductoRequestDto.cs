namespace Verduleria.Business.DTOs;

public record CrearProductoRequestDto(
    string Nombre,
    decimal PrecioPorKg,
    decimal StockKg
);