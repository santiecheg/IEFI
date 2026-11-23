namespace Verduleria.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioPorKg { get; set; }
    public decimal StockKg { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}