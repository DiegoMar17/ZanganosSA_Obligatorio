using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class RegistroFinanciero
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
        public string TipoMovimiento { get; set; } = "ingreso"; // ingreso | gasto | inversion

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Range(0.01, 10000000, ErrorMessage = "El monto debe ser mayor a 0.")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        public string ApiarioNombre { get; set; } = "General";

        // FK nullable hacia Cosecha — permite sincronizar ingresos al editar/eliminar
        public int? CosechaId { get; set; }
    }
}
