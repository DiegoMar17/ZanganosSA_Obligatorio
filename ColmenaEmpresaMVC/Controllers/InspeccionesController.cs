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
            // Auto-vencida: pendientes con fecha pasada
            var vencibles = _ctx.Inspecciones
                .Where(i => i.Estado == "pendiente" && i.Fecha < DateTime.Today)
                .ToList();
            if (vencibles.Any())
            {
                foreach (var v in vencibles) v.Estado = "vencida";
                _ctx.SaveChanges();
            }

            var todas = _ctx.Inspecciones.ToList();
            ViewBag.Pendientes  = todas.Count(i => i.Estado == "pendiente");
            ViewBag.Vencidas    = todas.Count(i => i.Estado == "vencida");
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

        private void CargarDatos()
        {
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");
            ViewBag.Colmenas = _ctx.Colmenas.OrderBy(c => c.ApiarioNombre).ThenBy(c => c.Codigo).ToList();
        }

        public IActionResult Exportar()
        {
            var inspecciones = _ctx.Inspecciones.OrderBy(i => i.Fecha).ToList();
            ViewBag.Completas  = inspecciones.Count(i => i.Estado == "completa");
            ViewBag.Alertas    = inspecciones.Count(i => i.Estado == "incompleta");
            ViewBag.Criticas   = inspecciones.Count(i => i.Estado == "pendiente");
            ViewBag.PromMarcos = inspecciones.Any() ? Math.Round(inspecciones.Average(i => (double)i.ColmenasInspeccionadas), 1) : 0;
            return View(inspecciones);
        }

        public IActionResult Crear() { CargarDatos(); return View(new Inspeccion { Fecha = DateTime.Today }); }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(Inspeccion inspeccion)
        {
            inspeccion.Estado = "pendiente";
            inspeccion.ColmenasInspeccionadas = 0;

            if (inspeccion.TipoInspeccion == "colmena" && inspeccion.ColmenaId.HasValue)
            {
                var colmena = _ctx.Colmenas.Find(inspeccion.ColmenaId.Value);
                if (colmena != null)
                {
                    inspeccion.ApiarioId     = colmena.ApiarioId;
                    inspeccion.ApiarioNombre = colmena.ApiarioNombre;
                    inspeccion.ColmenaCodigo = colmena.Codigo;
                    inspeccion.TotalColmenas = 1;
                    ModelState.Remove(nameof(Inspeccion.ApiarioId));
                }
            }
            else if (inspeccion.TipoInspeccion == "apiario" && inspeccion.ApiarioId > 0)
            {
                var apiario = _ctx.Apiarios.Find(inspeccion.ApiarioId);
                inspeccion.ApiarioNombre = apiario?.Nombre ?? string.Empty;
                inspeccion.TotalColmenas = _ctx.Colmenas.Count(c => c.ApiarioId == inspeccion.ApiarioId);
                ModelState.Remove(nameof(Inspeccion.ColmenaId));
            }

            if (!ModelState.IsValid) { CargarDatos(); return View(inspeccion); }

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
            CargarDatos(); return View(i);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inspeccion inspeccion)
        {
            if (id != inspeccion.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarDatos(); return View(inspeccion); }

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
        public IActionResult Completar(int id)
        {
            var i = _ctx.Inspecciones.Find(id);
            if (i is not null)
            {
                i.Estado = "completa";
                i.ColmenasInspeccionadas = i.TotalColmenas;
                _ctx.SaveChanges();
                TempData["Exito"] = "Inspección marcada como completada.";
            }
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
