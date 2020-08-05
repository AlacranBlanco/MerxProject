using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class HerramientaReporte
    {
        public int Id { get; set; }
        public Herramienta herramienta { get; set; }
        public string Empleado { get; set; }
        public DateTime DateTime { get; set; }
    }
}