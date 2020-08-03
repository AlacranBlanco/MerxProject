using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.Order
{
    public class OrdersDetails
    {
        [Key]
        public int IdOrderDetail { get; set; }

        public int idOrder { get; set; }

        public int idProducto { get; set; }

        public float Precio { get; set; }

        public int Cantidad { get; set; }
    }
}