using Backend.Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerBalanceGrupal : ResBase
    {
        public List<BalanceMiembro> Balances { get; set; } // Lista de balances por miembro

    }
}
