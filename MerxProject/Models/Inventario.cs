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
        public int Color_Id { get; set; }
        public int Producto_Id { get; set; }

        // Relaciones con otras tablas
        public virtual Color Color { get; set; }
        public virtual Producto Producto { get; set; }

    }
}