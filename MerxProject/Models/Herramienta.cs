using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Herramienta
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        public string Imagen { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public decimal Precio { get; set; }
        public virtual List<DetalleCompra> DetalleCompras { get; set; }
        public int EnUso { get; set; }
        public bool Reportada { get; set; }
        public int idProveedor { get; set; }
    }
}