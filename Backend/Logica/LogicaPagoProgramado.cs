using AccesoADatos;
using Backend.Entidades.Entity;
using Backend.Entidades;
using Backend.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.DTO;

namespace Backend.Logica
{
    public class LogicaPagoProgramado
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaPagoProgramado()
        {
            _dbContext = new ConexionLINQDataContext();
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


        public ResIngresarPagoProgramado IngresarPagoProgramado(ReqIngresarPagoProgramado req)
        {
            ResIngresarPagoProgramado res = new ResIngresarPagoProgramado()
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else
                {
                    #region Validaciones

                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioFaltante, "El ID del usuario es requerido y debe ser mayor a 0"));
                    }

                    if (string.IsNullOrEmpty(req.Titulo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.tituloFaltante, "El título es requerido"));
                    }

                    if (req.Monto <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.montoInvalido, "El monto es requerido y debe ser mayor a 0"));
                    }

                    if (req.CategoriaID < 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaFaltante, "El ID de la categoría no puede ser negativo"));
                    }

                    if (req.Fecha_Vencimiento == default(DateTime))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.fechaFaltante, "La fecha de vencimiento es requerida"));
                    }
                    else if (req.Fecha_Vencimiento.Date < DateTime.Now.Date)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.fechaInvalida, "La fecha de vencimiento no puede ser anterior a la fecha actual"));
                    }

                    #endregion

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        int? idBD = 0;
                        int? errorIdBD = 0;
                        string errorMsgBD = "";

                        _dbContext.SP_PAGO_PROGRAMADO_REGISTRAR(
                            req.UsuarioID,
                            req.Titulo,
                            req.Monto,
                            req.Fecha_Vencimiento,
                            req.CategoriaID, 
                            ref idBD,
                            ref errorIdBD,
                            ref errorMsgBD
                        );

                        if (idBD > 0)
                        {
                            res.resultado = true;
                        }
                        else
                        {
                            if (errorIdBD == 2)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, errorMsgBD));
                            }
                            else if (errorIdBD == 3)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.montoInvalido, errorMsgBD));
                            }
                            else if (errorIdBD == 4)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.fechaInvalida, errorMsgBD));
                            }
                            else if (errorIdBD == 5)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, errorMsgBD));
                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos: " + errorMsgBD));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada: " + ex.Message));
            }

            return res;
        }


        public ResPagosPorUsuario ListarPagosPorUsuario(ReqPagosPorUsuario req)
        {
            ResPagosPorUsuario res = new ResPagosPorUsuario()
            {
                error = new List<Error>(),
                Pagos = new List<PagoDTO>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else
                {
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El ID del usuario es requerido y debe ser mayor a 0"));
                    }

                    if (req.FechaInicio.HasValue && req.FechaFin.HasValue && req.FechaInicio > req.FechaFin)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.fechaInvalida, "La fecha de inicio no puede ser posterior a la fecha de fin"));
                    }

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        List<SP_PAGOSPROGRAMADOS_OBTENER_POR_USUARIOResult> listaPagos = new List<SP_PAGOSPROGRAMADOS_OBTENER_POR_USUARIOResult>();
                        listaPagos = _dbContext.SP_PAGOSPROGRAMADOS_OBTENER_POR_USUARIO(
                            req.UsuarioID,
                            req.FechaInicio?.Date,
                            req.FechaFin?.Date
                        ).ToList();

                        if (listaPagos != null && listaPagos.Count > 0)
                        {
                            foreach (var pago in listaPagos)
                            {
                                res.Pagos.Add(new PagoDTO
                                {
                                    PagoID = pago.PagoID,
                                    Titulo = pago.Titulo,
                                    Monto = pago.Monto,
                                    Categoria = pago.Categoria,
                                    Fecha_Vencimiento = pago.Fecha_Vencimiento,
                                    Estado = pago.Estado
                                });
                            }
                            res.resultado = true;
                        }
                        else
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.pagoNoEncontrado, "No se encontraron pagos programados para el usuario"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada: " + ex.Message));
            }

            return res;
        }


        public ResObtenerDetallePago ObtenerDetallePago(ReqObtenerDetallePago req)
        {
            ResObtenerDetallePago res = new ResObtenerDetallePago()
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else
                {
                    #region Validaciones
                    if (req.PagoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.idFaltante, "El ID del pago programado es requerido y debe ser mayor a 0"));
                    }

                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.idFaltante, "El ID del usuario es requerido y debe ser mayor a 0"));
                    }
                    #endregion

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        int? errorIdBD = 0;
                        string errorMsgBD = "";

                        SP_PAGO_PROGRAMADO_OBTENER_DETALLEResult tipoComplejo = new SP_PAGO_PROGRAMADO_OBTENER_DETALLEResult();

                        tipoComplejo = _dbContext.SP_PAGO_PROGRAMADO_OBTENER_DETALLE(
                            req.PagoID,
                            req.UsuarioID,
                            ref errorIdBD,
                            ref errorMsgBD
                        ).ToList().FirstOrDefault();

                        if (tipoComplejo != null && errorIdBD == 0)
                        {
                            res.PagoID = (int)tipoComplejo.PagoID;
                            res.UsuarioID = (int)tipoComplejo.UsuarioID;
                            res.Titulo = tipoComplejo.Titulo;
                            res.Monto = (decimal)tipoComplejo.Monto;
                            res.NombreCategoria = tipoComplejo.NombreCategoria;
                            res.Fecha_Vencimiento = (DateTime)tipoComplejo.Fecha_Vencimiento;
                            res.Estado = tipoComplejo.Estado;
                            res.CategoriaID = tipoComplejo.CategoriaID ?? 0;
                            res.resultado = true;
                        }
                        else
                        {
                            if (errorIdBD == 404)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.pagoNoEncontrado, errorMsgBD));
                            }
                            else if (errorIdBD == 403)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, errorMsgBD));
                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos: " + errorMsgBD));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada: " + ex.Message));
            }

            return res;
        }


        public ResActualizarPago ActualizarPago(ReqActualizarPago req)
        {
            ResActualizarPago res = new ResActualizarPago()
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else if (req.PagoID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de pago programado válido"));
                }
                else if (req.UsuarioID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de usuario válido"));
                }
                else
                {
                    #region Validaciones
                    if (string.IsNullOrEmpty(req.Titulo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El título es requerido"));
                    }

                    if (req.Monto < 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto no puede ser negativo"));
                    }

                    if (req.Fecha_Vencimiento == default(DateTime))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "La fecha de vencimiento es requerida"));
                    }
                    else if (req.Fecha_Vencimiento.Date < DateTime.Now.Date)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha de vencimiento no puede ser anterior a la fecha actual"));
                    }

                    if (string.IsNullOrEmpty(req.Estado))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El estado es requerido"));
                    }
                    else if (req.Estado != "Pendiente" && req.Estado != "Pagado")
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El estado debe ser 'Pendiente' o 'Pagado'"));
                    }

                    if (req.CategoriaID < 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El ID de la categoría no puede ser negativo"));
                    }
                    #endregion

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        int? errorIdBD = 0;
                        string errorMsgBD = "";

                        _dbContext.SP_PAGO_PROGRAMADO_ACTUALIZAR(
                            req.PagoID,
                            req.UsuarioID,
                            req.Titulo,
                            req.Monto,
                            req.Fecha_Vencimiento,
                            req.Estado,
                            req.CategoriaID,
                            ref errorIdBD,
                            ref errorMsgBD
                        );

                        if (errorIdBD == 0)
                        {
                            res.resultado = true;
                        }
                        else
                        {
                            if (errorIdBD == 404)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.pagoNoEncontrado, errorMsgBD));
                            }
                            else if (errorIdBD == 403)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, errorMsgBD));
                            }
                            else if (errorIdBD == 400)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, errorMsgBD));
                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos: " + errorMsgBD));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada: " + ex.Message));
            }

            return res;
        }


        public ResEliminarPago EliminarPago(ReqEliminarPago req)
        {
            ResEliminarPago res = new ResEliminarPago()
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else if (req.PagoID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de pago programado válido"));
                }
                else if (req.UsuarioID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de usuario válido"));
                }
                else
                {
                    int? errorIdBD = 0;
                    string errorMsgBD = "";

                    _dbContext.SP_PAGO_PROGRAMADO_ELIMINAR(
                        req.PagoID,
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
                        if (errorIdBD == 404)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.pagoNoEncontrado, errorMsgBD));
                        }
                        else if (errorIdBD == 403)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, errorMsgBD));
                        }
                        else
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos: " + errorMsgBD));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada: " + ex.Message));
            }

            return res;
        }
    }
}
