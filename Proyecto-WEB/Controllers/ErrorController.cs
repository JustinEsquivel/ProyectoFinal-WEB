using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Helpers;
using ProyectoFinal_WEB.Models;
using System.Diagnostics;

namespace ProyectoFinal_WEB.Controllers
{
    public class ErrorController(HttpClient _httpClient, IConfiguration _configuration, ILogger<ErrorController> _logger) : Controller
    {
        /// <summary>
        /// Recibe las excepciones no controladas de la WEB (UseExceptionHandler),
        /// las envía a la API para guardarlas en BitacoraError y muestra la vista de error.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CapturarError()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (ex != null)
            {
                try
                {
                    using var client = _httpClient;
                    var url = _configuration.GetValue<string>("Variables:ApiBaseUrl") + "Error/Registrar";

                    client.PostAsJsonAsync(url, new
                    {
                        Ruta = $"{HttpContext.Request.Method} {ex.Path}",
                        Mensaje = ex.Error.Message,
                        Detalle = ex.Error.ToString(),
                        UsuarioId = HttpContext.Session.GetInt32(SesionHelper.UsuarioId)
                    }).Wait();
                }
                catch (Exception exRegistro)
                {
                    // Si la API no responde, el error al menos queda en la consola
                    _logger.LogError(exRegistro, "No se pudo registrar el error en la API");
                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
