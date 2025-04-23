using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Presupuesto
    {
        public int PresupuestoID { get; set; }
        public int UsuarioID { get; set; }
        public int CategoriaID { get; set; }
        public decimal MontoLimite { get; set; }
        public string Periodo { get; set; } // 'Semanal', 'Mensual', 'Anual'
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Notificar { get; set; }
    }
}
