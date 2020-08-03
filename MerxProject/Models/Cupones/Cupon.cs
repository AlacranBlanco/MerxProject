using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.Cupones
{
    public class Cupon
    {
        [Key]
        public int IdCupon { get; set; }

        public string CodigoCupon { get; set; }

        public int Descuento { get; set; }

        public Boolean Utilizado { get; set; }

        public int IdPersona { get; set; }

        public int idOrder { get; set; }
    }
}