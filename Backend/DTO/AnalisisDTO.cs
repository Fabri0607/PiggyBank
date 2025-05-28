using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class AnalisisDTO
    {
      public int AnalisisID { get; set; }
      public DateTime FechaInicio { get; set; }
      public DateTime FechaFin { get; set; }
      public string Resumen { get; set; }
      public DateTime FechaGeneracion { get; set; }
      public int? Contexto { get; set; }
    }
}
