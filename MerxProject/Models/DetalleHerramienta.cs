using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class DetalleHerramienta
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Empleado { get; set; }
        public Herramienta Herramienta { get; set; }
    }
}