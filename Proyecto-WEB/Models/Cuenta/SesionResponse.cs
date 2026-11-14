namespace ProyectoFinal_WEB.Models
{
    /// <summary>
    /// Respuesta de api/Cuenta/IniciarSesion.
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
