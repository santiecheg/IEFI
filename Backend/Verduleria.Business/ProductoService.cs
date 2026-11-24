using Verduleria.Business.DTOs;
using Verduleria.Data.Repositories;
using Verduleria.Entities;

namespace Verduleria.Business;

public class ProductoService
{
    private readonly ProductoRepository _productoRepository;

    public ProductoService(ProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<List<ProductoResponseDto>> ObtenerTodosAsync()
    {
        var productos = await _productoRepository.ObtenerTodosAsync();
        return productos.Select(MapearADto).ToList();
    }

    public async Task<ProductoResponseDto?> ObtenerPorIdAsync(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id);
        return producto is null ? null : MapearADto(producto);
    }

    public async Task<(bool Ok, string Error, ProductoResponseDto? Producto)> CrearAsync(CrearProductoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return (false, "El nombre del producto es obligatorio.", null);

        if (dto.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.", null);

        if (dto.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.", null);

        string nombreNormalizado = dto.Nombre.Trim();

        bool existeNombre = await _productoRepository.ExisteNombreAsync(nombreNormalizado);
        if (existeNombre)
            return (false, "Ya existe un producto con ese nombre.", null);

        var producto = new Producto
        {
            Nombre = nombreNormalizado,
            PrecioPorKg = dto.PrecioPorKg,
            StockKg = dto.StockKg
        };

        var creado = await _productoRepository.AgregarAsync(producto);
        return (true, string.Empty, MapearADto(creado));
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, ActualizarProductoRequestDto dto)
    {
        var productoDb = await _productoRepository.ObtenerPorIdAsync(id);
        if (productoDb is null)
            return (false, "Producto no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return (false, "El nombre del producto es obligatorio.");

        if (dto.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.");

        if (dto.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.");

        string nombreNormalizado = dto.Nombre.Trim();

        bool duplicado = await _productoRepository.ExisteNombreAsync(nombreNormalizado, excluirId: id);
        if (duplicado)
            return (false, "Ya existe otro producto con ese nombre.");

        productoDb.Nombre = nombreNormalizado;
        productoDb.PrecioPorKg = dto.PrecioPorKg;
        productoDb.StockKg = dto.StockKg;

        await _productoRepository.ActualizarAsync(productoDb);
        return (true, string.Empty);
    }

    public async Task<(bool Ok, string Error)> EliminarAsync(int id)
    {
        var productoDb = await _productoRepository.ObtenerPorIdAsync(id);
        if (productoDb is null)
            return (false, "Producto no encontrado.");

        bool tieneVentas = await _productoRepository.TieneVentasAsync(id);
        if (tieneVentas)
            return (false, "No se puede eliminar un producto que ya tiene ventas registradas.");

        await _productoRepository.EliminarAsync(productoDb);
        return (true, string.Empty);
    }

    internal static ProductoResponseDto MapearADto(Producto producto) =>
        new(producto.Id, producto.Nombre, producto.PrecioPorKg, producto.StockKg);
}