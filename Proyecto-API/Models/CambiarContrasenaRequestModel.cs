using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    public class CambiarContrasenaRequestModel
    {
        [Required]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
