using System.Globalization;
using System.Net.Http.Headers;

namespace ProyectoFinal_WEB.Helpers
{
    /// <summary>
    /// Nombres de las variables de sesión y métodos de ayuda para trabajar con el usuario autenticado.
    /// </summary>
    public static class SesionHelper
    {
        public const string Token = "Token";
        public const string Expiracion = "Expiracion";
        public const string UsuarioId = "UsuarioId";
        public const string NombreUsuario = "NombreUsuario";
        public const string Correo = "Correo";
        public const string Rol = "Rol";

        public const string RolAdministrador = "Administrador";
        public const string RolCliente = "Cliente";

        /// <summary>Indica si hay un usuario con token vigente.</summary>
        public static bool EstaAutenticado(this ISession sesion)
        {
            if (string.IsNullOrEmpty(sesion.GetString(Token)))
                return false;

            var expiracion = sesion.GetString(Expiracion);
            return DateTime.TryParse(expiracion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fecha)
                && fecha > DateTime.UtcNow;
        }

        public static bool EsAdministrador(this ISession sesion)
            => sesion.EstaAutenticado() && sesion.GetString(Rol) == RolAdministrador;

        /// <summary>
        /// Agrega el token JWT de la sesión al HttpClient para llamar endpoints protegidos de la API.
        /// Uso: client.AgregarToken(HttpContext);
        /// </summary>
        public static void AgregarToken(this HttpClient client, HttpContext httpContext)
        {
            var token = httpContext.Session.GetString(Token);

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
