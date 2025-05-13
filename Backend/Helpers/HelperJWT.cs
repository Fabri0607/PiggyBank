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
    }
}
