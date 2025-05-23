using Backend.Entidades;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace APIRest.Controllers
{

    public class AsistenteController : ApiController

    {
        private readonly LogicaAsistente _logica = new LogicaAsistente();
        private readonly LogicaAutenticacion _logicaAutenticacion = new LogicaAutenticacion();
        // POST: /api/asistentes
        [HttpGet]
        [Route("api/asistente/contextos")]
        public ResObtenerTodosContexto GetContextos(ReqObtenerTodosContexto req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResObtenerTodosContexto
                {
                    resultado = false,
                    error = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = (int)enumErrores.tokenFaltante,
                        Message = "Token de autorización no proporcionado"
                    }
                }
                };
            }
            return _logica.ObtenerContextos(req);
        }

        [HttpGet]
        [Route("api/asistente/Mensajes/{id}")]
        public ResObtenerMensajes GetMensajes(int id)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                var req = new ReqObtenerMensajes
                {
                    token = authHeader.Parameter,
                    AnalisisID = id
                };
                return _logica.ObtenerMensajesChat(req);
            }
            else
            {
                return new ResObtenerMensajes
                {
                    resultado = false,
                    error = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = (int)enumErrores.tokenFaltante,
                        Message = "Token de autorización no proporcionado"
                    }
                }
                };
            }
        }
        [HttpGet]
        [Route("api/asistente/analisis/")]
        public ResObtenerAnalisisUsuario GetAnalisis()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                var req = new ReqObtenerAnalisisUsuario
                {
                    token = authHeader.Parameter
                };
                return _logica.ObtenerAnalisis(req);
            }
            else
            {
                return new ResObtenerAnalisisUsuario
                {
                    resultado = false,
                    error = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = (int)enumErrores.tokenFaltante,
                        Message = "Token de autorización no proporcionado"
                    }
                }
                };
            }
        }

        
        [HttpPost]
        [Route("api/asistente/analisis/")]
        public async Task<ResCrearAnalisis> CrearAnalisis(ReqCrearAnalisis req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResCrearAnalisis
                {
                    resultado = false,
                    error = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = (int)enumErrores.tokenFaltante,
                        Message = "Token de autorización no proporcionado"
                    }
                }
                };
            }
            return await _logica.CrearAnalisisAsync(req);
        }

        //arreglar tmb
        [HttpPost]
        [Route("api/asistente/analisis/{id}/mensaje")]
        public async Task<ResInsertarMensajeChat> InsertarMensaje(int id, ReqInsertarMensajeChat req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
                req.AnalisisID = id;
            }
            else
            {
                return new ResInsertarMensajeChat
                {
                    resultado = false,
                    error = new List<Error>
                {
                    new Error
                    {
                        ErrorCode = (int)enumErrores.tokenFaltante,
                        Message = "Token de autorización no proporcionado"
                    }
                }
                };
            }
            return await _logica.InsertarMensajeChat(req);
        }

    }
}
