using MerxProject.Models.CarritoCompras;
using MerxProject.Models.ProductosFavorito;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models.VentaViewModels
{
    public class VentaViewModels
    {
        public Producto Producto { get; set; }
        public Inventario Inventario { get; set; }
        public List<Inventario> InventarioCollection { get; set; }
        public Color Colors { get; set; }
        public List<Color> ColorsCollection { get; set; }
        public List<Producto> ProductoCollection { get; set; }
        public ProductosFavoritos ProductosFavorito { get; set; }
        public List<ProductosFavoritos> ProductosFavoritosColelction { get; set; }
        public List<CarritoCompra> ProductosCarritoCollection { get; set; }
        public CarritoCompra CarritoCompra { get; set; }
        public List<int> ProductosFavId { get; set; }

    }
}