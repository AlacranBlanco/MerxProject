using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.CarritoCompras
{
    public class CarritoCompra
    {
        [Key]
        public int IdCarrito { get; set; }

        public string Nombre { get; set; }

        public float Precio { get; set; }

        public string ColorNombre { get; set; }

        public string CodigoColor { get; set; }

        public string Imagen { get; set; }

        public int Cantidad { get; set; }

        public int idPersona { get; set; }

        public int idProducto { get; set; }

    }
}