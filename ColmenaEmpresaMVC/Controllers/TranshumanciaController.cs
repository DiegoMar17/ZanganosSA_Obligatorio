using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class TranshumanciaController : Controller
    {
        private readonly AppDbContext _ctx;

        public TranshumanciaController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index() => View(_ctx.Transhumancias.ToList());

        private void CargarApiarios() =>
            ViewBag.NombresApiarios = _ctx.Apiarios.OrderBy(a => a.Nombre).Select(a => a.Nombre).ToList();

        [Authorize(Roles = "ADMIN")]
        public IActionResult Crear() { CargarApiarios(); return View(new Transhumancia { FechaSalida = DateTime.Today }); }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Transhumancia traslado)
        {
            if (!ModelState.IsValid) { CargarApiarios(); return View(traslado); }
            traslado.Estado = "planificado";
            _ctx.Transhumancias.Add(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var traslado = _ctx.Transhumancias.Find(id);
            if (traslado is null) return NotFound();
            CargarApiarios(); return View(traslado);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Transhumancia traslado)
        {
            if (id != traslado.Id) return BadRequest();
            if (!ModelState.IsValid) { CargarApiarios(); return View(traslado); }
            _ctx.Transhumancias.Update(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var traslado = _ctx.Transhumancias.Find(id);
            if (traslado is not null)
            {
                _ctx.Transhumancias.Remove(traslado);
                _ctx.SaveChanges();
                TempData["Exito"] = "Traslado eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
