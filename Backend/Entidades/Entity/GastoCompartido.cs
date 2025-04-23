using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class GastoCompartido
    {
        public int GastoID { get; set; }
        public int TransaccionID { get; set; }
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; } // 'Pendiente', 'Pagado', 'Rechazado'
        public DateTime Fecha { get; set; }
    }
}
