using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqCerrarSesion : ReqBase
    {
        public int SesionID { get; set; }
        public string MotivoRevocacion { get; set; }
    }
}
