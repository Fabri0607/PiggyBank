using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades
{
    public enum enumErrores
    {
        excepcionBaseDatos = -2,
        excepcionLogica = -1,
        requestNulo = 1,
        nombreFaltante = 2,
        apellidoFaltante = 3,
        correoFaltante = 4,
        passwordFaltante = 5,
        correoIncorrecto = 6,
        passwordMuyDebil = 7,
        idFaltante = 8,
        sesionCerrada = 9,
        verificacionFallida = 10,
        verificacionExpirada = 11,
        usuarioFaltante = 12,
        campoRequerido = 13,
        valorInvalido = 14,
        grupoNoEncontrado = 15,
        usuarioNoEncontrado = 16,
        rolInvalido = 18,
        transaccionNoEncontrada = 19,
        estadoInvalido = 20,
        errorEnvioCorreo = 21,
        codigoVerificacionFaltante = 22,
        credencialesInvalidas = 23,
        emailNoVerificado = 24,
        sesionInvalida = 25,
        passwordIgual = 26,
        emailYaVerificado = 27,
        categoriaNoEncontrada = 28,
        autenticacionFallida = 29,
        tokenFaltante = 30,
        tokenInvalido = 31,
        sesionNoEncontrada = 32,
        sesionInactiva = 33,
        sesionExpirada = 34,
        permisoDenegado = 35
    }
}
