using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqInvitarMiembroGrupo : ReqBase
    {
        public int GrupoID { get; set; }
        public string correoUsuario { get; set; } // ID del usuario a invitar
        public string Rol { get; set; } // Rol: 'Administrador', 'Miembro', 'Consulta'
    }

}
