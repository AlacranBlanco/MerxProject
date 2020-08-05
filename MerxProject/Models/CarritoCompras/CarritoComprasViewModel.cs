using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models.CarritoCompras
{
    public class CarritoComprasViewModel
    {
        public CarritoCompra CarritoCompra { get; set; }

        public List<CarritoCompra> CarritoCollection { get; set; }

        public List<CarritoCompra> CarritoCollectionRepetido { get; set; }

        public List<CarritoCompra> CantidadesProductos { get; set; }
    }
}