using MerxProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject
{
    public class Persona
    {
        [Key]
        public int idPersona { get; set; }

        public string Nombre { get; set; }

        public string Correo { get; set; }

        public string Direccion { get; set; }

        public string CodigoPostal { get; set; }

        public string Estado { get; set; }

        public string Ciudad { get; set; }

        public string Telefono { get; set; }

        public int idUsuario { get; set; }
        
    }
}