using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Entidades;
using Backend.Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;

namespace APIRest.Controllers
{
    public class GrupoController : ApiController
    {
        private readonly LogicaGrupo _logica = new LogicaGrupo();
        private readonly LogicaAutenticacion _logicaAutenticacion = new LogicaAutenticacion();

        // POST: /api/grupos
        [HttpPost]
        [Route("api/grupos")]
        public ResCrearGrupoFamiliar CrearGrupo(ReqCrearGrupoFamiliar req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResCrearGrupoFamiliar
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
            return _logica.CrearGrupo(req);
        }

        // POST: /api/grupos/{id}/miembros
        [HttpPost]
        [Route("api/grupos/{id}/miembros")]
        public ResInvitarMiembroGrupo InvitarMiembro(int id, ReqInvitarMiembroGrupo req)
        {
            if (req == null || id != req.GrupoID)
            {
                return new ResInvitarMiembroGrupo
                {
                    resultado = false,
                    error = new System.Collections.Generic.List<Error>
                    {
                        new Error
                        {
                            ErrorCode = (int)enumErrores.requestNulo,
                            Message = "Solicitud no válida o ID de grupo inconsistente"
                        }
                    }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResInvitarMiembroGrupo
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

            return _logica.InvitarMiembro(req);
        }

        // POST: /api/grupos/{id}/gastos
        [HttpPost]
        [Route("api/grupos/{id}/gastos")]
        public ResRegistrarGastoCompartido RegistrarGastoCompartido(int id, ReqRegistrarGastoCompartido req)
        {
            if (req == null || id != req.GrupoID)
            {
                return new ResRegistrarGastoCompartido
                {
                    resultado = false,
                    error = new System.Collections.Generic.List<Error>
                    {
                        new Error
                        {
                            ErrorCode = (int)enumErrores.requestNulo,
                            Message = "Solicitud no válida o ID de grupo inconsistente"
                        }
                    }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResRegistrarGastoCompartido
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

            return _logica.RegistrarGastoCompartido(req);
        }

        // GET: /api/grupos/{id}/balances
        [HttpGet]
        [Route("api/grupos/{id}/balances")]
        public ResObtenerBalanceGrupal ObtenerBalanceGrupal(int id, [FromUri] DateTime? fechaInicio = null, [FromUri] DateTime? fechaFin = null)
        {
            var req = new ReqObtenerBalanceGrupal
            {
                GrupoID = id,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResObtenerBalanceGrupal
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

            return _logica.ObtenerBalanceGrupal(req);
        }

        [HttpPost]
        [Route("api/grupos/{id}/salir")]
        public ResSalirGrupo SalirGrupo(int id, ReqSalirGrupo req)
        {
            if (req == null || id != req.GrupoID)
            {
                return new ResSalirGrupo
                {
                    resultado = false,
                    error = new List<Error>
            {
                new Error
                {
                    ErrorCode = (int)enumErrores.requestNulo,
                    Message = "Solicitud no válida o ID de grupo inconsistente"
                }
            }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResSalirGrupo
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

            return _logica.SalirGrupo(req);
        }

        [HttpGet]
        [Route("api/grupos")]
        public ResListarGrupos ListarGrupos([FromUri] int usuarioId)
        {
            var req = new ReqListarGrupos
            {
                UsuarioID = usuarioId
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResListarGrupos
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

            return _logica.ListarGrupos(req);
        }

        [HttpGet]
        [Route("api/grupos/{id}")]
        public ResObtenerDetallesGrupo ObtenerDetallesGrupo(int id, [FromUri] int usuarioId)
        {
            var req = new ReqObtenerDetallesGrupo
            {
                GrupoID = id,
                UsuarioID = usuarioId
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResObtenerDetallesGrupo
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

            return _logica.ObtenerDetallesGrupo(req);
        }

        [HttpDelete]
        [Route("api/grupos/{id}/miembros/{memberId}")]
        public ResEliminarMiembro EliminarMiembro(int id, int memberId, [FromUri] int adminUsuarioId)
        {
            var req = new ReqEliminarMiembro
            {
                GrupoID = id,
                UsuarioID = memberId,
                AdminUsuarioID = adminUsuarioId
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResEliminarMiembro
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

            return _logica.EliminarMiembro(req);
        }

        [HttpPut]
        [Route("api/grupos/{id}")]
        public ResActualizarGrupo ActualizarGrupo(int id, ReqActualizarGrupo req)
        {
            if (req == null || id != req.GrupoID)
            {
                return new ResActualizarGrupo
                {
                    resultado = false,
                    error = new List<Error>
            {
                new Error
                {
                    ErrorCode = (int)enumErrores.requestNulo,
                    Message = "Solicitud no válida o ID de grupo inconsistente"
                }
            }
                };
            }

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResActualizarGrupo
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

            return _logica.ActualizarGrupo(req);
        }

        [HttpGet]
        [Route("api/grupos/{id}/gastos")]
        public ResListarGastos ListarGastos(int id, [FromUri] int usuarioId, [FromUri] DateTime? fechaInicio = null, [FromUri] DateTime? fechaFin = null)
        {
            var req = new ReqListarGastos
            {
                GrupoID = id,
                UsuarioID = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResListarGastos
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

            return _logica.ListarGastos(req);
        }

        [HttpDelete]
        [Route("api/grupos/{id}")]
        public ResEliminarGrupo EliminarGrupo(int id, [FromUri] int adminUsuarioId)
        {
            var req = new ReqEliminarGrupo
            {
                GrupoID = id,
                AdminUsuarioID = adminUsuarioId
            };

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResEliminarGrupo
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

            return _logica.EliminarGrupo(req);
        }

    }
}
