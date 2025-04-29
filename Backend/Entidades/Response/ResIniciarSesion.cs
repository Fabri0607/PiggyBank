using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.DTO;

namespace Backend.Entidades.Response
{
    public class ResIniciarSesion : ResBase
    {
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public int SesionID { get; set; }
        public UsuarioDTO Usuario { get; set; }
    }
}
