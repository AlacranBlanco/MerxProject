using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class DetalleCompra
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public decimal Cantidad { get; set; }
        [Required]
        [MaxLength(50)]
        public string Unidad { get; set; }
        [Required]
        public decimal PrecioUnitario { get; set; }
        [Required]
        public decimal PrecioTotal { get; set; }

        // Llaves foráneas
        /*public int? IdCompra { get; set; }
        public int? IdMateriaPrima { get; set; }
        public int? IdHerramienta { get; set; }*/

        // Relaciones con otras tablas
        public  Compra Compra { get; set; }
        public Material MateriaPrima { get; set; }
        public Herramienta Herramienta { get; set; }


        public string Tipo { get; set; }

        public void setTipo()
        {
            if (MateriaPrima != null)
            {
                Tipo = "MateriaPrima";
            }
            if (Herramienta != null)
            {
                Tipo = "Herramienta";
            }
        }
    }

    public enum Tipos
    {
        [Display(Name = "Materia Prima")]
        MateriaPrima,
        [Display(Name = "Herramienta")]
        Herramienta
    }

    public enum Unidades
    {
        [Display(Name = "Piezas")]
        Pz,
        [Display(Name = "Metros")]
        M,
        [Display(Name = "Metros Cuadrados")]
        M2,
        [Display(Name = "Kilogramos")]
        Kg,
        [Display(Name = "Gramos")]
        G,
        [Display(Name = "Litros")]
        L,
        [Display(Name = "Mililitros")]
        Ml,
        
    }
}