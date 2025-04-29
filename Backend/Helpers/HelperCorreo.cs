using System;
using System.Net;
using System.Net.Mail;

namespace Backend.Helpers
{
    public static class HelperCorreo
    {
        // Cambia estos valores por tus credenciales reales
        private static readonly string CorreoRemitente = "suMail@suDominio.com";
        private static readonly string NombreRemitente = "Nosotros Support";
        private static readonly string SmtpHost = "smtp.sudominio.com";
        private static readonly int SmtpPuerto = 587;
        private static readonly string PasswordSecreta = "{EL PASS ULTRASECRETO DE LA CUENTA}";

        /// <summary>
        /// Envía un correo de verificación de cuenta al usuario.
        /// </summary>
        public static bool EnviarCorreoVerificacion(string correoDestino, string nombreUsuario, string codigoVerificacion)
        {
            bool enviadoExitosamente = false;

            try
            {
                #region Construcción del cuerpo del correo
                string asunto = "Verificación de Cuenta";
                string cuerpoHtml = $@"
                <!DOCTYPE html>
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Verificación de Cuenta</title>
                </head>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>¡Hola, {nombreUsuario}!</h2>
                    <p>Gracias por registrarte. Para completar tu registro, utiliza el siguiente código de verificación:</p>
                    <div style='margin: 20px 0; padding: 10px; background-color: #f2f2f2; display: inline-block; font-size: 24px; letter-spacing: 4px;'>
                        {codigoVerificacion}
                    </div>
                    <p>O visita el siguiente enlace para verificar tu cuenta:</p>
                    <p><a href='http://www.tusitio.com/verificar?codigo={codigoVerificacion}' target='_blank'>Verificar Cuenta</a></p>
                    <br/>
                    <p>Saludos cordiales,</p>
                    <p><strong>Nosotros Support</strong></p>
                </body>
                </html>";
                #endregion

                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(CorreoRemitente, NombreRemitente);
                    mensaje.To.Add(new MailAddress(correoDestino));
                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpoHtml;
                    mensaje.IsBodyHtml = true;

                    using (var clienteSmtp = new SmtpClient(SmtpHost, SmtpPuerto))
                    {
                        clienteSmtp.Credentials = new NetworkCredential(CorreoRemitente, PasswordSecreta);
                        clienteSmtp.EnableSsl = true;

                        clienteSmtp.Send(mensaje);
                        enviadoExitosamente = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Puedes mejorar esta parte registrando el error en tu sistema de logs
                Console.WriteLine($"[HelperCorreo] Error al enviar correo: {ex.Message}");
                enviadoExitosamente = false;
            }

            return enviadoExitosamente;
        }
    }
}

