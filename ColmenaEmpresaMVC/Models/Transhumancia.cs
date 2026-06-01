using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Transhumancia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del traslado es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apiario de origen es obligatorio.")]
        [StringLength(100)]
        public string ApiarioOrigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apiario de destino es obligatorio.")]
        [StringLength(100)]
        public string ApiarioDestino { get; set; } = string.Empty;

        [Range(1, 500, ErrorMessage = "Cantidad de colmenas entre 1 y 500.")]
        public int CantidadColmenas { get; set; }

        [Range(0.1, 5000, ErrorMessage = "Distancia entre 0.1 y 5000 km.")]
        public double DistanciaKm { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        public DateTime FechaSalida { get; set; }

        public DateTime? FechaRetorno { get; set; }

        [StringLength(300)]
        public string Motivo { get; set; } = string.Empty;

        public string Estado { get; set; } = "en_curso"; // planificado | en_curso | completado

        [Range(0, 100)]
        public int PorcentajeAvance { get; set; }
    }
}
