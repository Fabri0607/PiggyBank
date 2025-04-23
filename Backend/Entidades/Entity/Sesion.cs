using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Sesion
    {
        public int SesionID { get; set; }
        public int UsuarioID { get; set; }
        public string TokenJWT { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool EsActivo { get; set; }
        public string MotivoRevocacion { get; set; }
    }
}
