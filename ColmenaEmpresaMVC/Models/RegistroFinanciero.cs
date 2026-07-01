using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class RegistroFinanciero
    {
        [Column("RegF_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
        [Column("RegF_TipoMov")]
        public string TipoMovimiento { get; set; } = "ingreso";

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(100)]
        [Column("RegF_Categ")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300)]
        [Column("RegF_Descr")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Column("RegF_Fec")]
        public DateTime Fecha { get; set; }

        [Range(0.01, 10000000, ErrorMessage = "El monto debe ser mayor a 0.")]
        [Column("RegF_Monto")]
        public decimal Monto { get; set; }

        [Column("RegF_IDApi")]
        public int? ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Column("RegF_IDCos")]
        public int? CosechaId { get; set; }
    }
}
