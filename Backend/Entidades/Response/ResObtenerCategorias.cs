using Backend.Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerCategorias: ResBase
    {
        public List<Categoria> categorias { get; set; }
    }
}
