using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ProyectoFinal_API.Services
{
    public class CorreoService(IConfiguration _configuration, ILogger<CorreoService> _logger) : ICorreoService
    {
        /// <summary>
        /// Envía el correo por medio de Gmail con la cuenta de Correo:Cuenta y Correo:Clave.
        /// Si no hay credenciales configuradas, no envía nada y deja el correo en la consola
        /// para que cada integrante pueda probar sin configurar Gmail.
        /// </summary>
        public void EnviarCorreo(string destinatario, string asunto, string cuerpoHtml)
        {
            var cuenta = _configuration.GetValue<string>("Correo:Cuenta");
            var clave = _configuration.GetValue<string>("Correo:Clave");

            if (string.IsNullOrEmpty(cuenta) || string.IsNullOrEmpty(clave))
            {
                _logger.LogWarning("Correo no configurado. Destinatario: {Destinatario}. Asunto: {Asunto}. Contenido: {Contenido}", destinatario, asunto, cuerpoHtml);
                return;
            }

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Súper Tico", cuenta));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

            using var client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            client.Authenticate(cuenta, clave);
            client.Send(mensaje);
            client.Disconnect(true);
        }
    }
}
