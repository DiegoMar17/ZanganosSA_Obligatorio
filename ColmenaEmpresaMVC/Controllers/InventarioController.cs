using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InventarioController : Controller
    {
        private readonly AppDbContext _ctx;

        public InventarioController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index()
        {
            var items = _ctx.ItemsInventario.ToList();
            ViewBag.BajoMinimo   = items.Count(i => i.CantidadActual <= i.CantidadMinima);
            ViewBag.ItemsTotales = items.Count;
            return View(items);
        }

        public IActionResult Crear() => View(new ItemInventario());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ItemInventario item)
        {
            if (!ModelState.IsValid) return View(item);
            _ctx.ItemsInventario.Add(item);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Ítem '{item.Nombre}' agregado al inventario.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var item = _ctx.ItemsInventario.Find(id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, ItemInventario item)
        {
            if (id != item.Id) return BadRequest();
            if (!ModelState.IsValid) return View(item);
            _ctx.ItemsInventario.Update(item);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Ítem '{item.Nombre}' actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var item = _ctx.ItemsInventario.Find(id);
            if (item is not null)
            {
                _ctx.ItemsInventario.Remove(item);
                _ctx.SaveChanges();
                TempData["Exito"] = "Ítem eliminado del inventario.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
