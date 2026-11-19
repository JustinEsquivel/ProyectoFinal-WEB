using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_WEB.Models;

namespace ProyectoFinal_WEB.ViewComponents
{
    public class CategoriasMenuViewComponent(
        HttpClient httpClient,
        IConfiguration configuration) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var url = configuration
                .GetValue<string>("Variables:ApiBaseUrl")
                + "Catalogo/Categorias";

            var categorias = await httpClient
                .GetFromJsonAsync<List<CategoriaCatalogoViewModel>>(url);

            return View(categorias ?? new List<CategoriaCatalogoViewModel>());
        }
    }
}
