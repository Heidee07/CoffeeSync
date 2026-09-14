using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSync.Models
{
    public class PagoOrden
    {
        [Key]
        public int Id { get; set; }

        public int OrdenId { get; set; }

        [ForeignKey("OrdenId")]
        public virtual Orden Orden { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoRecibido { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cambio { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;
    }
}