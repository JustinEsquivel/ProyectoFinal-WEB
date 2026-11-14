using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    /// <summary>
    /// Error ocurrido en la WEB que se envía a la API para guardarlo en BitacoraError.
    /// </summary>
    public class ErrorRequestModel
    {
        public string? Ruta { get; set; }

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        public string? Detalle { get; set; }

        public int? UsuarioId { get; set; }
    }
}
