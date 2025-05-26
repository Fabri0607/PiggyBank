using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqSalirGrupo : ReqBase
    {
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
    }
}