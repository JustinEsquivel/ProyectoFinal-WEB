using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoFinal_API.Models;
using ProyectoFinal_API.Services;
using System.Security.Claims;

namespace ProyectoFinal_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentaController(IConfiguration _configuration, ISeguridadService _seguridad, ICorreoService _correo) : ControllerBase
    {
        #region Registro de Usuarios (CU-01)

        /// <summary>
        /// Registra un usuario nuevo con el rol Cliente. La contraseña se guarda como hash.
        /// </summary>
        [HttpPost]
        [Route("Registrar")]
        public IActionResult Registrar(RegistroRequestModel model)
        {
            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@NombreUsuario", model.NombreUsuario.Trim());
            parametros.Add("@Correo", model.Correo.Trim());
            parametros.Add("@Contrasena", _seguridad.GenerarHash(model.Contrasena));

            var response = context.Execute("sp_Usuario_Registrar", parametros);

            if (response <= 0)
                return Conflict("Ya existe una cuenta registrada con ese correo electrónico.");

            return Ok("Cuenta creada correctamente.");
        }

        #endregion

        #region Recuperar Contraseña (CU-04)

        /// <summary>
        /// Envía al correo un enlace para restablecer la contraseña.
        /// Siempre responde lo mismo, exista o no el correo, para no revelar qué cuentas están registradas.
        /// </summary>
        [HttpPost]
        [Route("RecuperarContrasena")]
        public IActionResult RecuperarContrasena(RecuperarContrasenaRequestModel model)
        {
            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@Correo", model.Correo.Trim());

            var usuario = context.QueryFirstOrDefault<UsuarioResponse>("sp_Usuario_ConsultarPorCorreo", parametros);

            if (usuario != null && usuario.Activo)
            {
                var minutos = _configuration.GetValue<int>("Recuperacion:MinutosExpiracion");
                var enlace = _configuration.GetValue<string>("Recuperacion:UrlRestablecer") + Uri.EscapeDataString(_seguridad.GenerarTokenRecuperacion(usuario.UsuarioId));

                var html = System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Template", "RecuperarContrasena.html"))
                    .Replace("{{NOMBRE}}", System.Net.WebUtility.HtmlEncode(usuario.NombreUsuario))
                    .Replace("{{ENLACE}}", System.Net.WebUtility.HtmlEncode(enlace))
                    .Replace("{{MINUTOS}}", minutos.ToString());

                _correo.EnviarCorreo(usuario.Correo, "Recuperación de contraseña - Súper Tico", html);
            }

            return Ok("Si el correo está registrado, recibirá un enlace para restablecer su contraseña.");
        }

        /// <summary>
        /// Guarda la contraseña nueva usando el token del enlace que se envió por correo.
        /// </summary>
        [HttpPut]
        [Route("RestablecerContrasena")]
        public IActionResult RestablecerContrasena(RestablecerContrasenaRequestModel model)
        {
            var usuarioId = _seguridad.ValidarTokenRecuperacion(model.Token);

            if (usuarioId == null)
                return BadRequest("El enlace no es válido o ya expiró. Solicite uno nuevo.");

            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuarioId);
            parametros.Add("@Contrasena", _seguridad.GenerarHash(model.NuevaContrasena));

            var response = context.Execute("sp_Usuario_ActualizarContrasena", parametros);

            if (response <= 0)
                return BadRequest("El enlace no es válido o ya expiró. Solicite uno nuevo.");

            return Ok("Su contraseña se actualizó correctamente. Ya puede iniciar sesión.");
        }

        #endregion

        #region Cambiar Contraseña (CU-05)

        /// <summary>
        /// Cambia la contraseña del usuario autenticado después de verificar la contraseña actual.
        /// </summary>
        [Authorize]
        [HttpPut]
        [Route("CambiarContrasena")]
        public IActionResult CambiarContrasena(CambiarContrasenaRequestModel model)
        {
            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@Correo", User.FindFirstValue(ClaimTypes.Email));

            var usuario = context.QueryFirstOrDefault<UsuarioResponse>("sp_Usuario_ConsultarPorCorreo", parametros);

            if (usuario == null || !_seguridad.VerificarHash(model.ContrasenaActual, usuario.Contrasena))
                return BadRequest("La contraseña actual es incorrecta.");

            parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuario.UsuarioId);
            parametros.Add("@Contrasena", _seguridad.GenerarHash(model.NuevaContrasena));

            var response = context.Execute("sp_Usuario_ActualizarContrasena", parametros);

            if (response <= 0)
                return UnprocessableEntity("No se pudo actualizar la contraseña.");

            return Ok("Su contraseña se actualizó correctamente.");
        }

        #endregion

        #region Inicio de Sesión (CU-02)

        [HttpPost]
        [Route("IniciarSesion")]
        public IActionResult IniciarSesion(IniciarSesionRequestModel model)
        {
            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@Correo", model.Correo);

            var usuario = context.QueryFirstOrDefault<UsuarioResponse>("sp_Usuario_ConsultarPorCorreo", parametros);

            // No se indica cuál de los dos datos falló (CU-02, flujo 4a)
            if (usuario == null || !_seguridad.VerificarHash(model.Contrasena, usuario.Contrasena))
                return Unauthorized("Correo o contraseña incorrectos.");

            if (!usuario.Activo)
                return StatusCode(StatusCodes.Status403Forbidden, "Su cuenta está deshabilitada. Comuníquese con el administrador.");

            var (token, expiracion) = _seguridad.GenerarToken(usuario);

            return Ok(new SesionResponse
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo,
                Rol = usuario.NombreRol,
                Token = token,
                Expiracion = expiracion
            });
        }

        #endregion

        #region Validar Sesión

        /// <summary>
        /// Endpoint protegido de ejemplo: devuelve los datos que vienen dentro del token.
        /// Sirve para probar que el JWT funciona (401 si no se envía o está vencido).
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("ValidarSesion")]
        public IActionResult ValidarSesion()
        {
            return Ok(new
            {
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                NombreUsuario = User.FindFirstValue(ClaimTypes.Name),
                Rol = User.FindFirstValue(ClaimTypes.Role)
            });
        }

        #endregion
    }
}
