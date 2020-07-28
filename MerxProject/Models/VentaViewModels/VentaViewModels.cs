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
        public Colors Colors { get; set; }
        public List<Colors> ColorsCollection { get; set; }

    }
}