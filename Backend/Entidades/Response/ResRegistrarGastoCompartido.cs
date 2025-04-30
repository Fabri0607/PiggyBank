using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResRegistrarGastoCompartido : ResBase
    {
        public int GastoID { get; set; } // ID del gasto compartido registrado
        public int TransaccionID { get; set; } // ID de la transacción creada
    }
}
