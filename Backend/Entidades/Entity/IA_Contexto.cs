using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    class IA_Contexto
    {
        public int ContextoID { get; set; }
        public string Nombre { get; set; }
        public string Instruccion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
