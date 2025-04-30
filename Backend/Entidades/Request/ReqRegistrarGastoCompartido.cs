using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqRegistrarGastoCompartido : ReqBase
    {
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; } // 'Pendiente', 'Pagado', 'Rechazado'
        public int CategoriaID { get; set; }
        public string Descripcion { get; set; }
    }
}
