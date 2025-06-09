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

        [HttpPost]
        [Route("api/transaccion/transaccionesPorUsuario")]
        public ResTransaccionesPorUsuario ObtenerTransaccionesPorUusario(ReqTransaccionesPorUsuario req)
        {
            return _logica.ListarTransaccionesPorUsuario(req);
        }

        [HttpPost]
        [Route("api/transaccion/obtenerDetalle")]
        public ResObtenerDetalleTransaccion ObtenerDetalleTransaccion(ReqObtenerDetalleTransaccion req)
        {
            return _logica.ObtenerDetalleTransaccion(req);
        }

        [HttpPut]
        [Route("api/transaccion/actualizar")]
        public ResActualizarTransaccion ActualizarTransaccion(ReqActualizarTransaccion req)
        {
            return _logica.ActualizarTransaccion(req);
        }

        [HttpPost]
        [Route("api/transaccion/eliminar")]
        public ResEliminarTransaccion EliminarTransaccion(ReqEliminarTransaccion req)
        {
            return _logica.EliminarTransaccion(req);
        }
    }
}
