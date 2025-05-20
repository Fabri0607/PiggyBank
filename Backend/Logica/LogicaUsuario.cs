using AccesoADatos;
using Backend.Entidades.Entity;
using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Entidades;
using Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Backend.DTO;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Logica
{
    public class LogicaUsuario
    {
        private readonly ConexionLINQDataContext _dbContext;

        public LogicaUsuario()
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

        // 1. Registrar usuario
        public ResRegistrarUsuario RegistrarUsuario(ReqRegistrarUsuario req)
        {
            ResRegistrarUsuario res = new ResRegistrarUsuario
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
                    if (string.IsNullOrEmpty(req.Nombre))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.nombreFaltante, "El nombre es requerido"));
                    }
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    else if (!EsCorreoValido(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoIncorrecto, "El formato del correo electrónico no es válido"));
                    }
                    if (string.IsNullOrEmpty(req.Password))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordFaltante, "La contraseña es requerida"));
                    }
                    else if (!EsPasswordSeguro(req.Password))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordMuyDebil, "La contraseña no cumple con los requisitos de seguridad"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Generar llave única y hash de contraseña
                string llaveUnica = Guid.NewGuid().ToString("N");
                string passwordHash = HashearPassword(req.Password, llaveUnica);
                string codigoVerificacion = GenerarCodigoVerificacion(6);

                int? usuarioID = 0;
                int? errorIdBD = 0;
                string errorMsgBD = "";

                // Llamar al stored procedure para insertar usuario
                _dbContext.SP_INGRESAR_USUARIO(
                    req.Nombre,
                    req.Email,
                    llaveUnica,
                    passwordHash,
                    codigoVerificacion,
                    ref usuarioID,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (usuarioID > 0)
                {
                    // Enviar correo de verificación
                    bool correoEnviado = HelperCorreo.EnviarCorreoVerificacion(req.Email, req.Nombre, codigoVerificacion);

                    if (correoEnviado)
                    {
                        res.UsuarioID = usuarioID.Value;
                        res.resultado = true;
                    }
                    else
                    {
                        // El usuario se creó pero hubo problema al enviar el correo
                        res.error.Add(HelperValidacion.CrearError(enumErrores.errorEnvioCorreo, "No se pudo enviar el correo de verificación"));
                        res.resultado = false;
                    }
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
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

        // 2. Verificar usuario
        public ResVerificarUsuario VerificarUsuario(ReqVerificarUsuario req)
        {
            ResVerificarUsuario res = new ResVerificarUsuario
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
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    if (string.IsNullOrEmpty(req.CodigoVerificacion))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.codigoVerificacionFaltante, "El código de verificación es requerido"));
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

                _dbContext.SP_ACTIVAR_USUARIO(req.Email, req.CodigoVerificacion, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    // Manejo específico para errores conocidos
                    if (errorIdBD == 401)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.verificacionExpirada, "El código de verificación ha expirado"));
                    }
                    else if (errorIdBD == 400)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.verificacionFallida, "El código de verificación es incorrecto"));
                    }
                    else if (errorIdBD == 404)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                    }
                    else
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    }

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

        // 3. Iniciar sesión
        public ResIniciarSesion IniciarSesion(ReqIniciarSesion req)
        {
            ResIniciarSesion res = new ResIniciarSesion
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
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    if (string.IsNullOrEmpty(req.Password))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordFaltante, "La contraseña es requerida"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Consultar usuario para verificar credenciales
                int? errorIdBD = 0;
                string errorMsgBD = "";
                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_EMAIL(req.Email, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

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

                if (usuario == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.credencialesInvalidas, "Credenciales inválidas"));
                    res.resultado = false;
                    return res;
                }

                if (!usuario.EmailVerificado)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.emailNoVerificado, "El correo electrónico no ha sido verificado"));
                    res.resultado = false;
                    return res;
                }

                // Verificar contraseña
                string passwordHashCalculado = HashearPassword(req.Password, usuario.LlaveUnica);

                if (passwordHashCalculado != usuario.PasswordHash)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.credencialesInvalidas, "Credenciales inválidas"));
                    res.resultado = false;
                    return res;
                }

                // Crear sesión en blanco (sin token aún)
                int? sesionID = 0;
                errorIdBD = 0;
                errorMsgBD = "";
                string guid = Guid.NewGuid().ToString("N");
                DateTime fechaExpiracion = DateTime.Now.AddHours(24);

                _dbContext.SP_ABRIR_SESION(usuario.UsuarioID, guid, fechaExpiracion, ref sesionID, ref errorIdBD, ref errorMsgBD);

                if (sesionID <= 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                // Generar el token con el SesionID ya creado
                string token = HelperJWT.GenerarToken(guid);

                // Actualizar la sesión con el guid generado
                errorIdBD = 0;
                errorMsgBD = "";
                _dbContext.SP_ACTUALIZAR_TOKEN_SESION(sesionID.Value, guid, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                // Actualizar último acceso
                errorIdBD = 0;
                errorMsgBD = "";
                _dbContext.SP_ACTUALIZAR_ULTIMO_ACCESO(usuario.UsuarioID, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                // Armar respuesta
                res.Token = token;
                res.FechaExpiracion = fechaExpiracion;
                res.Usuario = new UsuarioDTO
                {
                    UsuarioID = usuario.UsuarioID,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    FechaRegistro = usuario.FechaRegistro,
                    UltimoAcceso = usuario.UltimoAcceso,
                    ConfiguracionNotificaciones = usuario.ConfiguracionNotificaciones
                };
                res.resultado = true;
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }


        // 4. Cerrar sesión
        public ResCerrarSesion CerrarSesion(ReqCerrarSesion req)
        {
            ResCerrarSesion res = new ResCerrarSesion
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
                    if (req.SesionID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.sesionInvalida, "ID de sesión inválido"));
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

                _dbContext.SP_CERRAR_SESION(req.SesionID, req.MotivoRevocacion, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
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

        // 5. Actualizar perfil
        public ResActualizarPerfil ActualizarPerfil(ReqActualizarPerfil req)
        {
            ResActualizarPerfil res = new ResActualizarPerfil
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
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "ID de usuario inválido"));
                    }
                    if (string.IsNullOrEmpty(req.Nombre))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.nombreFaltante, "El nombre es requerido"));
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

                _dbContext.SP_USUARIO_ACTUALIZAR_PERFIL(
                    req.UsuarioID,
                    req.Nombre,
                    req.ConfiguracionNotificaciones,
                    ref errorIdBD,
                    ref errorMsgBD
                );

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
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

        // 6. Cambiar contraseña
        public ResCambiarPassword CambiarPassword(ReqCambiarPassword req)
        {
            ResCambiarPassword res = new ResCambiarPassword
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
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "ID de usuario inválido"));
                    }
                    if (string.IsNullOrEmpty(req.PasswordActual))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordFaltante, "La contraseña actual es requerida"));
                    }
                    if (string.IsNullOrEmpty(req.NuevoPassword))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordFaltante, "La nueva contraseña es requerida"));
                    }
                    else if (!EsPasswordSeguro(req.NuevoPassword))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordMuyDebil, "La nueva contraseña no cumple con los requisitos de seguridad"));
                    }

                    if (req.PasswordActual == req.NuevoPassword)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordIgual, "La nueva contraseña debe ser diferente a la actual"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Verificar contraseña actual
                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_ID(req.UsuarioID).FirstOrDefault();

                if (usuario == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                    res.resultado = false;
                    return res;
                }

                string passwordHashActual = HashearPassword(req.PasswordActual, usuario.LlaveUnica);

                if (passwordHashActual != usuario.PasswordHash)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.credencialesInvalidas, "La contraseña actual es incorrecta"));
                    res.resultado = false;
                    return res;
                }

                // Generar nuevo hash para la nueva contraseña
                string nuevoPasswordHash = HashearPassword(req.NuevoPassword, usuario.LlaveUnica);

                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_USUARIO_CAMBIAR_PASSWORD(req.UsuarioID, nuevoPasswordHash, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
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

        // 7. Reenviar código de verificación
        public ResReenviarCodigoVerificacion ReenviarCodigoVerificacion(ReqReenviarCodigoVerificacion req)
        {
            ResReenviarCodigoVerificacion res = new ResReenviarCodigoVerificacion
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
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    else if (!EsCorreoValido(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoIncorrecto, "El formato del correo electrónico no es válido"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Generar nuevo código de verificación
                string nuevoCodigo = GenerarCodigoVerificacion(6);
                int? errorIdBD = 0;
                string errorMsgBD = "";

                _dbContext.SP_REENVIAR_CODIGO_VERIFICACION(req.Email, nuevoCodigo, 30, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD == 0)
                {
                    // Obtener información del usuario para el envío del correo
                    errorIdBD = 0;
                    errorMsgBD = "";
                    var usuario = _dbContext.SP_OBTENER_USUARIO_POR_EMAIL(req.Email, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

                    if (errorIdBD != 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                        res.resultado = false;
                        return res;
                    }

                    if (usuario != null)
                    {
                        // Enviar correo con el nuevo código
                        bool correoEnviado = HelperCorreo.EnviarCorreoVerificacion(req.Email, usuario.Nombre, nuevoCodigo);

                        if (correoEnviado)
                        {
                            res.resultado = true;
                        }
                        else
                        {
                            res.error.Add(HelperValidacion.CrearError(enumErrores.errorEnvioCorreo, "No se pudo enviar el correo de verificación"));
                            res.resultado = false;
                        }
                    }
                    else
                    {
                        // Esto no debería ocurrir ya que el SP actualizó el código
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                        res.resultado = false;
                    }
                }
                else
                {
                    if (errorIdBD == 404)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                    }
                    else if (errorIdBD == 402)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.emailYaVerificado, "El correo electrónico ya ha sido verificado"));
                    }
                    else
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    }

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

        // 8. Obtener información de usuario
        public ResObtenerUsuario ObtenerUsuario(ReqObtenerUsuario req)
        {
            ResObtenerUsuario res = new ResObtenerUsuario
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
                    if (req.UsuarioID <= 0)
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "ID de usuario inválido"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_ID(req.UsuarioID).FirstOrDefault();

                if (usuario != null)
                {
                    res.Usuario = new UsuarioDTO
                    {
                        UsuarioID = usuario.UsuarioID,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        FechaRegistro = usuario.FechaRegistro,
                        UltimoAcceso = usuario.UltimoAcceso,
                        ConfiguracionNotificaciones = usuario.ConfiguracionNotificaciones
                    };

                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
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

        // 9. Solicitar código para cambio de contraseña
        public ResSolicitarCambioPassword SolicitarCambioPassword(ReqSolicitarCambioPassword req)
        {
            ResSolicitarCambioPassword res = new ResSolicitarCambioPassword
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
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    else if (!EsCorreoValido(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoIncorrecto, "El formato del correo electrónico no es válido"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Verificar si el usuario existe
                int? errorIdBD = 0;
                string errorMsgBD = "";
                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_EMAIL(req.Email, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                if (usuario == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                    res.resultado = false;
                    return res;
                }

                // Generar nuevo código de verificación
                string nuevoCodigo = GenerarCodigoVerificacion(6);
                DateTime fechaExpiracion = DateTime.Now.AddMinutes(30);

                // Actualizar el código de verificación y su fecha de expiración
                errorIdBD = 0;
                errorMsgBD = "";
                _dbContext.SP_ACTUALIZAR_CODIGO_RECUPERACION(req.Email, nuevoCodigo, fechaExpiracion, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                // Enviar correo con el código de verificación
                bool correoEnviado = HelperCorreo.EnviarCorreoVerificacion(req.Email, usuario.Nombre, nuevoCodigo);

                if (correoEnviado)
                {
                    res.resultado = true;
                }
                else
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.errorEnvioCorreo, "No se pudo enviar el correo de verificación"));
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

        // 10. Confirmar cambio de contraseña
        public ResConfirmarCambioPassword ConfirmarCambioPassword(ReqConfirmarCambioPassword req)
        {
            ResConfirmarCambioPassword res = new ResConfirmarCambioPassword
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
                    if (string.IsNullOrEmpty(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoFaltante, "El correo electrónico es requerido"));
                    }
                    else if (!EsCorreoValido(req.Email))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.correoIncorrecto, "El formato del correo electrónico no es válido"));
                    }
                    if (string.IsNullOrEmpty(req.CodigoVerificacion))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.codigoVerificacionFaltante, "El código de verificación es requerido"));
                    }
                    if (string.IsNullOrEmpty(req.NuevoPassword))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordFaltante, "La nueva contraseña es requerida"));
                    }
                    else if (!EsPasswordSeguro(req.NuevoPassword))
                    {
                        res.error.Add(HelperValidacion.CrearError(enumErrores.passwordMuyDebil, "La nueva contraseña no cumple con los requisitos de seguridad"));
                    }
                }
                #endregion

                if (res.error.Any())
                {
                    res.resultado = false;
                    return res;
                }

                // Verificar si el usuario existe y obtener su información
                int? errorIdBD = 0;
                string errorMsgBD = "";
                var usuario = _dbContext.SP_OBTENER_USUARIO_POR_EMAIL(req.Email, ref errorIdBD, ref errorMsgBD).FirstOrDefault();

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                if (usuario == null)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.usuarioNoEncontrado, "Usuario no encontrado"));
                    res.resultado = false;
                    return res;
                }
                

                // Generar nuevo hash para la nueva contraseña
                string nuevoPasswordHash = HashearPassword(req.NuevoPassword, usuario.LlaveUnica);

                // Actualizar la contraseña
                errorIdBD = 0;
                errorMsgBD = "";
                _dbContext.SP_USUARIO_CAMBIAR_PASSWORD(usuario.UsuarioID, nuevoPasswordHash, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                // Invalidar el código de verificación
                errorIdBD = 0;
                errorMsgBD = "";
                _dbContext.SP_INVALIDAR_CODIGO_RECUPERACION(req.Email, ref errorIdBD, ref errorMsgBD);

                if (errorIdBD != 0)
                {
                    res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionBaseDatos, errorMsgBD));
                    res.resultado = false;
                    return res;
                }

                res.resultado = true;
            }
            catch (Exception ex)
            {
                res.error.Add(HelperValidacion.CrearError(enumErrores.excepcionLogica, "Error en la lógica: " + ex.Message));
                res.resultado = false;
            }

            return res;
        }

        #region Métodos auxiliares
        private bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            // Patrón para validar correo electrónico
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, patron);
        }

        private bool EsPasswordSeguro(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Mínimo 8 caracteres, al menos una letra mayúscula, una minúscula, un número y un carácter especial
            string patron = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(password, patron);
        }

        private string GenerarCodigoVerificacion(int longitud)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < longitud; i++)
            {
                builder.Append(random.Next(0, 10));
            }

            return builder.ToString();
        }

        private string HashearPassword(string password, string llaveUnica)
        {
            // Combinar password con llave única
            string combinado = password + llaveUnica;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(combinado);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convertir los bytes a formato hexadecimal
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
        #endregion
    }
}
