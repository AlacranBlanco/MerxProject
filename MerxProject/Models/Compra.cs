using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(20)]
        public string Folio { get; set; }
        public decimal MontoTotal { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; }
        [Required]
        public int Estatus { get; set; }

        public string DS_Estatus { get; set; }

        // Llaves foráneas
        //public int? IdProveedor { get; set; }
        //public int? IdEmpleado { get; set; }

        // Relaciones con otras tablas
        //public virtual List<DetalleCompra> DetalleCompra { get; set; }
        public Proveedor Proveedor { get; set; }
        public Empleado Empleado { get; set; }
    }

    public enum EstatusC
    {
        Vacía = 0,
        Pendiente = 1,
        Confirmada = 2,
        Entregada = 3,
        Cancelada = 4,
        Incompleta = 5
    }
}