﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Producto
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        public string Imagen { get; set; }

        [Required]
        public Mueble CategoriaMueble { get; set; }
        [Required]
        public Material CategoriaMaterial { get; set; }

        [Required]
        public float Precio { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}