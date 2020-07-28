using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string RazonSocial { get; set; }
        [Required]
        [MaxLength(20)]
        public string RFC { get; set; }
        [MaxLength(200)]
        public string Giro { get; set; }
        [Required]
        public virtual Persona Persona { get; set; }
        public virtual List<Compra> Compras { get; set; }

    }
}