using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqActualizarPerfil : ReqBase
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string ConfiguracionNotificaciones { get; set; }
    }
}
