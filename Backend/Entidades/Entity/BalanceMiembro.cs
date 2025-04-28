using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class BalanceMiembro
    {
        public int BalanceID { get; set; }
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; } // Para incluir en respuestas, se llena desde la consulta
        public decimal TotalGastos { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Saldo { get; set; }
        public DateTime FechaCalculo { get; set; }
    }
}