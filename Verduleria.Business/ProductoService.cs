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

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        return await _productoRepository.ObtenerTodosAsync();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _productoRepository.ObtenerPorIdAsync(id);
    }

    public async Task<(bool Ok, string Error, Producto? Producto)> CrearAsync(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return (false, "El nombre del producto es obligatorio.", null);

        if (producto.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.", null);

        if (producto.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.", null);

        string nombreNormalizado = producto.Nombre.Trim();

        bool existeNombre = await _productoRepository.ExisteNombreAsync(nombreNormalizado);

        if (existeNombre)
            return (false, "Ya existe un producto con ese nombre.", null);

        producto.Id = 0;
        producto.Nombre = nombreNormalizado;

        var creado = await _productoRepository.AgregarAsync(producto);

        return (true, string.Empty, creado);
    }

    public async Task<(bool Ok, string Error)> ActualizarAsync(int id, Producto productoActualizado)
    {
        var productoDb = await _productoRepository.ObtenerPorIdAsync(id);
        if (productoDb is null)
            return (false, "Producto no encontrado.");

        if (string.IsNullOrWhiteSpace(productoActualizado.Nombre))
            return (false, "El nombre del producto es obligatorio.");

        if (productoActualizado.PrecioPorKg <= 0)
            return (false, "El precio por kg debe ser mayor a 0.");

        if (productoActualizado.StockKg < 0)
            return (false, "El stock en kg no puede ser negativo.");

        string nombreNormalizado = productoActualizado.Nombre.Trim();

        bool duplicado = await _productoRepository.ExisteNombreAsync(nombreNormalizado, excluirId: id);

        if (duplicado)
            return (false, "Ya existe otro producto con ese nombre.");

        productoDb.Nombre = nombreNormalizado;
        productoDb.PrecioPorKg = productoActualizado.PrecioPorKg;
        productoDb.StockKg = productoActualizado.StockKg;

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
}