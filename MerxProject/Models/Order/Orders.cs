using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.Order
{
    public class Orders
    {
        [Key]
        public int IdOrder { get; set; }


        public string TipoPago { get; set; }

        public string Estatus { get; set; }

        public DateTime DiaOrden { get; set; }

        public string NombreCustomer { get; set; }

        public string NumeroCustomer { get; set; }

        public string EmailCustomer { get; set; }

        public string DireccionCustomer { get; set; }

        public int idPersona { get; set; }

        public int idDireccion { get; set; }
    }
}