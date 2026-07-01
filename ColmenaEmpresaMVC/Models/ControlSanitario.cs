using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class ControlSanitario
    {
        [Column("CtrlS_ID")]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        [Column("CtrlS_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [StringLength(500)]
        [Column("CtrlS_ColAfect")]
        public string? ColmenasAfectadas { get; set; }

        [Required(ErrorMessage = "El tipo de control es obligatorio.")]
        [StringLength(100)]
        [Column("CtrlS_TipoCtrl")]
        public string TipoControl { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("CtrlS_Resul")]
        public string? Resultado { get; set; }

        [StringLength(200)]
        [Column("CtrlS_Trat")]
        public string? Tratamiento { get; set; }

        [StringLength(100)]
        [Column("CtrlS_Dos")]
        public string? Dosis { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Column("CtrlS_Fec")]
        public DateTime Fecha { get; set; }

        [Column("CtrlS_Est")]
        public string Estado { get; set; } = "en_tratamiento";

        [StringLength(1000)]
        [Column("CtrlS_Obs")]
        public string? Observaciones { get; set; }
    }
}
