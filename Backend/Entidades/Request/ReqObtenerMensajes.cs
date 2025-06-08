using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqObtenerMensajes : ReqBase
    {
       public int AnalisisID { get; set; } // ID del análisis al que pertenecen los mensajes
    }
}
