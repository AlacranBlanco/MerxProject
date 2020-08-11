using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
        [Required]
        public double Cantidad { get; set; }
        [Required]
        public string Medida { get; set; }
        [Required]
        public decimal Precio { get; set; }
        [Required]
        public bool Piezas { get; set; }
        public virtual List<DetalleCompra> DetalleCompras { get; set; }
    }
}