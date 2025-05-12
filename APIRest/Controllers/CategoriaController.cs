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
        [Route("api/categoria/crear")]
        public ResCrearCategoria Crear(ReqCrearCategoria req)
        {
            return _logica.CrearCategoria(req);
        }

        [HttpPut]
        [Route("api/categoria/actualizar/{categoriaId}")]
        public ResActualizarCategoria ActualizarCategoria(int categoriaId, [FromBody] ReqActualizarCategoria req)
        {
            return _logica.ActualizarCategoria(categoriaId, req);
        }

        [HttpDelete]
        [Route("api/categoria/eliminar/{categoriaId}")]
        public ResEliminarCategoria EliminarCategoria(int categoriaId)
        {
            return _logica.EliminarCategoria(categoriaId);
        }

    }
}
