using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;

namespace ColmenaEmpresa.Controllers
{
    public class ColmenasController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AuditoriaService _auditoria;
        private const int PageSize = 10;

        public ColmenasController(AppDbContext ctx, UserManager<ApplicationUser> users, AuditoriaService auditoria)
        {
            _ctx       = ctx;
            _users     = users;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index(int page = 1, string? q = null)
        {
            var todas = _ctx.Colmenas.ToList();

            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                todas = sectorId.HasValue ? todas.Where(c => c.ApiarioId == sectorId.Value).ToList() : new List<Colmena>();
            }

            // Códigos únicos de colmenas bajo tratamiento sanitario activo
            var enTratamiento = _ctx.ControlesSanitarios
                .Where(cs => cs.Estado == "en_tratamiento")
                .ToList()
                .SelectMany(cs => cs.ColmenasAfectadas.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .Distinct()
                .Count();

            ViewBag.Resumen = new
            {
                Total         = todas.Count,
                EnProduccion  = todas.Count(c => c.CantidadAlzas > 0),
                EnTratamiento = enTratamiento,
                SinReina      = todas.Count(c => c.EstadoReina == "ausente")
            };

            var query = todas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c =>
                    c.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.ApiarioNombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    c.EstadoReina.Contains(q, StringComparison.OrdinalIgnoreCase));

            var total = query.Count();
            var items = query.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return View(new PagedResult<Colmena>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total, Q = q
            });
        }

        private void CargarApiarios(ApplicationUser? user = null)
        {
            var apiarios = user is not null && !User.IsInRole("ADMIN")
                ? _ctx.Apiarios.Where(a => a.Id == user.ApiarioAsignadoId).ToList()
                : _ctx.Apiarios.OrderBy(a => a.Nombre).ToList();
            ViewBag.Apiarios = new SelectList(apiarios, "Id", "Nombre");
        }

        public async Task<IActionResult> Crear()
        {
            var user = await _users.GetUserAsync(User);
            if (!User.IsInRole("ADMIN") && user?.ApiarioAsignadoId is null)
            {
                TempData["Error"] = "No tenés un sector asignado: pedile al administrador que te asigne un apiario primero.";
                return RedirectToAction(nameof(Index));
            }
            CargarApiarios(user);
            var colmena = new Colmena { FechaInstalacion = DateTime.Today };
            if (!User.IsInRole("ADMIN")) colmena.ApiarioId = user!.ApiarioAsignadoId!.Value;
            return View(colmena);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Colmena colmena)
        {
            var user = await _users.GetUserAsync(User);

            if (!User.IsInRole("ADMIN"))
            {
                if (user?.ApiarioAsignadoId is null || colmena.ApiarioId != user.ApiarioAsignadoId.Value)
                {
                    TempData["Error"] = "Solo podés registrar colmenas dentro de tu sector asignado.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!ModelState.IsValid) { CargarApiarios(user); return View(colmena); }
            var apiario = _ctx.Apiarios.Find(colmena.ApiarioId);
            colmena.ApiarioNombre = apiario?.Nombre ?? string.Empty;

            if (!User.IsInRole("ADMIN"))
            {
                // El empleado queda como responsable de la colmena que registra
                colmena.AsignadoAId    = user!.Id;
                colmena.AsignadoNombre = user.NombreCompleto;
            }

            _ctx.Colmenas.Add(colmena);
            _ctx.SaveChanges();
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "CREATE", "Colmenas", colmena.Codigo);
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' registrada.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var c = _ctx.Colmenas.Find(id);
            if (c is null) return NotFound();
            CargarApiarios(); return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Editar(int id, Colmena colmena)
        {
            if (id != colmena.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(colmena); }

            var existente = _ctx.Colmenas.Find(id);
            if (existente is null) return NotFound();

            var apiario = _ctx.Apiarios.Find(colmena.ApiarioId);
            existente.Codigo           = colmena.Codigo;
            existente.ApiarioId        = colmena.ApiarioId;
            existente.ApiarioNombre    = apiario?.Nombre ?? string.Empty;
            existente.Tipo             = colmena.Tipo;
            existente.FechaInstalacion = colmena.FechaInstalacion;
            existente.Origen           = colmena.Origen;
            existente.EstadoReina      = colmena.EstadoReina;
            existente.CantidadAlzas    = colmena.CantidadAlzas;
            existente.MarcosConCria    = colmena.MarcosConCria;
            existente.EstadoSemaforo   = colmena.EstadoSemaforo;
            existente.Observaciones    = colmena.Observaciones;
            // AsignadoAId / AsignadoNombre / UltimaVisita no viajan en este formulario: se preservan tal cual.
            _ctx.SaveChanges();
            var user = await _users.GetUserAsync(User);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "UPDATE", "Colmenas", colmena.Codigo);
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var c = _ctx.Colmenas.Find(id);
            if (c is not null)
            {
                _ctx.Colmenas.Remove(c);
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(user!.Id, user.NombreCompleto, "DELETE", "Colmenas", c.Codigo);
                TempData["Exito"] = "Colmena eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
