using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqObtenerBalanceGrupal : ReqBase
    {
        public int GrupoID { get; set; }
        public DateTime? FechaInicio { get; set; } // Opcional para filtrar por rango de fechas
        public DateTime? FechaFin { get; set; } // Opcional para filtrar por rango de fechas
    }
}
