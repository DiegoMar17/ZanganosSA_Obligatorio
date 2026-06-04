using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ApiariosController : Controller
    {
        private readonly AppDbContext _ctx;

        public ApiariosController(AppDbContext ctx) => _ctx = ctx;

        // Deriva el semáforo del apiario a partir del peor estado de sus colmenas.
        private static string CalcularSemaforo(IEnumerable<Colmena> colmenas)
        {
            if (colmenas.Any(c => c.EstadoSemaforo == "rojo"))     return "rojo";
            if (colmenas.Any(c => c.EstadoSemaforo == "amarillo")) return "amarillo";
            return "verde";
        }

        public IActionResult Index()
        {
            var apiarios = _ctx.Apiarios.ToList();
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

            var colmenas = _ctx.Colmenas
                .Where(c => c.ApiarioId == id)
                .OrderBy(c => c.Codigo)
                .ToList();

            apiario.TotalColmenas  = colmenas.Count;
            apiario.EstadoSemaforo = CalcularSemaforo(colmenas);

            ViewBag.Colmenas = colmenas;
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
