using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Request
{
    public class ReqObtenerTransacciones : ReqBase
    {
        public int UsuarioID { get; set; } // ID del usuario
        public DateTime? FechaInicio { get; set; } // Fecha de inicio del rango
        public DateTime? FechaFin { get; set; } // Fecha de fin del rango
        public string Tipo { get; set; } // Tipo de transacción (opcional) 
    }
}
