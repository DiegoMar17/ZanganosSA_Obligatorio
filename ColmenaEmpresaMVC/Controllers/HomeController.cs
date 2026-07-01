using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;

        public HomeController(AppDbContext ctx, UserManager<ApplicationUser> users)
        {
            _ctx   = ctx;
            _users = users;
        }

        [AllowAnonymous]
        public IActionResult Landing() => View();

        public async Task<IActionResult> Index()
        {
            var esAdmin     = User.IsInRole("ADMIN");
            var currentUser = await _users.GetUserAsync(User);
            var sectorId    = currentUser?.ApiarioAsignadoId;

            var colmenasQ     = _ctx.Colmenas.Include(c => c.Apiario).AsQueryable();
            var cosechasQ     = _ctx.Cosechas.AsQueryable();
            var inspeccionesQ = _ctx.Inspecciones.Include(i => i.Apiario).AsQueryable();
            var trasladosQ    = _ctx.Traslados.Include(t => t.ApiarioOrigen).Include(t => t.ApiarioDestino).AsQueryable();

            if (!esAdmin)
            {
                if (!sectorId.HasValue)
                {
                    return View(new DashboardViewModel());
                }
                colmenasQ     = colmenasQ.Where(c => c.ApiarioId == sectorId.Value);
                cosechasQ     = cosechasQ.Where(c => c.ApiarioId == sectorId.Value);
                inspeccionesQ = inspeccionesQ.Where(i => i.ApiarioId == sectorId.Value);
                trasladosQ    = trasladosQ.Where(t => t.ApiarioOrigenId == sectorId.Value || t.ApiarioDestinoId == sectorId.Value);
            }

            var colmenas     = colmenasQ.ToList();
            var cosechas     = cosechasQ.ToList();
            var inspecciones = inspeccionesQ.ToList();
            var traslados    = trasladosQ.ToList();

            var maxKg = cosechas.Any() ? cosechas.Max(c => c.PesoNeto) : 1.0;

            var produccionMensual = cosechas
                .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new ProduccionMensual
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    Kg  = Math.Round(g.Sum(c => c.PesoNeto), 1),
                    PorcentajeAlto = maxKg > 0 ? (int)(g.Sum(c => c.PesoNeto) * 100 / maxKg) : 0
                })
                .ToList();

            var alertas = new List<AlertaDashboard>();
            foreach (var c in colmenas.Where(c => c.EstadoSemaforo == "rojo"))
                alertas.Add(new AlertaDashboard { Tipo = "red",   Titulo = $"Colmena {c.Codigo} en estado crítico", Tiempo = c.Apiario?.Nombre ?? "" });
            foreach (var c in colmenas.Where(c => c.EstadoReina == "ausente"))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"Colmena {c.Codigo} sin reina",        Tiempo = c.Apiario?.Nombre ?? "" });
            foreach (var i in inspecciones.Where(i => i.Estado == "vencida"))
                alertas.Add(new AlertaDashboard { Tipo = "red",   Titulo = $"Inspección vencida — {i.Apiario?.Nombre ?? ""}",   Tiempo = i.Fecha.ToString("dd MMM yyyy") });
            foreach (var i in inspecciones.Where(i => i.Estado == "pendiente"))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"Inspección pendiente — {i.Apiario?.Nombre ?? ""}", Tiempo = i.Fecha.ToString("dd MMM yyyy") });
            if (!alertas.Any())
                alertas.Add(new AlertaDashboard { Tipo = "green", Titulo = "Todo en orden — sin alertas activas", Tiempo = "Operación saludable" });

            var ahora = DateTime.Now;

            int tareasPendientes;
            int empleadosActivos  = 0;
            int controlesVencidos = 0;
            var ultimosAuditoria  = new List<Auditoria>();

            if (esAdmin)
            {
                tareasPendientes  = _ctx.Tareas.Count(t => !t.Completada);
                empleadosActivos  = (await _users.GetUsersInRoleAsync("EMPLEADO")).Count(e => e.PinActivo);
                controlesVencidos = _ctx.ControlesSanitarios.Count(c => c.Estado == "en_tratamiento" && c.Fecha < DateTime.Today.AddDays(-30));
                ultimosAuditoria  = _ctx.Auditorias.OrderByDescending(a => a.FechaHora).Take(5).ToList();
            }
            else
            {
                tareasPendientes = _ctx.Tareas.Count(t => t.AsignadoAId == currentUser!.Id && !t.Completada);
            }

            var vm = new DashboardViewModel
            {
                TotalColmenas          = colmenas.Count,
                ColmenasNuevasMes      = colmenas.Count(c => c.FechaInstalacion.Month == ahora.Month && c.FechaInstalacion.Year == ahora.Year),
                TotalApiarios          = esAdmin ? _ctx.Apiarios.Count() : (sectorId.HasValue ? 1 : 0),
                EnTranshumancia        = traslados.Count(t => t.Estado == "en_curso"),
                InspeccionesPendientes = inspecciones.Count(i => i.Estado == "pendiente" || i.Estado == "vencida"),
                CosechaTotal           = $"{Math.Round(cosechas.Sum(c => c.PesoNeto) / 1000.0, 1):F1} t",
                ColmenasVerde          = colmenas.Count(c => c.EstadoSemaforo == "verde"),
                ColmenasAmarillo       = colmenas.Count(c => c.EstadoSemaforo == "amarillo"),
                ColmenasRojo           = colmenas.Count(c => c.EstadoSemaforo == "rojo"),
                Alertas                = alertas,
                ProduccionMensual      = produccionMensual,
                EsAdmin                = esAdmin,
                TareasPendientesCount  = tareasPendientes,
                EmpleadosActivos       = empleadosActivos,
                ControlesVencidos      = controlesVencidos,
                UltimosAuditoria       = ultimosAuditoria
            };

            return View(vm);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var vm = new ErrorViewModel
            {
                RequestId  = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            };
            return View(vm);
        }
    }
}
