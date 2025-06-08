using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqEliminarMiembro : ReqBase
    {
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public int AdminUsuarioID { get; set; }
    }
}
