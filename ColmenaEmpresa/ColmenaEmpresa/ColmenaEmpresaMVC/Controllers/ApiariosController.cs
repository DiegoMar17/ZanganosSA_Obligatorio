using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ApiariosController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ApiariosController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() => View(await _db.Apiarios.ToListAsync());

        public IActionResult Crear() => View(new Apiario());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Apiario apiario)
        {
            if (!ModelState.IsValid) return View(apiario);

            _db.Apiarios.Add(apiario);
            await _db.SaveChangesAsync();
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var apiario = await _db.Apiarios.FindAsync(id);
            if (apiario is null) return NotFound();

            ViewBag.Colmenas = await _db.Colmenas
                .Where(c => c.ApiarioId == id)
                .ToListAsync();

            return View(apiario);
        }
    }
}
