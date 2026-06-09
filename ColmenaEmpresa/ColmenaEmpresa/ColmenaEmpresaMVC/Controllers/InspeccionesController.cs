using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InspeccionesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public InspeccionesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var inspecciones = await _db.Inspecciones.OrderByDescending(i => i.Fecha).ToListAsync();
            ViewBag.Pendientes  = inspecciones.Count(i => i.Estado == "pendiente");
            ViewBag.EsteMes     = inspecciones.Count(i => i.Fecha.Month == DateTime.Now.Month && i.Fecha.Year == DateTime.Now.Year);
            ViewBag.Completadas = inspecciones.Count(i => i.Estado == "completa");
            return View(inspecciones);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
            return View(new Inspeccion { Fecha = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Inspeccion inspeccion)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
                return View(inspeccion);
            }

            var apiario = await _db.Apiarios.FindAsync(inspeccion.ApiarioId);
            inspeccion.ApiarioNombre = apiario?.Nombre ?? string.Empty;

            // Si no vino TotalColmenas del formulario, tomarlo del apiario
            if (inspeccion.TotalColmenas == 0 && apiario is not null)
                inspeccion.TotalColmenas = apiario.TotalColmenas;

            _db.Inspecciones.Add(inspeccion);

            // Actualizar UltimaVisita de todas las colmenas del apiario
            var colmenas = await _db.Colmenas
                .Where(c => c.ApiarioId == inspeccion.ApiarioId)
                .ToListAsync();
            foreach (var c in colmenas)
                c.UltimaVisita = inspeccion.Fecha;

            await _db.SaveChangesAsync();
            TempData["Exito"] = "Inspección registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
