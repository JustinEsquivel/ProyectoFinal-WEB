using System.Text.Json.Serialization;

namespace ProyectoFinal_API.Models
{
    /// <summary>
    /// Resultado de sp_Usuario_ConsultarPorCorreo.
    /// </summary>
    public class UsuarioResponse
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public bool Activo { get; set; }

        /// <summary>Hash guardado en la BD. Nunca se devuelve en las respuestas de la API.</summary>
        [JsonIgnore]
        public string Contrasena { get; set; } = string.Empty;
    }
}
