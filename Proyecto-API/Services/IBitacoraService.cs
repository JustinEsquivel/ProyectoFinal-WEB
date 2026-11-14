namespace ProyectoFinal_API.Services
{
    /// <summary>
    /// Guarda los errores del sistema en la tabla BitacoraError.
    /// </summary>
    public interface IBitacoraService
    {
        void RegistrarError(string origen, string? ruta, string mensaje, string? detalle, int? usuarioId);
    }
}
