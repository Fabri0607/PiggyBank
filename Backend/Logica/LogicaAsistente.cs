using AccesoADatos;
using Backend.DTO;
using Backend.Entidades;
using Backend.Entidades.Entity;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;


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
        //Obtener transacciones
        private ResObtenerTransacciones ObtenerTransacciones(ReqObtenerTransacciones req)

        {
            ResObtenerTransacciones res = new ResObtenerTransacciones
            {
                error = new List<Error>(),
                Transacciones = new List<TransaccionDTO>()
            };
            List<Error> errores = new List<Error>();
            try
            {
                if(!ValidarSesion(req, ref errores))
                {
                    res.resultado = false;
                    res.error = errores;
                    return res;
                }
                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.sesion == null)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.sesionInvalida, "La sesión no es válida"));
                    }
                    else
                    {
                        if(req.UsuarioID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario Inválido"));
                        }
                        if (!(String.IsNullOrEmpty(req.Tipo) || req.Tipo=="Gasto" || req.Tipo == "Ingreso"))
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.TipoTransaccionInvalido, "El tipo de transacción no es válido"));
                        }
                        // Si ambas fechas están presentes, validar el rango
                        if (req.FechaInicio.HasValue && req.FechaFin.HasValue && req.FechaInicio > req.FechaFin)
                        {
                                res.error.Add(HelperValidacion.CrearError(enumErrores.FechaInvalida, "La fecha de inicio no puede ser mayor que la fecha de fin"));
                        }

                    }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                
                var Transacciones = _dbContext.SP_TRANSACCIONES_OBTENER_POR_USUARIO(req.UsuarioID, req.FechaInicio, req.FechaFin, req.Tipo)
                    .Select( b=>new TransaccionDTO
                    {
                        Tipo = b.Tipo,
                        TransaccionID = b.TransaccionID,
                        Monto = b.Monto,
                        Fecha = b.Fecha,
                        Titulo = b.Titulo,
                        Descripcion = b.Descripcion,
                        Categoria = b.Categoria

                    }).ToList();
                res.Transacciones = Transacciones;
                res.resultado = true;
            }
            catch (Exception ex) {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al obtener las transacciones: {ex.Message}"));
            }
            return res;
        }
        private ResCrearAnalisis CrearAnalisis(ReqCrearAnalisis req)
        {
            throw new NotImplementedException();
        }

        private ResInsertarMensajeChat InsertarMensajeChat(ReqInsertarMensajeChat req)
        {
            throw new NotImplementedException();
        }
        private ResObtenerMensajes ObtenerMensajesChat(ReqObtenerMensajes req)
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
