using System;
using System.Net.Mail;
using System.Net;

namespace Backend.Helpers
{
    public static class HelperCorreo
    {
        public static bool EnviarCorreoVerificacion(string emailDestinatario, string nombreDestinatario, string codigo)
        {
            // Credenciales directamente en el método  
            string remitente = "falfaroarce0607@gmail.com";
            string contrasena = "ozqb ntfx iejd tqzc";

            try
            {
                Console.WriteLine($"Enviando correo a {emailDestinatario} con código {codigo}");

                MailMessage mensaje = new MailMessage
                {
                    From = new MailAddress(remitente, "PiggyBank"),
                    Subject = "Código de Verificación - PiggyBank",
                    IsBodyHtml = true,
                    Body = $@"  
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
    </html>"
                };

                mensaje.To.Add(new MailAddress(emailDestinatario, nombreDestinatario));

                // Cambiar la declaración using para ser compatible con C# 7.3  
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(remitente, contrasena),
                    EnableSsl = true
                };

                smtp.Send(mensaje);
                Console.WriteLine($"Correo enviado con éxito a {emailDestinatario}.");
                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Error SMTP al enviar correo a {emailDestinatario}: {ex.Message}. StatusCode: {ex.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al enviar correo a {emailDestinatario}: {ex.Message}");
                return false;
            }
        }

        public static bool EnviarCorreoContrasena(string emailDestinatario, string nombreDestinatario, string codigo)
        {
            // Credenciales directamente en el método  
            string remitente = "falfaroarce0607@gmail.com";
            string contrasena = "ozqb ntfx iejd tqzc";

            try
            {
                Console.WriteLine($"Enviando correo a {emailDestinatario} con código {codigo}");

                MailMessage mensaje = new MailMessage
                {
                    From = new MailAddress(remitente, "PiggyBank"),
                    Subject = "Código de Verificación - PiggyBank",
                    IsBodyHtml = true,
                    Body = $@"  
               <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);'>
                    <h2 style='color: #4CAF50;'>¡Hola {nombreDestinatario}!</h2>
                    <p style='font-size: 16px;'>Hemos recibido una solicitud para cambiar tu contraseña en <strong>PiggyBank</strong>.</p>
                    <p style='font-size: 16px;'>Utiliza el siguiente código para confirmar el cambio de contraseña:</p>
                    <div style='font-size: 24px; font-weight: bold; color: #333; background-color: #e7f3fe; padding: 10px; border-radius: 8px; text-align: center;'>
                        {codigo}
                    </div>
                    <p style='font-size: 14px; color: #555;'>Este código es válido por 30 minutos.</p>
                    <hr style='margin: 30px 0;'>
                    <p style='font-size: 12px; color: #999;'>Si no solicitaste cambiar tu contraseña, por favor ignora este mensaje o contacta con nuestro soporte.</p>
                    <p style='font-size: 14px;'>Atentamente,<br>El equipo de <strong>PiggyBank</strong></p>
                </div>
            </body>
            </html>"
                };

                mensaje.To.Add(new MailAddress(emailDestinatario, nombreDestinatario));

                // Cambiar la declaración using para ser compatible con C# 7.3  
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(remitente, contrasena),
                    EnableSsl = true
                };

                smtp.Send(mensaje);
                Console.WriteLine($"Correo enviado con éxito a {emailDestinatario}.");
                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Error SMTP al enviar correo a {emailDestinatario}: {ex.Message}. StatusCode: {ex.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al enviar correo a {emailDestinatario}: {ex.Message}");
                return false;
            }
        }
    }
}