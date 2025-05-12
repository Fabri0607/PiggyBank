using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqTransaccionesPorUsuario
    {
        public int UsuarioID { get; set; }
        public DateTime? FechaInicio { get; set; } 
        public DateTime? FechaFin { get; set; }
        public string TipoTransaccion { get; set; } = null;
    }
}
