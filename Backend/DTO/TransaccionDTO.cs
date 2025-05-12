using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class TransaccionDTO
    {
        public int TransaccionID { get; set; }
        public string Tipo { get; set; } // 'Ingreso' o 'Gasto'
        public decimal Monto { get; set; }
        public string Categoria { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public string ColorHex { get; set; }

        //public bool EsCompartido { get; set; }
        //public int? GrupoID { get; set; }
    }
}
