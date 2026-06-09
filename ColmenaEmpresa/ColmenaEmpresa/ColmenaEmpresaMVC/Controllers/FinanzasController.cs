using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class FinanzasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public FinanzasController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var registros = await _db.RegistrosFinancieros.OrderByDescending(r => r.Fecha).ToListAsync();

            ViewBag.TotalIngresos  = registros.Where(r => r.TipoMovimiento == "ingreso").Sum(r => r.Monto);
            ViewBag.TotalGastos    = registros.Where(r => r.TipoMovimiento == "gasto").Sum(r => r.Monto);
            ViewBag.TotalInversion = registros.Where(r => r.TipoMovimiento == "inversion").Sum(r => r.Monto);
            ViewBag.Balance        = (decimal)ViewBag.TotalIngresos - (decimal)ViewBag.TotalGastos;

            return View(registros);
        }

        public IActionResult Crear() => View(new RegistroFinanciero { Fecha = DateTime.Today });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RegistroFinanciero registro)
        {
            if (!ModelState.IsValid) return View(registro);

            _db.RegistrosFinancieros.Add(registro);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Registro financiero guardado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
