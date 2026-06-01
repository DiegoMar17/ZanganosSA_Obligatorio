using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Apiario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [StringLength(50)]
        public string Departamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicación es obligatoria.")]
        [StringLength(200)]
        public string Ubicacion { get; set; } = string.Empty;

        [Range(-90, 90, ErrorMessage = "Latitud entre -90 y 90.")]
        public double? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud entre -180 y 180.")]
        public double? Longitud { get; set; }

        [StringLength(100)]
        public string Flora { get; set; } = string.Empty;

        [StringLength(100)]
        public string Acceso { get; set; } = string.Empty;

        public bool FuenteAgua { get; set; }

        [Range(1, 500, ErrorMessage = "Capacidad entre 1 y 500 colmenas.")]
        public int CapacidadColmenas { get; set; }

        public string EstadoSemaforo { get; set; } = "verde"; // verde | amarillo | rojo
        public int TotalColmenas { get; set; }
    }
}
