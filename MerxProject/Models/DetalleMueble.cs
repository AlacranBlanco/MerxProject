using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class DetalleMueble
    {
        public int Id { get; set; }
        [Required]
        public Mueble Mueble { get; set; }
        [Required]
        public Pieza Pieza { get; set; }
        [Required]
        public int Cantidad { get; set; }
    }
}