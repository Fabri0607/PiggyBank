using Backend.Entidades.Request;
using Backend.Entidades.Response;
using Backend.Logica;
using System.Web.Http;

namespace Backend.API.Controllers
{
    public class UsuarioController : ApiController
    {
        private readonly LogicaUsuario _logica = new LogicaUsuario();

        [HttpPost]
        public ResRegistrarUsuario Registrar(ReqRegistrarUsuario req)
        {
            return _logica.RegistrarUsuario(req);
        }

        [HttpPost]
        public ResVerificarUsuario Verificar(ReqVerificarUsuario req)
        {
            return _logica.VerificarUsuario(req);
        }

        [HttpPost]
        public ResIniciarSesion IniciarSesion(ReqIniciarSesion req)
        {
            return _logica.IniciarSesion(req);
        }

        [HttpPost]
        public ResCerrarSesion CerrarSesion(ReqCerrarSesion req)
        {
            return _logica.CerrarSesion(req);
        }

        [HttpPost]
        public ResActualizarPerfil ActualizarPerfil(ReqActualizarPerfil req)
        {
            return _logica.ActualizarPerfil(req);
        }

        [HttpPost]
        public ResCambiarPassword CambiarPassword(ReqCambiarPassword req)
        {
            return _logica.CambiarPassword(req);
        }

        [HttpPost]
        public ResReenviarCodigoVerificacion ReenviarCodigo(ReqReenviarCodigoVerificacion req)
        {
            return _logica.ReenviarCodigoVerificacion(req);
        }

        [HttpPost]
        public ResObtenerUsuario ObtenerUsuario(ReqObtenerUsuario req)
        {
            return _logica.ObtenerUsuario(req);
        }
    }
}
