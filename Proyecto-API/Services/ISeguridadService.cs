using ProyectoFinal_API.Models;

namespace ProyectoFinal_API.Services
{
    /// <summary>
    /// Funciones de seguridad compartidas: hash de contraseñas y generación del token JWT.
    /// </summary>
    public interface ISeguridadService
    {
        /// <summary>Genera el hash PBKDF2 de una contraseña para guardarlo en Usuario.Contrasena.</summary>
        string GenerarHash(string contrasena);

        /// <summary>Compara una contraseña digitada contra el hash guardado en la base de datos.</summary>
        bool VerificarHash(string contrasena, string hashGuardado);

        /// <summary>Genera el token JWT con el identificador, el nombre y el rol del usuario.</summary>
        (string Token, DateTime Expiracion) GenerarToken(UsuarioResponse usuario);
    }
}
