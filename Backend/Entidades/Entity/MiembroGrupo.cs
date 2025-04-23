using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class MiembroGrupo
    {
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public string Rol { get; set; } // 'Administrador', 'Miembro', 'Consulta'
        public DateTime FechaUnion { get; set; }
    }
}
