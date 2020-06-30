using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject
{
    public class Usuario
    {
        [Key]
        public int idUsuario { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Rol { get; set; }
    }
}