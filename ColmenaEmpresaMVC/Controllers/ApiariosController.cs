using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ApiariosController : Controller
    {
        private readonly AppDbContext _ctx;

        public ApiariosController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index()
        {
            var apiarios = _ctx.Apiarios.ToList();
            var conteos  = _ctx.Colmenas
                .GroupBy(c => c.ApiarioId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var a in apiarios)
                a.TotalColmenas = conteos.TryGetValue(a.Id, out var n) ? n : 0;

            return View(apiarios);
        }

        public IActionResult Crear() => View(new Apiario());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Apiario apiario)
        {
            if (!ModelState.IsValid) return View(apiario);
            _ctx.Apiarios.Add(apiario);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Detalle(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is null) return NotFound();
            return View(apiario);
        }

        public IActionResult Editar(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is null) return NotFound();
            return View(apiario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Apiario apiario)
        {
            if (id != apiario.Id) return BadRequest();
            if (!ModelState.IsValid) return View(apiario);
            _ctx.Apiarios.Update(apiario);
            _ctx.SaveChanges();
            TempData["Exito"] = $"Apiario '{apiario.Nombre}' actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var apiario = _ctx.Apiarios.Find(id);
            if (apiario is not null)
            {
                _ctx.Apiarios.Remove(apiario);
                _ctx.SaveChanges();
                TempData["Exito"] = "Apiario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
