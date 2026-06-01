using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InspeccionesController : Controller
    {
        private readonly AppDbContext _ctx;
        private const int PageSize = 10;

        public InspeccionesController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index(int page = 1, string? q = null)
        {
            var todas = _ctx.Inspecciones.ToList();
            ViewBag.Pendientes  = todas.Count(i => i.Estado == "pendiente");
            ViewBag.EsteMes     = todas.Count(i => i.Fecha.Month == DateTime.Now.Month && i.Fecha.Year == DateTime.Now.Year);
            ViewBag.Completadas = todas.Count(i => i.Estado == "completa");

            var query = todas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(i =>
                    i.ApiarioNombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    i.Estado.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    i.Clima.Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.OrderByDescending(i => i.Fecha).Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<Inspeccion>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        public IActionResult Crear() => View(new Inspeccion { Fecha = DateTime.Today });

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(Inspeccion inspeccion)
        {
            if (!ModelState.IsValid) return View(inspeccion);
            _ctx.Inspecciones.Add(inspeccion);
            _ctx.SaveChanges();
            TempData["Exito"] = "Inspección registrada.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var i = _ctx.Inspecciones.Find(id);
            if (i is null) return NotFound();
            return View(i);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inspeccion inspeccion)
        {
            if (id != inspeccion.Id) return BadRequest();
            if (!ModelState.IsValid) return View(inspeccion);
            _ctx.Inspecciones.Update(inspeccion);
            _ctx.SaveChanges();
            TempData["Exito"] = "Inspección actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var i = _ctx.Inspecciones.Find(id);
            if (i is not null) { _ctx.Inspecciones.Remove(i); _ctx.SaveChanges(); TempData["Exito"] = "Inspección eliminada."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
