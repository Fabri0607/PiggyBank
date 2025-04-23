using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class GrupoFamiliar
    {
        public int GrupoID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CreadoPorUsuarioID { get; set; }
    }
}
