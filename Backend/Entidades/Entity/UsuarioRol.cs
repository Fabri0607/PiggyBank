using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class UsuarioRol
    {
        public int UsuarioID { get; set; }
        public int RolID { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }
}
