namespace Verduleria.Business.DTOs;

public record CrearCajeroRequestDto(
    string Nombre,
    string Legajo,
    bool Activo = true
);