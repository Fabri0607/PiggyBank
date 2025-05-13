using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqVerificarUsuario
    {
        public string Email { get; set; }
        public string CodigoVerificacion { get; set; }
    }
}
