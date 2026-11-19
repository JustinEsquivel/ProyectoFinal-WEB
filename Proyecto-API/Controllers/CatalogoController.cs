using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoFinal_API.Models;

namespace ProyectoFinal_API.Controllers;

/// consulta del catálogo para visitantes y clientes>
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class CatalogoController(IConfiguration configuration) : ControllerBase
{
    /// Consulta los productos activos con su categoría y existencias>
    [HttpGet]
    [Route("Productos")]
    public IActionResult Productos(string? busqueda, int? categoriaId)
    {
        using var conexion = new SqlConnection(
            configuration.GetConnectionString("DefaultConnection"));

        var parametros = new DynamicParameters();
        parametros.Add("@Busqueda", busqueda);
        parametros.Add("@CategoriaId", categoriaId);

        var productos = conexion.Query<ProductoCatalogoResponse>(
            "dbo.sp_Producto_CatalogoListar",
            parametros,
            commandType: CommandType.StoredProcedure).ToList();

        return Ok(productos);
    }

    [HttpGet]
    [Route("Categorias")]
    public IActionResult Categorias()
    {
        using var conexion = new SqlConnection(
            configuration.GetConnectionString("DefaultConnection"));

        var categorias = conexion.Query<CategoriaCatalogoResponse>(
            "dbo.sp_Categoria_AdminListar",
            commandType: CommandType.StoredProcedure).ToList();

        return Ok(categorias);
    }
    [HttpGet]
    [Route("Productos/{id:int}")]
    public IActionResult Detalle(int id)
    {
        using var conexion = new SqlConnection(
            configuration.GetConnectionString("DefaultConnection"));

        var parametros = new DynamicParameters();
        parametros.Add("@ProductoId", id);

        var producto = conexion.QuerySingleOrDefault<ProductoCatalogoResponse>(
            "dbo.sp_Producto_CatalogoListar",
            parametros,
            commandType: CommandType.StoredProcedure);

        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto);
    }
}
