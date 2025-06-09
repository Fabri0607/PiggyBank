using Backend.Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqIngresarTransaccion: ReqBase
    {
        public Transaccion Transaccion { get; set; }
    }
}
