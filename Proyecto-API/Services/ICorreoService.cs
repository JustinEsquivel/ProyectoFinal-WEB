namespace ProyectoFinal_API.Services
{
    /// <summary>
    /// Envío de notificaciones por correo electrónico.
    /// </summary>
    public interface ICorreoService
    {
        void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml);
    }
}
