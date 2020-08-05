using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }
        public int Cantidad { get; set; }

        // Relaciones con otras tablas
        public Color Color { get; set; }
        public Producto Producto { get; set; }

    }
}