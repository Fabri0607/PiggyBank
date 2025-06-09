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
using System.Text;
using System.Threading.Tasks;

namespace Backend.Logica
{
    public class LogicaTransaccion
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaTransaccion()
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

        public ResIngresarTransaccion IngresarTransaccion(ReqIngresarTransaccion req)
        {
            ResIngresarTransaccion res = new ResIngresarTransaccion()
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

                    if (string.IsNullOrEmpty(req.Transaccion.Tipo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.tipoTransaccionFaltante, "El tipo de transacción es requerido"));
                    }
                    else if (req.Transaccion.Tipo != "Ingreso" && req.Transaccion.Tipo != "Gasto")
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.tipoTransaccionInvalido, "El tipo de transacción debe ser 'Ingreso' o 'Gasto'"));
                    }

                    if (req.Transaccion.Monto <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.montoInvalido, "El monto es requerido y debe ser mayor a 0"));
                    }

                    if (req.Transaccion.CategoriaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaFaltante, "El ID de la categoría es requerido y debe ser mayor a 0"));
                    }

                    if (req.Transaccion.Fecha == default(DateTime))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.fechaFaltante, "La fecha es requerida"));
                    }
                    else if (req.Transaccion.Fecha > DateTime.Now)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.fechaInvalida, "La fecha no puede ser futura"));
                    }

                    if (string.IsNullOrEmpty(req.Transaccion.Titulo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.tituloFaltante, "El título es requerido"));
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

                        _dbContext.SP_TRANSACCION_REGISTRAR(
                            req.Transaccion.UsuarioID,
                            req.Transaccion.Tipo,
                            req.Transaccion.Monto,
                            req.Transaccion.CategoriaID,
                            req.Transaccion.Fecha,
                            req.Transaccion.Descripcion,
                            req.Transaccion.EsCompartido,
                            req.Transaccion.GrupoID,
                            req.Transaccion.Titulo,
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
                                res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, errorMsgBD));
                            }
                            else if (errorIdBD == 4)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, errorMsgBD));
                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada"));
            }

            return res;
        }


        public ResTransaccionesPorUsuario ListarTransaccionesPorUsuario(ReqTransaccionesPorUsuario req)
        {
            ResTransaccionesPorUsuario res = new ResTransaccionesPorUsuario()
            {
                error = new List<Error>(),
                transacciones = new List<TransaccionDTO>()
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

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        List<SP_TRANSACCIONES_OBTENER_POR_USUARIOResult> listaTC = new List<SP_TRANSACCIONES_OBTENER_POR_USUARIOResult>();
                        listaTC = _dbContext.SP_TRANSACCIONES_OBTENER_POR_USUARIO(req.UsuarioID, req.FechaInicio, req.FechaFin, req.TipoTransaccion).ToList();

                        if (listaTC != null && listaTC.Count > 0)
                        {
                            foreach (var tc in listaTC)
                            {
                                res.transacciones.Add(FactoriaTransaccion(tc));
                            }
                            res.resultado = true;
                        }
                        else
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, "No se encontraron transacciones para el usuario"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada"));
            }

            return res;
        }


        public ResObtenerDetalleTransaccion ObtenerDetalleTransaccion(ReqObtenerDetalleTransaccion req)
        {
            ResObtenerDetalleTransaccion res = new ResObtenerDetalleTransaccion()
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
                    if (req.TransaccionID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.idFaltante, "El ID de la transacción es requerido y debe ser mayor a 0"));
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

                        SP_TRANSACCION_OBTENER_DETALLEResult tipoComplejo = new SP_TRANSACCION_OBTENER_DETALLEResult();

                        tipoComplejo = _dbContext.SP_TRANSACCION_OBTENER_DETALLE(
                            req.TransaccionID,
                            req.UsuarioID,
                            ref errorIdBD,
                            ref errorMsgBD
                        ).ToList().FirstOrDefault();

                        if (tipoComplejo != null && errorIdBD == 0)
                        {
                            res.TransaccionID = (int)tipoComplejo.TransaccionID;
                            res.UsuarioID = (int)tipoComplejo.UsuarioID;
                            res.Tipo = tipoComplejo.Tipo;
                            res.Monto = (decimal)tipoComplejo.Monto;
                            res.Categoria = tipoComplejo.NombreCategoria;
                            res.Fecha = (DateTime)tipoComplejo.Fecha;
                            res.Titulo = tipoComplejo.Titulo;
                            res.Descripcion = tipoComplejo.Descripcion;
                            res.EsCompartido = (bool)tipoComplejo.EsCompartido;
                            res.GrupoID = tipoComplejo.GrupoID;
                            res.resultado = true;
                        }
                        else
                        {
                            if (errorIdBD == 404)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, errorMsgBD));
                            }
                            else if (errorIdBD == 403)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, errorMsgBD));
                            }
                            else
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada"));
            }

            return res;
        }


        public ResActualizarTransaccion ActualizarTransaccion(ReqActualizarTransaccion req)
        {
            ResActualizarTransaccion res = new ResActualizarTransaccion()
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
                else if (req.TransaccionID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de transacción válido"));
                }
                else if (req.UsuarioID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de usuario válido"));
                }
                else
                {
                    #region Validaciones
                    if (string.IsNullOrEmpty(req.Tipo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El tipo de transacción es requerido"));
                    }
                    else if (req.Tipo != "Ingreso" && req.Tipo != "Gasto")
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El tipo de transacción debe ser 'Ingreso' o 'Gasto'"));
                    }

                    if (req.Monto <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El monto debe ser mayor a 0"));
                    }

                    if (req.CategoriaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El ID de la categoría debe ser mayor a 0"));
                    }

                    if (req.Fecha == default(DateTime))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "La fecha es requerida"));
                    }
                    else if (req.Fecha > DateTime.Now)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha no puede ser futura"));
                    }

                    if (string.IsNullOrEmpty(req.Titulo))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El título es requerido"));
                    }

                    if (req.EsCompartido && req.GrupoID == null)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "El ID del grupo es requerido para una transacción compartida"));
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

                        _dbContext.SP_TRANSACCION_ACTUALIZAR(
                            req.TransaccionID,
                            req.UsuarioID,
                            req.Tipo,
                            req.Monto,
                            req.CategoriaID,
                            req.Fecha,
                            req.Titulo,
                            req.Descripcion,
                            req.EsCompartido,
                            req.GrupoID,
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
                                res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, errorMsgBD));
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
                                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada"));
            }

            return res;
        }


        public ResEliminarTransaccion EliminarTransaccion(ReqEliminarTransaccion req)
        {
            ResEliminarTransaccion res = new ResEliminarTransaccion()
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
                else if (req.TransaccionID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de transacción válido"));
                }
                else if (req.UsuarioID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un ID de usuario válido"));
                }
                else
                {
                    int? errorIdBD = 0;
                    string errorMsgBD = "";

                    _dbContext.SP_TRANSACCION_ELIMINAR(
                        req.TransaccionID,
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
                            res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, errorMsgBD));
                        }
                        else if (errorIdBD == 403)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.permisoDenegado, errorMsgBD));
                        }
                        else
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error en la base de datos"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepción no controlada"));
            }

            return res;
        }


        #region Metodos Auxiliares

        private TransaccionDTO FactoriaTransaccion(SP_TRANSACCIONES_OBTENER_POR_USUARIOResult tc)
        {
            TransaccionDTO transaccionDTO = new TransaccionDTO();

            transaccionDTO.TransaccionID = tc.TransaccionID;
            transaccionDTO.Tipo = tc.Tipo;
            transaccionDTO.Monto = tc.Monto;
            transaccionDTO.Fecha = tc.Fecha;
            transaccionDTO.Titulo = tc.Titulo;
            transaccionDTO.Descripcion = tc.Descripcion;
            transaccionDTO.Categoria = tc.Categoria;
            transaccionDTO.Icono = tc.Icono;
            transaccionDTO.ColorHex = tc.ColorHex;

            return transaccionDTO;
        }

        #endregion
    }
}
