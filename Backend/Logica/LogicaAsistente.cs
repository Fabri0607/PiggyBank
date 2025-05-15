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
            ResCrearAnalisis res = new ResCrearAnalisis
            {
                error = new List<Error>(),
                AnalisisID = 0
            };
            List<Error> errores = new List<Error>();
            try {
                    if (!ValidarSesion(req, ref errores))
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
                        if (req.UsuarioID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario Inválido"));
                        }
                        if (req.FechaInicio.HasValue && req.FechaFin.HasValue && req.FechaInicio > req.FechaFin)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.FechaInvalida, "La fecha de inicio no puede ser mayor que la fecha de fin"));
                        }
                        if (req.ContextoID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El ID del contexto es requerido"));
                        }
                    }
                }

                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                int? AnalisisID = 0;
                int? errorIDDB = 0;
                string errorMsgDB = "";
                var analisis = _dbContext.SP_CREAR_ANALISIS(req.UsuarioID, req.FechaInicio, req.FechaFin, req.ContextoID, ref AnalisisID, ref errorIDDB, ref errorMsgDB);
                if (errorIDDB != 0)
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                }
                else
                {
                    res.AnalisisID = AnalisisID ?? 0;
                    res.resultado = true;
                }

            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al crear el análisis: {ex.Message}"));
            }

            return res;
        }

        private ResInsertarMensajeChat InsertarMensajeChat(ReqInsertarMensajeChat req)
        {
            ResInsertarMensajeChat res = new ResInsertarMensajeChat
            {
                error = new List<Error>(),
                MensajeID = 0
            };
            List<Error> errores = new List<Error>();
            try
            {
                if (!ValidarSesion(req, ref errores))
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
                        if (req.AnalisisID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El ID del análisis es requerido"));
                        }
                        if (string.IsNullOrEmpty(req.Role))
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El rol es requerido"));
                        }
                        
                    }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                int? MensajeID = 0;
                int? errorIDDB = 0;
                string errorMsgDB = "";
                var mensaje = _dbContext.SP_INSERTAR_MENSAJE_CHAT(req.AnalisisID, req.Role, req.Content, ref MensajeID, ref errorIDDB, ref errorMsgDB);
                if (errorIDDB != 0)
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                }
                else
                {
                    res.MensajeID = MensajeID ?? 0;
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al insertar el mensaje: {ex.Message}"));
            }
            return res;
        }
        private ResObtenerMensajes ObtenerMensajesChat(ReqObtenerMensajes req)
        {
            ResObtenerMensajes res = new ResObtenerMensajes
            {
                error = new List<Error>(),
                MensajesChat = new List<MensajeChat>()
            };
            List<Error> errores = new List<Error>();
            try
            {
                if (!ValidarSesion(req, ref errores))
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
                        if(req.AnalisisID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El ID del análisis es requerido"));
                        }
                    }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                var mensajes = _dbContext.SP_OBTENER_MENSAJES(req.AnalisisID)
                    .Select(b => new MensajeChat
                    {
                        MensajeID = b.MensajeID,
                        AnalisisID = b.AnalisisID,
                        Role = b.Role,
                        Content = b.Content,
                        FechaEnvio = b.FechaEnvio,
                        Orden = b.Orden
                    }).ToList();
                res.MensajesChat = mensajes;
                res.resultado = true;
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al obtener los mensajes: {ex.Message}"));
            }
            return res;

        }

        
        private ResObtenerAnalisisUsuario ObtenerAnalisis(ReqObtenerAnalisisUsuario req)
        {
            ResObtenerAnalisisUsuario res = new ResObtenerAnalisisUsuario
            {
                error = new List<Error>(),
                AnalisisIA = new List<AnalisisIA>()
            };
            List<Error> errores = new List<Error>();
            try {
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
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El Usuario es requerido"));
                    }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                var analisis = _dbContext.SP_OBTENER_ANALISIS_USUARIO(req.UsuarioID)
                    .Select(b => new AnalisisIA
                    {
                        AnalisisID = b.AnalisisID,
                        UsuarioID = b.UsuarioID,
                        Contexto = b.ContextoID,
                        FechaInicio = b.FechaInicio,
                        FechaFin = b.FechaFin,
                        FechaGeneracion = b.FechaGeneracion,
                        Resumen = b.Resumen,
                    }).ToList();
                res.AnalisisIA = analisis;


            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al obtener el análisis: {ex.Message}"));
            }

            return res;
        }
        
        private ResActualizarResumen ActualizarAnalisis(ReqActualizarResumen req)
        {
            ResActualizarResumen res = new ResActualizarResumen
            {
                error = new List<Error>()
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
                    
                        if (req.AnalisisID <= 0)
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El ID del análisis es requerido"));
                        }
                        if (string.IsNullOrEmpty(req.Resumen))
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El resumen es requerido"));
                        }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                int? errorIDDB = 0;
                string errorMsgDB = "";
                var analisis = _dbContext.SP_ACTUALIZAR_RESUMEN(req.AnalisisID, req.Resumen, ref errorIDDB, ref errorMsgDB);
                if (errorIDDB != 0)
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                }
                else
                {
                    res.resultado = true;
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al actualizar el análisis: {ex.Message}"));
            }
            return res;
        }
    } 
}
