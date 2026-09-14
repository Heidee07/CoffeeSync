using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeSync.Data;
using CoffeeSync.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeSync.Controllers
{
    [Authorize]
    public class OrdenesController : Controller
    {
        private readonly CoffeeSyncContext _context;

        public OrdenesController(CoffeeSyncContext context)
        {
            _context = context;
        }

        // GET: Ordenes
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.Remove("Carrito");

            var coffeeSyncContext = _context.Ordenes.Include(o => o.Detalles);
            return View(await coffeeSyncContext.ToListAsync());
        }

        // GET: Ordenes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orden == null)
            {
                return NotFound();
            }

            var pago = await _context.PagosOrdenes
                .FirstOrDefaultAsync(x => x.OrdenId == id);

            ViewBag.Pago = pago;

            return View(orden);
        }

        // GET: Ordenes/Create
        public IActionResult Create()
        {
            PrepararPantalla();

            return View();
        }

        // POST: Ordenes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreCliente,TotalPagar,Fecha")] Orden orden, decimal montoRecibido)
        {
            ModelState.Remove("Pago");
            ModelState.Remove("Detalles");

            if (ModelState.IsValid)
            {
                List<DetalleOrden> carrito = ObtenerCarrito();

                if (carrito.Count != 0)
                {
                    orden.Detalles = carrito;
                    orden.TotalPagar = 0;

                    foreach (var item in orden.Detalles)
                    {
                        var producto = await _context.Productos.FindAsync(item.ProductoId);
                        if (producto != null)
                        {
                            item.PrecioUnitario = producto.Precio;
                            decimal calculo = item.Cantidad * item.PrecioUnitario;
                            orden.TotalPagar += calculo;
                        }
                    }
                    orden.Fecha = DateTime.Now;

                    _context.Add(orden);
                    await _context.SaveChangesAsync();

                    var nuevoPago = new PagoOrden
                    {
                        OrdenId = orden.Id,
                        MontoRecibido = montoRecibido,
                        Cambio = montoRecibido - orden.TotalPagar,
                        FechaPago = DateTime.Now
                    };

                    _context.PagosOrdenes.Add(nuevoPago);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove("Carrito");
                    return RedirectToAction("Ticket", new { id = orden.Id });
                }

                else
                {
                    ModelState.AddModelError(string.Empty, "El carrito está vacío. Agrega al menos un producto.");
                }

            }
            PrepararPantalla();

            return View(orden);
        }

        // GET: Ordenes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }
            //ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", orden.ProductoId);
            return View(orden);
        }

        // POST: Ordenes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCliente,TotalPagar,Fecha")] Orden orden)
        {
            if (id != orden.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orden);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenExists(orden.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Id", orden.ProductoId);
            return View(orden);
        }

        // GET: Ordenes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }

        // POST: Ordenes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden != null)
            {
                _context.Ordenes.Remove(orden);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenExists(int id)
        {
            return _context.Ordenes.Any(e => e.Id == id);
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(int productoId, int cantidad)
        {
            var producto = _context.Productos.Find(productoId);

            DetalleOrden nuevoDetalle = new DetalleOrden { ProductoId = productoId, Cantidad = cantidad, ProductoNombre = producto.Nombre };

            List<DetalleOrden> carrito = ObtenerCarrito();

            var itemExistente = carrito.FirstOrDefault(x => x.ProductoId == productoId);
            // Si el producto a agregar ya está en el carrito
            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(nuevoDetalle);

            }

            // Guardar el carrito actualizado en la sesión (de objeto a JSON)
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));

            return RedirectToAction("Create", "Ordenes");
        }

        private List<DetalleOrden> ObtenerCarrito()
        {
            string carritoJson = HttpContext.Session.GetString("Carrito");

            List<DetalleOrden> carrito;

            // Si el carrito está vacío
            if (string.IsNullOrEmpty(carritoJson))
            {
                carrito = new List<DetalleOrden>();
            }
            // Si el carrito está lleno
            else
            {
                // De JSON a objeto
                carrito = JsonSerializer.Deserialize<List<DetalleOrden>>(carritoJson);
            }

            return carrito;
        }

        public IActionResult EliminarDelCarrito(int productoId)
        {
            List<DetalleOrden> carrito = ObtenerCarrito();

            // Buscar el producto en el carrito
            var itemEliminar = carrito.FirstOrDefault(x => x.ProductoId == productoId);
            //Borrar el producto del carrito
            carrito.Remove(itemEliminar);

            // Guardar el carrito actualizado en la sesión (de objeto a JSON)
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));

            return RedirectToAction("Create", "Ordenes");
        }

        private decimal CalcularTotal()
        {
            List<DetalleOrden> carrito = ObtenerCarrito();
            decimal total = 0;

            foreach (var item in carrito)
            {
                var producto = _context.Productos.Find(item.ProductoId);
                if (producto != null)
                {
                    total += item.Cantidad * producto.Precio;
                }
            }
            return total;
        }

        private void PrepararPantalla()
        {
            //Cargar carrito
            List<DetalleOrden> carrito = ObtenerCarrito();
            ViewBag.Carrito = carrito;

            //Calcular el total
            var total = CalcularTotal();
            ViewBag.Total = total;

            //Cargar la lista de productos en el select
            ViewBag.ListaProductos = _context.Productos
                .Include(p => p.Categoria)
                .Where(x => x.Disponible == true)
                .ToList();

            //Cargar la lista de categorías
            ViewBag.ListaCategorias = _context.Categorias.ToList();
        }

        public IActionResult SumarCantidad(int productoId)
        {
            List<DetalleOrden> carrito = ObtenerCarrito().ToList();
            var itemSumar = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            if (itemSumar != null)
            {
                itemSumar.Cantidad += 1;
            }
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
            return RedirectToAction("Create", "Ordenes");

        }

        public IActionResult RestarCantidad(int productoId)
        {
            List<DetalleOrden> carrito = ObtenerCarrito().ToList();
            var itemRestar = carrito.FirstOrDefault(x => x.ProductoId == productoId);

            //Si la cantidad del producto es mayor a 1
            if (itemRestar != null && itemRestar.Cantidad > 1)
            {
                itemRestar.Cantidad -= 1;
            }
            //Si la cantidad del producto es 1
            else if (itemRestar != null && itemRestar.Cantidad == 1)
            {
                carrito.Remove(itemRestar);
            }

            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(carrito));
            return RedirectToAction("Create", "Ordenes");
        }

        private bool ProductoNoDisponible(bool disponible)
        {
            return _context.Productos.Any(e => e.Disponible == disponible);
        }

        [HttpPost]
        public IActionResult Cambio(int Dinero, int Centavos, decimal Total)
        {
            decimal cambio = 0;

            if (Total > 0)
            {
                var dinero = Dinero;
                var centavos = Centavos;
                decimal pago = Convert.ToDecimal($"{dinero}.{centavos}");

                cambio = pago - Total;
                ViewBag.Cambio = cambio;
            }
            return new JsonResult(cambio);
        }

        public IActionResult CancelarOrden()
        {
            HttpContext.Session.Remove("Carrito");

            return RedirectToAction(nameof(Create));
        }

        public async Task<IActionResult> Ticket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orden = await _context.Ordenes
                .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (orden == null)
            {
                return NotFound();
            }

            return View(orden);
        }
    }
}
