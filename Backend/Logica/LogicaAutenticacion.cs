using AccesoADatos;
using Backend.Entidades.Entity;
using Backend.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;

namespace Backend.Logica
{
    public class LogicaAutenticacion
    {
        public Sesion ValidarSesion(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new SecurityTokenException("Token no proporcionado.");
                }

                // Validar el token y obtener el GUID
                string guid = HelperJWT.ValidarTokenYObtenerGuid(token);

                // Consultar la sesión usando el SP con LINQ
                using (var dbContext = new ConexionLINQDataContext())
                {
                    int? errorId = 0;
                    string errorMensaje = string.Empty;

                    var resultado = dbContext.SP_SESION_OBTENER_POR_GUID(guid, ref errorId, ref errorMensaje);

                    if (errorId != 0)
                    {
                        throw new SecurityTokenException($"Error al obtener la sesión: {errorMensaje}");
                    }

                    // Mapear el resultado a la clase Sesion
                    var sesionResult = resultado.FirstOrDefault();
                    Sesion sesion = null;
                    if (sesionResult != null)
                    {
                        sesion = new Sesion
                        {
                            SesionID = sesionResult.SesionID,
                            UsuarioID = sesionResult.UsuarioID,
                            Guid = sesionResult.Guid,
                            FechaCreacion = sesionResult.FechaCreacion,
                            FechaExpiracion = sesionResult.FechaExpiracion,
                            EsActivo = sesionResult.EsActivo,
                            MotivoRevocacion = sesionResult.MotivoRevocacion
                        };
                    }

                    if (sesion == null)
                    {
                        throw new SecurityTokenException("Sesión no encontrada, inactiva o expirada.");
                    }

                    return sesion;
                }
            }
            catch (SecurityTokenException ex)
            {
                throw; // Re-lanzar la excepción para que el llamador la maneje
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al validar la sesión: {ex.Message}");
            }
        }
    }
}