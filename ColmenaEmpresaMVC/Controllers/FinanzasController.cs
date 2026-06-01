using System.Text;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class FinanzasController : Controller
    {
        private readonly AppDbContext _ctx;
        private const int PageSize = 10;

        public FinanzasController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index(int page = 1, string? q = null)
        {
            var todos = _ctx.RegistrosFinancieros.ToList();
            ViewBag.TotalIngresos = todos.Where(r => r.TipoMovimiento == "ingreso").Sum(r => r.Monto);
            ViewBag.TotalGastos   = todos.Where(r => r.TipoMovimiento != "ingreso").Sum(r => r.Monto);
            ViewBag.Balance       = (decimal)ViewBag.TotalIngresos - (decimal)ViewBag.TotalGastos;

            var query = todos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(r =>
                    r.Descripcion.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.Categoria.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.ApiarioNombre.Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.OrderByDescending(r => r.Fecha).Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<RegistroFinanciero>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        public IActionResult Crear() => View(new RegistroFinanciero { Fecha = DateTime.Today });

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(RegistroFinanciero registro)
        {
            if (!ModelState.IsValid) return View(registro);
            _ctx.RegistrosFinancieros.Add(registro);
            _ctx.SaveChanges();
            TempData["Exito"] = "Registro financiero guardado.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var r = _ctx.RegistrosFinancieros.Find(id);
            if (r is null) return NotFound();
            return View(r);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, RegistroFinanciero registro)
        {
            if (id != registro.Id) return BadRequest();
            if (!ModelState.IsValid) return View(registro);
            _ctx.RegistrosFinancieros.Update(registro);
            _ctx.SaveChanges();
            TempData["Exito"] = "Registro actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var r = _ctx.RegistrosFinancieros.Find(id);
            if (r is not null) { _ctx.RegistrosFinancieros.Remove(r); _ctx.SaveChanges(); TempData["Exito"] = "Registro eliminado."; }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Exportar()
        {
            var registros = _ctx.RegistrosFinancieros.OrderBy(r => r.Fecha).ToList();
            var ingresos  = registros.Where(r => r.TipoMovimiento == "ingreso").Sum(r => r.Monto);
            var gastos    = registros.Where(r => r.TipoMovimiento != "ingreso").Sum(r => r.Monto);
            ViewBag.TotalIngresos = ingresos;
            ViewBag.TotalGastos   = gastos;
            ViewBag.Balance       = ingresos - gastos;
            ViewBag.Margen        = ingresos > 0 ? Math.Round((ingresos - gastos) / ingresos * 100, 1) : 0m;
            return View(registros);
        }

        public IActionResult ExportarCsv()
        {
            var registros = _ctx.RegistrosFinancieros.OrderByDescending(r => r.Fecha).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Tipo,Categoría,Descripción,Fecha,Monto,Apiario");
            foreach (var r in registros)
                sb.AppendLine($"{r.TipoMovimiento},{r.Categoria},\"{r.Descripcion}\",{r.Fecha:dd/MM/yyyy},{r.Monto},{r.ApiarioNombre}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"finanzas_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
