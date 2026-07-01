using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;

namespace ColmenaEmpresa.Controllers
{
    public class InspeccionesController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AuditoriaService _auditoria;
        private const int PageSize = 10;

        public InspeccionesController(AppDbContext ctx, UserManager<ApplicationUser> users, AuditoriaService auditoria)
        {
            _ctx       = ctx;
            _users     = users;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index(int page = 1, string? q = null)
        {
            var vencibles = _ctx.Inspecciones
                .Where(i => i.Estado == "pendiente" && i.Fecha < DateTime.Today)
                .ToList();
            if (vencibles.Any())
            {
                foreach (var v in vencibles) v.Estado = "vencida";
                _ctx.SaveChanges();
            }

            var todas = _ctx.Inspecciones.Include(i => i.Apiario).ToList();

            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                todas = sectorId.HasValue ? todas.Where(i => i.ApiarioId == sectorId.Value).ToList() : new List<Inspeccion>();
            }

            ViewBag.Pendientes  = todas.Count(i => i.Estado == "pendiente");
            ViewBag.Vencidas    = todas.Count(i => i.Estado == "vencida");
            ViewBag.EsteMes     = todas.Count(i => i.Fecha.Month == DateTime.Now.Month && i.Fecha.Year == DateTime.Now.Year);
            ViewBag.Completadas = todas.Count(i => i.Estado == "completa");

            var query = todas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(i =>
                    (i.Apiario!.Nombre).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    i.Estado.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (i.Clima ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.OrderByDescending(i => i.Fecha).Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<Inspeccion>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        private async Task CargarDatosAsync()
        {
            var apiarios = _ctx.Apiarios.OrderBy(a => a.Nombre).ToList();
            var colmenas = _ctx.Colmenas.Include(c => c.Apiario).OrderBy(c => c.Apiario!.Nombre).ThenBy(c => c.Codigo).ToList();

            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                apiarios = sectorId.HasValue ? apiarios.Where(a => a.Id == sectorId.Value).ToList() : new List<Apiario>();
                colmenas = sectorId.HasValue ? colmenas.Where(c => c.ApiarioId == sectorId.Value).ToList() : new List<Colmena>();
            }

            ViewBag.Apiarios = new SelectList(apiarios, "Id", "Nombre");
            ViewBag.Colmenas = colmenas;
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Exportar()
        {
            var inspecciones = _ctx.Inspecciones.Include(i => i.Apiario).OrderBy(i => i.Fecha).ToList();
            ViewBag.Completas  = inspecciones.Count(i => i.Estado == "completa");
            ViewBag.Alertas    = inspecciones.Count(i => i.Estado == "incompleta");
            ViewBag.Criticas   = inspecciones.Count(i => i.Estado == "pendiente");
            ViewBag.PromMarcos = inspecciones.Any() ? Math.Round(inspecciones.Average(i => (double)i.ColmenasInspeccionadas), 1) : 0;
            return View(inspecciones);
        }

        public async Task<IActionResult> Crear() { await CargarDatosAsync(); return View(new Inspeccion { Fecha = DateTime.Today }); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Inspeccion inspeccion)
        {
            inspeccion.Estado = "pendiente";
            inspeccion.ColmenasInspeccionadas = 0;

            if (inspeccion.TipoInspeccion == "colmena" && inspeccion.ColmenaId.HasValue)
            {
                var colmena = _ctx.Colmenas.Find(inspeccion.ColmenaId.Value);
                if (colmena != null)
                {
                    inspeccion.ApiarioId     = colmena.ApiarioId;
                    inspeccion.TotalColmenas = 1;
                    ModelState.Remove(nameof(Inspeccion.ApiarioId));
                }
            }
            else if (inspeccion.TipoInspeccion == "apiario" && inspeccion.ApiarioId > 0)
            {
                inspeccion.TotalColmenas = _ctx.Colmenas.Count(c => c.ApiarioId == inspeccion.ApiarioId);
                ModelState.Remove(nameof(Inspeccion.ColmenaId));
            }

            var user = await _users.GetUserAsync(User);
            if (!User.IsInRole("ADMIN") && user?.ApiarioAsignadoId != inspeccion.ApiarioId)
            {
                TempData["Error"] = "Solo podés registrar inspecciones en tu sector asignado.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) { await CargarDatosAsync(); return View(inspeccion); }

            _ctx.Inspecciones.Add(inspeccion);

            var colmenas = _ctx.Colmenas.Where(c => c.ApiarioId == inspeccion.ApiarioId).ToList();
            foreach (var c in colmenas)
                c.UltimaVisita = inspeccion.Fecha;

            _ctx.SaveChanges();
            var apiario = _ctx.Apiarios.Find(inspeccion.ApiarioId);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "CREATE", "Inspecciones", apiario?.Nombre ?? "");
            TempData["Exito"] = "Inspección registrada.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var i = _ctx.Inspecciones.Include(x => x.Apiario).Include(x => x.Colmena).FirstOrDefault(x => x.Id == id);
            if (i is null) return NotFound();
            return View(i);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Editar(int id, Inspeccion inspeccion)
        {
            if (id != inspeccion.Id) return BadRequest();
            if (!ModelState.IsValid) return View(inspeccion);

            if (inspeccion.Estado == "completa")
                inspeccion.ColmenasInspeccionadas = inspeccion.TotalColmenas;

            _ctx.Inspecciones.Update(inspeccion);

            var colmenas = _ctx.Colmenas.Where(c => c.ApiarioId == inspeccion.ApiarioId).ToList();
            foreach (var c in colmenas)
                c.UltimaVisita = inspeccion.Fecha;

            _ctx.SaveChanges();
            var user    = await _users.GetUserAsync(User);
            var apiario = _ctx.Apiarios.Find(inspeccion.ApiarioId);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "UPDATE", "Inspecciones", apiario?.Nombre ?? "");
            TempData["Exito"] = "Inspección actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Completar(int id)
        {
            var i = _ctx.Inspecciones.Find(id);
            if (i is not null)
            {
                var user = await _users.GetUserAsync(User);
                if (!User.IsInRole("ADMIN") && user?.ApiarioAsignadoId != i.ApiarioId)
                    return Forbid();

                i.Estado = "completa";
                i.ColmenasInspeccionadas = i.TotalColmenas;
                _ctx.SaveChanges();
                _auditoria.Registrar(user!.Id, user.NombreCompleto, "UPDATE", "Inspecciones", $"Completar #{id}");
                TempData["Exito"] = "Inspección marcada como completada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var i = _ctx.Inspecciones.Include(x => x.Apiario).FirstOrDefault(x => x.Id == id);
            if (i is not null)
            {
                var nombre = i.Apiario?.Nombre ?? "";
                _ctx.Inspecciones.Remove(i);
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(user!.Id, user.NombreCompleto, "DELETE", "Inspecciones", nombre);
                TempData["Exito"] = "Inspección eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
