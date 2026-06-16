using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Tarea
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string Categoria { get; set; } = "General";

        public string Prioridad { get; set; } = "media"; // alta | media | baja

        public DateTime? FechaVencimiento { get; set; }

        public bool Completada { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? AsignadoAId { get; set; }
        public string AsignadoNombre { get; set; } = string.Empty;
    }
}
