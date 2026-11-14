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
    public class CuentaController(IConfiguration _configuration, ISeguridadService _seguridad) : ControllerBase
    {
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
