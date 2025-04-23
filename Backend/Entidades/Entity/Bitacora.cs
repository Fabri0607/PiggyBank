using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class Bitacora
    {
        public int BitacoraID { get; set; }
        public int UsuarioID { get; set; }
        public string TipoEvento { get; set; } // 'Creacion', 'Modificacion', 'Eliminacion', 'Acceso'
        public string EntidadAfectada { get; set; }
        public int EntidadID { get; set; }
        public string Detalles { get; set; }
        public DateTime FechaEvento { get; set; }
    }
}
