using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class TransaccionDTO
    {
        public int TransaccionID { get; set; } // ID de la transacción
        public string Tipo { get; set; } // Tipo de transacción
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } // Fecha de la transacción                  
        public string Titulo { get; set; } // Título de la transacción
        public string Descripcion { get; set; } // Descripción de la transacción
        public string Categoria { get; set; } // Categoría de la transacción


    }
}
