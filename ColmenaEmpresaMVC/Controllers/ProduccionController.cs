using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ProduccionController : Controller
    {
        private static readonly List<Cosecha> _cosechas = new()
        {
            new() { Id=1, ApiarioNombre="Monte Olivo",   Fecha=new DateTime(2024,11,10), TipoMiel="Multifloral", AlzasCosechadas=12, PesoBruto=855, Merma=15, Destino="Exportación" },
            new() { Id=2, ApiarioNombre="Paso Carrasco", Fecha=new DateTime(2024,11,20), TipoMiel="Eucalipto",   AlzasCosechadas=9,  PesoBruto=640, Merma=10, Destino="Fraccionado local" },
            new() { Id=3, ApiarioNombre="El Eucaliptal", Fecha=new DateTime(2024,12,5),  TipoMiel="Eucalipto",   AlzasCosechadas=8,  PesoBruto=515, Merma=10, Destino="Stock" },
            new() { Id=4, ApiarioNombre="La Rinconada",  Fecha=new DateTime(2024,12,18), TipoMiel="Monte nativo",AlzasCosechadas=7,  PesoBruto=415, Merma=10, Destino="Exportación" },
        };

        public IActionResult Index()
        {
            ViewBag.TotalKg        = 2380;
            ViewBag.MejorApiario   = "Monte Olivo";
            ViewBag.MejorKg        = 840;
            ViewBag.PromColmena    = 16;
            ViewBag.VariacionPct   = "+12%";
            return View(_cosechas);
        }

        public IActionResult Crear() => View(new Cosecha { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Cosecha cosecha)
        {
            if (!ModelState.IsValid) return View(cosecha);

            cosecha.Id = _cosechas.Count + 1;
            _cosechas.Add(cosecha);
            TempData["Exito"] = "Cosecha registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
