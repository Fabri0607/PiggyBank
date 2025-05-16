using Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerTodosContexto : ResBase
    {
        public List<ContextoDTO> Contextos { get; set; } // Lista de contextos del usuario
    }
}
