using Backend.Entidades;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Helpers;
using Backend.Logica;
using System.Collections.Generic;
using System.Web.Http;

namespace Backend.Controllers
{
    public class MetaController : ApiController
    {
        private readonly LogicaMeta _logica = new LogicaMeta();
        private readonly LogicaAutenticacion _logicaAutenticacion = new LogicaAutenticacion();

        [HttpPost]
        [Route("api/metas")]
        public ResCrearMeta CrearMeta(ReqCrearMeta req)
        {
            if (req == null)
            {
                return new ResCrearMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida") }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResCrearMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.CrearMeta(req);
        }

        [HttpPost]
        [Route("api/metas/{id}/progreso")]
        public ResActualizarProgresoMeta ActualizarProgresoMeta(int id, ReqActualizarProgresoMeta req)
        {
            if (req == null || req.MetaID != id)
            {
                return new ResActualizarProgresoMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida o ID de meta inconsistente") }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResActualizarProgresoMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.ActualizarProgresoMeta(req);
        }

        [HttpGet]
        [Route("api/metas")]
        public ResListarMetas ListarMetas([FromUri] int usuarioId)
        {
            var req = new ReqListarMetas { UsuarioID = usuarioId };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResListarMetas
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.ListarMetas(req);
        }

        [HttpGet]
        [Route("api/metas/{id}")]
        public ResObtenerDetallesMeta ObtenerDetallesMeta(int id, [FromUri] int usuarioId)
        {
            var req = new ReqObtenerDetallesMeta { MetaID = id, UsuarioID = usuarioId };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResObtenerDetallesMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.ObtenerDetallesMeta(req);
        }

        [HttpPut]
        [Route("api/metas/{id}")]
        public ResActualizarMeta ActualizarMeta(int id, ReqActualizarMeta req)
        {
            if (req == null || req.MetaID != id)
            {
                return new ResActualizarMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida o ID de meta inconsistente") }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResActualizarMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.ActualizarMeta(req);
        }

        [HttpDelete]
        [Route("api/metas/{id}")]
        public ResEliminarMeta EliminarMeta(int id, [FromUri] int usuarioId)
        {
            var req = new ReqEliminarMeta { MetaID = id, UsuarioID = usuarioId };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResEliminarMeta
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.EliminarMeta(req);
        }

        [HttpPost]
        [Route("api/metas/{id}/transacciones")]
        public ResAsignarTransaccion AsignarTransaccion(int id, ReqAsignarTransaccion req)
        {
            if (req == null || req.MetaID != id)
            {
                return new ResAsignarTransaccion
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida o ID de meta inconsistente") }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResAsignarTransaccion
                {
                    resultado = false,
                    error = new List<Error> { HelperValidacion.CrearError(enumErrores.tokenFaltante, "Token de autorización no proporcionado") }
                };
            }

            return _logica.AsignarTransaccion(req);
        }
    }
}