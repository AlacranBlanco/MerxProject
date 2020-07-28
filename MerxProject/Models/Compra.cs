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
        [Required]
        public decimal MontoTotal { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; }
        [Required]
        public int Estatus { get; set; }

        public string DS_Estatus { get; set; }

        // Llaves foráneas
        public int? IdProveedor { get; set; }
        public int? IdEmpleado { get; set; }

        // Relaciones con otras tablas
        public virtual List<DetalleCompra> DetalleCompra { get; set; }
        public virtual Proveedor Proveedor { get; set; }
        public virtual Empleado Empleado { get; set; }
    }

    public enum EstatusC
    {
        Pendiente = 0,
        Entregada = 1,
        Cancelada = 2,
        Incorrecta = 3
    }
}