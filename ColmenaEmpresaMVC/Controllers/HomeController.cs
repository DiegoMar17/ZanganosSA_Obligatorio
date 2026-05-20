using Microsoft.AspNetCore.Mvc;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    /// <summary>
    /// Dashboard principal con resumen de la operación apícola.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var vm = new DashboardViewModel
            {
                TotalColmenas        = 148,
                InspeccionesPendientes = 3,
                CosechaEstimada      = "2.4 t",
                TotalApiarios        = 12,
                ColmenasVerde        = 88,
                ColmenasAmarillo     = 43,
                ColmenasRojo         = 17,
                Alertas = new List<AlertaDashboard>
                {
                    new() { Tipo = "red",   Titulo = "La Rinconada — inspección vencida 12 días",  Tiempo = "Hace 12 días · San José" },
                    new() { Tipo = "amber", Titulo = "Colmena #47 — reina no vista en última visita", Tiempo = "Hace 3 días" },
                    new() { Tipo = "green", Titulo = "Transhumancia \"Verano 24\" completada",       Tiempo = "Hace 1 semana" },
                },
                ProduccionMensual = new List<ProduccionMensual>
                {
                    new() { Mes = "Oct", Kg = 310, PorcentajeAlto = 38 },
                    new() { Mes = "Nov", Kg = 445, PorcentajeAlto = 55 },
                    new() { Mes = "Dic", Kg = 550, PorcentajeAlto = 68 },
                    new() { Mes = "Ene", Kg = 420, PorcentajeAlto = 52 },
                    new() { Mes = "Feb", Kg = 712, PorcentajeAlto = 88 },
                    new() { Mes = "Mar", Kg = 808, PorcentajeAlto = 100 },
                    new() { Mes = "Abr", Kg = 534, PorcentajeAlto = 66 },
                }
            };

            return View(vm);
        }
    }
}
