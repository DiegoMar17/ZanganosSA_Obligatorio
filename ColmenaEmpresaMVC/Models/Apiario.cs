using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Apiario
    {
        [Column("Api_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Column("Api_Nom")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [StringLength(50)]
        [Column("Api_Dep")]
        public string Departamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ubicación es obligatoria.")]
        [StringLength(200)]
        [Column("Api_Ubic")]
        public string Ubicacion { get; set; } = string.Empty;

        [Range(-90, 90, ErrorMessage = "Latitud entre -90 y 90.")]
        [Column("Api_Lat")]
        public double? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud entre -180 y 180.")]
        [Column("Api_Lon")]
        public double? Longitud { get; set; }

        [StringLength(100)]
        [Column("Api_Flora")]
        public string? Flora { get; set; }

        [StringLength(200)]
        [Column("Api_Acc")]
        public string? Acceso { get; set; }

        [Column("Api_FntAgu")]
        public bool FuenteAgua { get; set; }

        [Range(0, 500, ErrorMessage = "Capacidad entre 0 y 500 colmenas.")]
        [Column("Api_CapCol")]
        public int CapacidadColmenas { get; set; }

        [Column("Api_EstSem")]
        public string EstadoSemaforo { get; set; } = "verde";

        [NotMapped]
        public int TotalColmenas { get; set; }

        [Column("Api_IDResp")]
        public string? ResponsableId { get; set; }
        public ApplicationUser? Responsable { get; set; }

    }
}
