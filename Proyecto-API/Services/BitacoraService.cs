using Dapper;
using Microsoft.Data.SqlClient;

namespace ProyectoFinal_API.Services
{
    public class BitacoraService(IConfiguration _configuration) : IBitacoraService
    {
        public void RegistrarError(string origen, string? ruta, string mensaje, string? detalle, int? usuarioId)
        {
            using var context = new SqlConnection(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"));

            var parametros = new DynamicParameters();
            parametros.Add("@Origen", origen);
            parametros.Add("@Ruta", ruta);
            parametros.Add("@Mensaje", mensaje);
            parametros.Add("@Detalle", detalle);
            parametros.Add("@UsuarioId", usuarioId);

            context.Execute("sp_BitacoraError_Registrar", parametros);
        }
    }
}
