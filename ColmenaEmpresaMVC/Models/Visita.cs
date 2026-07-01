using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Visita
    {
        [Column("Vis_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El apiario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        [Column("Vis_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Column("Vis_FecPlan")]
        public DateTime FechaPlanificada { get; set; }

        [Column("Vis_FecReal")]
        public DateTime? FechaReal { get; set; }

        [StringLength(300)]
        [Column("Vis_Mat")]
        public string? Materiales { get; set; }

        [Column("Vis_Est")]
        public string Estado { get; set; } = "planificada";

        public bool EstaVencida => Estado == "planificada" && FechaPlanificada.Date < DateTime.Today;
    }
}
