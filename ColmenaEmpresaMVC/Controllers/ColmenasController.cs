using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            // Códigos únicos de colmenas bajo tratamiento sanitario activo
            var enTratamiento = _ctx.ControlesSanitarios
                .Where(cs => cs.Estado == "en_tratamiento")
                .ToList()
                .SelectMany(cs => cs.ColmenasAfectadas.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .Distinct()
                .Count();

            ViewBag.Resumen = new
            {
                Total         = todas.Count,
                EnProduccion  = todas.Count(c => c.CantidadAlzas > 0),
                EnTratamiento = enTratamiento,
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

        private void CargarApiarios() =>
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");

        public IActionResult Crear() { CargarApiarios(); return View(new Colmena { FechaInstalacion = DateTime.Today }); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Colmena colmena)
        {
            if (!ModelState.IsValid) { CargarApiarios(); return View(colmena); }
            var apiario = _ctx.Apiarios.Find(colmena.ApiarioId);
            colmena.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.Colmenas.Add(colmena);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' registrada.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var c = _ctx.Colmenas.Find(id);
            if (c is null) return NotFound();
            CargarApiarios(); return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Colmena colmena)
        {
            if (id != colmena.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(colmena); }
            var apiario = _ctx.Apiarios.Find(colmena.ApiarioId);
            colmena.ApiarioNombre = apiario?.Nombre ?? string.Empty;
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
