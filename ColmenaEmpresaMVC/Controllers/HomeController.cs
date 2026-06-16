using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var esAdmin = User.IsInRole("ADMIN");
            var currentUser = await _users.GetUserAsync(User);
            var sectorId = currentUser?.ApiarioAsignadoId;

            var colmenas     = _ctx.Colmenas.ToList();
            var cosechas     = _ctx.Cosechas.ToList();
            var inspecciones = _ctx.Inspecciones.ToList();
            var transhumancias = _ctx.Transhumancias.ToList();

            if (!esAdmin)
            {
                var sectorNombre = sectorId.HasValue ? _ctx.Apiarios.Find(sectorId.Value)?.Nombre : null;
                colmenas     = sectorId.HasValue ? colmenas.Where(c => c.ApiarioId == sectorId.Value).ToList() : new List<Colmena>();
                cosechas     = sectorId.HasValue ? cosechas.Where(c => c.ApiarioId == sectorId.Value).ToList() : new List<Cosecha>();
                inspecciones = sectorId.HasValue ? inspecciones.Where(i => i.ApiarioId == sectorId.Value).ToList() : new List<Inspeccion>();
                transhumancias = sectorNombre is not null
                    ? transhumancias.Where(t => t.ApiarioOrigen == sectorNombre || t.ApiarioDestino == sectorNombre).ToList()
                    : new List<Transhumancia>();
            }

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

            // Alertas reales generadas desde la BD
            var alertas = new List<AlertaDashboard>();
            foreach (var c in colmenas.Where(c => c.EstadoSemaforo == "rojo"))
                alertas.Add(new AlertaDashboard { Tipo = "red", Titulo = $"Colmena {c.Codigo} en estado crítico", Tiempo = c.ApiarioNombre });
            foreach (var c in colmenas.Where(c => c.EstadoReina == "ausente"))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"Colmena {c.Codigo} sin reina", Tiempo = c.ApiarioNombre });
            foreach (var i in inspecciones.Where(i => i.Estado == "vencida"))
                alertas.Add(new AlertaDashboard { Tipo = "red", Titulo = $"Inspección vencida — {i.ApiarioNombre}", Tiempo = i.Fecha.ToString("dd MMM yyyy") });
            foreach (var i in inspecciones.Where(i => i.Estado == "pendiente"))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"Inspección pendiente — {i.ApiarioNombre}", Tiempo = i.Fecha.ToString("dd MMM yyyy") });
            if (!alertas.Any())
                alertas.Add(new AlertaDashboard { Tipo = "green", Titulo = "Todo en orden — sin alertas activas", Tiempo = "Operación saludable" });

            var ahora = DateTime.Now;

            int tareasPendientes;
            int empleadosActivos = 0;
            int controlesVencidos = 0;
            var ultimosAuditoria = new List<Auditoria>();

            if (esAdmin)
            {
                tareasPendientes = _ctx.Tareas.Count(t => !t.Completada);
                empleadosActivos = (await _users.GetUsersInRoleAsync("EMPLEADO")).Count(e => e.PinActivo);
                controlesVencidos = _ctx.ControlesSanitarios.Count(c => c.Estado == "en_tratamiento" && c.Fecha < DateTime.Today.AddDays(-30));
                ultimosAuditoria = _ctx.Auditorias.OrderByDescending(a => a.FechaHora).Take(5).ToList();
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
                EnTranshumancia        = transhumancias.Count(t => t.Estado == "en_curso"),
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
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
