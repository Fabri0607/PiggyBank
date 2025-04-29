using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.DTO;

namespace Backend.Entidades.Response
{
    public class ResObtenerUsuario : ResBase
    {
        public UsuarioDTO Usuario { get; set; }
    }
}
