using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarGrupo : ReqBase
    {
        public int GrupoID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int AdminUsuarioID { get; set; }
    }
}
