using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Filtros;
using ProyectoFinal_WEB.Helpers;

namespace ProyectoFinal_WEB.Controllers
{
    /// <summary>
    /// Panel administrativo. Todo el controlador exige sesión con rol Administrador.
    /// Pendiente: Keylor (CU-10 Categorías y CU-11 Productos).
    /// </summary>
    [SesionActiva(Rol = SesionHelper.RolAdministrador)]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
