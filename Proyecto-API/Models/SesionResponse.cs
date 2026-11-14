namespace ProyectoFinal_API.Models
{
    /// <summary>
    /// Respuesta del inicio de sesión: datos del usuario y su token JWT.
    /// </summary>
    public class SesionResponse
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
    }
}
