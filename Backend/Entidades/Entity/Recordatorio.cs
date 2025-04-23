using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Recordatorio
    {
        public int RecordatorioID { get; set; }
        public int UsuarioID { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRecordatorio { get; set; }
        public string Estado { get; set; } // 'Pendiente', 'Completado'
    }
}
