using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;

        public ProduccionController(AppDbContext ctx, UserManager<ApplicationUser> users)
        {
            _ctx   = ctx;
            _users = users;
        }

        private void CargarApiarios(int? soloApiarioId = null)
        {
            var query = _ctx.Apiarios.OrderBy(a => a.Nombre);
            var lista = soloApiarioId.HasValue
                ? query.Where(a => a.Id == soloApiarioId.Value).ToList()
                : query.ToList();
            ViewBag.Apiarios = new SelectList(lista, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
        {
            var query = _ctx.Cosechas.Include(c => c.Apiario).AsQueryable();

            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId is null)
                {
                    TempData["Error"] = "No tenés un sector asignado.";
                    return RedirectToAction("Index", "Home");
                }
                query = query.Where(c => c.ApiarioId == user.ApiarioAsignadoId.Value);
            }

            var cosechas = query.OrderByDescending(c => c.Fecha).ToList();
            var vendidas = cosechas.Where(c => c.Vendida).ToList();
            var mejor    = cosechas.GroupBy(c => c.Apiario?.Nombre ?? "")
                                   .OrderByDescending(g => g.Sum(c => c.PesoNeto))
                                   .FirstOrDefault();

            ViewBag.TotalKg      = Math.Round(cosechas.Sum(c => c.PesoNeto), 1);
            ViewBag.Ingresos     = vendidas.Sum(c => c.MontoVenta);
            ViewBag.KgVendidos   = Math.Round(vendidas.Sum(c => c.PesoNeto), 1);
            ViewBag.MejorApiario = mejor?.Key ?? "—";
            ViewBag.MejorKg      = mejor is not null ? Math.Round(mejor.Sum(c => c.PesoNeto), 1) : 0;
            ViewBag.Cantidad     = cosechas.Count;
            return View(cosechas);
        }

        public async Task<IActionResult> Crear()
        {
            int? soloApiario = null;
            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId is null)
                {
                    TempData["Error"] = "No tenés un sector asignado para registrar cosechas.";
                    return RedirectToAction(nameof(Index));
                }
                soloApiario = user.ApiarioAsignadoId;
            }
            CargarApiarios(soloApiario);
            return View(new Cosecha { Fecha = DateTime.Today });
        }

        private void SincronizarIngreso(Cosecha cosecha)
        {
            var anterior = _ctx.RegistrosFinancieros.FirstOrDefault(r => r.CosechaId == cosecha.Id);
            if (anterior is not null) _ctx.RegistrosFinancieros.Remove(anterior);

            if (cosecha.Vendida && cosecha.MontoVenta > 0)
            {
                var apiarioNombre = _ctx.Apiarios.Find(cosecha.ApiarioId)?.Nombre ?? "";
                _ctx.RegistrosFinancieros.Add(new RegistroFinanciero
                {
                    CosechaId      = cosecha.Id,
                    TipoMovimiento = "ingreso",
                    Categoria      = "Venta de miel",
                    Descripcion    = $"Venta cosecha {Math.Round(cosecha.PesoNeto, 1)} kg ({cosecha.TipoMiel}) — {apiarioNombre}",
                    Fecha          = cosecha.Fecha,
                    Monto          = cosecha.MontoVenta,
                    ApiarioId      = cosecha.ApiarioId
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cosecha cosecha)
        {
            int? soloApiario = null;
            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId is null || cosecha.ApiarioId != user.ApiarioAsignadoId.Value)
                {
                    TempData["Error"] = "Solo podés registrar cosechas de tu propio sector.";
                    CargarApiarios(user?.ApiarioAsignadoId);
                    return View(cosecha);
                }
                soloApiario = user.ApiarioAsignadoId;
            }

            if (!ModelState.IsValid) { CargarApiarios(soloApiario); return View(cosecha); }

            _ctx.Cosechas.Add(cosecha);
            _ctx.SaveChanges();

            SincronizarIngreso(cosecha);
            _ctx.SaveChanges();

            TempData["Exito"] = cosecha.Vendida && cosecha.MontoVenta > 0
                ? $"Cosecha registrada e ingreso de ${cosecha.MontoVenta:N2} cargado en Finanzas."
                : "Cosecha registrada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var cosecha = _ctx.Cosechas.Find(id);
            if (cosecha is null) return NotFound();

            int? soloApiario = null;
            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId is null || cosecha.ApiarioId != user.ApiarioAsignadoId.Value)
                    return Forbid();
                soloApiario = user.ApiarioAsignadoId;
            }
            CargarApiarios(soloApiario);
            return View(cosecha);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cosecha cosecha)
        {
            if (id != cosecha.Id) return BadRequest();

            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                if (user?.ApiarioAsignadoId is null || cosecha.ApiarioId != user.ApiarioAsignadoId.Value)
                    return Forbid();
            }

            if (!ModelState.IsValid) { CargarApiarios(); return View(cosecha); }

            _ctx.Cosechas.Update(cosecha);
            SincronizarIngreso(cosecha);
            _ctx.SaveChanges();

            TempData["Exito"] = cosecha.Vendida && cosecha.MontoVenta > 0
                ? $"Cosecha actualizada e ingreso de ${cosecha.MontoVenta:N2} sincronizado en Finanzas."
                : "Cosecha actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var cosecha = _ctx.Cosechas.Find(id);
            if (cosecha is not null)
            {
                var ingreso = _ctx.RegistrosFinancieros.FirstOrDefault(r => r.CosechaId == id);
                if (ingreso is not null) _ctx.RegistrosFinancieros.Remove(ingreso);
                _ctx.Cosechas.Remove(cosecha);
                _ctx.SaveChanges();
                TempData["Exito"] = "Cosecha eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Exportar()
        {
            var cosechas = _ctx.Cosechas.Include(c => c.Apiario).OrderBy(c => c.Fecha).ToList();
            ViewBag.TotalKg  = Math.Round(cosechas.Sum(c => c.PesoNeto), 1);
            ViewBag.Promedio = cosechas.Any() ? Math.Round(cosechas.Average(c => c.PesoNeto), 1) : 0;
            var mejor        = cosechas.OrderByDescending(c => c.PesoNeto).FirstOrDefault();
            ViewBag.MejorCosecha = mejor is not null ? $"{mejor.Apiario?.Nombre ?? ""} · {mejor.PesoNeto} kg" : "—";
            return View(cosechas);
        }

        public IActionResult ExportarCsv()
        {
            var cosechas = _ctx.Cosechas.Include(c => c.Apiario).OrderByDescending(c => c.Fecha).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Apiario,Fecha,Tipo Miel,Alzas,Peso Bruto (kg),Merma (kg),Peso Neto (kg),Humedad (%),HMF,Destino,Vendida,Precio/kg,Ingreso");
            foreach (var c in cosechas)
                sb.AppendLine($"{c.Apiario?.Nombre ?? ""},{c.Fecha:dd/MM/yyyy},{c.TipoMiel},{c.AlzasCosechadas},{c.PesoBruto},{c.Merma},{c.PesoNeto},{c.Humedad},{c.HMF},{c.Destino},{c.Vendida},{c.PrecioPorKg},{(c.Vendida ? c.MontoVenta : 0)}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"produccion_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
