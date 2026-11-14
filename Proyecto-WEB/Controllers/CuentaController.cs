using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Helpers;
using ProyectoFinal_WEB.Models;
using System.Globalization;
using System.Net;

namespace ProyectoFinal_WEB.Controllers
{
    public class CuentaController(HttpClient _httpClient, IConfiguration _configuration) : Controller
    {
        #region Inicio de Sesión (CU-02)

        [HttpGet]
        public IActionResult IniciarSesion()
        {
            if (HttpContext.Session.EstaAutenticado())
                return RedirigirSegunRol();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarSesion(IniciarSesionViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var client = _httpClient;
            var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Cuenta/IniciarSesion";

            var response = client.PostAsJsonAsync(url, modelo).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<SesionResponse>().Result!;

                HttpContext.Session.SetString(SesionHelper.Token, datos.Token);
                HttpContext.Session.SetString(SesionHelper.Expiracion, datos.Expiracion.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
                HttpContext.Session.SetInt32(SesionHelper.UsuarioId, datos.UsuarioId);
                HttpContext.Session.SetString(SesionHelper.NombreUsuario, datos.NombreUsuario);
                HttpContext.Session.SetString(SesionHelper.Correo, datos.Correo);
                HttpContext.Session.SetString(SesionHelper.Rol, datos.Rol);

                return RedirigirSegunRol();
            }

            // 401 credenciales incorrectas, 403 cuenta inactiva: la API envía el mensaje
            ModelState.AddModelError(string.Empty, response.Content.ReadAsStringAsync().Result);
            modelo.Contrasena = string.Empty;
            return View(modelo);
        }

        /// <summary>El Cliente va al catálogo y el Administrador al panel administrativo.</summary>
        private IActionResult RedirigirSegunRol()
        {
            if (HttpContext.Session.EsAdministrador())
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Cerrar Sesión (CU-03)

        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Registro de Usuarios (CU-01) - pendiente: Fabián

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            TempData["MensajeEstado"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        #endregion

        #region Recuperar Contraseña (CU-04) - pendiente: Fabián

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarContrasena(RecuperarContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            TempData["MensajeEstado"] = "Si el correo está registrado, recibirá un enlace para restablecer su contraseña.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        #endregion
    }
}
