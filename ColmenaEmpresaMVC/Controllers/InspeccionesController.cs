using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        private void CargarApiarios() =>
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");

        public IActionResult Exportar()
        {
            var inspecciones = _ctx.Inspecciones.OrderBy(i => i.Fecha).ToList();
            ViewBag.Completas  = inspecciones.Count(i => i.Estado == "completa");
            ViewBag.Alertas    = inspecciones.Count(i => i.Estado == "incompleta");
            ViewBag.Criticas   = inspecciones.Count(i => i.Estado == "pendiente");
            ViewBag.PromMarcos = inspecciones.Any() ? Math.Round(inspecciones.Average(i => (double)i.ColmenasInspeccionadas), 1) : 0;
            return View(inspecciones);
        }

        public IActionResult Crear() { CargarApiarios(); return View(new Inspeccion { Fecha = DateTime.Today }); }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(Inspeccion inspeccion)
        {
            if (!ModelState.IsValid) { CargarApiarios(); return View(inspeccion); }

            var apiario = _ctx.Apiarios.Find(inspeccion.ApiarioId);
            inspeccion.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.Inspecciones.Add(inspeccion);

            // Actualiza la última visita de las colmenas del apiario inspeccionado
            var colmenas = _ctx.Colmenas.Where(c => c.ApiarioId == inspeccion.ApiarioId).ToList();
            foreach (var c in colmenas)
                c.UltimaVisita = inspeccion.Fecha;

            _ctx.SaveChanges();
            TempData["Exito"] = "Inspección registrada.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var i = _ctx.Inspecciones.Find(id);
            if (i is null) return NotFound();
            CargarApiarios(); return View(i);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inspeccion inspeccion)
        {
            if (id != inspeccion.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(inspeccion); }

            var apiario = _ctx.Apiarios.Find(inspeccion.ApiarioId);
            inspeccion.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.Inspecciones.Update(inspeccion);

            // Refresca la última visita de las colmenas del apiario
            var colmenas = _ctx.Colmenas.Where(c => c.ApiarioId == inspeccion.ApiarioId).ToList();
            foreach (var c in colmenas)
                c.UltimaVisita = inspeccion.Fecha;

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
