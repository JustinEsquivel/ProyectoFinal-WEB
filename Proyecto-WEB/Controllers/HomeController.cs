using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Models;

namespace ProyectoFinal_WEB.Controllers
{
    public class HomeController(
        HttpClient httpClient,
        IConfiguration configuration) : Controller
    {
        public async Task<IActionResult> Index(
            string? busqueda,
            int? categoriaId)
        {
            var apiBaseUrl = configuration
                .GetValue<string>("Variables:ApiBaseUrl");

            var urlProductos = apiBaseUrl + "Catalogo/Productos"
                + "?busqueda=" + Uri.EscapeDataString(busqueda ?? "");

            if (categoriaId.HasValue)
            {
                urlProductos += "&categoriaId=" + categoriaId.Value;
            }

            var productos = await httpClient
                .GetFromJsonAsync<List<ProductoCatalogoViewModel>>(
                    urlProductos);

            var categorias = await httpClient
                .GetFromJsonAsync<List<CategoriaCatalogoViewModel>>(
                    apiBaseUrl + "Catalogo/Categorias");

            var modelo = new CatalogoViewModel
            {
                Productos = productos
                    ?? new List<ProductoCatalogoViewModel>(),

                Categorias = categorias
                    ?? new List<CategoriaCatalogoViewModel>(),

                Busqueda = busqueda,
                CategoriaId = categoriaId
            };

            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var url = configuration.GetValue<string>("Variables:ApiBaseUrl")
                + "Catalogo/Productos/" + id;

            using var respuesta = await httpClient.GetAsync(url);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            respuesta.EnsureSuccessStatusCode();

            var producto = await respuesta.Content
                .ReadFromJsonAsync<ProductoCatalogoViewModel>();

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}
