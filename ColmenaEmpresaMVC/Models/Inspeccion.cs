using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Inspeccion
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        public int ApiarioId { get; set; }

        public string ApiarioNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [StringLength(50)]
        public string Clima { get; set; } = string.Empty;

        [Range(-10, 50, ErrorMessage = "Temperatura entre -10 y 50 °C.")]
        public double Temperatura { get; set; }

        [Range(0, 1000, ErrorMessage = "Valor inválido.")]
        public int ColmenasInspeccionadas { get; set; }

        [Range(0, 1000, ErrorMessage = "Valor inválido.")]
        public int TotalColmenas { get; set; }

        public string Estado { get; set; } = "completa"; // completa | incompleta | pendiente

        [StringLength(1000)]
        public string NotasCampo { get; set; } = string.Empty;
    }
}
