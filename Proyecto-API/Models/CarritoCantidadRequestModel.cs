using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models;

public class CarritoCantidadRequestModel
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}
