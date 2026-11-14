using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProyectoFinal_WEB.Helpers;

namespace ProyectoFinal_WEB.Filtros
{
    /// <summary>
    /// Protege acciones o controladores que requieren sesión iniciada.
    /// Uso:
    ///   [SesionActiva]                                       → cualquier usuario autenticado
    ///   [SesionActiva(Rol = SesionHelper.RolAdministrador)]  → solo administradores
    /// Si el token venció, cierra la sesión y envía al inicio de sesión (CU-03, flujo 1a).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SesionActivaAttribute : ActionFilterAttribute
    {
        public string? Rol { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var sesion = httpContext.Session;

            if (!sesion.EstaAutenticado())
            {
                var teniaSesion = !string.IsNullOrEmpty(sesion.GetString(SesionHelper.Token));
                sesion.Clear();

                var tempData = httpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(httpContext);
                tempData["MensajeError"] = teniaSesion
                    ? "Su sesión expiró. Inicie sesión nuevamente."
                    : "Debe iniciar sesión para continuar.";

                context.Result = new RedirectToActionResult("IniciarSesion", "Cuenta", null);
                return;
            }

            if (!string.IsNullOrEmpty(Rol) && sesion.GetString(SesionHelper.Rol) != Rol)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
