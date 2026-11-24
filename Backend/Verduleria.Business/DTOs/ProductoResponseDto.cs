namespace Verduleria.Business.DTOs;

public record ProductoResponseDto(
    int Id,
    string Nombre,
    decimal PrecioPorKg,
    decimal StockKg
);