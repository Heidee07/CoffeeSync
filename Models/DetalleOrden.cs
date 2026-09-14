using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSync.Models
{
    public class DetalleOrden
    {
        public int Id { get; set; }

        //Llave foránea
        public int OrdenId { get; set; }

        public int ProductoId { get; set; }
        [NotMapped] public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }        
        public decimal PrecioUnitario { get; set; }

        //Propiedades de navegación
        public Orden? Orden { get; set; }
        public Producto? Producto { get; set; }
    }
}
