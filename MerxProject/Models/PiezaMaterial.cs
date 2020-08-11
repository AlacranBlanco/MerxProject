using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class PiezaMaterial
    {
        public int Id { get; set; }
        public Pieza Pieza { get; set; }
        public int Cantidad { get; set; }
        public Material Material { get; set; }
    }
}