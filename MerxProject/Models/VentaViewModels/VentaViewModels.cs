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

    }
}