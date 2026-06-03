namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// ViewModel que agrupa todos los datos necesarios para el Dashboard principal.
    /// </summary>
    public class DashboardViewModel
    {
        // ── Indicadores clave ──────────────────────────────────────────────
        public int TotalColmenas { get; set; }
        public int ColmenasNuevasMes { get; set; }
        public int InspeccionesPendientes { get; set; }
        public string CosechaTotal { get; set; } = "0";
        public int TotalApiarios { get; set; }
        public int EnTranshumancia { get; set; }

        // ── Semáforo sanitario ─────────────────────────────────────────────
        public int ColmenasVerde { get; set; }
        public int ColmenasAmarillo { get; set; }
        public int ColmenasRojo { get; set; }

        // ── Alertas activas ────────────────────────────────────────────────
        public List<AlertaDashboard> Alertas { get; set; } = new();

        // ── Producción mensual (últimos 7 meses) ───────────────────────────
        public List<ProduccionMensual> ProduccionMensual { get; set; } = new();
    }

    public class AlertaDashboard
    {
        public string Tipo { get; set; } = "red"; // red | amber | green
        public string Titulo { get; set; } = string.Empty;
        public string Tiempo { get; set; } = string.Empty;
    }

    public class ProduccionMensual
    {
        public string Mes { get; set; } = string.Empty;
        public double Kg { get; set; }
        public int PorcentajeAlto { get; set; } // altura relativa de la barra (0-100)
    }
}
