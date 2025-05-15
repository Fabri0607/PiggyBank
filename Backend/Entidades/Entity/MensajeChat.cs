using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class MensajeChat
    {
        public int MensajeID { get; set; }
        public int AnalisisID { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public int Orden { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
