using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoFinal_API.Models.Admin;

namespace ProyectoFinal_API.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/[controller]")]
public class AdminCatalogoController(IConfiguration configuration) : ControllerBase
{
	private SqlConnection Conexion() =>
		new(configuration.GetConnectionString("DefaultConnection"));

	[HttpGet("Categorias")]
	public IActionResult Categorias()
	{
		using var db = Conexion();
		return Ok(db.Query<CategoriaFila>("dbo.sp_Categoria_AdminListar",
			commandType: CommandType.StoredProcedure).ToList());
	}

	[HttpGet("Categorias/{id:int}")]
	public IActionResult Categoria(int id)
	{
		using var db = Conexion();
		var fila = db.QuerySingleOrDefault<CategoriaFila>(
			"dbo.sp_Categoria_AdminListar", new { Id = id },
			commandType: CommandType.StoredProcedure);

		return fila is null ? NotFound() : Ok(fila);
	}

	[HttpPost("Categorias")]
	[HttpPut("Categorias/{id:int}")]
	public IActionResult GuardarCategoria(CategoriaForm model, [FromRoute] int? id)
	{
		return Ejecutar("dbo.sp_Categoria_AdminGuardar",
			new
			{
				Id = id,
				Nombre = model.Nombre.Trim(),
				Descripcion = model.Descripcion?.Trim()
			},
			"Ya existe una categoría con ese nombre.");
	}

	[HttpDelete("Categorias/{id:int}")]
	public IActionResult EliminarCategoria(int id) =>
		Ejecutar("dbo.sp_Categoria_AdminEliminar", new { Id = id },
			"No puede eliminar una categoría que tiene productos asociados.");

	[HttpGet("Productos")]
	public IActionResult Productos()
	{
		using var db = Conexion();
		return Ok(db.Query<ProductoFila>("dbo.sp_Producto_AdminListar",
			commandType: CommandType.StoredProcedure).ToList());
	}

	[HttpGet("Productos/{id:int}")]
	public IActionResult Producto(int id)
	{
		using var db = Conexion();
		var fila = db.QuerySingleOrDefault<ProductoFila>(
			"dbo.sp_Producto_AdminListar", new { Id = id },
			commandType: CommandType.StoredProcedure);

		return fila is null ? NotFound() : Ok(fila);
	}

	[HttpPost("Productos")]
	[HttpPut("Productos/{id:int}")]
	public IActionResult GuardarProducto(ProductoForm model, [FromRoute] int? id)
	{
		if (decimal.Round(model.Precio, 2) != model.Precio)
			return BadRequest("El precio admite hasta dos decimales.");

		return Ejecutar("dbo.sp_Producto_AdminGuardar",
			new
			{
				Id = id,
				model.CategoriaId,
				Nombre = model.Nombre.Trim(),
				Descripcion = model.Descripcion?.Trim(),
				model.Precio,
				model.Existencias,
				ImagenUrl = model.ImagenUrl?.Trim()
			},
			"La categoría seleccionada no existe o los datos del producto no son válidos.");
	}

	[HttpPatch("Productos/{id:int}/Estado/{activo:bool}")]
	public IActionResult CambiarEstado(int id, bool activo) =>
		Ejecutar("dbo.sp_Producto_AdminCambiarEstado",
			new { Id = id, Activo = activo },
			"No se pudo cambiar el estado del producto.");

	private IActionResult Ejecutar(string procedimiento, object parametros, string mensaje)
	{
		try
		{
			using var db = Conexion();
			var filas = db.QuerySingle<int>(procedimiento, parametros,
				commandType: CommandType.StoredProcedure);

			return filas == 0 ? NotFound() : NoContent();
		}
		catch (SqlException ex) when (ex.Number is 547 or 2601 or 2627)
		{
			return Conflict(mensaje);
		}
	}
}