using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Colmena
    {
        [Column("Col_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Column("Col_Cod")]
        public string Codigo { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        [Column("Col_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio.")]
        [Column("Col_Tipo")]
        public string Tipo { get; set; } = "Langstroth";

        [Required(ErrorMessage = "La fecha de instalación es obligatoria.")]
        [Column("Col_FecIns")]
        public DateTime FechaInstalacion { get; set; }

        [StringLength(100)]
        [Column("Col_Orig")]
        public string? Origen { get; set; }

        [Required(ErrorMessage = "El estado de la reina es obligatorio.")]
        [Column("Col_EstRei")]
        public string EstadoReina { get; set; } = "vista";

        [Range(0, 20, ErrorMessage = "Cantidad de alzas entre 0 y 20.")]
        [Column("Col_CantAlz")]
        public int CantidadAlzas { get; set; }

        [Range(0, 30, ErrorMessage = "Marcos con cría entre 0 y 30.")]
        [Column("Col_MarCria")]
        public int MarcosConCria { get; set; }

        [Column("Col_EstSem")]
        public string EstadoSemaforo { get; set; } = "verde";

        [Column("Col_UltVis")]
        public DateTime? UltimaVisita { get; set; }

        [StringLength(500)]
        [Column("Col_Obs")]
        public string? Observaciones { get; set; }

        [Column("Col_IDAsig")]
        public string? AsignadoAId { get; set; }
        public ApplicationUser? AsignadoA { get; set; }
    }
}
