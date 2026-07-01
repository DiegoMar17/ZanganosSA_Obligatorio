using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColmenaEmpresa.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class EquipoController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly AppDbContext _ctx;
        private readonly AuditoriaService _auditoria;

        public EquipoController(UserManager<ApplicationUser> users, AppDbContext ctx, AuditoriaService auditoria)
        {
            _users     = users;
            _ctx       = ctx;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _users.GetUsersInRoleAsync("EMPLEADO");

            var semanaAtras = DateTime.Now.AddDays(-7);

            var registrosPorUsuario = _ctx.Auditorias
                .Where(a => a.FechaHora >= semanaAtras)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Cantidad = g.Count() })
                .ToList();

            var ultimosAccesos = _ctx.HistorialesAcceso
                .Where(h => h.Exitoso)
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, Ultimo = g.Max(h => h.FechaHora) })
                .ToList();

            var vm = empleados.OrderBy(e => e.NombreCompleto).Select(e =>
            {
                var apiario = e.ApiarioAsignadoId.HasValue
                    ? _ctx.Apiarios.Find(e.ApiarioAsignadoId.Value)?.Nombre
                    : null;
                return new EmpleadoEquipoViewModel
                {
                    UserId           = e.Id,
                    NombreCompleto   = e.NombreCompleto,
                    Email            = e.Email ?? string.Empty,
                    ApiarioNombre    = apiario,
                    PinActivo        = e.PinActivo,
                    TienePin         = !string.IsNullOrEmpty(e.PinHash),
                    RegistrosSemana  = registrosPorUsuario.FirstOrDefault(r => r.UserId == e.Id)?.Cantidad ?? 0,
                    UltimoAcceso     = ultimosAccesos.FirstOrDefault(u => u.UserId == e.Id)?.Ultimo
                };
            }).ToList();

            ViewBag.TotalEmpleados   = vm.Count;
            ViewBag.EmpleadosActivos = vm.Count(e => e.PinActivo);
            return View(vm);
        }

        public async Task<IActionResult> Detalle(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var empleado = await _users.FindByIdAsync(id);
            if (empleado is null || !await _users.IsInRoleAsync(empleado, "EMPLEADO"))
                return NotFound();

            var apiario = empleado.ApiarioAsignadoId.HasValue
                ? _ctx.Apiarios.Find(empleado.ApiarioAsignadoId.Value)
                : null;

            var tareas = _ctx.Tareas
                .Where(t => t.AsignadoAId == empleado.Id)
                .OrderBy(t => t.Completada)
                .ThenByDescending(t => t.FechaCreacion)
                .ToList();

            var colmenasAsignadas = _ctx.Colmenas
                .Where(c => c.AsignadoAId == empleado.Id)
                .OrderBy(c => c.Codigo)
                .ToList();

            var colmenasDisponibles = apiario is not null
                ? _ctx.Colmenas
                    .Where(c => c.ApiarioId == apiario.Id && c.AsignadoAId == null)
                    .OrderBy(c => c.Codigo)
                    .ToList()
                : new List<Colmena>();

            var semanaAtras     = DateTime.Now.AddDays(-7);
            var registrosSemana = _ctx.Auditorias.Count(a => a.UserId == empleado.Id && a.FechaHora >= semanaAtras);
            var ultimoAcceso    = _ctx.HistorialesAcceso
                .Where(h => h.UserId == empleado.Id && h.Exitoso)
                .OrderByDescending(h => h.FechaHora)
                .Select(h => (DateTime?)h.FechaHora)
                .FirstOrDefault();

            var vm = new EmpleadoDetalleViewModel
            {
                UserId              = empleado.Id,
                NombreCompleto      = empleado.NombreCompleto,
                Email               = empleado.Email ?? string.Empty,
                ApiarioNombre       = apiario?.Nombre,
                ApiarioId           = apiario?.Id,
                PinActivo           = empleado.PinActivo,
                TienePin            = !string.IsNullOrEmpty(empleado.PinHash),
                RegistrosSemana     = registrosSemana,
                UltimoAcceso        = ultimoAcceso,
                Tareas              = tareas,
                ColmenasAsignadas   = colmenasAsignadas,
                ColmenasDisponibles = colmenasDisponibles
            };

            ViewBag.Apiarios = new SelectList(_ctx.Apiarios.OrderBy(a => a.Nombre).ToList(), "Id", "Nombre", apiario?.Id);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarTarea(int id, string userId)
        {
            var t = _ctx.Tareas.Find(id);
            if (t is not null)
            {
                t.Completada = !t.Completada;
                _ctx.SaveChanges();
                var admin = await _users.GetUserAsync(User);
                _auditoria.Registrar(_users.GetUserId(User)!, admin?.NombreCompleto ?? "Admin", "UPDATE", "Tareas", $"Admin marcó tarea #{id}");
            }
            return RedirectToAction(nameof(Detalle), new { id = userId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTarea(string userId, string nombre, string categoria, string prioridad, DateTime? fechaVencimiento)
        {
            var empleado = await _users.FindByIdAsync(userId);
            if (empleado is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                _ctx.Tareas.Add(new Tarea
                {
                    Nombre           = nombre,
                    Categoria        = string.IsNullOrWhiteSpace(categoria) ? "General" : categoria,
                    Prioridad        = prioridad ?? "media",
                    FechaVencimiento = fechaVencimiento,
                    FechaCreacion    = DateTime.Now,
                    AsignadoAId      = empleado.Id
                });
                _ctx.SaveChanges();

                var admin = await _users.GetUserAsync(User);
                _auditoria.Registrar(_users.GetUserId(User)!, admin?.NombreCompleto ?? "Admin", "CREATE", "Tareas", $"Tarea '{nombre}' asignada a {empleado.NombreCompleto}");
                TempData["Exito"] = $"Tarea '{nombre}' asignada a {empleado.NombreCompleto}.";
            }
            return RedirectToAction(nameof(Detalle), new { id = userId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarApiario(string userId, int? apiarioId)
        {
            var empleado = await _users.FindByIdAsync(userId);
            if (empleado is null) return NotFound();

            empleado.ApiarioAsignadoId = apiarioId;
            await _users.UpdateAsync(empleado);

            var colmenasFueraDeSector = _ctx.Colmenas
                .Where(c => c.AsignadoAId == empleado.Id && c.ApiarioId != apiarioId)
                .ToList();
            foreach (var c in colmenasFueraDeSector) c.AsignadoAId = null;

            var colmenasSinDueno = apiarioId.HasValue
                ? _ctx.Colmenas.Where(c => c.ApiarioId == apiarioId.Value && c.AsignadoAId == null).ToList()
                : new List<Colmena>();
            foreach (var c in colmenasSinDueno) c.AsignadoAId = empleado.Id;

            if (colmenasFueraDeSector.Count > 0 || colmenasSinDueno.Count > 0) _ctx.SaveChanges();

            var apiario = apiarioId.HasValue ? _ctx.Apiarios.Find(apiarioId.Value) : null;
            var admin   = await _users.GetUserAsync(User);
            _auditoria.Registrar(_users.GetUserId(User)!, admin?.NombreCompleto ?? "Admin", "UPDATE", "Empleados",
                $"Sector de {empleado.NombreCompleto} → {apiario?.Nombre ?? "Sin asignar"}");
            TempData["Exito"] = $"Sector actualizado para {empleado.NombreCompleto}.";
            return RedirectToAction(nameof(Detalle), new { id = userId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarColmena(string userId, int colmenaId)
        {
            var empleado = await _users.FindByIdAsync(userId);
            var colmena  = _ctx.Colmenas.Find(colmenaId);
            if (empleado is null || colmena is null) return NotFound();

            if (colmena.ApiarioId != empleado.ApiarioAsignadoId)
            {
                TempData["Error"] = "La colmena no pertenece al sector asignado del empleado.";
                return RedirectToAction(nameof(Detalle), new { id = userId });
            }

            colmena.AsignadoAId = empleado.Id;
            _ctx.SaveChanges();

            var admin = await _users.GetUserAsync(User);
            _auditoria.Registrar(_users.GetUserId(User)!, admin?.NombreCompleto ?? "Admin", "UPDATE", "Colmenas",
                $"Colmena {colmena.Codigo} asignada a {empleado.NombreCompleto}");
            TempData["Exito"] = $"Colmena '{colmena.Codigo}' asignada a {empleado.NombreCompleto}.";
            return RedirectToAction(nameof(Detalle), new { id = userId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarColmena(string userId, int colmenaId)
        {
            var colmena = _ctx.Colmenas.Find(colmenaId);
            if (colmena is not null)
            {
                var anteriorId = colmena.AsignadoAId;
                colmena.AsignadoAId = null;
                _ctx.SaveChanges();

                var anterior = anteriorId is not null ? (await _users.FindByIdAsync(anteriorId))?.NombreCompleto ?? anteriorId : "nadie";
                var admin    = await _users.GetUserAsync(User);
                _auditoria.Registrar(_users.GetUserId(User)!, admin?.NombreCompleto ?? "Admin", "UPDATE", "Colmenas",
                    $"Colmena {colmena.Codigo} desasignada de {anterior}");
                TempData["Exito"] = $"Colmena '{colmena.Codigo}' desasignada.";
            }
            return RedirectToAction(nameof(Detalle), new { id = userId });
        }
    }

    public class EmpleadoEquipoViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ApiarioNombre { get; set; }
        public bool PinActivo { get; set; }
        public bool TienePin { get; set; }
        public int RegistrosSemana { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string EstadoAcceso => !TienePin ? "Sin PIN" : PinActivo ? "Activo" : "Revocado";
    }

    public class EmpleadoDetalleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ApiarioNombre { get; set; }
        public int? ApiarioId { get; set; }
        public bool PinActivo { get; set; }
        public bool TienePin { get; set; }
        public int RegistrosSemana { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public List<Tarea> Tareas { get; set; } = new();
        public List<Colmena> ColmenasAsignadas { get; set; } = new();
        public List<Colmena> ColmenasDisponibles { get; set; } = new();
        public string EstadoAcceso => !TienePin ? "Sin PIN" : PinActivo ? "Activo" : "Revocado";
        public int TareasAsignadas => Tareas.Count;
        public int TareasPendientes => Tareas.Count(t => !t.Completada);
        public int TareasCompletadas => Tareas.Count(t => t.Completada);
    }
}
