using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CoffeeSync.Data;
using CoffeeSync.Models;

namespace CoffeeSync.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly CoffeeSyncContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductosController(CoffeeSyncContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var coffeeSyncContext = _context.Productos.Include(p => p.Categoria);
            return View(await coffeeSyncContext.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,CategoriaId,Precio,Disponible")] Producto producto, IFormFile? imagen)
        {
            if (ModelState.IsValid)
            {

                if (imagen != null && imagen.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string rutaCarpeta = Path.Combine(wwwRootPath, "images", "productos");

                    if (!Directory.Exists(rutaCarpeta))
                    {
                        Directory.CreateDirectory(rutaCarpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    string rutaFisica = Path.Combine(rutaCarpeta, nombreArchivo);

                    using (var fileStream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        await imagen.CopyToAsync(fileStream);
                    }

                    producto.UrlImagen = "/images/productos/" + nombreArchivo;
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Id", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,CategoriaId,Precio,Disponible,UrlImagen")] Producto producto, IFormFile? imagen)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imagen != null && imagen.Length > 0)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string rutaCarpeta = Path.Combine(wwwRootPath, "images", "productos");

                        if (!Directory.Exists(rutaCarpeta))
                        {
                            Directory.CreateDirectory(rutaCarpeta);
                        }

                        if (!string.IsNullOrEmpty(producto.UrlImagen))
                        {
                            var rutaAntigua = Path.Combine(wwwRootPath, producto.UrlImagen.TrimStart('/'));
                            if (System.IO.File.Exists(rutaAntigua))
                            {
                                System.IO.File.Delete(rutaAntigua);
                            }
                        }

                        string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                        string rutaFisica = Path.Combine(rutaCarpeta, nombreArchivo);

                        using (var fileStream = new FileStream(rutaFisica, FileMode.Create))
                        {
                            await imagen.CopyToAsync(fileStream);
                        }

                        producto.UrlImagen = "/images/productos/" + nombreArchivo;
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
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

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
