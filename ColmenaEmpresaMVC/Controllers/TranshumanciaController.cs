using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class TranshumanciaController : Controller
    {
        private readonly AppDbContext _ctx;

        public TranshumanciaController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index() =>
            View(_ctx.Traslados.Include(t => t.ApiarioOrigen).Include(t => t.ApiarioDestino).ToList());

        private void CargarApiarios()
        {
            var apiarios = _ctx.Apiarios.OrderBy(a => a.Nombre).ToList();
            ViewBag.Apiarios = new SelectList(apiarios, "Id", "Nombre");
        }

        private bool ValidarOrigenDestino(Traslado traslado)
        {
            if (traslado.ApiarioOrigenId != traslado.ApiarioDestinoId) return true;
            ModelState.AddModelError(nameof(traslado.ApiarioDestinoId),
                "El apiario de destino debe ser diferente al de origen.");
            return false;
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Crear() { CargarApiarios(); return View(new Traslado { FechaSalida = DateTime.Today }); }

        [HttpPost, Authorize(Roles = "ADMIN"), ValidateAntiForgeryToken]
        public IActionResult Crear(Traslado traslado)
        {
            ValidarOrigenDestino(traslado);
            if (!ModelState.IsValid) { CargarApiarios(); return View(traslado); }
            traslado.Estado = "planificado";
            _ctx.Traslados.Add(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Editar(int id)
        {
            var traslado = _ctx.Traslados.Find(id);
            if (traslado is null) return NotFound();
            CargarApiarios(); return View(traslado);
        }

        [HttpPost, Authorize(Roles = "ADMIN"), ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Traslado traslado)
        {
            if (id != traslado.Id) return BadRequest();
            ValidarOrigenDestino(traslado);
            if (!ModelState.IsValid) { CargarApiarios(); return View(traslado); }
            _ctx.Traslados.Update(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, Authorize(Roles = "ADMIN"), ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var traslado = _ctx.Traslados.Find(id);
            if (traslado is not null)
            {
                _ctx.Traslados.Remove(traslado);
                _ctx.SaveChanges();
                TempData["Exito"] = "Traslado eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
