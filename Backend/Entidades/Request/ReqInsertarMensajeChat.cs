using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqInsertarMensajeChat : ReqBase
    {
        public int AnalisisID { get; set; } // ID del análisis al que pertenece el mensaje
        public string Role { get; set; } // Rol del remitente (user o assistant)
        public string Content { get; set; } // Contenido del mensaje
    }
}
