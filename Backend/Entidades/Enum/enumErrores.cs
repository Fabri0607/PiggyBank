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
        categoriaRepetida = 29,
        errorAlBorrarPorDependencias = 30,
        tipoTransaccionFaltante = 31,
        tipoTransaccionInvalido = 32,
        montoInvalido = 33,
        categoriaFaltante = 34,
        fechaFaltante = 35,
        fechaInvalida = 36,
        tituloFaltante = 37,
        permisoDenegado = 38,
        autenticacionFallida = 39,
        tokenFaltante = 40,
        tokenInvalido = 41,
        sesionNoEncontrada = 42,
        sesionInactiva = 43,
        sesionExpirada = 44,
        TipoTransaccionInvalido = 45,
        FechaInvalida = 46,
        gastoNoEncontrado = 47,
        metaNoEncontrada = 48
    }
}
