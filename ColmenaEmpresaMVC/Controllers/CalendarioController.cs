using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var desde = new DateTime(y, m, 1);
            var hasta = desde.AddMonths(1);

            // Empleado: solo eventos de su sector asignado. Admin: todo.
            int? sectorId = null;
            string? sectorNombre = null;
            if (!User.IsInRole("ADMIN"))
            {
                var user = await _users.GetUserAsync(User);
                sectorId = user?.ApiarioAsignadoId;
                sectorNombre = sectorId.HasValue ? _ctx.Apiarios.Find(sectorId.Value)?.Nombre : null;
                if (!sectorId.HasValue)
                    return View(new CalendarioViewModel { Year = y, Month = m, Eventos = new List<EventoCalendario>() });
            }

            var eventos = new List<EventoCalendario>();

            var inspecciones = _ctx.Inspecciones.Where(i => i.Fecha >= desde && i.Fecha < hasta);
            if (sectorId.HasValue) inspecciones = inspecciones.Where(i => i.ApiarioId == sectorId.Value);
            foreach (var i in inspecciones.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = i.Fecha.Day, Tipo = "inspeccion",
                    Titulo = $"Inspección — {i.ApiarioNombre}",
                    Meta = $"{i.ColmenasInspeccionadas}/{i.TotalColmenas} colmenas · {i.Estado}"
                });

            var cosechas = _ctx.Cosechas.Where(c => c.Fecha >= desde && c.Fecha < hasta);
            if (sectorId.HasValue) cosechas = cosechas.Where(c => c.ApiarioId == sectorId.Value);
            foreach (var c in cosechas.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = c.Fecha.Day, Tipo = "cosecha",
                    Titulo = $"Cosecha — {c.ApiarioNombre}",
                    Meta = $"{c.PesoNeto} kg netos · {c.TipoMiel}"
                });

            var controles = _ctx.ControlesSanitarios.Where(cs => cs.Fecha >= desde && cs.Fecha < hasta);
            if (sectorId.HasValue) controles = controles.Where(cs => cs.ApiarioId == sectorId.Value);
            foreach (var cs in controles.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = cs.Fecha.Day, Tipo = "sanidad",
                    Titulo = $"{cs.TipoControl} — {cs.ApiarioNombre}",
                    Meta = $"Resultado: {cs.Resultado}"
                });

            // Transhumancia no tiene FK a Apiario (Origen/Destino son texto libre) — se compara por nombre.
            var traslados = _ctx.Transhumancias.Where(t => t.FechaSalida >= desde && t.FechaSalida < hasta);
            if (sectorNombre is not null)
                traslados = traslados.Where(t => t.ApiarioOrigen == sectorNombre || t.ApiarioDestino == sectorNombre);
            foreach (var t in traslados.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = t.FechaSalida.Day, Tipo = "traslado",
                    Titulo = $"Traslado: {t.Nombre}",
                    Meta = $"{t.ApiarioOrigen} → {t.ApiarioDestino}"
                });

            // Tareas de planificación con fecha límite — al empleado solo le importan las suyas.
            var tareas = _ctx.Tareas.Where(t => t.FechaVencimiento != null
                        && t.FechaVencimiento >= desde && t.FechaVencimiento < hasta);
            if (!User.IsInRole("ADMIN"))
            {
                var userId = _users.GetUserId(User);
                tareas = tareas.Where(t => t.AsignadoAId == userId);
            }
            foreach (var ta in tareas.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = ta.FechaVencimiento!.Value.Day, Tipo = "tarea",
                    Titulo = $"Tarea: {ta.Nombre}",
                    Meta = $"{ta.Categoria} · prioridad {ta.Prioridad}" + (ta.Completada ? " · ✓ completada" : "")
                });

            // Visitas planificadas — Visita tampoco tiene FK a Apiario, se compara por nombre.
            var visitas = _ctx.Visitas.Where(v => v.FechaPlanificada >= desde && v.FechaPlanificada < hasta);
            if (sectorNombre is not null) visitas = visitas.Where(v => v.ApiarioNombre == sectorNombre);
            foreach (var v in visitas.ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = v.FechaPlanificada.Day, Tipo = "visita",
                    Titulo = $"Visita — {v.ApiarioNombre}",
                    Meta = string.IsNullOrWhiteSpace(v.Materiales) ? v.Estado : $"{v.Estado} · {v.Materiales}"
                });

            return View(new CalendarioViewModel { Year = y, Month = m, Eventos = eventos });
        }
    }
}
