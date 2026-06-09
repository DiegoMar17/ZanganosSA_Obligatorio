using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ColmenasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ColmenasController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var colmenas = await _db.Colmenas.ToListAsync();
            ViewBag.Resumen = new
            {
                Total         = colmenas.Count,
                EnProduccion  = colmenas.Count(c => c.EstadoSemaforo == "verde" || c.EstadoSemaforo == "amarillo"),
                EnTratamiento = colmenas.Count(c => c.EstadoSemaforo == "rojo"),
                SinReina      = colmenas.Count(c => c.EstadoReina == "ausente"),
            };
            return View(colmenas);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
            return View(new Colmena());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Colmena colmena)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
                return View(colmena);
            }

            var apiario = await _db.Apiarios.FindAsync(colmena.ApiarioId);
            colmena.ApiarioNombre = apiario?.Nombre ?? string.Empty;

            _db.Colmenas.Add(colmena);
            await _db.SaveChangesAsync();
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
