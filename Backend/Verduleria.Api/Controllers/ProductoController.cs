using Microsoft.AspNetCore.Mvc;
using Verduleria.Business;
using Verduleria.Business.DTOs;

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
    public async Task<ActionResult<List<ProductoResponseDto>>> GetAll()
    {
        try
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return Ok(productos);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al obtener los productos." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponseDto>> GetById(int id)
    {
        try
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto is null)
                return NotFound(new { mensaje = "Producto no encontrado." });

            return Ok(producto);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al buscar el producto." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponseDto>> Create([FromBody] CrearProductoRequestDto request)
    {
        try
        {
            var resultado = await _productoService.CrearAsync(request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return CreatedAtAction(nameof(GetById), new { id = resultado.Producto!.Id }, resultado.Producto);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al crear el producto." });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarProductoRequestDto request)
    {
        try
        {
            var resultado = await _productoService.ActualizarAsync(id, request);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al actualizar el producto." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var resultado = await _productoService.EliminarAsync(id);
            if (!resultado.Ok)
                return BadRequest(new { mensaje = resultado.Error });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Error interno al eliminar el producto." });
        }
    }
}