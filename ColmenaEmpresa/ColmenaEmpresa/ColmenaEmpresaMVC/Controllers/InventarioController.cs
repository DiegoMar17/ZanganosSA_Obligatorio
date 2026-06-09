using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _db;
        public InventarioController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var items = await _db.ItemsInventario.ToListAsync();
            ViewBag.BajoMinimo   = items.Count(i => i.CantidadActual <= i.CantidadMinima);
            ViewBag.ItemsTotales = items.Count;
            return View(items);
        }

        public IActionResult Crear() => View(new ItemInventario());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ItemInventario item)
        {
            if (!ModelState.IsValid) return View(item);

            _db.ItemsInventario.Add(item);
            await _db.SaveChangesAsync();
            TempData["Exito"] = $"Item '{item.Nombre}' agregado al inventario.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarStock(int id, double cantidad)
        {
            var item = await _db.ItemsInventario.FindAsync(id);
            if (item is null) return NotFound();

            item.CantidadActual = Math.Max(0, item.CantidadActual + cantidad);
            await _db.SaveChangesAsync();
            TempData["Exito"] = $"Stock de '{item.Nombre}' actualizado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
