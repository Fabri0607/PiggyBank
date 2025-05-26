using Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerDetallesGrupo : ResBase
    {
        public GrupoDTO Grupo { get; set; }
        public List<MiembroDTO> Miembros { get; set; }
    }
}
