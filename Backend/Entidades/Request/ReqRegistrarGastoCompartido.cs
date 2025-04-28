using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqRegistrarGastoCompartido : ReqBase
    {
        public int TransaccionID { get; set; } // ID de la transacción asociada
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; } // ID del usuario responsable
        public decimal Monto { get; set; }
        public string Estado { get; set; } // 'Pendiente', 'Pagado', 'Rechazado'
    }
}
