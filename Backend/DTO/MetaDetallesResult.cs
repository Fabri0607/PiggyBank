using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class MetaDetallesResult
    {
        public int MetaID { get; set; }
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        public bool Completada { get; set; }
        public int? TransaccionID { get; set; }
        public decimal? MontoAsignado { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public string Descripcion { get; set; }
        public decimal? MontoTransaccion { get; set; }
    }
}
