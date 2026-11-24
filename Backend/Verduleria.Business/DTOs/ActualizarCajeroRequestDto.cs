namespace Verduleria.Business.DTOs;

public record ActualizarCajeroRequestDto(
    int Id,
    string Nombre,
    string Legajo,
    bool Activo
);