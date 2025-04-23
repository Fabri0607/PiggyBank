using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Categoria
    {
        public int CategoriaID { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // 'Ingreso' o 'Gasto'
        public string Icono { get; set; }
        public string ColorHex { get; set; }
    }
}
