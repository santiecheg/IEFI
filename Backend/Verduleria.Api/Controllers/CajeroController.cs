using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Business.DTOs;

namespace Verduleria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CajeroController : ControllerBase
{
    private readonly CajeroService _cajeroService;

    public CajeroController(CajeroService cajeroService)
    {
        _cajeroService = cajeroService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CajeroResponseDto>>> GetAll()
    {
        try
        {
            var cajeros = await _cajeroService.ObtenerTodosAsync();
            return Ok(cajeros);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener los cajeros." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CajeroResponseDto>> GetById(int id)
    {
        try
        {
            var cajero = await _cajeroService.ObtenerPorIdAsync(id);
            if (cajero is null)
                return NotFound(new { mensaje = "Cajero no encontrado." });

            return Ok(cajero);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al buscar el cajero." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CajeroResponseDto>> Create([FromBody] CrearCajeroRequestDto request)
    {
        try
        {
            var resultado = await _cajeroService.CrearAsync(request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return CreatedAtAction(nameof(GetById), new { id = resultado.Cajero!.Id }, resultado.Cajero);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al crear el cajero." });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarCajeroRequestDto request)
    {
        try
        {
            var resultado = await _cajeroService.ActualizarAsync(id, request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al actualizar el cajero." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var resultado = await _cajeroService.EliminarAsync(id);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al eliminar el cajero." });
        }
    }
}