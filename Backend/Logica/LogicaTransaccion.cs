using AccesoADatos;
using Backend.DTO;
using Backend.Entidades;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Helpers;
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

        public ResIngresarTransaccion IngresarTransaccion(ReqIngresarTransaccion req)
        {
            ResIngresarTransaccion res = new ResIngresarTransaccion()
            {
                error = new List<Error>()
            };

            try
            {
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else
                {
                    #region Validaciones
                    if (req.Transaccion.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.idFaltante, "El ID del usuario es requerido y debe ser mayor a 0"));
                    }

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

            try
            {
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
