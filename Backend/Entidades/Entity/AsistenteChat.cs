using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class AsistenteChat
    {
        public int ChatID { get; set; }
        public int UsuarioID { get; set; }
        public string Consulta { get; set; }
        public string Respuesta { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string ContextoJSON { get; set; }
    }
}
