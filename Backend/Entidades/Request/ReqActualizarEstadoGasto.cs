using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarEstadoGasto : ReqBase
    {
        public int GastoID { get; set; }
        public int UsuarioID { get; set; }
        public string NuevoEstado { get; set; }
    }
}