namespace Verduleria.Business.DTOs;

public record CajeroResponseDto(
    int Id,
    string Nombre,
    string Legajo,
    bool Activo
);