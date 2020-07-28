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
        public virtual Persona Personass { get; set; }
        public virtual Usuario Usuarioss { get; set; }

    }
}