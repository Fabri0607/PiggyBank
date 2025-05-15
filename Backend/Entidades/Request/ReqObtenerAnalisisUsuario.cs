using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqObtenerAnalisisUsuario : ReqBase
    {
        public int UsuarioID { get; set; } // ID del usuario para el que se obtienen los análisis
    }
}
