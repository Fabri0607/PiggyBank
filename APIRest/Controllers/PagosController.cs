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
    public class PagosController : ApiController
    {
        private readonly LogicaPagoProgramado _logica = new LogicaPagoProgramado();

        [HttpPost]
        [Route("api/pagos/ingresar")]
        public ResIngresarPagoProgramado IngresarPagoProgramado(ReqIngresarPagoProgramado req)
        {
            return _logica.IngresarPagoProgramado(req);
        }

        [HttpPost]
        [Route("api/pagos/listarPorUsuario")]
        public ResPagosPorUsuario ListarPagosPorUsuario(ReqPagosPorUsuario req)
        {
            return _logica.ListarPagosPorUsuario(req);
        }

        [HttpPost]
        [Route("api/pagos/obtenerDetalle")]
        public ResObtenerDetallePago ObtenerDetallePagoProgramado(ReqObtenerDetallePago req)
        {
            return _logica.ObtenerDetallePago(req);
        }

        [HttpPut]
        [Route("api/pagos/actualizar")]
        public ResActualizarPago ActualizarPagoProgramado(ReqActualizarPago req)
        {
            return _logica.ActualizarPago(req);
        }

        [HttpPost]
        [Route("api/pagos/eliminar")]
        public ResEliminarPago EliminarPagoProgramado(ReqEliminarPago req)
        {
            return _logica.EliminarPago(req);
        }
    }
}
