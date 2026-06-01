using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class ControlSanitario
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        public int ApiarioId { get; set; }

        public string ApiarioNombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string ColmenasAfectadas { get; set; } = string.Empty; // CSV de códigos

        [Required(ErrorMessage = "El tipo de control es obligatorio.")]
        [StringLength(100)]
        public string TipoControl { get; set; } = string.Empty;

        [StringLength(50)]
        public string Resultado { get; set; } = string.Empty; // positivo | negativo | dudoso

        [StringLength(200)]
        public string Tratamiento { get; set; } = string.Empty;

        [StringLength(100)]
        public string Dosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        public string Estado { get; set; } = "en_tratamiento"; // en_tratamiento | limpio

        [StringLength(1000)]
        public string Observaciones { get; set; } = string.Empty;
    }
}
