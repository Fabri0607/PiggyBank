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
                string body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #4CAF50;'>¡Hola {nombreDestinatario}!</h2>
                    <p style='font-size: 16px;'>Gracias por registrarte en <strong>PiggyBank</strong>.</p>
                    <p style='font-size: 16px;'>Tu código de verificación es:</p>
                    <div style='font-size: 24px; font-weight: bold; color: #333; background-color: #e7f3fe; padding: 10px; border-radius: 8px; text-align: center;'>
                        {codigo}
                    </div>
                    <p style='font-size: 14px; color: #555;'>Este código es válido por 30 minutos.</p>
                    <hr style='margin: 30px 0;'>
                    <p style='font-size: 12px; color: #999;'>Si no solicitaste este código, puedes ignorar este mensaje.</p>
                    <p style='font-size: 14px;'>Atentamente,<br>El equipo de <strong>PiggyBank</strong></p>
                </div>
            </body>
            </html>";

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
                    IsBodyHtml = true // Cambia a true si usas HTML
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