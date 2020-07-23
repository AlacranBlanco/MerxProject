using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Color
    {
        public int Id { get; set; }
        [Required]
        public string Codigo { get; set; }
        public string Nombre { get; set; }
    }
}