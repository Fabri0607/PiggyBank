using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class GastoCompartidoDTO
    {
        public int GastoID { get; set; }
        public int TransaccionID { get; set; }
        public int GrupoID { get; set; }
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }
}
