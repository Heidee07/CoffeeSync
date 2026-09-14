using System;

namespace CoffeeSync.Models
{
    public class Orden
    {
        public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();
        public int Id { get; set; }
        public string NombreCliente { get; set; }        
        public decimal TotalPagar { get; set; }
        public DateTime Fecha { get; set; }

        public virtual PagoOrden Pago { get; set; }
    }
}
