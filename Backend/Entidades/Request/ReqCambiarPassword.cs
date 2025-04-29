using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqCambiarPassword : ReqBase
    {
        public int UsuarioID { get; set; }
        public string PasswordActual { get; set; }
        public string NuevoPassword { get; set; }
    }
}
