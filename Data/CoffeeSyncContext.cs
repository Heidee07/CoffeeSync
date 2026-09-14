using Microsoft.EntityFrameworkCore;
using CoffeeSync.Models;

namespace CoffeeSync.Data
{
    public class CoffeeSyncContext : DbContext
    {
        public CoffeeSyncContext(DbContextOptions<CoffeeSyncContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<DetalleOrden> DetalleOrdenes { get; set; }
        public DbSet<PagoOrden> PagosOrdenes { get; set; }
    }
}
