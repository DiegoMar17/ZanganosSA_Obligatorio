using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Controllers
{
    public class AlertasController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;
        public AlertasController(AppDbContext ctx, UserManager<ApplicationUser> users) { _ctx = ctx; _users = users; }

        public IActionResult Index()
        {
            var alertas = _ctx.AlertasComunitarias
                .Include(a => a.Notificaciones)
                .OrderByDescending(a => a.FechaCreacion)
                .ToList();

            ViewBag.Total            = alertas.Count;
            ViewBag.Activas          = alertas.Count(a => a.Estado == "activa");
            ViewBag.Resueltas        = alertas.Count(a => a.Estado == "resuelta");
            ViewBag.ApiariosCercanos = alertas
                .Where(a => a.Estado == "activa")
                .Sum(a => a.Notificaciones.Count);

            return View(alertas);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Crear()
        {
            ViewBag.Apiarios = _ctx.Apiarios
                .Where(a => a.Latitud.HasValue && a.Longitud.HasValue)
                .ToList();
            return View(new AlertaComunitaria { RadioKm = 10, FechaCreacion = DateTime.Now });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "ADMIN")]
        public IActionResult Crear(AlertaComunitaria alerta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Apiarios = _ctx.Apiarios.Where(a => a.Latitud.HasValue && a.Longitud.HasValue).ToList();
                return View(alerta);
            }

            alerta.FechaCreacion  = DateTime.Now;
            alerta.ReportadoPorId = _users.GetUserId(User);
            alerta.Estado         = "activa";

            var apiarios = _ctx.Apiarios
                .Where(a => a.Latitud.HasValue && a.Longitud.HasValue)
                .ToList();

            foreach (var api in apiarios)
            {
                var dist = Haversine(alerta.Latitud, alerta.Longitud, api.Latitud!.Value, api.Longitud!.Value);
                if (dist <= alerta.RadioKm)
                {
                    alerta.Notificaciones.Add(new NotificacionAlerta
                    {
                        ApiarioId   = api.Id,
                        DistanciaKm = Math.Round(dist, 2),
                        FechaEnvio  = DateTime.Now
                    });
                }
            }

            _ctx.AlertasComunitarias.Add(alerta);
            _ctx.SaveChanges();

            int n = alerta.Notificaciones.Count;
            TempData["Exito"] = $"Alerta creada. {n} apiario{(n != 1 ? "s" : "")} notificado{(n != 1 ? "s" : "")} dentro del radio de {alerta.RadioKm} km.";
            return RedirectToAction(nameof(Detalle), new { id = alerta.Id });
        }

        public IActionResult Detalle(int id)
        {
            var alerta = _ctx.AlertasComunitarias
                .Include(a => a.Notificaciones).ThenInclude(n => n.Apiario)
                .FirstOrDefault(a => a.Id == id);
            if (alerta == null) return NotFound();

            var apiarioIds = alerta.Notificaciones.Select(n => n.ApiarioId).ToList();
            ViewBag.ApiariosPosJson = System.Text.Json.JsonSerializer.Serialize(
                _ctx.Apiarios
                    .Where(a => apiarioIds.Contains(a.Id) && a.Latitud.HasValue && a.Longitud.HasValue)
                    .Select(a => new { nombre = a.Nombre, lat = a.Latitud!.Value, lng = a.Longitud!.Value })
                    .ToList()
            );

            return View(alerta);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "ADMIN")]
        public IActionResult Resolver(int id)
        {
            var alerta = _ctx.AlertasComunitarias.Find(id);
            if (alerta == null) return NotFound();
            alerta.Estado          = "resuelta";
            alerta.FechaResolucion = DateTime.Now;
            _ctx.SaveChanges();
            TempData["Exito"] = "Alerta marcada como resuelta.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "ADMIN")]
        public IActionResult Eliminar(int id)
        {
            var alerta = _ctx.AlertasComunitarias
                .Include(a => a.Notificaciones)
                .FirstOrDefault(a => a.Id == id);
            if (alerta == null) return NotFound();
            _ctx.NotificacionesAlerta.RemoveRange(alerta.Notificaciones);
            _ctx.AlertasComunitarias.Remove(alerta);
            _ctx.SaveChanges();
            TempData["Exito"] = "Alerta eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // Fórmula Haversine — distancia en km entre dos puntos geográficos
        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
