using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Entities;

namespace Verduleria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController : ControllerBase
{
    private readonly ProductoService _productoService;

    public ProductoController(ProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Producto>>> GetAll()
    {
        var productos = await _productoService.ObtenerTodosAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto is null)
            return NotFound(new { mensaje = "Producto no encontrado." });

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        var resultado = await _productoService.CrearAsync(producto);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return CreatedAtAction(nameof(GetById), new { id = resultado.Producto!.Id }, resultado.Producto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
    {
        var resultado = await _productoService.ActualizarAsync(id, producto);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _productoService.EliminarAsync(id);
        if (!resultado.Ok)
            return BadRequest(new { mensaje = resultado.Error });

        return NoContent();
    }
}