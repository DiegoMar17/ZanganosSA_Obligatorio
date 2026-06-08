using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _ctx;

        public HomeController(AppDbContext ctx) => _ctx = ctx;

        [AllowAnonymous]
        public IActionResult Landing() => View();

        public IActionResult Index()
        {
            var colmenas     = _ctx.Colmenas.ToList();
            var cosechas     = _ctx.Cosechas.ToList();
            var inspecciones = _ctx.Inspecciones.ToList();
            var transhumancias = _ctx.Transhumancias.ToList();

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

            var vm = new DashboardViewModel
            {
                TotalColmenas          = colmenas.Count,
                ColmenasNuevasMes      = colmenas.Count(c => c.FechaInstalacion.Month == ahora.Month && c.FechaInstalacion.Year == ahora.Year),
                TotalApiarios          = _ctx.Apiarios.Count(),
                EnTranshumancia        = transhumancias.Count(t => t.Estado == "en_curso"),
                InspeccionesPendientes = inspecciones.Count(i => i.Estado == "pendiente" || i.Estado == "vencida"),
                CosechaTotal           = $"{Math.Round(cosechas.Sum(c => c.PesoNeto) / 1000.0, 1):F1} t",
                ColmenasVerde          = colmenas.Count(c => c.EstadoSemaforo == "verde"),
                ColmenasAmarillo       = colmenas.Count(c => c.EstadoSemaforo == "amarillo"),
                ColmenasRojo           = colmenas.Count(c => c.EstadoSemaforo == "rojo"),
                Alertas                = alertas,
                ProduccionMensual      = produccionMensual
            };

            return View(vm);
        }
    }
}
