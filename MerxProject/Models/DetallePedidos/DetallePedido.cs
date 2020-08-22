using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.DetallePedidos
{
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        public string IdPedidoDetalle { get; set; }

        public string IdProceso { get; set; }
    }
}