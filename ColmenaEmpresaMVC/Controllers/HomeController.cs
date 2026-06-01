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
            var colmenas    = _ctx.Colmenas.ToList();
            var cosechas    = _ctx.Cosechas.ToList();
            var inspecciones = _ctx.Inspecciones.ToList();

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

            var vm = new DashboardViewModel
            {
                TotalColmenas          = colmenas.Count,
                TotalApiarios          = _ctx.Apiarios.Count(),
                InspeccionesPendientes = inspecciones.Count(i => i.Estado == "pendiente"),
                CosechaEstimada        = $"{Math.Round(cosechas.Sum(c => c.PesoNeto) / 1000.0, 1):F1} t",
                ColmenasVerde          = colmenas.Count(c => c.EstadoSemaforo == "verde"),
                ColmenasAmarillo       = colmenas.Count(c => c.EstadoSemaforo == "amarillo"),
                ColmenasRojo           = colmenas.Count(c => c.EstadoSemaforo == "rojo"),
                Alertas                = new List<AlertaDashboard>(),
                ProduccionMensual      = produccionMensual
            };

            return View(vm);
        }
    }
}
