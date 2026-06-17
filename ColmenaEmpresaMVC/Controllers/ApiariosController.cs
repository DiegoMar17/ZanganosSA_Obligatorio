using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;

namespace ColmenaEmpresa.Controllers
{
    public class ApiariosController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AuditoriaService _auditoria;

        public ApiariosController(AppDbContext ctx, UserManager<ApplicationUser> users, AuditoriaService auditoria)
        {
            _ctx       = ctx;
            _users     = users;
            _auditoria = auditoria;
        }

        // Deriva el semáforo del apiario a partir del peor estado de sus colmenas.
        private static string CalcularSemaforo(IEnumerable<Colmena> colmenas)
        {
            if (colmenas.Any(c => c.EstadoSemaforo == "rojo"))     return "rojo";
            if (colmenas.Any(c => c.EstadoSemaforo == "amarillo")) return "amarillo";
            return "verde";
        }

        public async Task<IActionResult> Index()
        {
            var apiarios = _ctx.Apiarios.ToList();

            if (!User.IsInRole("ADMIN"))
            {
                var sectorId = (await _users.GetUserAsync(User))?.ApiarioAsignadoId;
                apiarios = sectorId.HasValue ? apiarios.Where(a => a.Id == sectorId.Value).ToList() : new List<Apiario>();
            }

            var colmenasPorApiario = _ctx.Colmenas
                .ToList()
                .GroupBy(c => c.ApiarioId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var a in apiarios)
            {
                if (colmenasPorApiario.TryGetValue(a.Id, out var cols))
                {
                    a.TotalColmenas  = cols.Count;
                    a.EstadoSemaforo = CalcularSemaforo(cols);
                }
                else
                {
                    a.TotalColmenas  = 0;
                    a.EstadoSemaforo = "verde";
                }
            }

            return View(apiarios);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Crear() => View(new Apiario());

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Apiario apiario)
        {
            if (!ModelState.IsValid) return View(apiario);
            _ctx.Apiarios.Add(apiario);
            _ctx.SaveChanges();
            var user = await _users.GetUserAsync(User);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "CREATE", "Apiarios", apiario.Nombre);
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is null) return NotFound();

            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId != id) return Forbid();
            }

            var colmenas = _ctx.Colmenas
                .Where(c => c.ApiarioId == id)
                .OrderBy(c => c.Codigo)
                .ToList();

            apiario.TotalColmenas  = colmenas.Count;
            apiario.EstadoSemaforo = CalcularSemaforo(colmenas);

            ViewBag.Colmenas = colmenas;
            return View(apiario);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is null) return NotFound();
            return View(apiario);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Apiario apiario)
        {
            if (id != apiario.Id) return BadRequest();
            if (!ModelState.IsValid) return View(apiario);
            _ctx.Apiarios.Update(apiario);
            _ctx.SaveChanges();
            var user = await _users.GetUserAsync(User);
            _auditoria.Registrar(user!.Id, user.NombreCompleto, "UPDATE", "Apiarios", apiario.Nombre);
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is not null)
            {
                _ctx.Apiarios.Remove(apiario);
                _ctx.SaveChanges();
                var user = await _users.GetUserAsync(User);
                _auditoria.Registrar(user!.Id, user.NombreCompleto, "DELETE", "Apiarios", apiario.Nombre);
                TempData["Exito"] = "Apiario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
