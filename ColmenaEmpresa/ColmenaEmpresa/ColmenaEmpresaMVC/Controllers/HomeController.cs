using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var colmenas    = await _db.Colmenas.ToListAsync();
            var apiarios    = await _db.Apiarios.ToListAsync();
            var cosechas    = await _db.Cosechas.ToListAsync();
            var controles   = await _db.ControlesSanitarios.ToListAsync();
            var inspecciones = await _db.Inspecciones.ToListAsync();

            // Alertas dinámicas basadas en estado real
            var alertas = new List<AlertaDashboard>();

            foreach (var c in colmenas.Where(c => c.EstadoReina == "ausente"))
                alertas.Add(new AlertaDashboard { Tipo = "red", Titulo = $"Colmena {c.Codigo} — reina ausente", Tiempo = c.ApiarioNombre });

            foreach (var c in colmenas.Where(c => c.UltimaVisita.HasValue && (DateTime.Now - c.UltimaVisita.Value).Days > 14))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"Colmena {c.Codigo} — sin visita hace {(DateTime.Now - c.UltimaVisita!.Value).Days} días", Tiempo = c.ApiarioNombre });

            foreach (var cs in controles.Where(cs => cs.Estado == "en_tratamiento"))
                alertas.Add(new AlertaDashboard { Tipo = "amber", Titulo = $"{cs.ApiarioNombre} — tratamiento activo ({cs.TipoControl})", Tiempo = cs.Fecha.ToString("dd/MM/yyyy") });

            // Producción mensual de los últimos 7 meses
            var hoy = DateTime.Today;
            var produccionMensual = Enumerable.Range(0, 7)
                .Select(i => {
                    var mes = hoy.AddMonths(-6 + i);
                    var kg = cosechas
                        .Where(c => c.Fecha.Month == mes.Month && c.Fecha.Year == mes.Year)
                        .Sum(c => c.PesoNeto);
                    return new ProduccionMensual { Mes = mes.ToString("MMM"), Kg = (int)kg };
                })
                .ToList();

            var maxKg = produccionMensual.Max(p => p.Kg);
            foreach (var p in produccionMensual)
                p.PorcentajeAlto = maxKg > 0 ? (int)Math.Round((double)p.Kg / maxKg * 100) : 0;

            var cosechaEstimadaKg = cosechas.Sum(c => c.PesoNeto);

            var vm = new DashboardViewModel
            {
                TotalColmenas          = colmenas.Count,
                InspeccionesPendientes = inspecciones.Count(i => i.Estado == "pendiente"),
                CosechaEstimada        = cosechaEstimadaKg >= 1000
                                            ? $"{cosechaEstimadaKg / 1000.0:F1} t"
                                            : $"{cosechaEstimadaKg} kg",
                TotalApiarios          = apiarios.Count,
                ColmenasVerde          = colmenas.Count(c => c.EstadoSemaforo == "verde"),
                ColmenasAmarillo       = colmenas.Count(c => c.EstadoSemaforo == "amarillo"),
                ColmenasRojo           = colmenas.Count(c => c.EstadoSemaforo == "rojo"),
                Alertas                = alertas,
                ProduccionMensual      = produccionMensual,
            };

            return View(vm);
        }
    }
}
