using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Empleado
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime FechaIngreso { get; set; }
        [Required]
        public Persona Personas { get; set; }
        [Required]
        public Usuario Usuarios { get; set; }
        public virtual List<Compra> Compras { get; set; }
    }
}