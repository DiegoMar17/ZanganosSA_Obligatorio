using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class FinanzasController : Controller
    {
        private static readonly List<RegistroFinanciero> _registros = new()
        {
            new() { Id=1, TipoMovimiento="ingreso",   Categoria="Cosecha miel",  Descripcion="Venta primavera 2024",     Fecha=new DateTime(2024,11,15), Monto=1850, ApiarioNombre="Monte Olivo" },
            new() { Id=2, TipoMovimiento="gasto",     Categoria="Insumos",       Descripcion="Ácido oxálico + frames",   Fecha=new DateTime(2024,11,20), Monto=320,  ApiarioNombre="General" },
            new() { Id=3, TipoMovimiento="inversion",  Categoria="Equipamiento",  Descripcion="Extractor nuevo",          Fecha=new DateTime(2024,12,1),  Monto=2100, ApiarioNombre="General" },
            new() { Id=4, TipoMovimiento="ingreso",   Categoria="Polen",         Descripcion="Venta mercado local",      Fecha=new DateTime(2024,12,10), Monto=480,  ApiarioNombre="General" },
        };

        public IActionResult Index()
        {
            var ingresos = _registros.Where(r => r.TipoMovimiento == "ingreso").Sum(r => r.Monto);
            var gastos   = _registros.Where(r => r.TipoMovimiento != "ingreso").Sum(r => r.Monto);

            ViewBag.TotalIngresos = ingresos;
            ViewBag.TotalGastos   = gastos;
            ViewBag.Balance       = ingresos - gastos;

            return View(_registros);
        }

        public IActionResult Crear() => View(new RegistroFinanciero { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(RegistroFinanciero registro)
        {
            if (!ModelState.IsValid) return View(registro);

            registro.Id = _registros.Count + 1;
            _registros.Add(registro);
            TempData["Exito"] = "Registro financiero guardado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
