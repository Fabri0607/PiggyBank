﻿using Backend.Entidades;
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
    public class UsuarioController : ApiController
    {
        private readonly LogicaUsuario _logica = new LogicaUsuario();

        [HttpPost]
        [Route("api/usuarios/registrar")]
        public ResRegistrarUsuario Registrar(ReqRegistrarUsuario req)
        {
            return _logica.RegistrarUsuario(req);
        }

        [HttpPost]
        [Route("api/usuarios/verificar")]
        public ResVerificarUsuario Verificar(ReqVerificarUsuario req)
        {
            return _logica.VerificarUsuario(req);
        }

        [HttpPost]
        [Route("api/usuarios/iniciar-sesion")]
        public ResIniciarSesion IniciarSesion(ReqIniciarSesion req)
        {
            return _logica.IniciarSesion(req);
        }

        [HttpPost]
        [Route("api/usuarios/cerrar-sesion")]
        public ResCerrarSesion CerrarSesion(ReqCerrarSesion req)
        {

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResCerrarSesion
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

            return _logica.CerrarSesion(req);
        }

        [HttpPost]
        [Route("api/usuarios/actualizar-perfil")]
        public ResActualizarPerfil ActualizarPerfil(ReqActualizarPerfil req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResActualizarPerfil
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

            return _logica.ActualizarPerfil(req);
        }

        [HttpPost]
        [Route("api/usuarios/cambiar-password")]
        public ResCambiarPassword CambiarPassword(ReqCambiarPassword req)
        {

            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && authHeader.Scheme == "Bearer")
            {
                req.token = authHeader.Parameter;
            }
            else
            {
                return new ResCambiarPassword
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

            return _logica.CambiarPassword(req);
        }

        [HttpPost]
        [Route("api/usuarios/reenviar-codigo")]
        [AllowAnonymous]
        public ResReenviarCodigoVerificacion ReenviarCodigo(ReqReenviarCodigoVerificacion req)
        {
            return _logica.ReenviarCodigoVerificacion(req);
        }

        [HttpPost]
        [Route("api/usuarios/obtener")]
        public ResObtenerUsuario ObtenerUsuario(ReqObtenerUsuario req)
        {
            return _logica.ObtenerUsuario(req);
        }

        [HttpPost]
        [Route("api/usuarios/solicitar-cambio-password")]
        [AllowAnonymous]
        public ResSolicitarCambioPassword SolicitarCambioPassword(ReqSolicitarCambioPassword req)
        {
            return _logica.SolicitarCambioPassword(req);
        }

        [HttpPost]
        [Route("api/usuarios/confirmar-cambio-password")]
        [AllowAnonymous]
        public ResConfirmarCambioPassword ConfirmarCambioPassword(ReqConfirmarCambioPassword req)
        {
            return _logica.ConfirmarCambioPassword(req);
        }
    }
}
