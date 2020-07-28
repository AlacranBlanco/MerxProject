using MerxProject.Models.ProductosFavorito;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models.TiendaViewModels
{
    public class TiendaViewModel
    {
        public Producto Producto { get; set; }

        public List<Producto> ProductoCollection { get; set; }

        public ProductosFavoritos ProductosFavorito { get; set; }

        public List<ProductosFavoritos> ProductosFavoritosColelction { get; set; }
        
        public List<int> ProductosFavId { get; set; }
    }
}