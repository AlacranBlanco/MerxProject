using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.ProductosFavorito
{
    public class ProductosFavoritos
    {
        [Key]
        public int IdProductoFav { get; set; }

        public string Nombre { get; set; }

        public int idProducto { get; set; }

        public int idPersona { get; set; }
    }
}