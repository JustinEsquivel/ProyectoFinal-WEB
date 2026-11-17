using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Filtros;
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

        #region Registro de Usuarios (CU-01)

        [HttpGet]
        public IActionResult Registro()
        {
            if (HttpContext.Session.EstaAutenticado())
                return RedirigirSegunRol();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var client = _httpClient;
            var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Cuenta/Registrar";

            var response = client.PostAsJsonAsync(url, modelo).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TempData["MensajeEstado"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
                return RedirectToAction(nameof(IniciarSesion));
            }

            // 409 correo repetido: la API envía el mensaje
            ModelState.AddModelError(string.Empty, response.Content.ReadAsStringAsync().Result);
            modelo.Contrasena = string.Empty;
            return View(modelo);
        }

        #endregion

        #region Recuperar Contraseña (CU-04)

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
                return View(modelo);

            using var client = _httpClient;
            var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Cuenta/RecuperarContrasena";

            var response = client.PostAsJsonAsync(url, modelo).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TempData["MensajeEstado"] = response.Content.ReadAsStringAsync().Result;
                return RedirectToAction(nameof(IniciarSesion));
            }

            ModelState.AddModelError(string.Empty, response.Content.ReadAsStringAsync().Result);
            return View(modelo);
        }

        /// <summary>Pantalla a la que llega el usuario desde el enlace del correo.</summary>
        [HttpGet]
        public IActionResult RestablecerContrasena(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["MensajeError"] = "El enlace no es válido o ya expiró. Solicite uno nuevo.";
                return RedirectToAction(nameof(RecuperarContrasena));
            }

            return View(new RestablecerContrasenaViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RestablecerContrasena(RestablecerContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var client = _httpClient;
            var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Cuenta/RestablecerContrasena";

            var response = client.PutAsJsonAsync(url, new { modelo.Token, modelo.NuevaContrasena }).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TempData["MensajeEstado"] = response.Content.ReadAsStringAsync().Result;
                return RedirectToAction(nameof(IniciarSesion));
            }

            // 400 enlace vencido o alterado: se pide uno nuevo
            TempData["MensajeError"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction(nameof(RecuperarContrasena));
        }

        #endregion

        #region Cambiar Contraseña (CU-05)

        [SesionActiva]
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        [SesionActiva]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasena(CambiarContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var client = _httpClient;
            client.AgregarToken(HttpContext);
            var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Cuenta/CambiarContrasena";

            var response = client.PutAsJsonAsync(url, new { modelo.ContrasenaActual, modelo.NuevaContrasena }).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TempData["MensajeEstado"] = response.Content.ReadAsStringAsync().Result;
                return RedirectToAction(nameof(CambiarContrasena));
            }

            // 400 contraseña actual incorrecta: la API envía el mensaje
            ModelState.AddModelError(string.Empty, response.Content.ReadAsStringAsync().Result);
            return View(new CambiarContrasenaViewModel());
        }

        #endregion
    }
}
