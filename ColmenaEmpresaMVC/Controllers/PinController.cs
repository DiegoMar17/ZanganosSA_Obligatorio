using BCrypt.Net;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColmenaEmpresa.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class PinController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly AppDbContext _ctx;

        public PinController(UserManager<ApplicationUser> users, AppDbContext ctx)
        {
            _users = users;
            _ctx   = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _users.GetUsersInRoleAsync("EMPLEADO");

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
                return new EmpleadoPinViewModel
                {
                    UserId          = e.Id,
                    NombreCompleto  = e.NombreCompleto,
                    Email           = e.Email ?? string.Empty,
                    PinActivo       = e.PinActivo,
                    TienePin        = !string.IsNullOrEmpty(e.PinHash),
                    ApiarioNombre   = apiario,
                    ApiarioAsignadoId = e.ApiarioAsignadoId,
                    UltimoAcceso    = ultimosAccesos.FirstOrDefault(u => u.UserId == e.Id)?.Ultimo
                };
            }).ToList();

            ViewBag.PinGenerado  = TempData["PinGenerado"];
            ViewBag.PinUsuario   = TempData["PinUsuario"];
            ViewBag.Apiarios     = _ctx.Apiarios.OrderBy(a => a.Nombre).ToList();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPin(string userId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var pin = new Random().Next(100000, 999999).ToString();
            user.PinHash  = BCrypt.Net.BCrypt.HashPassword(pin);
            user.PinActivo = true;
            await _users.UpdateAsync(user);

            TempData["PinGenerado"] = pin;
            TempData["PinUsuario"]  = user.NombreCompleto;
            TempData["Exito"]       = $"PIN generado para {user.NombreCompleto}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RevocarPin(string userId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return NotFound();

            user.PinActivo = false;
            await _users.UpdateAsync(user);

            TempData["Exito"] = $"Acceso revocado para {user.NombreCompleto}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> HabilitarPin(string userId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null || string.IsNullOrEmpty(user.PinHash)) return NotFound();

            user.PinActivo = true;
            await _users.UpdateAsync(user);

            TempData["Exito"] = $"Acceso habilitado para {user.NombreCompleto}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarApiario(string userId, int? apiarioId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return NotFound();

            user.ApiarioAsignadoId = apiarioId;
            await _users.UpdateAsync(user);

            TempData["Exito"] = $"Sector actualizado para {user.NombreCompleto}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpleado(string nombreCompleto, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Nombre y email son requeridos.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _users.FindByEmailAsync(email);
            if (existing is not null)
            {
                TempData["Error"] = "Ya existe un usuario con ese email.";
                return RedirectToAction(nameof(Index));
            }

            var user = new ApplicationUser
            {
                UserName       = email,
                Email          = email,
                NombreCompleto = nombreCompleto,
                EmailConfirmed = true,
                Rol            = "EMPLEADO",
                PinActivo      = false
            };

            // Contraseña aleatoria — empleados solo ingresan con PIN
            var result = await _users.CreateAsync(user, Guid.NewGuid().ToString("N") + "Aa1!");
            if (result.Succeeded)
            {
                await _users.AddToRoleAsync(user, "EMPLEADO");
                TempData["Exito"] = $"Empleado {nombreCompleto} creado. Generá un PIN para que pueda ingresar.";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEmpleado(string userId)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var nombre = user.NombreCompleto;
            await _users.DeleteAsync(user);
            TempData["Exito"] = $"Empleado {nombre} eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class EmpleadoPinViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool PinActivo { get; set; }
        public bool TienePin { get; set; }
        public string? ApiarioNombre { get; set; }
        public int? ApiarioAsignadoId { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string EstadoAcceso => !TienePin ? "Sin PIN" : PinActivo ? "Activo" : "Revocado";
    }
}
