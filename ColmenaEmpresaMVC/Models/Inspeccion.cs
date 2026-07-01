using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Inspeccion
    {
        [Column("Insp_ID")]
        public int Id { get; set; }

        [Column("Insp_Tipo")]
        public string TipoInspeccion { get; set; } = "apiario";

        [Column("Insp_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Column("Insp_IDCol")]
        public int? ColmenaId { get; set; }
        public Colmena? Colmena { get; set; }

        [Column("Insp_IDVis")]
        public int? VisitaId { get; set; }
        public Visita? Visita { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Column("Insp_Fec")]
        public DateTime Fecha { get; set; }

        [StringLength(50)]
        [Column("Insp_Clim")]
        public string? Clima { get; set; }

        [Range(-10, 50, ErrorMessage = "Temperatura entre -10 y 50 °C.")]
        [Column("Insp_Temp")]
        public double Temperatura { get; set; }

        [Range(0, 1000, ErrorMessage = "Valor inválido.")]
        [Column("Insp_ColInsp")]
        public int ColmenasInspeccionadas { get; set; }

        [Range(0, 1000, ErrorMessage = "Valor inválido.")]
        [Column("Insp_TotCol")]
        public int TotalColmenas { get; set; }

        [Column("Insp_Est")]
        public string Estado { get; set; } = "pendiente";

        [StringLength(1000)]
        [Column("Insp_NotCam")]
        public string? NotasCampo { get; set; }
    }
}
