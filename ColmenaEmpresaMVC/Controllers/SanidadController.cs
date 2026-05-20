using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class SanidadController : Controller
    {
        private static readonly List<ControlSanitario> _controles = new()
        {
            new() { Id=1, ApiarioNombre="La Rinconada", ColmenasAfectadas="C-47",      TipoControl="Varroa",  Resultado="positivo", Tratamiento="Ácido oxálico",  Fecha=new DateTime(2024,4,20), Estado="en_tratamiento" },
            new() { Id=2, ApiarioNombre="Monte Olivo",  ColmenasAfectadas="C-12",      TipoControl="Nosema",  Resultado="negativo", Tratamiento="—",             Fecha=new DateTime(2024,4,18), Estado="limpio"         },
            new() { Id=3, ApiarioNombre="La Rinconada", ColmenasAfectadas="C-82",      TipoControl="Varroa",  Resultado="positivo", Tratamiento="Ácido fórmico", Fecha=new DateTime(2024,4,14), Estado="en_tratamiento" },
        };

        public IActionResult Index()
        {
            ViewBag.ControlesActivos = 12;
            ViewBag.VarroaDetectado  = 5;
            ViewBag.TratadosOk       = 7;
            return View(_controles);
        }

        public IActionResult Crear() => View(new ControlSanitario { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ControlSanitario control)
        {
            if (!ModelState.IsValid) return View(control);

            control.Id = _controles.Count + 1;
            _controles.Add(control);
            TempData["Exito"] = "Control sanitario registrado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
