using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class MiembroDTO
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public DateTime FechaUnion { get; set; }
    }
}
