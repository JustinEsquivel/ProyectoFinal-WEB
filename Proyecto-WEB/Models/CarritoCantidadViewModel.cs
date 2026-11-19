using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_WEB.Models;

public class CarritoCantidadViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto válido.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un número entero mayor que cero.")]
    public int Cantidad { get; set; }
}
