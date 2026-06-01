using System.Text;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly AppDbContext _ctx;

        public ProduccionController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index()
        {
            var cosechas = _ctx.Cosechas.ToList();
            var totalKg  = cosechas.Sum(c => c.PesoNeto);
            var mejor    = cosechas.GroupBy(c => c.ApiarioNombre)
                                   .OrderByDescending(g => g.Sum(c => c.PesoNeto))
                                   .FirstOrDefault();

            ViewBag.TotalKg      = Math.Round(totalKg, 1);
            ViewBag.MejorApiario = mejor?.Key ?? "—";
            ViewBag.MejorKg      = mejor is not null ? Math.Round(mejor.Sum(c => c.PesoNeto), 1) : 0;
            ViewBag.VariacionPct = "—";
            return View(cosechas);
        }

        public IActionResult Crear() => View(new Cosecha { Fecha = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Cosecha cosecha)
        {
            if (!ModelState.IsValid) return View(cosecha);
            _ctx.Cosechas.Add(cosecha);
            _ctx.SaveChanges();
            TempData["Exito"] = "Cosecha registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var cosecha = _ctx.Cosechas.Find(id);
            if (cosecha is null) return NotFound();
            return View(cosecha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Cosecha cosecha)
        {
            if (id != cosecha.Id) return BadRequest();
            if (!ModelState.IsValid) return View(cosecha);
            _ctx.Cosechas.Update(cosecha);
            _ctx.SaveChanges();
            TempData["Exito"] = "Cosecha actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var cosecha = _ctx.Cosechas.Find(id);
            if (cosecha is not null) { _ctx.Cosechas.Remove(cosecha); _ctx.SaveChanges(); TempData["Exito"] = "Cosecha eliminada."; }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Exportar()
        {
            var cosechas = _ctx.Cosechas.OrderBy(c => c.Fecha).ToList();
            ViewBag.TotalKg    = Math.Round(cosechas.Sum(c => c.PesoNeto), 1);
            ViewBag.Promedio   = cosechas.Any() ? Math.Round(cosechas.Average(c => c.PesoNeto), 1) : 0;
            var mejor = cosechas.OrderByDescending(c => c.PesoNeto).FirstOrDefault();
            ViewBag.MejorCosecha = mejor is not null ? $"{mejor.ApiarioNombre} · {mejor.PesoNeto} kg" : "—";
            return View(cosechas);
        }

        public IActionResult ExportarCsv()
        {
            var cosechas = _ctx.Cosechas.OrderByDescending(c => c.Fecha).ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Apiario,Fecha,Tipo Miel,Alzas,Peso Bruto (kg),Merma (kg),Peso Neto (kg),Humedad (%),HMF,Destino");
            foreach (var c in cosechas)
                sb.AppendLine($"{c.ApiarioNombre},{c.Fecha:dd/MM/yyyy},{c.TipoMiel},{c.AlzasCosechadas},{c.PesoBruto},{c.Merma},{c.PesoNeto},{c.Humedad},{c.HMF},{c.Destino}");
            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"produccion_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
