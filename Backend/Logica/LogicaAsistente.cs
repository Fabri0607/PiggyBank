﻿using AccesoADatos;
using Backend.DTO;
using Backend.Entidades;
using Backend.Entidades.Entity;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Helpers;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;


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

        public async Task<ResCrearAnalisis> CrearAnalisisAsync(ReqCrearAnalisis req)
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

                // Crear el análisis en la base de datos
                int? AnalisisID = 0;
                int? errorIDDB = 0;
                string errorMsgDB = "";
                var Analisis = _dbContext.SP_CREAR_ANALISIS(req.sesion.UsuarioID ,req.FechaInicio,
                req.FechaFin, req.ContextoID,ref AnalisisID, ref errorIDDB, ref errorMsgDB);
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
                //obtener el contexto
                var contexto = _dbContext.SP_OBTENER_CONTEXTO_POR_ID(req.ContextoID)
                    .Select(b => new IA_Contexto{Instruccion = b.INSTRUCCION,ContextoID = b.CONTEXTOID, Modelo = b.MODELO}).FirstOrDefault();
                if (contexto != null)
                {
                    //obtener las transacciones
                    
                    errorIDDB = 0;
                    errorMsgDB = "";
                    var transacciones = _dbContext.SP_TRANSACCIONES_OBTENER_POR_USUARIO(req.sesion.UsuarioID, req.FechaInicio,
                    req.FechaFin, null).Select(b => new TransaccionDTO
                    {
                        Tipo = b.Tipo,
                        TransaccionID = b.TransaccionID,
                        Monto = b.Monto,
                        Fecha = b.Fecha,
                        Titulo = b.Titulo,
                        Descripcion = b.Descripcion,
                        Categoria = b.Categoria
                    }).ToList();

                    decimal TotalGastos = transacciones.Where(t => t.Tipo == "Gasto").ToList().Sum(m => m.Monto);
                    decimal TotalEntradas = transacciones.Where(t => t.Tipo == "Ingreso").ToList().Sum(m => m.Monto);


                    //crear el llamado al API

                    ClienteLlm _clienteLlm = new ClienteLlm(LLM_Api_key, contexto.Modelo);

                    List<MensajeChat> mensajes = new List<MensajeChat>();
                    
                    mensajes.Add(new MensajeChat
                    {
                        Role = "user",
                        Content = req.Consulta,
                        FechaEnvio = DateTime.Now,
                        Orden = 1
                    });
                    var usuario = _dbContext.SP_OBTENER_USUARIO_POR_ID(req.sesion.UsuarioID);
                    string NombreUsuario = usuario.Select(b => new UsuarioDTO { Nombre = b.Nombre}).FirstOrDefault().Nombre;

                    string Json = _clienteLlm.GenerarJSON(contexto.Instruccion, NombreUsuario, TotalGastos, TotalEntradas, transacciones, mensajes);
                    Console.WriteLine(Json);
                    RespuestaDTO Resultado = await _clienteLlm.GenerarRespuestaAsync(Json);

                    if (Resultado != null)// Crear el mensaje del usuario en la base de datos
                    {
                        int? MensajeID = 0;
                        errorIDDB = 0;
                        errorMsgDB = "";
                        var mensaje = _dbContext.SP_INSERTAR_MENSAJE_CHAT(AnalisisID,"user",req.Consulta, ref MensajeID, ref errorIDDB, ref errorMsgDB);
                        if (errorIDDB != 0)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                        }
                        else
                        {
                            // Crear el mensaje de respuesta en la base de datos
                            int? MensajeIDRespuesta = 0;
                            var mensajeRespuesta = _dbContext.SP_INSERTAR_MENSAJE_CHAT(AnalisisID, "assistant", Resultado.Recomendaciones, ref MensajeIDRespuesta, ref errorIDDB, ref errorMsgDB);
                            if (errorIDDB != 0)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                            }
                            else
                            {
                                errorIDDB = 0;
                                errorMsgDB = "";
                                _dbContext.SP_ACTUALIZAR_RESUMEN(AnalisisID, Resultado.Resumen, ref errorIDDB, ref errorMsgDB);
                                if (errorIDDB != 0)
                                {
                                    res.resultado = false;
                                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                                }
                                else
                                {
                                    res.resultado = true;
                                    res.AnalisisID = AnalisisID ?? 0;
                                }
                            }
                        }

                    }
                    else
                    {
                         res.resultado = false;
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se pudo obtener la respuesta del modelo"));
                    }
                }
                else
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se encontró el contexto"));
                }

            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al crear el análisis: {ex.Message}"));
            }
            return res;
        }

        

        public  async Task<ResInsertarMensajeChat> InsertarMensajeChat(ReqInsertarMensajeChat req)
        {
            ResInsertarMensajeChat res = new ResInsertarMensajeChat
            {
                error = new List<Error>(),
                MensajeConsultaID = 0,
                MensajeRespuestaID = 0
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

                    }
                }
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }
                //formar el texto del request al llm

                AnalisisIA analisis = _dbContext.SP_OBTENER_ANALISIS_ID(req.AnalisisID).
                 Select(
                 a => new AnalisisIA
                 {
                     Contexto = a.ContextoID,
                     Resumen = a.Resumen,
                     UsuarioID = a.UsuarioID,
                     FechaInicio = a.FechaInicio,
                     FechaFin = a.FechaFin,
                     AnalisisID = a.AnalisisID,
                 }).FirstOrDefault();
                if (analisis == null)
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se encontró el análisis"));
                    return res;
                }
                else
                {
                    var contexto = _dbContext.SP_OBTENER_CONTEXTO_POR_ID(analisis.Contexto).
                       Select(b => new IA_Contexto { Instruccion = b.INSTRUCCION, ContextoID = b.CONTEXTOID, Modelo = b.MODELO }).FirstOrDefault();
                    if (contexto != null)
                    {
                        //nombre del usuario
                        var usuario = _dbContext.SP_OBTENER_USUARIO_POR_ID(analisis.UsuarioID);
                        string NombreUsuario = usuario.Select(b => new UsuarioDTO { Nombre = b.Nombre }).FirstOrDefault().Nombre;
                        if (usuario != null)
                        {
                            //obtener las transacciones
                            var transacciones = _dbContext.SP_TRANSACCIONES_OBTENER_POR_USUARIO(req.sesion.UsuarioID, analisis.FechaInicio,
                                 analisis.FechaFin, null).Select(b => new TransaccionDTO
                                 {
                                     Tipo = b.Tipo,
                                     TransaccionID = b.TransaccionID,
                                     Monto = b.Monto,
                                     Fecha = b.Fecha,
                                     Titulo = b.Titulo,
                                     Descripcion = b.Descripcion,
                                     Categoria = b.Categoria
                                 }).ToList();
                            decimal TotalGastos = transacciones.Where(t => t.Tipo == "Gasto").ToList().Sum(m => m.Monto);
                            decimal TotalEntradas = transacciones.Where(t => t.Tipo == "Ingreso").ToList().Sum(m => m.Monto);

                            // Obtener los mensajes anteriores
                            var mensajes = _dbContext.SP_OBTENER_MENSAJES(analisis.AnalisisID)
                                .Select(b => new MensajeChat
                                {
                                    MensajeID = b.MensajeID,
                                    AnalisisID = b.AnalisisID,
                                    Role = b.Role,
                                    Content = b.Content,
                                    FechaEnvio = b.FechaEnvio,
                                    Orden = b.Orden
                                }).ToList();
                            //obtener el mensaje del usuario
                            string mensaje = req.Content;
                            mensajes.Add(new MensajeChat
                            {
                                Role = "user",
                                Content = mensaje,
                                FechaEnvio = DateTime.Now,
                                Orden = 1
                            });
                            //formar el json
                            ClienteLlm _clienteLlm = new ClienteLlm(LLM_Api_key, contexto.Modelo);
                            string json = _clienteLlm.GenerarJSON(contexto.Instruccion, NombreUsuario, TotalGastos, TotalEntradas, transacciones, mensajes);
                            Console.WriteLine(json);
                            RespuestaDTO resultado =  await _clienteLlm.GenerarRespuestaAsync(json);

                            if (resultado != null)
                            {
                                // Crear el mensaje del usuario en la base de datos
                                int? MensajeID = 0;
                                int? errorIDDB = 0;
                                string errorMsgDB = "";
                                var mensajeChat = _dbContext.SP_INSERTAR_MENSAJE_CHAT(analisis.AnalisisID, "user", mensaje, ref MensajeID, ref errorIDDB, ref errorMsgDB);
                                if (errorIDDB != 0)
                                {
                                    res.resultado = false;
                                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                                }
                                else
                                {
                                    
                                    // Crear el mensaje de respuesta en la base de datos
                                    int? MensajeIDRespuesta = 0;
                                    errorIDDB = 0;
                                    errorMsgDB = "";
                                    var mensajeRespuesta = _dbContext.SP_INSERTAR_MENSAJE_CHAT(analisis.AnalisisID, "assistant", resultado.Recomendaciones, ref MensajeIDRespuesta, ref errorIDDB, ref errorMsgDB);
                                    if (errorIDDB != 0)
                                    {
                                        res.resultado = false;
                                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                                    }
                                    else
                                    {
                                        
                                        // Actualizar el resumen en la base de datos
                                        errorIDDB = 0;
                                        errorMsgDB = "";
                                        _dbContext.SP_ACTUALIZAR_RESUMEN(analisis.AnalisisID, resultado.Resumen, ref errorIDDB, ref errorMsgDB);
                                        if (errorIDDB != 0)
                                        {
                                            res.resultado = false;
                                            res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgDB));
                                        }
                                        else
                                        {
                                            res.resultado = true;
                                            res.MensajeConsultaID = MensajeID ?? 0;
                                            res.MensajeRespuestaID = MensajeIDRespuesta ?? 0;
                                        }

                                    }
                                }

                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se pudo obtener la respuesta del modelo"));
                            }

                        }
                        else
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se encontró el usuario"));
                        }
                    }
                    else
                    {
                        res.resultado = false;
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "No se encontró el contexto"));
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al insertar el mensaje: {ex.Message}"));
            }
            return res;
        }

        public ResObtenerMensajes ObtenerMensajesChat(ReqObtenerMensajes req)
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
                        if (req.AnalisisID <= 0)
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


        public ResObtenerAnalisisUsuario ObtenerAnalisis(ReqObtenerAnalisisUsuario req)
        {
            ResObtenerAnalisisUsuario res = new ResObtenerAnalisisUsuario
            {  
                error = new List<Error>(),
                AnalisisIA = new List<AnalisisDTO>()
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
                    if (req.sesion.UsuarioID <= 0)
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
                var analisis = _dbContext.SP_OBTENER_ANALISIS_USUARIO(req.sesion.UsuarioID)
                    .Select(b => new AnalisisDTO
                    {
                        AnalisisID = b.AnalisisID,
                        Contexto = b.ContextoID,
                        FechaInicio = b.FechaInicio,
                        FechaFin = b.FechaFin,
                        FechaGeneracion = b.FechaGeneracion,
                        Resumen = b.Resumen,
                    }).ToList();
                res.AnalisisIA = analisis;
                res.resultado = true;


            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al obtener el análisis: {ex.Message}"));
            }

            return res;
        }
        public ResObtenerTodosContexto ObtenerContextos(ReqObtenerTodosContexto req)
        {
            ResObtenerTodosContexto res = new ResObtenerTodosContexto
            {
                error = new List<Error>(),
                Contextos = new List<ContextoDTO>()
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
                #endregion
                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                var contextos = _dbContext.SP_OBTENER_TODOS_CONTEXTO()
                    .Select(b => new ContextoDTO
                    {
                        ContextoID = b.CONTEXTOID,
                        FechaCreacion = b.FECHACREACION,
                        Nombre = b.NOMBRE
                    }).ToList();
                res.Contextos = contextos;
                res.resultado = true; 
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, $"Error al obtener los contextos: {ex.Message}"));
            }
            return res;
        }
    } 
    
}
