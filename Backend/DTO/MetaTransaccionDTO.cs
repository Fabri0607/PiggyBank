using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class MetaTransaccionDTO
    {
        public int TransaccionID { get; set; }
        public decimal MontoAsignado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string Descripcion { get; set; }
        public decimal MontoTransaccion { get; set; }
    }
}
