namespace Verduleria.Entities;

public class Cajero
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Legajo { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}