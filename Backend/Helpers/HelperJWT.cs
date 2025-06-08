using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend.Helpers
{
    public static class HelperJWT
    {
        // Clave secreta usada para firmar el token (debe tener al menos 32 caracteres para SHA256)
        private static readonly string ClaveSecreta = "ClaveJWT_SuperSegura_ParaTuAPI_123456";

        public static string GenerarToken(string guid)
        {
            var key = Encoding.UTF8.GetBytes(ClaveSecreta);

            var claims = new[]
            {
                new Claim("Guid", guid),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public static string ValidarTokenYObtenerGuid(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(ClaveSecreta);

                // Configurar los parámetros de validación del token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // No validamos emisor
                    ValidateAudience = false, // No validamos audiencia
                    ValidateLifetime = true, // Validamos que el token no haya expirado
                    ClockSkew = TimeSpan.Zero // Sin margen de tiempo para expiración
                };

                // Validar el token y obtener los claims
                var claimsPrincipal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Extraer el GUID del claim
                var guidClaim = claimsPrincipal.FindFirst("Guid");
                if (guidClaim == null)
                {
                    throw new SecurityTokenException("El token no contiene un GUID válido.");
                }

                return guidClaim.Value;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new SecurityTokenException("El token ha expirado.");
            }
            catch (SecurityTokenException ex)
            {
                throw new SecurityTokenException($"Error al validar el token: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al procesar el token: {ex.Message}");
            }
        }

    }
}
