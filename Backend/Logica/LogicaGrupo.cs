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
using Microsoft.IdentityModel.Tokens;
using Backend.DTO;

namespace Backend.Logica
{
    public class LogicaGrupo
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaGrupo()
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

        // 1. Crear grupo familiar
        public ResCrearGrupoFamiliar CrearGrupo(ReqCrearGrupoFamiliar req)
        {
            ResCrearGrupoFamiliar res = new ResCrearGrupoFamiliar
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {

                // Validar la sesión
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

            List<Error> errores = new List<Error>();

            try
            {

                // Validar la sesión
                if (!ValidarSesion(req, ref errores))
                {
                    res.error = errores;
                    res.resultado = false;
                    return res;
                }

                // Consultar usuario para verificar credenciales
                int? errorIdBD = 0;
                string errorMsgBD = "";
                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_EMAIL(req.correoUsuario, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

                if (errorIdBD != 0)
                {
                    if (errorIdBD == 400)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, errorMsgBD));
                    }
                    else
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    }
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
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
                    }
                    if (usuario.UsuarioID <= 0)
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

                int? errorIdBD1 = 0;
                string errorMsgBD1 = "";

                _dbContext.SP_GRUPO_INVITAR_MIEMBRO(req.GrupoID, usuario.UsuarioID, req.Rol, ref errorIdBD1, ref errorMsgBD1);

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

            List<Error> errores = new List<Error>();

            try
            {

                // Validar la sesión
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
                    if (req.CategoriaID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.categoriaNoEncontrada, "Categoría inválida"));
                    }
                    if (!string.IsNullOrEmpty(req.Descripcion) && req.Descripcion.Length > 1000)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.valorInvalido, "La descripción es demasiado larga"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                int? gastoID = 0;
                int? transaccionID = 0;
                int? errorIdBD = 0;
                string errorMsgBD = "";

                // Llamar al stored procedure
                _dbContext.SP_GASTO_COMPARTIDO_REGISTRAR(
                    req.GrupoID,
                    req.UsuarioID,
                    req.Monto,
                    req.Estado,
                    req.CategoriaID,
                    req.Descripcion,
                    ref gastoID,
                    ref transaccionID,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (gastoID > 0)
                {
                    res.GastoID = gastoID.Value;
                    res.TransaccionID = transaccionID.Value;
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

        // 4. Obtener balance grupal
        public ResObtenerBalanceGrupal ObtenerBalanceGrupal(ReqObtenerBalanceGrupal req)
        {
            ResObtenerBalanceGrupal res = new ResObtenerBalanceGrupal
            {
                error = new List<Error>(),
                Balances = new List<BalanceMiembro>()
            };

            List<Error> errores = new List<Error>();

            try
            {

                // Validar la sesión
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

        // 5. Salir de grupo
        public ResSalirGrupo SalirGrupo(ReqSalirGrupo req)
        {
            ResSalirGrupo res = new ResSalirGrupo
            {
                error = new List<Error>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar la sesión
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
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
                    }
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario inválido"));
                    }
                    // Validar que el usuario del token coincide con el UsuarioID
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
                _dbContext.SP_GRUPO_SALIR_MIEMBRO(
                    req.GrupoID,
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

        // 6. Listar Grupos por Usuario
        public ResListarGrupos ListarGrupos(ReqListarGrupos req)
        {
            ResListarGrupos res = new ResListarGrupos
            {
                error = new List<Error>(),
                Grupos = new List<GrupoDTO>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar la sesión
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

                var grupos = _dbContext.SP_GRUPO_LISTAR_POR_USUARIO(req.UsuarioID, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.Grupos = grupos.Select(g => new GrupoDTO
                    {
                        GrupoID = g.GrupoID,
                        Nombre = g.Nombre,
                        Descripcion = g.Descripcion,
                        FechaCreacion = g.FechaCreacion,
                        CreadoPorUsuarioID = g.CreadoPorUsuarioID,
                        Estado = g.Estado,
                        FechaActualizacion = g.FechaActualizacion,
                        Rol = g.Rol
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

        // 7. Obtener detalles de grupo
        /*
        public ResObtenerDetallesGrupo ObtenerDetallesGrupo(ReqObtenerDetallesGrupo req)
        {
            ResObtenerDetallesGrupo res = new ResObtenerDetallesGrupo
            {
                error = new List<Error>(),
                Miembros = new List<MiembroDTO>()
            };

            List<Error> errores = new List<Error>();

            try
            {
                // Validar la sesión
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
                    if (req.GrupoID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo inválido"));
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

                var grupoResult = _dbContext.SP_GRUPO_OBTENER_DETALLES(req.GrupoID, req.UsuarioID, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    var grupo = grupoResult.FirstOrDefault();
                    if (grupo != null)
                    {
                        res.Grupo = new GrupoDTO
                        {
                            GrupoID = grupo.GrupoID,
                            Nombre = grupo.Nombre,
                            Descripcion = grupo.Descripcion,
                            FechaCreacion = grupo.FechaCreacion,
                            CreadoPorUsuarioID = grupo.CreadoPorUsuarioID,
                            Estado = grupo.Estado,
                            FechaActualizacion = grupo.FechaActualizacion
                        };

                        res.Miembros = grupoResult.Select(m => new MiembroDTO
                        {
                            UsuarioID = m.UsuarioID,
                            NombreUsuario = m.NombreUsuario,
                            Rol = m.Rol,
                            FechaUnion = m.FechaUnion
                        }).Where(m => m.UsuarioID > 0).ToList(); // Filtrar solo miembros
                        res.resultado = true;
                    }
                    else
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.grupoNoEncontrado, "Grupo no encontrado"));
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
        */
    }

}
