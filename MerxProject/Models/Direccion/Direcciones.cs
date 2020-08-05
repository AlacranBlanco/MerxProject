using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models.Direccion
{
    public class Direcciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; }
        [Required]
        public string DirCalle { get; set; }
        [Required]
        public string CodigoPostal { get; set; }
        [Required]
        public string Estado { get; set; }
        [Required]
        public string Ciudad { get; set; }
        [Required]
        public string Asentamiento { get; set; }
        [Required]
        public string NumTelefono { get; set; }

        public int IdPersona { get; set; }
    }
}