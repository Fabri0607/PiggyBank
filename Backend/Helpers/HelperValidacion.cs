using Backend.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Helpers
{
    public static class HelperValidacion
    {
        public static bool EsTextoValido(string texto, int longitudMaxima = 100)
        {
            return !string.IsNullOrWhiteSpace(texto) && texto.Length <= longitudMaxima;
        }

        public static bool EsDecimalValido(decimal valor)
        {
            return valor > 0;
        }

        public static bool EsRolValido(string rol)
        {
            return new[] { "Administrador", "Miembro", "Consulta" }.Contains(rol);
        }

        public static bool EsEstadoValido(string estado)
        {
            return new[] { "Pendiente", "Pagado", "Rechazado" }.Contains(estado);
        }

        public static Error CrearError(enumErrores codigo, string mensaje)
        {
            return new Error
            {
                ErrorCode = (int)codigo,
                Message = mensaje
            };
        }
    }
}
