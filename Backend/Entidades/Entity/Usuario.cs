using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string CodigoVerificacion { get; set; }
        public DateTime? FechaExpiracionCodigo { get; set; }
        public bool EmailVerificado { get; set; }
        public string LlaveUnica { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string ConfiguracionNotificaciones { get; set; }
    }
}
