using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqIngresarPagoProgramado: ReqBase
    {
        public int UsuarioID { get; set; }
        public string Titulo { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
        public int CategoriaID { get; set; }
    }
}
