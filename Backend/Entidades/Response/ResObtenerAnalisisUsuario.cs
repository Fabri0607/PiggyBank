using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Backend.Entidades.Entity;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerAnalisisUsuario : ResBase
    {
        public List<AnalisisIA> AnalisisIA { get; set; } // Lista de análisis por usuario
       
    }
}
