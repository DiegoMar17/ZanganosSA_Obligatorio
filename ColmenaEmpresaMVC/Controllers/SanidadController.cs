using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class SanidadController : Controller
    {
        private readonly AppDbContext _ctx;
        private const int PageSize = 10;

        public SanidadController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index(int page = 1, string? q = null)
        {
            var todos = _ctx.ControlesSanitarios.ToList();
            ViewBag.ControlesActivos = todos.Count(c => c.Estado == "en_tratamiento");
            ViewBag.VarroaDetectado  = todos.Count(c => c.TipoControl.Contains("Varroa") && c.Resultado == "positivo");
            ViewBag.TratadosOk       = todos.Count(c => c.Estado == "limpio");

            var query = todos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c =>
                    c.ApiarioNombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.TipoControl.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.ColmenasAfectadas.Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.OrderByDescending(c => c.Fecha).Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<ControlSanitario>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        private void CargarApiarios() =>
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");

        public IActionResult Exportar()
        {
            var controles = _ctx.ControlesSanitarios.OrderBy(c => c.Fecha).ToList();
            ViewBag.Aplicados  = controles.Count(c => c.Estado == "limpio");
            ViewBag.Pendientes = controles.Count(c => c.Estado == "en_tratamiento");
            ViewBag.MasUsado   = controles.GroupBy(c => c.Tratamiento).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "—";
            ViewBag.Afectadas  = controles.SelectMany(c => c.ColmenasAfectadas.Split(',', StringSplitOptions.RemoveEmptyEntries)).Distinct().Count();
            return View(controles);
        }

        public IActionResult Crear() { CargarApiarios(); return View(new ControlSanitario { Fecha = DateTime.Today }); }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(ControlSanitario control)
        {
            if (!ModelState.IsValid) { CargarApiarios(); return View(control); }
            var apiario = _ctx.Apiarios.Find(control.ApiarioId);
            control.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.ControlesSanitarios.Add(control);
            _ctx.SaveChanges();
            TempData["Exito"] = "Control sanitario registrado.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var c = _ctx.ControlesSanitarios.Find(id);
            if (c is null) return NotFound();
            CargarApiarios(); return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(int id, ControlSanitario control)
        {
            if (id != control.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(control); }
            var apiario = _ctx.Apiarios.Find(control.ApiarioId);
            control.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.ControlesSanitarios.Update(control);
            _ctx.SaveChanges();
            TempData["Exito"] = "Control sanitario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var c = _ctx.ControlesSanitarios.Find(id);
            if (c is not null) { _ctx.ControlesSanitarios.Remove(c); _ctx.SaveChanges(); TempData["Exito"] = "Control eliminado."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
