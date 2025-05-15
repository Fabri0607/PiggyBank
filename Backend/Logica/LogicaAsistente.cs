using AccesoADatos;
using Backend.Entidades;
using Backend.Entidades.Entity;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Backend.Logica
{
    public class LogicaAsistente
    {
        private readonly ConexionLINQDataContext _dbContext;
        private readonly string LLM_Api_key; 
        public LogicaAsistente() 
        {
            _dbContext = new ConexionLINQDataContext();
            LLM_Api_key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        }

        private bool ValidarSesion(ReqBase req, ref List<Error> errores)
        {
            try
            {
                // Validar que el token no sea nulo o vacío
                if (string.IsNullOrEmpty(req.token))
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.tokenFaltante, "El token de autorización es requerido"));
                    return false;
                }

                // Extraer el GUID del JWT
                string guid;
                try
                {
                    guid = HelperJWT.ValidarTokenYObtenerGuid(req.token);
                }
                catch (SecurityTokenException ex)
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.tokenInvalido, ex.Message));
                    return false;
                }

                // Consultar la sesión en la base de datos
                int? errorIdBD = 0;
                string errorMsgBD = "";
                var sesion = _dbContext.SP_SESION_OBTENER_POR_GUID(guid, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

                if (errorIdBD != 0)
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    return false;
                }

                if (sesion == null)
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.sesionNoEncontrada, "Sesión no encontrada"));
                    return false;
                }

                // Verificar si la sesión está activa
                if (!sesion.EsActivo)
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.sesionInactiva, "La sesión no está activa"));
                    return false;
                }

                // Verificar si la sesión ha expirado
                if (sesion.FechaExpiracion < DateTime.Now)
                {
                    errores.Add(HelperValidacion.CrearError(enumErrores.sesionExpirada, "La sesión ha expirado"));
                    return false;
                }

                // Asignar la sesión al request
                req.sesion = new Sesion
                {
                    SesionID = sesion.SesionID,
                    UsuarioID = sesion.UsuarioID,
                    Guid = sesion.Guid,
                    FechaCreacion = sesion.FechaCreacion,
                    FechaExpiracion = sesion.FechaExpiracion,
                    EsActivo = sesion.EsActivo,
                    MotivoRevocacion = sesion.MotivoRevocacion
                };

                return true;
            }
            catch (Exception ex)
            {
                errores.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al validar la sesión: {ex.Message}"));
                return false;
            }
        }

        private ResObtenerTransacciones ObtenerTransacciones(ReqObtenerTransacciones req)

        {
            throw new NotImplementedException();
        }
        //actualizar resumen
        //crearanalisis
        //insertarmensaje
        //obtener contexto x id
        //obtener todos contextos
        //obtener transacciones
        //obtener mensajes
        //obtener analisis usuario




        private ResInsertarMensajeChat InsertarMensajeChat(ReqInsertarMensajeChat req)
        {
            throw new NotImplementedException();
        }
        private ResObtenerMensajes ObtenerMensajesChat(ReqObtenerMensajes req)
        {
            throw new NotImplementedException();
        }

        private ResCrearAnalisis CrearAnalisis(ReqCrearAnalisis req)
        {
            throw new NotImplementedException();
        }
        private ResObtenerAnalisisUsuario ObtenerAnalisis(ReqObtenerAnalisisUsuario req)
        {
            throw new NotImplementedException();
        }
        
        private ResActualizarResumen ActualizarAnalisis(ReqActualizarResumen req)
        {
            throw new NotImplementedException();
        }



    } 
}
