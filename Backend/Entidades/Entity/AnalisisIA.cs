using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class AnalisisIA
    {
        public int AnalisisID { get; set; }
        public int UsuarioID { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Resumen { get; set; }
        public string Recomendaciones { get; set; }
        public DateTime FechaGeneracion { get; set; }
    }
}
