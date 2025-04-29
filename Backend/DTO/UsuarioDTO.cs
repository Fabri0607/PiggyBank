using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class UsuarioDTO
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string ConfiguracionNotificaciones { get; set; }
    }
}
