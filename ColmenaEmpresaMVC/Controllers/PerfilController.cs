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
            ViewBag.TotalApiarios = _ctx.Apiarios.Count();
            ViewBag.TotalColmenas = _ctx.Colmenas.Count();
            ViewBag.TotalCosechas = _ctx.Cosechas.Count();
            ViewBag.CosechaTotal  = Math.Round(_ctx.Cosechas.ToList().Sum(c => c.PesoNeto) / 1000.0, 1);
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDatos(string nombreCompleto, string email)
        {
            var user = await _users.GetUserAsync(User);
            user.NombreCompleto = nombreCompleto;
            user.Email    = email;
            user.UserName = email;
            await _users.UpdateAsync(user);
            await _signIn.RefreshSignInAsync(user);
            TempData["Exito"] = "Datos actualizados correctamente.";
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
