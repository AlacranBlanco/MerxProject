using MerxProject.Models.ProductosFavorito;
using MerxProject.Models.CarritoCompras;
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
        public List<CarritoCompra> ProductosCarritoCollection { get; set; }

        public CarritoCompra CarritoCompra { get; set; }

        public List<int> ProductosFavId { get; set; }
        public List<int> ProductosCarrito { get; set; }

    }
}