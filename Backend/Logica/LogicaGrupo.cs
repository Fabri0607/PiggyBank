using AccesoADatos;
using Backend.Entidades.Entity;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Entidades;
using Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Logica
{
    public class LogicaGrupo
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaGrupo()
        {
            _dbContext = new ConexionLINQDataContext();
        }

        // 1. Crear grupo familiar
        public ResCrearGrupoFamiliar CrearGrupo(ReqCrearGrupoFamiliar req)
        {
            ResCrearGrupoFamiliar res = new ResCrearGrupoFamiliar
            {
                error = new List<Error>()
            };

            try
            {
                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (!HelperValidacion.EsTextoValido(req.Nombre, 100))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.campoRequerido, "El nombre del grupo es requerido o excede la longitud máxima"));
                    }
                    if (req.Descripcion != null && req.Descripcion.Length > 1000)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La descripción excede la longitud máxima"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? grupoID = 0;
                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_GRUPO_CREAR(req.Nombre, req.Descripcion, req.UsuarioID, ref grupoID, ref errorIdBD, ref errorMsgBD);

                if (grupoID > 0)
                {
                    res.GrupoID = grupoID.Value;
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error al crear el grupo"));
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

        // 2. Invitar miembro a grupo
        public ResInvitarMiembroGrupo InvitarMiembro(ReqInvitarMiembroGrupo req)
        {
            ResInvitarMiembroGrupo res = new ResInvitarMiembroGrupo
            {
                error = new List<Error>()
            };

            try
            {
                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (!HelperValidacion.EsRolValido(req.Rol))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.rolInvalido, "Rol inválido"));
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

                _dbContext.SP_GRUPO_INVITAR_MIEMBRO(req.GrupoID, req.UsuarioID, req.Rol, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error al invitar al miembro"));
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

        // 3. Registrar gasto compartido
        public ResRegistrarGastoCompartido RegistrarGastoCompartido(ReqRegistrarGastoCompartido req)
        {
            ResRegistrarGastoCompartido res = new ResRegistrarGastoCompartido
            {
                error = new List<Error>()
            };

            try
            {
                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.TransaccionID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.transaccionNoEncontrada, "Transacción inválida"));
                    }
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    if (!HelperValidacion.EsDecimalValido(req.Monto))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "Monto inválido"));
                    }
                    if (!HelperValidacion.EsEstadoValido(req.Estado))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.estadoInvalido, "Estado inválido"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? gastoID = 0;
                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_GASTO_COMPARTIDO_REGISTRAR(req.TransaccionID, req.GrupoID, req.UsuarioID, req.Monto, req.Estado, ref gastoID, ref errorIdBD, ref errorMsgBD);

                if (gastoID > 0)
                {
                    res.GastoID = gastoID.Value;
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error al registrar el gasto compartido"));
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

        // 4. Obtener balance grupal
        public ResObtenerBalanceGrupal ObtenerBalanceGrupal(ReqObtenerBalanceGrupal req)
        {
            ResObtenerBalanceGrupal res = new ResObtenerBalanceGrupal
            {
                error = new List<Error>(),
                Balances = new List<BalanceMiembro>()
            };

            try
            {
                #region Validaciones
                if (req == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.requestNulo, "Solicitud no válida"));
                }
                else
                {
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
                    }
                    if (req.FechaInicio.HasValue && req.FechaFin.HasValue && req.FechaInicio > req.FechaFin)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La fecha de inicio no puede ser mayor a la fecha de fin"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Calcular balances primero
                int? errorIdBD = 0;
                string errorMsgBD = "";
                _dbContext.SP_BALANCE_CALCULAR_REGISTRAR(req.GrupoID, req.FechaInicio, req.FechaFin, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, "Error al calcular los balances"));
                    res.resultado = false;
                    return res;
                }

                // Obtener balances calculados
                var balances = _dbContext.SP_BALANCE_OBTENER_POR_GRUPO(req.GrupoID, req.FechaInicio, req.FechaFin)
                    .Select(b => new BalanceMiembro
                    {
                        BalanceID = b.BalanceID,
                        GrupoID = b.GrupoID,
                        UsuarioID = b.UsuarioID,
                        NombreUsuario = b.NombreUsuario,
                        TotalGastos = b.TotalGastos,
                        TotalPagado = b.TotalPagado,
                        Saldo = b.Saldo,
                        FechaCalculo = b.FechaCalculo
                    }).ToList();

                res.Balances = balances;
                res.resultado = true;
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
