using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class InspeccionesController : Controller
    {
        private static readonly List<Inspeccion> _inspecciones = new()
        {
            new() { Id=1, ApiarioNombre="Monte Olivo",   ColmenasInspeccionadas=18, TotalColmenas=24, Fecha=new DateTime(2024,4,18), Clima="⛅ Nublado",  Temperatura=19, Estado="completa"   },
            new() { Id=2, ApiarioNombre="El Eucaliptal", ColmenasInspeccionadas=12, TotalColmenas=15, Fecha=new DateTime(2024,4,14), Clima="🌧 Lluvia",   Temperatura=15, Estado="incompleta" },
            new() { Id=3, ApiarioNombre="Paso Carrasco", ColmenasInspeccionadas=21, TotalColmenas=21, Fecha=new DateTime(2024,4,10), Clima="☀ Soleado",   Temperatura=24, Estado="completa"   },
        };

        public IActionResult Index()
        {
            ViewBag.Pendientes  = 3;
            ViewBag.EsteMes     = 18;
            ViewBag.Completadas = 142;
            return View(_inspecciones);
        }

        public IActionResult Crear() => View(new Inspeccion { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Inspeccion inspeccion)
        {
            if (!ModelState.IsValid) return View(inspeccion);

            inspeccion.Id = _inspecciones.Count + 1;
            _inspecciones.Add(inspeccion);
            TempData["Exito"] = "Inspección registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
