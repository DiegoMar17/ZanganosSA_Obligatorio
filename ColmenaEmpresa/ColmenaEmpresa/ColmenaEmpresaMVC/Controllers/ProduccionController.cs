using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProduccionController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var cosechas = await _db.Cosechas.OrderByDescending(c => c.Fecha).ToListAsync();

            var totalKg = cosechas.Sum(c => c.PesoNeto);
            var mejorGrupo = cosechas
                .GroupBy(c => c.ApiarioNombre)
                .Select(g => new { Nombre = g.Key, Kg = g.Sum(c => c.PesoNeto) })
                .OrderByDescending(g => g.Kg)
                .FirstOrDefault();

            ViewBag.TotalKg      = (int)totalKg;
            ViewBag.MejorApiario = mejorGrupo?.Nombre ?? "—";
            ViewBag.MejorKg      = (int)(mejorGrupo?.Kg ?? 0);
            ViewBag.PromColmena  = cosechas.Any()
                ? (int)(totalKg / cosechas.Sum(c => c.AlzasCosechadas))
                : 0;
            ViewBag.VariacionPct = "+12%";

            return View(cosechas);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
            return View(new Cosecha { Fecha = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cosecha cosecha)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
                return View(cosecha);
            }

            var apiario = await _db.Apiarios.FindAsync(cosecha.ApiarioId);
            cosecha.ApiarioNombre = apiario?.Nombre ?? string.Empty;

            _db.Cosechas.Add(cosecha);

            // Crear registro de ingreso automático si el usuario lo solicitó
            if (cosecha.CrearRegistroIngreso)
            {
                _db.RegistrosFinancieros.Add(new RegistroFinanciero
                {
                    TipoMovimiento = "ingreso",
                    Categoria      = "Cosecha miel",
                    Descripcion    = $"Cosecha {cosecha.TipoMiel} — {cosecha.PesoNeto:F1} kg netos",
                    Fecha          = cosecha.Fecha,
                    Monto          = 0,
                    ApiarioNombre  = cosecha.ApiarioNombre,
                });
            }

            await _db.SaveChangesAsync();
            TempData["Exito"] = "Cosecha registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
