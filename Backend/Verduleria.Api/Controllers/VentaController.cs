using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Business.DTOs;

namespace Verduleria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentaController : ControllerBase
{
    private readonly VentaService _ventaService;

    public VentaController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VentaResponseDto>>> GetAll()
    {
        try
        {
            var ventas = await _ventaService.ObtenerTodasAsync();
            return Ok(ventas);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener las ventas." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaResponseDto>> GetById(int id)
    {
        try
        {
            var venta = await _ventaService.ObtenerPorIdAsync(id);
            if (venta is null)
                return NotFound(new { mensaje = "Venta no encontrada." });

            return Ok(venta);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al buscar la venta." });
        }
    }

    // Resumen: últimas N ventas (por defecto 10) -> GET api/venta/ultimas?cantidad=10
    [HttpGet("ultimas")]
    public async Task<ActionResult<List<VentaResponseDto>>> GetUltimas([FromQuery] int cantidad = 10)
    {
        try
        {
            var ventas = await _ventaService.ObtenerUltimasVentasAsync(cantidad);
            return Ok(ventas);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener las últimas ventas." });
        }
    }

    // Resumen: ventas de hoy + total recaudado -> GET api/venta/hoy
    [HttpGet("hoy")]
    public async Task<ActionResult<ResumenVentasResponseDto>> GetHoy()
    {
        try
        {
            var resumen = await _ventaService.ObtenerVentasHoyAsync();
            return Ok(resumen);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener las ventas de hoy." });
        }
    }

    // Resumen: ventas de la semana actual + total recaudado -> GET api/venta/semana
    [HttpGet("semana")]
    public async Task<ActionResult<ResumenVentasResponseDto>> GetSemana()
    {
        try
        {
            var resumen = await _ventaService.ObtenerVentasSemanaAsync();
            return Ok(resumen);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener las ventas de la semana." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<VentaResponseDto>> Create([FromBody] CrearVentaRequestDto request)
    {
        try
        {
            var resultado = await _ventaService.RegistrarVentaAsync(request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return CreatedAtAction(nameof(GetById), new { id = resultado.Venta!.Id }, resultado.Venta);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al registrar la venta." });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarVentaRequestDto request)
    {
        try
        {
            var resultado = await _ventaService.ActualizarCantidadAsync(id, request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al actualizar la venta." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var resultado = await _ventaService.EliminarAsync(id);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al eliminar la venta." });
        }
    }
}