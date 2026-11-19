namespace ProyectoFinal_WEB.Models;

public class CarritoViewModel
{
    public List<CarritoProductoViewModel> Productos { get; set; }
        = new List<CarritoProductoViewModel>();

    public decimal Total => Productos.Sum(producto => producto.Subtotal);
}
