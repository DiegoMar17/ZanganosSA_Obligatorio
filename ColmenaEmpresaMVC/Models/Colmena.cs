using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Colmena
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        public int ApiarioId { get; set; }

        public string ApiarioNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es obligatorio.")]
        public string Tipo { get; set; } = "Langstroth"; // Langstroth | Núcleo | Otro

        [Required(ErrorMessage = "La fecha de instalación es obligatoria.")]
        public DateTime FechaInstalacion { get; set; }

        [StringLength(100)]
        public string Origen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado de la reina es obligatorio.")]
        public string EstadoReina { get; set; } = "vista"; // vista | no_vista | ausente

        [Range(0, 20, ErrorMessage = "Cantidad de alzas entre 0 y 20.")]
        public int CantidadAlzas { get; set; }

        [Range(0, 30, ErrorMessage = "Marcos con cría entre 0 y 30.")]
        public int MarcosConCria { get; set; }

        public string EstadoSemaforo { get; set; } = "verde"; // verde | amarillo | rojo | viaje
        public DateTime? UltimaVisita { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ── Asignación a empleado (gestionada desde la ficha del empleado) ──
        public string? AsignadoAId { get; set; }
        public string AsignadoNombre { get; set; } = string.Empty;
    }
}
