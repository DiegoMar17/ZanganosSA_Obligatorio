using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly AppDbContext _ctx;

        public ProduccionController(AppDbContext ctx) => _ctx = ctx;

        private void CargarApiarios() =>
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");

        public IActionResult Index()
        {
            var cosechas = _ctx.Cosechas.OrderByDescending(c => c.Fecha).ToList();
            var vendidas = cosechas.Where(c => c.Vendida).ToList();
            var mejor    = cosechas.GroupBy(c => c.ApiarioNombre)
                                   .OrderByDescending(g => g.Sum(c => c.PesoNeto))
                                   .FirstOrDefault();

            ViewBag.TotalKg      = Math.Round(cosechas.Sum(c => c.PesoNeto), 1);
            ViewBag.Ingresos     = vendidas.Sum(c => c.MontoVenta);
            ViewBag.KgVendidos   = Math.Round(vendidas.Sum(c => c.PesoNeto), 1);
            ViewBag.MejorApiario = mejor?.Key ?? "—";
            ViewBag.MejorKg      = mejor is not null ? Math.Round(mejor.Sum(c => c.PesoNeto), 1) : 0;
            ViewBag.Cantidad     = cosechas.Count;
            return View(cosechas);
        }

        public IActionResult Crear() { CargarApiarios(); return View(new Cosecha { Fecha = DateTime.Today }); }

        // Genera un ingreso en Finanzas a partir de una cosecha vendida.
        private void GenerarIngreso(Cosecha cosecha)
        {
            if (!cosecha.Vendida || cosecha.MontoVenta <= 0) return;
            _ctx.RegistrosFinancieros.Add(new RegistroFinanciero
            {
                TipoMovimiento = "ingreso",
                Categoria      = "Venta de miel",
                Descripcion    = $"Venta cosecha {cosecha.PesoNeto} kg ({cosecha.TipoMiel}) — {cosecha.ApiarioNombre}",
                Fecha          = cosecha.Fecha,
                Monto          = cosecha.MontoVenta,
                ApiarioNombre  = cosecha.ApiarioNombre
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Cosecha cosecha)
        {
            if (!ModelState.IsValid) { CargarApiarios(); return View(cosecha); }
            var apiario = _ctx.Apiarios.Find(cosecha.ApiarioId);
            cosecha.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.Cosechas.Add(cosecha);
            GenerarIngreso(cosecha);
            _ctx.SaveChanges();
            TempData["Exito"] = cosecha.Vendida && cosecha.MontoVenta > 0
                ? $"Cosecha registrada e ingreso de ${cosecha.MontoVenta} cargado en Finanzas."
                : "Cosecha registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var cosecha = _ctx.Cosechas.Find(id);
            if (cosecha is null) return NotFound();
            CargarApiarios(); return View(cosecha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Cosecha cosecha)
        {
            if (id != cosecha.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(cosecha); }
            var apiario = _ctx.Apiarios.Find(cosecha.ApiarioId);
            cosecha.ApiarioNombre = apiario?.Nombre ?? string.Empty;
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
