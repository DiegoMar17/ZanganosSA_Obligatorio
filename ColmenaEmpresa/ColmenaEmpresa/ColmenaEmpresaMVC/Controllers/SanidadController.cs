using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class SanidadController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SanidadController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var controles = await _db.ControlesSanitarios.OrderByDescending(c => c.Fecha).ToListAsync();
            ViewBag.ControlesActivos = controles.Count(c => c.Estado == "en_tratamiento");
            ViewBag.VarroaDetectado  = controles.Count(c => c.TipoControl.Contains("Varroa") && c.Resultado == "positivo");
            ViewBag.TratadosOk       = controles.Count(c => c.Estado == "limpio");
            return View(controles);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
            return View(new ControlSanitario { Fecha = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ControlSanitario control)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = await _db.Apiarios.ToListAsync();
                return View(control);
            }

            var apiario = await _db.Apiarios.FindAsync(control.ApiarioId);
            control.ApiarioNombre = apiario?.Nombre ?? string.Empty;

            _db.ControlesSanitarios.Add(control);
            await _db.SaveChangesAsync();
            TempData["Exito"] = "Control sanitario registrado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
