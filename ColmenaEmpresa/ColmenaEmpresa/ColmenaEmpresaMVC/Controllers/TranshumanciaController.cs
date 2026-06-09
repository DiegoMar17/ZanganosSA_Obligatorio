using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class TranshumanciaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public TranshumanciaController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() =>
            View(await _db.Transhumancias.OrderByDescending(t => t.FechaSalida).ToListAsync());

        public IActionResult Crear() => View(new Transhumancia { FechaSalida = DateTime.Today });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Transhumancia traslado)
        {
            if (!ModelState.IsValid) return View(traslado);

            // Nombre automático solo si el usuario no ingresó uno
            if (string.IsNullOrWhiteSpace(traslado.Nombre))
                traslado.Nombre = $"Traslado {traslado.ApiarioOrigen} → {traslado.ApiarioDestino}";

            traslado.Estado          = "planificado";
            traslado.PorcentajeAvance = 0;

            _db.Transhumancias.Add(traslado);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Traslado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
