using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColmenaEmpresa.Controllers
{
    public class PlanificacionController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AuditoriaService _auditoria;

        public PlanificacionController(AppDbContext ctx, UserManager<ApplicationUser> users, AuditoriaService auditoria)
        {
            _ctx      = ctx;
            _users    = users;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index()
        {
            var userId  = _users.GetUserId(User);
            var esAdmin = User.IsInRole("ADMIN");

            IEnumerable<Tarea> tareas;
            if (esAdmin)
                tareas = _ctx.Tareas.OrderBy(t => t.Completada).ThenByDescending(t => t.FechaCreacion).ToList();
            else
                tareas = _ctx.Tareas
                    .Where(t => t.AsignadoAId == userId)
                    .OrderBy(t => t.Completada).ThenByDescending(t => t.FechaCreacion).ToList();

            var visitas = _ctx.Visitas.OrderBy(v => v.Estado).ThenBy(v => v.FechaPlanificada).ToList();

            ViewBag.Pendientes      = tareas.Count(t => !t.Completada);
            ViewBag.VisitasPend     = visitas.Count(v => v.Estado == "planificada");
            ViewBag.Tareas          = tareas;
            ViewBag.EsAdmin         = esAdmin;
            ViewBag.NombresApiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Nombre", "Nombre");

            if (esAdmin)
            {
                var empleados = await _users.GetUsersInRoleAsync("EMPLEADO");
                ViewBag.Empleados = empleados.OrderBy(e => e.NombreCompleto).ToList();
            }

            return View(visitas);
        }

        // ── Tareas ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTarea(string nombre, string categoria, string prioridad, DateTime? fechaVencimiento, string? asignadoAId)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var userId = _users.GetUserId(User)!;
                var user   = await _users.GetUserAsync(User);
                string nombre2Asignado = string.Empty;
                string? idAsignado = null;

                if (User.IsInRole("ADMIN") && !string.IsNullOrEmpty(asignadoAId))
                {
                    var empleado = await _users.FindByIdAsync(asignadoAId);
                    if (empleado is not null)
                    {
                        idAsignado     = empleado.Id;
                        nombre2Asignado = empleado.NombreCompleto;
                    }
                }
                else if (!User.IsInRole("ADMIN"))
                {
                    idAsignado     = userId;
                    nombre2Asignado = user?.NombreCompleto ?? string.Empty;
                }

                _ctx.Tareas.Add(new Tarea
                {
                    Nombre           = nombre,
                    Categoria        = string.IsNullOrWhiteSpace(categoria) ? "General" : categoria,
                    Prioridad        = prioridad ?? "media",
                    FechaVencimiento = fechaVencimiento,
                    FechaCreacion    = DateTime.Now,
                    AsignadoAId      = idAsignado,
                    AsignadoNombre   = nombre2Asignado
                });
                _ctx.SaveChanges();

                _auditoria.Registrar(userId, user?.NombreCompleto ?? userId, "CREATE", "Tareas", $"Tarea: {nombre}");
                TempData["Exito"] = $"Tarea '{nombre}' creada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarTarea(int id)
        {
            var t = _ctx.Tareas.Find(id);
            if (t is not null)
            {
                var userId = _users.GetUserId(User)!;
                if (!User.IsInRole("ADMIN") && t.AsignadoAId != userId)
                    return Forbid();
                t.Completada = !t.Completada;
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(userId, user?.NombreCompleto ?? userId, "UPDATE", "Tareas", $"Completar tarea #{id}");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarTarea(int id)
        {
            var t = _ctx.Tareas.Find(id);
            if (t is not null)
            {
                var userId = _users.GetUserId(User)!;
                if (!User.IsInRole("ADMIN") && t.AsignadoAId != userId)
                    return Forbid();
                _ctx.Tareas.Remove(t);
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(userId, user?.NombreCompleto ?? userId, "DELETE", "Tareas", $"Tarea #{id}");
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Visitas ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearVisita(string apiarioNombre, DateTime fechaPlanificada, string? materiales)
        {
            if (!string.IsNullOrWhiteSpace(apiarioNombre))
            {
                // Verificar que empleado solo cree visitas en su sector
                if (!User.IsInRole("ADMIN"))
                {
                    var user2 = await _users.GetUserAsync(User);
                    if (user2?.ApiarioAsignadoId.HasValue == true)
                    {
                        var apiario = _ctx.Apiarios.Find(user2.ApiarioAsignadoId.Value);
                        if (apiario is not null && apiario.Nombre != apiarioNombre)
                        {
                            TempData["Error"] = "Solo podés registrar visitas en tu sector asignado.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                _ctx.Visitas.Add(new Visita
                {
                    ApiarioNombre    = apiarioNombre,
                    FechaPlanificada = fechaPlanificada,
                    Materiales       = materiales ?? string.Empty,
                    Estado           = "planificada"
                });
                _ctx.SaveChanges();

                var userId = _users.GetUserId(User)!;
                var user   = await _users.GetUserAsync(User);
                _auditoria.Registrar(userId, user?.NombreCompleto ?? userId, "CREATE", "Visitas", $"Visita a {apiarioNombre}");
                TempData["Exito"] = $"Visita a {apiarioNombre} planificada.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CompletarVisita(int id)
        {
            var v = _ctx.Visitas.Find(id);
            if (v is not null) { v.Estado = v.Estado == "planificada" ? "completada" : "planificada"; _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EliminarVisita(int id)
        {
            var v = _ctx.Visitas.Find(id);
            if (v is not null) { _ctx.Visitas.Remove(v); _ctx.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
