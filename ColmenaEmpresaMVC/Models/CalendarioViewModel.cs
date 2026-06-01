using System.Globalization;

namespace ColmenaEmpresa.Models
{
    public class EventoCalendario
    {
        public int Dia { get; set; }
        public string Tipo { get; set; } = string.Empty; // visit | cosecha | salud | tarea
        public string Titulo { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
    }

    public class CalendarioViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<EventoCalendario> Eventos { get; set; } = new();

        public string MesNombre => new DateTime(Year, Month, 1)
            .ToString("MMMM yyyy", new CultureInfo("es-UY"));

        public int DiasEnMes => DateTime.DaysInMonth(Year, Month);

        // Offset lunes=0 ... domingo=6
        public int OffsetPrimerDia
        {
            get
            {
                var dow = (int)new DateTime(Year, Month, 1).DayOfWeek;
                return dow == 0 ? 6 : dow - 1;
            }
        }

        public int PrevYear  => Month == 1  ? Year - 1 : Year;
        public int PrevMonth => Month == 1  ? 12 : Month - 1;
        public int NextYear  => Month == 12 ? Year + 1 : Year;
        public int NextMonth => Month == 12 ? 1 : Month + 1;

        public IEnumerable<EventoCalendario> EventosDel(int dia) =>
            Eventos.Where(e => e.Dia == dia);
    }
}
