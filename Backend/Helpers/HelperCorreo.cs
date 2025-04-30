using System;
using System.Net.Mail;
using System.Net;

namespace Backend.Helpers
{
    public static class HelperCorreo
    {
        public static bool EnviarCorreoVerificacion(string emailDestinatario, string nombreDestinatario, string codigo)
        {
            try
            {
                // Configuración del correo
                var fromAddress = new MailAddress("piggybank@28223796dae901db.maileroo.org", "PiggyBank");
                var toAddress = new MailAddress(emailDestinatario, nombreDestinatario);
                const string subject = "Código de Verificación - PiggyBank";
                string body = $"Hola {nombreDestinatario},\n\nTu código de verificación es: {codigo}\n\nEste código es válido por 30 minutos.\n\nGracias,\nEquipo PiggyBank";

                // Configuración del servidor SMTP de Maileroo
                var smtp = new SmtpClient
                {
                    Host = "smtp.maileroo.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        "piggybank@28223796dae901db.maileroo.org",
                        "5498360af4d588a060a1a0a2"
                    )
                };

                // Crear y enviar el correo
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false // Cambia a true si usas HTML
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Error SMTP: {ex.Message}. StatusCode: {ex.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al enviar correo: {ex.Message}");
                return false;
            }
        }
    }
}