using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace APIRest.Controllers
{
    public class CategoriaController : ApiController
    {
        private readonly LogicaCategoria _logica = new LogicaCategoria();


        [HttpPost]
        [Route("api/categorias/crear")]
        public ResCrearCategoria Crear(ReqCrearCategoria req)
        {
            return _logica.CrearCategoria(req);
        }

    }
}
