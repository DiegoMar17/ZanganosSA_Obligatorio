using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColmenaEmpresa.Controllers
{
    public class PlanificacionController : Controller
    {
        private readonly AppDbContext _ctx;

        public PlanificacionController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index()
        {
            var tareas  = _ctx.Tareas.OrderBy(t => t.Completada).ThenByDescending(t => t.FechaCreacion).ToList();
            var visitas = _ctx.Visitas.OrderBy(v => v.Estado).ThenBy(v => v.FechaPlanificada).ToList();

            ViewBag.Pendientes      = tareas.Count(t => !t.Completada);
            ViewBag.VisitasPend     = visitas.Count(v => v.Estado == "planificada");
            ViewBag.Tareas          = tareas;
            ViewBag.NombresApiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Nombre", "Nombre");

            return View(visitas);
        }

        // ── Tareas ──
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrearTarea(string nombre, string categoria, string prioridad, DateTime? fechaVencimiento)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                _ctx.Tareas.Add(new Tarea
                {
                    Nombre           = nombre,
                    Categoria        = string.IsNullOrWhiteSpace(categoria) ? "General" : categoria,
                    Prioridad        = prioridad ?? "media",
                    FechaVencimiento = fechaVencimiento,
                    FechaCreacion    = DateTime.Now
                });
                _ctx.SaveChanges();
                TempData["Exito"] = $"Tarea '{nombre}' creada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CompletarTarea(int id)
        {
            var t = _ctx.Tareas.Find(id);
            if (t is not null) { t.Completada = !t.Completada; _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EliminarTarea(int id)
        {
            var t = _ctx.Tareas.Find(id);
            if (t is not null) { _ctx.Tareas.Remove(t); _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }

        // ── Visitas ──
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrearVisita(string apiarioNombre, DateTime fechaPlanificada, string? materiales)
        {
            if (!string.IsNullOrWhiteSpace(apiarioNombre))
            {
                _ctx.Visitas.Add(new Visita
                {
                    ApiarioNombre   = apiarioNombre,
                    FechaPlanificada = fechaPlanificada,
                    Materiales      = materiales ?? string.Empty,
                    Estado          = "planificada"
                });
                _ctx.SaveChanges();
                TempData["Exito"] = $"Visita a {apiarioNombre} planificada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CompletarVisita(int id)
        {
            var v = _ctx.Visitas.Find(id);
            if (v is not null) { v.Estado = v.Estado == "planificada" ? "completada" : "planificada"; _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EliminarVisita(int id)
        {
            var v = _ctx.Visitas.Find(id);
            if (v is not null) { _ctx.Visitas.Remove(v); _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
