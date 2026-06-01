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

        public IActionResult Crear() => View(new Transhumancia { FechaSalida = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Transhumancia traslado)
        {
            if (!ModelState.IsValid) return View(traslado);
            traslado.Estado = "planificado";
            _ctx.Transhumancias.Add(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var traslado = _ctx.Transhumancias.Find(id);
            if (traslado is null) return NotFound();
            return View(traslado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Transhumancia traslado)
        {
            if (id != traslado.Id) return BadRequest();
            if (!ModelState.IsValid) return View(traslado);
            _ctx.Transhumancias.Update(traslado);
            _ctx.SaveChanges();
            TempData["Exito"] = "Traslado actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
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
