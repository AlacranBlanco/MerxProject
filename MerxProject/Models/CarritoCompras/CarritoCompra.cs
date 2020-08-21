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

        public double Precio { get; set; }

        public string ColorNombre { get; set; }
        public string MaterialNombre { get; set; }

        public string CodigoColor { get; set; }

        public string Imagen { get; set; }

        public string ImagenMaterial { get; set; }

        public string SvgColor { get; set; }

        public int Cantidad { get; set; }

        public int StockProducto { get; set; }
        public int StockColor { get; set; }
        public int StockMaterial { get; set; }

        public double precioTotal { get; set; }


        public int idPersona { get; set; }

        public int idProducto { get; set; }
        public int idColor { get; set; }
        public int idMaterial { get; set; }

    }
}