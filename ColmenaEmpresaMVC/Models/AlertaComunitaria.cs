using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class AlertaComunitaria
    {
        [Column("AltC_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Column("AltC_Tit")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000)]
        [Column("AltC_Descr")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("AltC_TipoAmen")]
        public string TipoAmenaza { get; set; } = "sanitaria";

        [Range(-90, 90, ErrorMessage = "Latitud entre -90 y 90.")]
        [Column("AltC_Lat")]
        public double Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud entre -180 y 180.")]
        [Column("AltC_Lon")]
        public double Longitud { get; set; }

        [Range(1, 200, ErrorMessage = "Radio entre 1 y 200 km.")]
        [Column("AltC_RadKm")]
        public double RadioKm { get; set; } = 10;

        [StringLength(200)]
        [Column("AltC_Ubic")]
        public string? Ubicacion { get; set; }

        [Column("AltC_Est")]
        public string Estado { get; set; } = "activa";

        [Column("AltC_FecCreac")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("AltC_FecResol")]
        public DateTime? FechaResolucion { get; set; }

        [Column("AltC_IDReportPor")]
        public string? ReportadoPorId { get; set; }
        public ApplicationUser? ReportadoPor { get; set; }

        public ICollection<NotificacionAlerta> Notificaciones { get; set; } = new List<NotificacionAlerta>();
    }
}
