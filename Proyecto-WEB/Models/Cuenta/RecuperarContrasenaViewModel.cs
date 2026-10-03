using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_WEB.Models
{
    public class RecuperarContrasenaViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;
    }
}
