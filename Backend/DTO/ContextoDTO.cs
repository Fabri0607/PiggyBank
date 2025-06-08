using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class ContextoDTO
    {
        public int ContextoID { get; set; } // ID del contexto
        public string Nombre { get; set; } // Nombre del contexto
        public DateTime? FechaCreacion { get; set; } // Fecha de creación del contexto
    }
}
