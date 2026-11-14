using ProyectoFinal_API.Services;
using System.Security.Claims;

namespace ProyectoFinal_API.Middleware
{
    /// <summary>
    /// Middleware de administración de excepciones: cualquier error no controlado en la API
    /// se registra en la tabla BitacoraError y se responde un 500 con un mensaje genérico.
    /// Así los controladores no necesitan try/catch.
    /// </summary>
    public class ManejoErroresMiddleware(RequestDelegate _next, ILogger<ManejoErroresMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext context, IBitacoraService _bitacora)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    var idUsuario = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    int? usuarioId = int.TryParse(idUsuario, out var id) ? id : null;
                    var ruta = $"{context.Request.Method} {context.Request.Path}";

                    _bitacora.RegistrarError("API", ruta, ex.Message, ex.ToString(), usuarioId);
                }
                catch (Exception exBitacora)
                {
                    // Si la base de datos no responde, al menos queda en la consola
                    _logger.LogError(exBitacora, "No se pudo registrar el error en BitacoraError");
                }

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync("Se presentó un inconveniente técnico. Intente más tarde.");
                }
            }
        }
    }
}
