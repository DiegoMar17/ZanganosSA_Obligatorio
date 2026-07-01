using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class NotificacionAlerta
    {
        [Column("NotA_ID")]
        public int Id { get; set; }

        [Column("NotA_IDAlt")]
        public int AlertaId { get; set; }
        public AlertaComunitaria Alerta { get; set; } = null!;

        [Column("NotA_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Column("NotA_DistKm")]
        public double DistanciaKm { get; set; }

        [Column("NotA_FecEnv")]
        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        [Column("NotA_Leida")]
        public bool Leida { get; set; }
    }
}
