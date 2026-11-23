using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Entities;

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

    public record CrearVentaRequest(int CajeroId, int ProductoId, decimal CantidadKg);
    public record ActualizarVentaRequest(int CajeroId, decimal CantidadKg);

    [HttpGet]
    public async Task<ActionResult<List<Venta>>> GetAll()
    {
        var ventas = await _ventaService.ObtenerTodasAsync();
        return Ok(ventas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Venta>> GetById(int id)
    {
        var venta = await _ventaService.ObtenerPorIdAsync(id);
        if (venta is null)
            return NotFound(new { mensaje = "Venta no encontrada." });

        return Ok(venta);
    }

    // Resumen: últimas N ventas (por defecto 10) -> GET api/venta/ultimas?cantidad=10
    [HttpGet("ultimas")]
    public async Task<ActionResult<List<Venta>>> GetUltimas([FromQuery] int cantidad = 10)
    {
        var ventas = await _ventaService.ObtenerUltimasVentasAsync(cantidad);
        return Ok(ventas);
    }

    // Resumen: ventas de hoy + total recaudado -> GET api/venta/hoy
    [HttpGet("hoy")]
    public async Task<IActionResult> GetHoy()
    {
        var (ventas, totalRecaudado) = await _ventaService.ObtenerVentasHoyAsync();
        return Ok(new
        {
            fecha = DateTime.Today,
            cantidadVentas = ventas.Count,
            totalRecaudado,
            ventas
        });
    }

    // Resumen: ventas de la semana actual + total recaudado -> GET api/venta/semana
    [HttpGet("semana")]
    public async Task<IActionResult> GetSemana()
    {
        var (ventas, totalRecaudado) = await _ventaService.ObtenerVentasSemanaAsync();
        return Ok(new
        {
            cantidadVentas = ventas.Count,
            totalRecaudado,
            ventas
        });
    }

    [HttpPost]
    public async Task<ActionResult<Venta>> Create([FromBody] CrearVentaRequest request)
    {
        var resultado = await _ventaService.RegistrarVentaAsync(
            request.CajeroId,
            request.ProductoId,
            request.CantidadKg);

        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return CreatedAtAction(nameof(GetById), new { id = resultado.Venta!.Id }, resultado.Venta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarVentaRequest request)
    {
        var resultado = await _ventaService.ActualizarCantidadAsync(
            id,
            request.CajeroId,
            request.CantidadKg);

        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _ventaService.EliminarAsync(id);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }
}