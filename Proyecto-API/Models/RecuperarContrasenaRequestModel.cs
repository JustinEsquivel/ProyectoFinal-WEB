using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    public class RecuperarContrasenaRequestModel
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }
}
