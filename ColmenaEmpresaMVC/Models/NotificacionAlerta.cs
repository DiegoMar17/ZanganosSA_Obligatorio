using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class NotificacionAlerta
    {
        public int Id { get; set; }

        public int AlertaId { get; set; }
        public AlertaComunitaria Alerta { get; set; } = null!;

        public int ApiarioId { get; set; }

        [StringLength(100)]
        public string ApiarioNombre { get; set; } = string.Empty;

        public double DistanciaKm { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        public bool Leida { get; set; }
    }
}
