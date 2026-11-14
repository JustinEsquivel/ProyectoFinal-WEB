using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_API.Models;
using ProyectoFinal_API.Services;

namespace ProyectoFinal_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController(IBitacoraService _bitacora) : ControllerBase
    {
        /// <summary>
        /// Recibe los errores que ocurren en la aplicación WEB y los guarda en BitacoraError.
        /// (Los errores de la API los guarda directamente el ManejoErroresMiddleware.)
        /// </summary>
        [HttpPost]
        [Route("Registrar")]
        public IActionResult Registrar(ErrorRequestModel model)
        {
            _bitacora.RegistrarError("WEB", model.Ruta, model.Mensaje, model.Detalle, model.UsuarioId);

            return Ok("Se registró el error correctamente");
        }
    }
}
