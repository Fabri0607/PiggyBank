using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class PagoDTO
    {
        public int PagoID { get; set; }
        public int UsuarioID { get; set; }
        public string Titulo { get; set; }
        public decimal Monto { get; set; }
        public string Categoria { get; set; } 
        public DateTime Fecha_Vencimiento { get; set; }
        public string Estado { get; set; } 
        public int CategoriaID { get; set; }
    }
}
