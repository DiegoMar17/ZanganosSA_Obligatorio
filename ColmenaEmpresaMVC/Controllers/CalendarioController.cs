using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColmenaEmpresa.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly UserManager<ApplicationUser> _users;

        public CalendarioController(AppDbContext ctx, UserManager<ApplicationUser> users)
        {
            _ctx   = ctx;
            _users = users;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var now = DateTime.Now;
            var y   = year  ?? now.Year;
            var m   = month ?? now.Month;

            var desde = new DateTime(y, m, 1);
            var hasta = desde.AddMonths(1);

            int? sectorId = null;
            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                sectorId = user?.ApiarioAsignadoId;
                if (!sectorId.HasValue)
                    return View(new CalendarioViewModel { Year = y, Month = m, Eventos = new List<EventoCalendario>() });
            }

            var eventos = new List<EventoCalendario>();

            var inspQuery = _ctx.Inspecciones.Include(i => i.Apiario)
                .Where(i => i.Fecha >= desde && i.Fecha < hasta);
            if (sectorId.HasValue) inspQuery = inspQuery.Where(i => i.ApiarioId == sectorId.Value);
            foreach (var i in inspQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = i.Fecha.Day,
                    Tipo   = "inspeccion",
                    Titulo = $"Inspección — {i.Apiario?.Nombre ?? ""}",
                    Meta   = $"{i.ColmenasInspeccionadas}/{i.TotalColmenas} colmenas · {i.Estado}"
                });

            var cosQuery = _ctx.Cosechas.Include(c => c.Apiario)
                .Where(c => c.Fecha >= desde && c.Fecha < hasta);
            if (sectorId.HasValue) cosQuery = cosQuery.Where(c => c.ApiarioId == sectorId.Value);
            foreach (var c in cosQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = c.Fecha.Day,
                    Tipo   = "cosecha",
                    Titulo = $"Cosecha — {c.Apiario?.Nombre ?? ""}",
                    Meta   = $"{c.PesoNeto} kg netos · {c.TipoMiel}"
                });

            var csQuery = _ctx.ControlesSanitarios.Include(cs => cs.Apiario)
                .Where(cs => cs.Fecha >= desde && cs.Fecha < hasta);
            if (sectorId.HasValue) csQuery = csQuery.Where(cs => cs.ApiarioId == sectorId.Value);
            foreach (var cs in csQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = cs.Fecha.Day,
                    Tipo   = "sanidad",
                    Titulo = $"{cs.TipoControl} — {cs.Apiario?.Nombre ?? ""}",
                    Meta   = $"Resultado: {cs.Resultado}"
                });

            var trasQuery = _ctx.Traslados
                .Include(t => t.ApiarioOrigen)
                .Include(t => t.ApiarioDestino)
                .Where(t => t.FechaSalida >= desde && t.FechaSalida < hasta);
            if (sectorId.HasValue)
                trasQuery = trasQuery.Where(t => t.ApiarioOrigenId == sectorId.Value || t.ApiarioDestinoId == sectorId.Value);
            foreach (var t in trasQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = t.FechaSalida.Day,
                    Tipo   = "traslado",
                    Titulo = $"Traslado: {t.Nombre}",
                    Meta   = $"{t.ApiarioOrigen?.Nombre ?? ""} → {t.ApiarioDestino?.Nombre ?? ""}"
                });

            var tarQuery = _ctx.Tareas
                .Where(t => t.FechaVencimiento != null
                         && t.FechaVencimiento >= desde
                         && t.FechaVencimiento < hasta);
            if (!User.IsInRole("ADMIN"))
            {
                var userId = _users.GetUserId(User);
                tarQuery   = tarQuery.Where(t => t.AsignadoAId == userId);
            }
            foreach (var ta in tarQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = ta.FechaVencimiento!.Value.Day,
                    Tipo   = "tarea",
                    Titulo = $"Tarea: {ta.Nombre}",
                    Meta   = $"{ta.Categoria} · prioridad {ta.Prioridad}" + (ta.Completada ? " · completada" : "")
                });

            var visQuery = _ctx.Visitas.Include(v => v.Apiario)
                .Where(v => v.FechaPlanificada >= desde && v.FechaPlanificada < hasta);
            if (sectorId.HasValue) visQuery = visQuery.Where(v => v.ApiarioId == sectorId.Value);
            foreach (var v in visQuery.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia    = v.FechaPlanificada.Day,
                    Tipo   = "visita",
                    Titulo = $"Visita — {v.Apiario?.Nombre ?? ""}",
                    Meta   = string.IsNullOrWhiteSpace(v.Materiales) ? v.Estado : $"{v.Estado} · {v.Materiales}"
                });

            return View(new CalendarioViewModel { Year = y, Month = m, Eventos = eventos });
        }
    }
}
