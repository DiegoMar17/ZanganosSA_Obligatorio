using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColmenaEmpresa.Controllers
{
    public class PerfilController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly AppDbContext _ctx;

        public PerfilController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AppDbContext ctx)
        {
            _users  = users;
            _signIn = signIn;
            _ctx    = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            var esAdmin = User.IsInRole("ADMIN");

            if (esAdmin)
            {
                ViewBag.TotalApiarios = _ctx.Apiarios.Count();
                ViewBag.TotalColmenas = _ctx.Colmenas.Count();
                ViewBag.TotalCosechas = _ctx.Cosechas.Count();
                ViewBag.CosechaTotal  = Math.Round(_ctx.Cosechas.ToList().Sum(c => c.PesoNeto) / 1000.0, 1);
            }
            else
            {
                var sectorId = user?.ApiarioAsignadoId;
                ViewBag.TotalApiarios = sectorId.HasValue ? 1 : 0;
                ViewBag.TotalColmenas = sectorId.HasValue ? _ctx.Colmenas.Count(c => c.ApiarioId == sectorId.Value) : 0;
                ViewBag.TotalCosechas = sectorId.HasValue ? _ctx.Cosechas.Count(c => c.ApiarioId == sectorId.Value) : 0;
                ViewBag.CosechaTotal  = sectorId.HasValue
                    ? Math.Round(_ctx.Cosechas.Where(c => c.ApiarioId == sectorId.Value).ToList().Sum(c => c.PesoNeto) / 1000.0, 1)
                    : 0;
            }

            ViewBag.EsAdmin       = esAdmin;
            ViewBag.Rol           = user?.Rol ?? "EMPLEADO";
            ViewBag.PinActivo     = user?.PinActivo ?? false;
            ViewBag.TienePin      = !string.IsNullOrEmpty(user?.PinHash);
            ViewBag.SectorNombre  = user?.ApiarioAsignadoId.HasValue == true
                ? _ctx.Apiarios.Find(user.ApiarioAsignadoId.Value)?.Nombre
                : null;
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDatos(string nombreCompleto, string email)
        {
            var user = await _users.GetUserAsync(User);
            if (user is null) return RedirectToAction(nameof(Index));

            // ¿El email ya está en uso por otra cuenta?
            var existente = await _users.FindByEmailAsync(email);
            if (existente is not null && existente.Id != user.Id)
            {
                TempData["Error"] = "Ese correo ya está en uso por otra cuenta.";
                return RedirectToAction(nameof(Index));
            }

            user.NombreCompleto = nombreCompleto;
            user.Email    = email;
            user.UserName = email;

            var result = await _users.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signIn.RefreshSignInAsync(user);
                TempData["Exito"] = "Datos actualizados correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudieron actualizar los datos: " +
                    string.Join(" ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string passwordActual, string passwordNuevo, string passwordConfirm)
        {
            if (passwordNuevo != passwordConfirm)
            {
                TempData["Error"] = "Las contraseñas nuevas no coinciden.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _users.GetUserAsync(User);
            var result = await _users.ChangePasswordAsync(user, passwordActual, passwordNuevo);
            if (result.Succeeded)
            {
                await _signIn.RefreshSignInAsync(user);
                TempData["Exito"] = "Contraseña cambiada correctamente.";
            }
            else
                TempData["Error"] = "La contraseña actual es incorrecta.";

            return RedirectToAction(nameof(Index));
        }
    }
}
