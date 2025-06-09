using AccesoADatos;
using Backend.Entidades;
using Backend.Entidades.Entity;
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
    public class LogicaCategoria
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaCategoria()
        {
            _dbContext = new ConexionLINQDataContext();
        }

        public ResCrearCategoria CrearCategoria(ReqCrearCategoria req)
        {
            ResCrearCategoria res = new ResCrearCategoria()
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
                    if (String.IsNullOrEmpty(req.Nombre))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El campo Nombre es requerido"));
                    }

                    // Fin de validaciones

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        int? idBD = 0;
                        int? errorIdBD = 0;
                        string errorMsgBD = "";

                        _dbContext.SP_INGRESAR_CATEGORIA(
                            req.Nombre,
                            req.Icono,
                            req.ColorHex,
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
                            if (errorIdBD == 1)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaRepetida, errorMsgBD));
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
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada" + ex.Message));
            }
            return res;
        }


        public ResActualizarCategoria ActualizarCategoria(int CategoriaId, ReqActualizarCategoria req)
        {
            ResActualizarCategoria res = new ResActualizarCategoria()
            {
                error = new List<Error>()
            };
            try
            {
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "El objeto request es nulo"));
                }
                else if (CategoriaId <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un Id valido"));
                }
                else
                {
                    if (String.IsNullOrEmpty(req.Nombre))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El campo Nombre es requerido"));
                    }

                    // Fin de validaciones

                    if (res.error.Any())
                    {
                        res.resultado = false;
                    }
                    else
                    {
                        int? idBD = 0;
                        int? errorIdBD = 0;
                        string errorMsgBD = "";
                        _dbContext.SP_ACTUALIZAR_CATEGORIA(
                            CategoriaId,
                            req.Nombre,
                            req.Icono,
                            req.ColorHex,
                            ref errorIdBD,
                            ref errorMsgBD
                        );
                        if (errorIdBD == 0)
                        {
                            res.resultado = true;
                        }
                        else
                        {
                            if (errorIdBD == 1)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, errorMsgBD));
                            }
                            else if (errorIdBD == 2)
                            {
                                res.resultado = false;
                                res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaRepetida, errorMsgBD));
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
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada" + ex.Message));
            }
            return res;
        }


        public ResEliminarCategoria EliminarCategoria(int CategoriaId)
        {
            ResEliminarCategoria res = new ResEliminarCategoria()
            {
                error = new List<Error>()
            };
            try
            {
                if (CategoriaId <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Debe ingresar un Id valido"));
                }
                else
                {
                    int? errorIdBD = 0;
                    string errorMsgBD = "";
                    _dbContext.SP_BORRAR_CATEGORIA(
                        CategoriaId,
                        ref errorIdBD,
                        ref errorMsgBD
                    );
                    if (errorIdBD == 0)
                    {
                        res.resultado = true;
                    }
                    else
                    {
                        if (errorIdBD == 1)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, errorMsgBD));
                        }
                        else if (errorIdBD == 2)
                        {
                            res.resultado = false;
                            res.error.Add(HelperValidacion.CrearError(enumErrores.errorAlBorrarPorDependencias, errorMsgBD));
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
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada" + ex.Message));
            }
            return res;
        }


        public ResObtenerCategorias ObtenerCategorias()
        {
            ResObtenerCategorias res = new ResObtenerCategorias()
            {
                error = new List<Error>(),
                categorias = new List<Categoria>()
            };
            try
            {
                var categoriasBD = _dbContext.SP_OBTENER_CATEGORIAS().ToList();
                if (categoriasBD != null && categoriasBD.Any())
                {
                    res.categorias = categoriasBD.Select(c => new Categoria
                    {
                        CategoriaID = c.CategoriaID,
                        Nombre = c.Nombre,
                        Icono = c.Icono,
                        ColorHex = c.ColorHex
                    }).ToList();
                    res.resultado = true;
                }
                else
                {
                    res.resultado = false;
                    res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, "No se encontraron categorias"));
                }
            }
            catch (Exception ex)
            {
                res.resultado = false;
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada" + ex.Message));
            }
            return res;
        }
    }
}
