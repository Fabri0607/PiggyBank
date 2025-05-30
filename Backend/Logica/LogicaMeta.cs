using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entidades.Entity;
using Backend.Entidades;
using Backend.Helpers;
using AccesoADatos;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Data;

namespace Backend.Logica
{
    public class LogicaMeta
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaMeta()
        {
            _dbContext = new ConexionLINQDataContext();
        }

        private decimal CalcularAhorroMensualSugerido(decimal montoObjetivo, decimal montoActual, DateTime fechaInicio, DateTime? fechaObjetivo)
        {
            if (!fechaObjetivo.HasValue || fechaObjetivo.Value <= DateTime.Now)
                return 0;

            var mesesRestantes = (fechaObjetivo.Value.Year - DateTime.Now.Year) * 12 + fechaObjetivo.Value.Month - DateTime.Now.Month;
            if (mesesRestantes <= 0)
                return 0;

            var montoFaltante = montoObjetivo - montoActual;
            return montoFaltante > 0 ? montoFaltante / mesesRestantes : 0;
        }

        // Validar Sesión
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

        // Crear Meta
        public ResCrearMeta CrearMeta(ReqCrearMeta req)
        {
            ResCrearMeta res = new ResCrearMeta
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
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
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (!HelperValidacion.EsTextoValido(req.Nombre, 100))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El nombre es obligatorio o excede la longitud máxima"));
                    }
                    else if (req.MontoObjetivo <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto objetivo debe ser mayor a cero"));
                    }
                    if (req.FechaInicio.Date < DateTime.Now.Date)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha de inicio no puede ser anterior a hoy"));
                    }
                    if (req.FechaObjetivo.HasValue && req.FechaObjetivo.Value <= req.FechaInicio)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha objetivo debe ser posterior a la fecha de inicio"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";
                int? metaID = 0;

                _dbContext.SP_META_CREAR(
                    req.UsuarioID,
                    req.Nombre,
                    req.MontoObjetivo,
                    req.FechaInicio,
                    req.FechaObjetivo,
                    ref metaID,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (errorIdBD == 0)
                {
                    res.MetaID = (int)metaID;
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
            }

            return res;
        }

        // Actualizar Progreso
        public ResActualizarProgresoMeta ActualizarProgresoMeta(ReqActualizarProgresoMeta req)
        {
            ResActualizarProgresoMeta res = new ResActualizarProgresoMeta
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.MetaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta inválida"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (req.MontoActual < 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto actual no puede ser negativo"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_META_ACTUALIZAR_PROGRESO(
                    req.MetaID,
                    req.MontoActual,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        // Listar Metas
        public ResListarMetas ListarMetas(ReqListarMetas req)
        {
            ResListarMetas res = new ResListarMetas
            {
                error = new List<Error>(),
                Metas = new List<MetaDTO>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
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
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                var metas = _dbContext.SP_META_LISTAR_POR_USUARIO(req.UsuarioID, ref errorIdBD, ref errorMsgBD).ToList();

                if (errorIdBD == 0)
                {
                    res.Metas = metas.Select(m => new MetaDTO
                    {
                        MetaID = m.MetaID,
                        UsuarioID = m.UsuarioID,
                        Nombre = m.Nombre,
                        MontoObjetivo = m.MontoObjetivo,
                        MontoActual = m.MontoActual,
                        FechaInicio = m.FechaInicio,
                        FechaObjetivo = m.FechaObjetivo,
                        Completada = m.Completada,
                        Progreso = m.MontoObjetivo > 0 ? (m.MontoActual / m.MontoObjetivo) * 100 : 0,
                        AhorroMensualSugerido = CalcularAhorroMensualSugerido(m.MontoObjetivo, m.MontoActual, m.FechaInicio, m.FechaObjetivo)
                    }).ToList();
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        public ResObtenerDetallesMeta ObtenerDetallesMeta(ReqObtenerDetallesMeta req)
        {
            ResObtenerDetallesMeta res = new ResObtenerDetallesMeta
            {
                error = new List<Error>(),
                Transacciones = new List<MetaTransaccionDTO>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.MetaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta inválida"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                // Llamar al stored procedure
                var detallesResult = _dbContext.SP_META_OBTENER_DETALLES(req.MetaID, req.UsuarioID, ref errorIdBD, ref errorMsgBD).ToList();

                if (errorIdBD == 0)
                {
                    // Procesar los resultados
                    var metaGroup = detallesResult.FirstOrDefault();
                    if (metaGroup != null)
                    {
                        res.Meta = new MetaDTO
                        {
                            MetaID = (int)metaGroup.MetaID,
                            UsuarioID = (int)metaGroup.UsuarioID,
                            Nombre = metaGroup.Nombre,
                            MontoObjetivo = (decimal)metaGroup.MontoObjetivo,
                            MontoActual = (decimal)metaGroup.MontoActual,
                            FechaInicio = (DateTime)metaGroup.FechaInicio,
                            FechaObjetivo = metaGroup.FechaObjetivo,
                            Completada = (bool)metaGroup.Completada,
                            Progreso = (decimal)(metaGroup.MontoObjetivo > 0 ? (metaGroup.MontoActual / metaGroup.MontoObjetivo) * 100 : 0),
                            AhorroMensualSugerido = CalcularAhorroMensualSugerido((decimal)metaGroup.MontoObjetivo, (decimal)metaGroup.MontoActual, (DateTime)metaGroup.FechaInicio, metaGroup.FechaObjetivo)
                        };

                        // Filtrar transacciones válidas (donde TransaccionID no sea NULL)
                        res.Transacciones = detallesResult
                            .Where(d => d.TransaccionID.HasValue)
                            .Select(d => new MetaTransaccionDTO
                            {
                                TransaccionID = d.TransaccionID.Value,
                                MontoAsignado = d.MontoAsignado.Value,
                                FechaAsignacion = d.FechaAsignacion.Value,
                                Descripcion = d.Descripcion ?? "",
                                MontoTransaccion = d.MontoTransaccion.Value
                            }).ToList();

                        res.resultado = true;
                    }
                    else
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta no encontrada"));
                        res.resultado = false;
                    }
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        // Actualizar Meta
        public ResActualizarMeta ActualizarMeta(ReqActualizarMeta req)
        {
            ResActualizarMeta res = new ResActualizarMeta
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.MetaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta inválida"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (!HelperValidacion.EsTextoValido(req.Nombre, 100))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El nombre es obligatorio o excede la longitud máxima"));
                    }
                    if (req.MontoObjetivo <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto objetivo debe ser mayor a cero"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                    if (req.FechaObjetivo.HasValue && req.FechaObjetivo.Value < DateTime.Now.Date)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha objetivo no puede ser anterior a hoy"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_META_ACTUALIZAR(
                    req.MetaID,
                    req.UsuarioID,
                    req.Nombre,
                    req.MontoObjetivo,
                    req.FechaObjetivo,
                    ref errorIdBD,
                    ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        // Eliminar Meta
        public ResEliminarMeta EliminarMeta(ReqEliminarMeta req)
        {
            ResEliminarMeta res = new ResEliminarMeta
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.MetaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta inválida"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_META_ELIMINAR(
                    req.MetaID,
                    req.UsuarioID,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        // Asignar Transacción
        public ResAsignarTransaccion AsignarTransaccion(ReqAsignarTransaccion req)
        {
            ResAsignarTransaccion res = new ResAsignarTransaccion
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.MetaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.metaNoEncontrada, "Meta inválida"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (req.TransaccionID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, "Transacción inválida"));
                    }
                    if (req.MontoAsignado <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto asignado debe ser mayor a cero"));
                    }
                    if (req.sesion.UsuarioID != req.UsuarioID)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, "No tienes permiso para realizar esta acción"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_META_ASIGNAR_TRANSACCION(
                    req.MetaID,
                    req.UsuarioID,
                    req.TransaccionID,
                    req.MontoAsignado,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError((enumErrores)errorIdBD, errorMsgBD));
                    res.resultado = false;
                }
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }


    }
}