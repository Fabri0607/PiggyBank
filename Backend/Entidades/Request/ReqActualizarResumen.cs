using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarResumen : ReqBase
    {
        public int AnalisisID { get; set; } // ID del análisis al que pertenece el resumen
        public string Resumen { get; set; } // Contenido del resumen

    }
}
