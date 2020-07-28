using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Proceso
    {

        [Key]
        public string Id { get; set; }

        public string Nombre { get; set; }

        public string Estado { get; set; }

        public string Empleado { get; set; }

        public Inventario Inventario { get; set; }

        public DateTime Tiempo { get; set; }

        public DateTime Registro { get; set; }

    }
}