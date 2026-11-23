namespace Verduleria.Entities;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal CantidadKg { get; set; }
    public decimal Total { get; set; }

    public int CajeroId { get; set; }
    public Cajero? Cajero { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
}