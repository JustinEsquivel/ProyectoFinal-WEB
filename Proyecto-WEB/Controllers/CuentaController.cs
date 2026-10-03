using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Models;

namespace ProyectoFinal_WEB.Controllers
{
    // Solo diseño: todavía no hay lógica de autenticación.
    public class CuentaController : Controller
    {
        [HttpGet]
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarSesion(IniciarSesionViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            return RedirectToAction("Index", "Home");
        }

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
    }
}
