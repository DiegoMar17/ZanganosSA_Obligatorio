using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    /// <summary>
    /// CRUD para apiarios (ubicaciones de colmenas).
    /// </summary>
    public class ApiariosController : Controller
    {
        // Datos de ejemplo en memoria (reemplazar con base de datos)
        private static readonly List<Apiario> _apiarios = new()
        {
            new() { Id=1, Nombre="La Rinconada",  Departamento="San José",  Ubicacion="34°23'S 56°41'W", EstadoSemaforo="rojo",     TotalColmenas=18, Flora="Eucaliptal",   Acceso="Todo tiempo" },
            new() { Id=2, Nombre="Monte Olivo",   Departamento="Canelones", Ubicacion="34°28'S 56°15'W", EstadoSemaforo="verde",    TotalColmenas=24, Flora="Monte nativo", Acceso="Todo tiempo" },
            new() { Id=3, Nombre="El Eucaliptal", Departamento="Lavalleja", Ubicacion="34°19'S 55°38'W", EstadoSemaforo="amarillo", TotalColmenas=15, Flora="Eucaliptal",   Acceso="Solo con buen tiempo" },
            new() { Id=4, Nombre="Paso Carrasco", Departamento="Rocha",     Ubicacion="34°08'S 54°55'W", EstadoSemaforo="amarillo", TotalColmenas=21, Flora="Pradera",      Acceso="Requiere 4x4" },
        };

        // GET: /Apiarios
        public IActionResult Index() => View(_apiarios);

        // GET: /Apiarios/Crear
        public IActionResult Crear() => View(new Apiario());

        // POST: /Apiarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Apiario apiario)
        {
            if (!ModelState.IsValid) return View(apiario);

            apiario.Id = _apiarios.Count + 1;
            _apiarios.Add(apiario);
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Apiarios/Detalle/1
        public IActionResult Detalle(int id)
        {
            var apiario = _apiarios.FirstOrDefault(a => a.Id == id);
            if (apiario is null) return NotFound();
            return View(apiario);
        }
    }
}
