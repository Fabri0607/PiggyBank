using Backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Entidades.Response
{
    public class ResObtenerDetallesMeta : ResBase
    {
        public MetaDTO Meta { get; set; }
        public List<MetaTransaccionDTO> Transacciones { get; set; }
    }
}
