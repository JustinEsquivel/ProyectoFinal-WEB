namespace ProyectoFinal_WEB.Models;

public class CarritoProductoViewModel
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public int Existencias { get; set; }
    public bool Activo { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => Precio * Cantidad;
}
