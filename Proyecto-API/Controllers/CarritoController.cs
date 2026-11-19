using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoFinal_API.Models;

namespace ProyectoFinal_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CarritoController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("Consultar")]
    public IActionResult Consultar()
    {
        using var conexion = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var productos = conexion.Query<CarritoProductoResponse>(
            "dbo.sp_Carrito_Consultar", new { UsuarioId = usuarioId },
            commandType: CommandType.StoredProcedure).ToList();

        return Ok(productos);
    }

    [HttpPost("Agregar")]
    public IActionResult Agregar(CarritoCantidadRequestModel modelo)
    {
        return Guardar(modelo, true);
    }

    [HttpPut("Modificar")]
    public IActionResult Modificar(CarritoCantidadRequestModel modelo)
    {
        return Guardar(modelo, false);
    }

    [HttpDelete("Eliminar/{productoId:int}")]
    public IActionResult Eliminar(int productoId)
    {
        using var conexion = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        conexion.Execute("dbo.sp_Carrito_Eliminar",
            new { UsuarioId = usuarioId, ProductoId = productoId },
            commandType: CommandType.StoredProcedure);

        return NoContent();
    }

    private IActionResult Guardar(CarritoCantidadRequestModel modelo, bool sumar)
    {
        using var conexion = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        var parametros = new DynamicParameters();
        parametros.Add("@UsuarioId", int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        parametros.Add("@ProductoId", modelo.ProductoId);
        parametros.Add("@Cantidad", modelo.Cantidad);
        parametros.Add("@Sumar", sumar);

        var mensaje = conexion.QuerySingle<string?>("dbo.sp_Carrito_Guardar",
            parametros, commandType: CommandType.StoredProcedure);

        if (mensaje != null)
        {
            return BadRequest(mensaje);
        }

        return NoContent();
    }
}
