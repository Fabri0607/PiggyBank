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
    public class TransaccionController : ApiController
    {
        private readonly LogicaTransaccion _logica = new LogicaTransaccion();


        [HttpPost]
        [Route("api/transaccion/ingresar")]
        public ResIngresarTransaccion IngresarTransaccion(ReqIngresarTransaccion req)
        {
            return _logica.IngresarTransaccion(req);
        }

        [HttpGet]
        [Route("api/transaccion/transaccionesPorUsuario")]
        public ResTransaccionesPorUsuario ObtenerTransaccionesPorUusario(ReqTransaccionesPorUsuario req)
        {
            return _logica.ListarTransaccionesPorUsuario(req);
        }
    }
}
