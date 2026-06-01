using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Visita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El apiario es obligatorio.")]
        [StringLength(100)]
        public string ApiarioNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaPlanificada { get; set; }

        [StringLength(300)]
        public string Materiales { get; set; } = string.Empty;

        public string Estado { get; set; } = "planificada"; // planificada | completada

        public bool EstaVencida => Estado == "planificada" && FechaPlanificada.Date < DateTime.Today;
    }
}
