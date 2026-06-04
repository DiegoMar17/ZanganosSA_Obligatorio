using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Mvc;

namespace ColmenaEmpresa.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly AppDbContext _ctx;

        public CalendarioController(AppDbContext ctx) => _ctx = ctx;

        public IActionResult Index(int? year, int? month)
        {
            var now = DateTime.Now;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var desde = new DateTime(y, m, 1);
            var hasta = desde.AddMonths(1);

            var eventos = new List<EventoCalendario>();

            foreach (var i in _ctx.Inspecciones.Where(i => i.Fecha >= desde && i.Fecha < hasta).ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = i.Fecha.Day, Tipo = "inspeccion",
                    Titulo = $"Inspección — {i.ApiarioNombre}",
                    Meta = $"{i.ColmenasInspeccionadas}/{i.TotalColmenas} colmenas · {i.Estado}"
                });

            foreach (var c in _ctx.Cosechas.Where(c => c.Fecha >= desde && c.Fecha < hasta).ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = c.Fecha.Day, Tipo = "cosecha",
                    Titulo = $"Cosecha — {c.ApiarioNombre}",
                    Meta = $"{c.PesoNeto} kg netos · {c.TipoMiel}"
                });

            foreach (var cs in _ctx.ControlesSanitarios.Where(cs => cs.Fecha >= desde && cs.Fecha < hasta).ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = cs.Fecha.Day, Tipo = "sanidad",
                    Titulo = $"{cs.TipoControl} — {cs.ApiarioNombre}",
                    Meta = $"Resultado: {cs.Resultado}"
                });

            foreach (var t in _ctx.Transhumancias.Where(t => t.FechaSalida >= desde && t.FechaSalida < hasta).ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = t.FechaSalida.Day, Tipo = "traslado",
                    Titulo = $"Traslado: {t.Nombre}",
                    Meta = $"{t.ApiarioOrigen} → {t.ApiarioDestino}"
                });

            // Tareas de planificación con fecha límite
            foreach (var ta in _ctx.Tareas.Where(t => t.FechaVencimiento != null
                        && t.FechaVencimiento >= desde && t.FechaVencimiento < hasta).ToList())
                eventos.Add(new EventoCalendario
                {
                    Dia = ta.FechaVencimiento!.Value.Day, Tipo = "tarea",
                    Titulo = $"Tarea: {ta.Nombre}",
                    Meta = $"{ta.Categoria} · prioridad {ta.Prioridad}" + (ta.Completada ? " · ✓ completada" : "")
                });

            // Visitas planificadas
            foreach (var v in _ctx.Visitas.Where(v => v.FechaPlanificada >= desde && v.FechaPlanificada < hasta).ToList())
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
