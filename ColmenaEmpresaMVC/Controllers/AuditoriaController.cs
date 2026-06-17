using ColmenaEmpresa.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AuditoriaController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private const int PageSize = 20;

        public AuditoriaController(AppDbContext ctx, UserManager<ApplicationUser> users)
        {
            _ctx   = ctx;
            _users = users;
        }

        public async Task<IActionResult> Index(string? userId = null, string? accion = null, string? tabla = null,
            DateTime? desde = null, DateTime? hasta = null, int page = 1)
        {
            var query = _ctx.Auditorias.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);
            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);
            if (!string.IsNullOrEmpty(tabla))
                query = query.Where(a => a.Tabla == tabla);
            if (desde.HasValue)
                query = query.Where(a => a.FechaHora >= desde.Value);
            if (hasta.HasValue)
                query = query.Where(a => a.FechaHora <= hasta.Value.AddDays(1));

            var total = query.Count();
            var items = query.OrderByDescending(a => a.FechaHora)
                .Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var empleados = await _users.GetUsersInRoleAsync("EMPLEADO");
            var admins    = await _users.GetUsersInRoleAsync("ADMIN");
            ViewBag.Usuarios = empleados.Concat(admins).OrderBy(u => u.NombreCompleto).ToList();
            ViewBag.Tablas   = _ctx.Auditorias.Select(a => a.Tabla).Distinct().OrderBy(t => t).ToList();
            ViewBag.FiltroUserId = userId;
            ViewBag.FiltroAccion = accion;
            ViewBag.FiltroTabla  = tabla;
            ViewBag.FiltroDesde  = desde;
            ViewBag.FiltroHasta  = hasta;

            return View(new PagedResult<Auditoria>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total
            });
        }

        public async Task<IActionResult> HistorialAccesos(string? userId = null, int page = 1)
        {
            var query = _ctx.HistorialesAcceso.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
                query = query.Where(h => h.UserId == userId);

            var total = query.Count();
            var items = query.OrderByDescending(h => h.FechaHora)
                .Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var empleados = await _users.GetUsersInRoleAsync("EMPLEADO");
            var admins    = await _users.GetUsersInRoleAsync("ADMIN");
            ViewBag.Usuarios     = empleados.Concat(admins).OrderBy(u => u.NombreCompleto).ToList();
            ViewBag.FiltroUserId = userId;

            return View(new PagedResult<HistorialAcceso>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total
            });
        }
    }
}
