using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;

namespace ColmenaEmpresa.Controllers
{
    public class SanidadController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AuditoriaService _auditoria;
        private const int PageSize = 10;

        public SanidadController(AppDbContext ctx, UserManager<ApplicationUser> users, AuditoriaService auditoria)
        {
            _ctx       = ctx;
            _users     = users;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index(int page = 1, string? q = null)
        {
            var todos = _ctx.ControlesSanitarios.ToList();

            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                todos = sectorId.HasValue ? todos.Where(c => c.ApiarioId == sectorId.Value).ToList() : new List<ControlSanitario>();
            }

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

        private async Task CargarApiariosAsync()
        {
            var apiarios = _ctx.Apiarios.OrderBy(a => a.Nombre).ToList();
            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                apiarios = sectorId.HasValue ? apiarios.Where(a => a.Id == sectorId.Value).ToList() : new List<Apiario>();
            }
            ViewBag.Apiarios = new SelectList(apiarios, "Id", "Nombre");
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Exportar()
        {
            var controles = _ctx.ControlesSanitarios.OrderBy(c => c.Fecha).ToList();
            ViewBag.Aplicados  = controles.Count(c => c.Estado == "limpio");
            ViewBag.Pendientes = controles.Count(c => c.Estado == "en_tratamiento");
            ViewBag.MasUsado   = controles.GroupBy(c => c.Tratamiento).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "—";
            ViewBag.Afectadas  = controles.SelectMany(c => c.ColmenasAfectadas.Split(',', StringSplitOptions.RemoveEmptyEntries)).Distinct().Count();
            return View(controles);
        }

        public async Task<IActionResult> Crear() { await CargarApiariosAsync(); return View(new ControlSanitario { Fecha = DateTime.Today }); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ControlSanitario control)
        {
            var user = await _users.GetUserAsync(User);

            if (!User.IsInRole("ADMIN") && user?.ApiarioAsignadoId != control.ApiarioId)
            {
                TempData["Error"] = "Solo podés registrar controles en tu sector asignado.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) { await CargarApiariosAsync(); return View(control); }
            var apiario = _ctx.Apiarios.Find(control.ApiarioId);
            control.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.ControlesSanitarios.Add(control);
            _ctx.SaveChanges();
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "CREATE", "ControlesSanitarios", control.ApiarioNombre);
            TempData["Exito"] = "Control sanitario registrado.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var c = _ctx.ControlesSanitarios.Find(id);
            if (c is null) return NotFound();
            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Editar(int id, ControlSanitario control)
        {
            if (id != control.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre");
                return View(control);
            }
            var apiario = _ctx.Apiarios.Find(control.ApiarioId);
            control.ApiarioNombre = apiario?.Nombre ?? string.Empty;
            _ctx.ControlesSanitarios.Update(control);
            _ctx.SaveChanges();
            var user = await _users.GetUserAsync(User);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "UPDATE", "ControlesSanitarios", control.ApiarioNombre);
            TempData["Exito"] = "Control sanitario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var c = _ctx.ControlesSanitarios.Find(id);
            if (c is not null)
            {
                _ctx.ControlesSanitarios.Remove(c);
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(user!.Id, user.NombreCompleto, "DELETE", "ControlesSanitarios", c.ApiarioNombre);
                TempData["Exito"] = "Control eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
