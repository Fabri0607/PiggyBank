using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResInsertarMensajeChat : ResBase
    {
        public int MensajeConsultaID { get; set; } 
        public int MensajeRespuestaID { get; set; }
    }
}
