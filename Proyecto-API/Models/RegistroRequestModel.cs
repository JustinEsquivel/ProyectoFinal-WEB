using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    public class RegistroRequestModel
    {
        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Contrasena { get; set; } = string.Empty;
    }
}
