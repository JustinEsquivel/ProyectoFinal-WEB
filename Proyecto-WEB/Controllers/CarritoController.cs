using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Filtros;
using ProyectoFinal_WEB.Helpers;
using ProyectoFinal_WEB.Models;

namespace ProyectoFinal_WEB.Controllers;

[SesionActiva]
public class CarritoController(HttpClient httpClient, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index()
    {
        httpClient.AgregarToken(HttpContext);
        using var respuesta = await httpClient.GetAsync(UrlApi("Consultar"));

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SesionExpirada();
        }

        respuesta.EnsureSuccessStatusCode();
        var productos = await respuesta.Content.ReadFromJsonAsync<List<CarritoProductoViewModel>>();
        var modelo = new CarritoViewModel
        {
            Productos = productos ?? new List<CarritoProductoViewModel>()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Agregar(CarritoCantidadViewModel modelo)
    {
        return Guardar(modelo, "Agregar");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Modificar(CarritoCantidadViewModel modelo)
    {
        return Guardar(modelo, "Modificar");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar([Range(1, int.MaxValue)] int productoId)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorCarrito"] = "Seleccione un producto válido.";
            return RedirectToAction(nameof(Index));
        }

        httpClient.AgregarToken(HttpContext);
        using var respuesta = await httpClient.DeleteAsync(UrlApi("Eliminar/" + productoId));
        return ProcesarRespuesta(respuesta, "Producto eliminado del carrito.");
    }

    private async Task<IActionResult> Guardar(CarritoCantidadViewModel modelo, string accion)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorCarrito"] = "Seleccione un producto y una cantidad entera mayor que cero.";
            return RedirectToAction(nameof(Index));
        }

        httpClient.AgregarToken(HttpContext);
        using var solicitud = new HttpRequestMessage(
            accion == "Agregar" ? HttpMethod.Post : HttpMethod.Put, UrlApi(accion));
        solicitud.Content = JsonContent.Create(modelo);
        using var respuesta = await httpClient.SendAsync(solicitud);

        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            TempData["ErrorCarrito"] = await respuesta.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }

        return ProcesarRespuesta(respuesta, "Carrito actualizado correctamente.");
    }

    private IActionResult ProcesarRespuesta(HttpResponseMessage respuesta, string mensaje)
    {
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SesionExpirada();
        }

        respuesta.EnsureSuccessStatusCode();
        TempData["MensajeCarrito"] = mensaje;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult SesionExpirada()
    {
        HttpContext.Session.Clear();
        TempData["MensajeError"] = "Su sesión expiró. Inicie sesión nuevamente.";
        return RedirectToAction("IniciarSesion", "Cuenta");
    }

    private string UrlApi(string accion)
    {
        return configuration.GetValue<string>("Variables:ApiBaseUrl") + "Carrito/" + accion;
    }
}
