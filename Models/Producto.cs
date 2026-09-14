namespace CoffeeSync.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        //Llave foránea
        public int CategoriaId { get; set; }

        public decimal Precio { get; set; }
        public bool Disponible { get; set; }

        //Propiedad de navegación
        public Categoria? Categoria { get; set; }

        public string? UrlImagen { get; set; }
    }
}
