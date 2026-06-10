using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class AlertaComunitaria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoAmenaza { get; set; } = "sanitaria";

        [Range(-90, 90, ErrorMessage = "Latitud entre -90 y 90.")]
        public double Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud entre -180 y 180.")]
        public double Longitud { get; set; }

        [Range(1, 200, ErrorMessage = "Radio entre 1 y 200 km.")]
        public double RadioKm { get; set; } = 10;

        [StringLength(200)]
        public string Ubicacion { get; set; } = string.Empty;

        public string Estado { get; set; } = "activa";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaResolucion { get; set; }

        [StringLength(150)]
        public string ReportadoPor { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notas { get; set; } = string.Empty;

        public ICollection<NotificacionAlerta> Notificaciones { get; set; } = new List<NotificacionAlerta>();
    }
}
