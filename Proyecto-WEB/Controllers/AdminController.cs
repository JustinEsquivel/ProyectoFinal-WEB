using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoFinal_WEB.Filtros;
using ProyectoFinal_WEB.Helpers;
using ProyectoFinal_WEB.Models.Admin;

namespace ProyectoFinal_WEB.Controllers;

[SesionActiva(Rol = SesionHelper.RolAdministrador)]
public class AdminController(HttpClient client, IConfiguration configuration) : Controller
{
	[HttpGet]
	public IActionResult Index() => View();

	[HttpGet]
	public async Task<IActionResult> Categorias()
	{
		var filas = await Leer<List<CategoriaFila>>("Categorias");

		if (filas is null)
			ViewBag.Error = "No se pudieron cargar las categorías.";

		return View(filas ?? []);
	}

	[HttpGet]
	public async Task<IActionResult> FormularioCategoria(int? id)
	{
		CategoriaForm? model = id.HasValue
			? await Leer<CategoriaFila>($"Categorias/{id}")
			: new CategoriaForm();

		if (model is null)
		{
			TempData["ErrorAdmin"] = "No se pudo cargar la categoría.";
			return RedirectToAction(nameof(Categorias));
		}

		ViewBag.Id = id;
		return View(model);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> GuardarCategoria(int? id, CategoriaForm model)
	{
		if (ModelState.IsValid)
		{
			var error = await Enviar(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
				id.HasValue ? $"Categorias/{id}" : "Categorias", model);

			if (error is null)
			{
				TempData["ExitoAdmin"] = "Categoría guardada.";
				return RedirectToAction(nameof(Categorias));
			}

			ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Id = id;
		return View(nameof(FormularioCategoria), model);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> EliminarCategoria(int id)
	{
		var error = await Enviar(HttpMethod.Delete, $"Categorias/{id}");

		TempData[error is null ? "ExitoAdmin" : "ErrorAdmin"] =
			error ?? "Categoría eliminada.";

		return RedirectToAction(nameof(Categorias));
	}

	[HttpGet]
	public async Task<IActionResult> Productos()
	{
		var filas = await Leer<List<ProductoFila>>("Productos");

		if (filas is null)
			ViewBag.Error = "No se pudieron cargar los productos.";

		ViewBag.Umbral = Math.Max(0,
			configuration.GetValue<int?>("Variables:UmbralStockBajo") ?? 5);

		return View(filas ?? []);
	}

	[HttpGet]
	public async Task<IActionResult> FormularioProducto(int? id)
	{
		ProductoForm? model = id.HasValue
			? await Leer<ProductoFila>($"Productos/{id}")
			: new ProductoForm();

		if (model is null)
		{
			TempData["ErrorAdmin"] = "No se pudo cargar el producto.";
			return RedirectToAction(nameof(Productos));
		}

		ViewBag.Id = id;
		ViewBag.PrecioTexto = id.HasValue
			? model.Precio.ToString("0.00", CultureInfo.InvariantCulture)
			: "";

		await CargarCategorias(model.CategoriaId);

		return View(model);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> GuardarProducto(
		int? id, ProductoForm model, string? precioTexto)
	{
		var texto = precioTexto?.Trim().Replace(',', '.');

		if (!decimal.TryParse(texto, NumberStyles.AllowDecimalPoint,
				CultureInfo.InvariantCulture, out var precio)
			|| precio < 0.01m || precio > 99999999.99m
			|| decimal.Round(precio, 2) != precio)
		{
			ModelState.AddModelError(nameof(ProductoForm.Precio),
				"Ingrese un precio válido, con hasta dos decimales y sin separador de miles.");
		}
		else
		{
			model.Precio = precio;
		}

		if (ModelState.IsValid)
		{
			var error = await Enviar(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
				id.HasValue ? $"Productos/{id}" : "Productos", model);

			if (error is null)
			{
				TempData["ExitoAdmin"] = "Producto guardado.";
				return RedirectToAction(nameof(Productos));
			}

			ModelState.AddModelError(string.Empty, error);
		}

		ViewBag.Id = id;
		ViewBag.PrecioTexto = precioTexto;

		await CargarCategorias(model.CategoriaId);

		return View(nameof(FormularioProducto), model);
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> CambiarEstadoProducto(int id, bool activo)
	{
		var estado = activo ? "true" : "false";
		var error = await Enviar(HttpMethod.Patch, $"Productos/{id}/Estado/{estado}");

		TempData[error is null ? "ExitoAdmin" : "ErrorAdmin"] =
			error ?? (activo ? "Producto activado." : "Producto desactivado.");

		return RedirectToAction(nameof(Productos));
	}

	private async Task CargarCategorias(int seleccion)
	{
		var filas = await Leer<List<CategoriaFila>>("Categorias");

		if (filas is null)
			ModelState.AddModelError(string.Empty, "No se pudieron cargar las categorías.");
		else if (filas.Count == 0)
			ModelState.AddModelError(string.Empty, "Cree una categoría antes de agregar productos.");

		ViewBag.Categorias = new SelectList(filas ?? [],
			nameof(CategoriaFila.CategoriaId),
			nameof(CategoriaFila.Nombre),
			seleccion);
	}

	private string UrlApi(string ruta) =>
		configuration["Variables:ApiBaseUrl"]!.TrimEnd('/') + "/AdminCatalogo/" + ruta;

	private async Task<T?> Leer<T>(string ruta) where T : class
	{
		try
		{
			client.AgregarToken(HttpContext);
			using var respuesta = await client.GetAsync(UrlApi(ruta));

			return respuesta.IsSuccessStatusCode
				? await respuesta.Content.ReadFromJsonAsync<T>()
				: null;
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
		{
			return null;
		}
	}

	private async Task<string?> Enviar(HttpMethod metodo, string ruta, object? datos = null)
	{
		try
		{
			client.AgregarToken(HttpContext);
			using var solicitud = new HttpRequestMessage(metodo, UrlApi(ruta));

			if (datos is not null)
				solicitud.Content = JsonContent.Create(datos, datos.GetType());

			using var respuesta = await client.SendAsync(solicitud);

			if (respuesta.IsSuccessStatusCode)
				return null;

			if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
				return "No tiene acceso a esta operación.";

			if (respuesta.StatusCode == HttpStatusCode.NotFound)
				return "El registro ya no existe.";

			if (respuesta.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
			{
				return respuesta.Content.Headers.ContentType?.MediaType == "text/plain"
					? await respuesta.Content.ReadAsStringAsync()
					: "Revise los datos ingresados.";
			}

			return "No se pudo completar la operación.";
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			return "No se pudo completar la operación.";
		}
	}
}