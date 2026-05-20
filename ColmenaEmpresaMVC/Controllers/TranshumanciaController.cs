using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class TranshumanciaController : Controller
    {
        private static readonly List<Transhumancia> _traslados = new()
        {
            new() { Id=1, Nombre="Verano 2024",    ApiarioOrigen="Paso Carrasco", ApiarioDestino="El Trébol (temp.)", CantidadColmenas=21, DistanciaKm=184, FechaSalida=new DateTime(2024,1,1),  FechaRetorno=new DateTime(2024,6,30), Estado="en_curso",  PorcentajeAvance=45 },
            new() { Id=2, Nombre="Primavera 2023", ApiarioOrigen="Paso Carrasco", ApiarioDestino="La Rinconada",      CantidadColmenas=18, DistanciaKm=140, FechaSalida=new DateTime(2023,9,1),  FechaRetorno=new DateTime(2023,12,1), Estado="completado", PorcentajeAvance=100 },
            new() { Id=3, Nombre="Verano 2023",    ApiarioOrigen="Monte Olivo",   ApiarioDestino="El Eucaliptal",     CantidadColmenas=12, DistanciaKm=95,  FechaSalida=new DateTime(2023,12,15), FechaRetorno=new DateTime(2024,3,15), Estado="completado", PorcentajeAvance=100 },
        };

        public IActionResult Index() => View(_traslados);

        public IActionResult Crear() => View(new Transhumancia { FechaSalida = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Transhumancia traslado)
        {
            if (!ModelState.IsValid) return View(traslado);

            traslado.Id = _traslados.Count + 1;
            traslado.Estado = "planificado";
            _traslados.Add(traslado);
            TempData["Exito"] = "Traslado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
