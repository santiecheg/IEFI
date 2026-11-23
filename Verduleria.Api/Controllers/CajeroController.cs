using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Entities;

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
    public async Task<ActionResult<List<Cajero>>> GetAll()
    {
        var cajeros = await _cajeroService.ObtenerTodosAsync();
        return Ok(cajeros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cajero>> GetById(int id)
    {
        var cajero = await _cajeroService.ObtenerPorIdAsync(id);
        if (cajero is null)
            return NotFound(new { mensaje = "Cajero no encontrado." });

        return Ok(cajero);
    }

    [HttpPost]
    public async Task<ActionResult<Cajero>> Create([FromBody] Cajero cajero)
    {
        var resultado = await _cajeroService.CrearAsync(cajero);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return CreatedAtAction(nameof(GetById), new { id = resultado.Cajero!.Id }, resultado.Cajero);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Cajero cajero)
    {
        var resultado = await _cajeroService.ActualizarAsync(id, cajero);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _cajeroService.EliminarAsync(id);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }
}