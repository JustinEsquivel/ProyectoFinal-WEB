using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    public class IniciarSesionRequestModel
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }
}
