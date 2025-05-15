using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqCrearAnalisis : ReqBase
    {
        public int UsuarioID { get; set; } // ID del usuario que crea el análisis
        public DateTime? FechaInicio { get; set; } // Fecha de inicio del análisis
        public DateTime? FechaFin { get; set; } // Fecha de fin del análisis
        public int ContextoID { get; set; } // ID del contexto del análisis

    }
}
