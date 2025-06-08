using Backend.Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerMensajes : ResBase
    {
        public List<MensajeChat> MensajesChat { get; set; } // Lista de mensajes del chat

    }
}
