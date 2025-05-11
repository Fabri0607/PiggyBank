using AccesoADatos;
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
                if(req == null)
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
                            if(errorIdBD == 1)
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
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Excepcion no controlada"));
            }
            return res;
        }


    }
}
