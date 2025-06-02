using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerDetalleTransaccion: ResBase
    {
        public int TransaccionID { get; set; }
        public int UsuarioID { get; set; }
        public string Tipo { get; set; } // 'Ingreso' o 'Gasto'
        public decimal Monto { get; set; }
        public string Categoria { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool EsCompartido { get; set; }
        public int? GrupoID { get; set; }
    }
}
