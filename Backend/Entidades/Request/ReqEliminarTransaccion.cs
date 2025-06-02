using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqEliminarTransaccion: ReqBase
    {
        public int TransaccionID { get; set; }
        public int UsuarioID { get; set; }
    }
}
