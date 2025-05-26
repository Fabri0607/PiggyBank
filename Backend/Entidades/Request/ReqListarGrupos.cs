using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqListarGrupos : ReqBase
    {
        public int UsuarioID { get; set; }
    }
}