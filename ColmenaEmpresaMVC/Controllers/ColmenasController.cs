using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ColmenasController : Controller
    {
        private readonly AppDbContext _ctx;
        private const int PageSize = 10;

        public ColmenasController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index(int page = 1, string? q = null)
        {
            var todas = _ctx.Colmenas.ToList();

            ViewBag.Resumen = new
            {
                Total         = todas.Count,
                EnProduccion  = todas.Count(c => c.CantidadAlzas > 0),
                EnTratamiento = _ctx.ControlesSanitarios.Count(cs => cs.Estado == "en_tratamiento"),
                SinReina      = todas.Count(c => c.EstadoReina == "ausente")
            };

            var query = todas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c =>
                    c.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.ApiarioNombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.EstadoReina.Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<Colmena>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        public IActionResult Crear() => View(new Colmena { FechaInstalacion = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Colmena colmena)
        {
            if (!ModelState.IsValid) return View(colmena);
            _ctx.Colmenas.Add(colmena);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' registrada.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var c = _ctx.Colmenas.Find(id);
            if (c is null) return NotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Colmena colmena)
        {
            if (id != colmena.Id) return BadRequest();
            if (!ModelState.IsValid) return View(colmena);
            _ctx.Colmenas.Update(colmena);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var c = _ctx.Colmenas.Find(id);
            if (c is not null) { _ctx.Colmenas.Remove(c); _ctx.SaveChanges(); TempData["Exito"] = "Colmena eliminada."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
