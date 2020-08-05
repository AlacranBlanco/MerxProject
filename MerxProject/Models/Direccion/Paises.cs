using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.Direccion
{
    public class Paises
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

    }
}