using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarProgresoMeta : ReqBase
    {
        public int MetaID { get; set; }
        public int UsuarioID { get; set; }
        public decimal MontoActual { get; set; }
    }
}
