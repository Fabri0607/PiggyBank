using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarCategoria: ReqBase
    {
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public string ColorHex { get; set; }
    }
}
