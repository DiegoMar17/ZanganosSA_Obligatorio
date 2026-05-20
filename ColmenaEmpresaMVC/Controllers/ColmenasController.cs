using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    /// <summary>
    /// CRUD para colmenas.
    /// </summary>
    public class ColmenasController : Controller
    {
        private static readonly List<Colmena> _colmenas = new()
        {
            new() { Id=1, Codigo="C-01",  ApiarioNombre="Monte Olivo",   EstadoReina="vista",    EstadoSemaforo="verde",    CantidadAlzas=2, UltimaVisita=DateTime.Now.AddDays(-4) },
            new() { Id=2, Codigo="C-47",  ApiarioNombre="La Rinconada",  EstadoReina="no_vista", EstadoSemaforo="amarillo", CantidadAlzas=1, UltimaVisita=DateTime.Now.AddDays(-18) },
            new() { Id=3, Codigo="C-82",  ApiarioNombre="La Rinconada",  EstadoReina="ausente",  EstadoSemaforo="rojo",     CantidadAlzas=0, UltimaVisita=DateTime.Now.AddDays(-25) },
            new() { Id=4, Codigo="C-110", ApiarioNombre="Paso Carrasco", EstadoReina="vista",    EstadoSemaforo="viaje",    CantidadAlzas=1, UltimaVisita=null },
        };

        // GET: /Colmenas
        public IActionResult Index()
        {
            var resumen = new
            {
                Total         = _colmenas.Count,
                EnProduccion  = 131,
                EnTratamiento = 12,
                SinReina      = _colmenas.Count(c => c.EstadoReina == "ausente")
            };
            ViewBag.Resumen = resumen;
            return View(_colmenas);
        }

        // GET: /Colmenas/Crear
        public IActionResult Crear() => View(new Colmena());

        // POST: /Colmenas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Colmena colmena)
        {
            if (!ModelState.IsValid) return View(colmena);

            colmena.Id = _colmenas.Count + 1;
            _colmenas.Add(colmena);
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
