using BCrypt.Net;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ColmenaEmpresa.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AppDbContext _ctx;

        public AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, AppDbContext ctx)
        {
            _signIn = signIn;
            _users  = users;
            _ctx    = ctx;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signIn.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var result = await _signIn.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _users.FindByEmailAsync(email);
                if (user is not null)
                    RegistrarAcceso(user, true);
                return LocalRedirect(returnUrl ?? "/Home/Index");
            }

            ViewBag.Error     = "Email o contraseña incorrectos.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPin(string email, string pin, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pin))
            {
                ViewBag.ErrorPin  = "Email y PIN son requeridos.";
                ViewBag.TabActivo = "pin";
                return View("Login");
            }

            var user = await _users.FindByEmailAsync(email);

            if (user is null || !user.PinActivo || string.IsNullOrEmpty(user.PinHash))
            {
                ViewBag.ErrorPin  = "Acceso con PIN no disponible para este usuario.";
                ViewBag.TabActivo = "pin";
                return View("Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(pin, user.PinHash))
            {
                RegistrarAcceso(user, false);
                ViewBag.ErrorPin  = "PIN incorrecto.";
                ViewBag.TabActivo = "pin";
                return View("Login");
            }

            await _signIn.SignInAsync(user, isPersistent: true);
            RegistrarAcceso(user, true);
            return LocalRedirect(returnUrl ?? "/Home/Index");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Landing", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado() => View();

        private void RegistrarAcceso(ApplicationUser user, bool exitoso)
        {
            try
            {
                _ctx.HistorialesAcceso.Add(new HistorialAcceso
                {
                    UserId        = user.Id,
                    NombreUsuario = user.NombreCompleto,
                    FechaHora     = DateTime.Now,
                    Ip            = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Dispositivo   = HttpContext.Request.Headers["User-Agent"].ToString().Split(' ').FirstOrDefault() ?? "Desconocido",
                    Exitoso       = exitoso
                });
                _ctx.SaveChanges();
            }
            catch { }
        }
    }
}
