using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_API.Models
{
    public class RestablecerContrasenaRequestModel
    {
        /// <summary>Token que llega en el enlace del correo de recuperación.</summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
