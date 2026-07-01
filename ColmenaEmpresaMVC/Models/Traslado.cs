using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Traslado
    {
        [Column("Tras_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del traslado es obligatorio.")]
        [StringLength(100)]
        [Column("Tras_Nom")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apiario de origen es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario de origen.")]
        [Column("Tras_IDApiOrig")]
        public int ApiarioOrigenId { get; set; }
        public Apiario? ApiarioOrigen { get; set; }

        [Required(ErrorMessage = "El apiario de destino es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario de destino.")]
        [Column("Tras_IDApiDest")]
        public int ApiarioDestinoId { get; set; }
        public Apiario? ApiarioDestino { get; set; }

        [Range(1, 500, ErrorMessage = "Cantidad de colmenas entre 1 y 500.")]
        [Column("Tras_CantCol")]
        public int CantidadColmenas { get; set; }

        [Range(0.0, 5000, ErrorMessage = "Distancia entre 0 y 5000 km.")]
        [Column("Tras_DistKm")]
        public double DistanciaKm { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        [Column("Tras_FecSal")]
        public DateTime FechaSalida { get; set; }

        [Column("Tras_FecRet")]
        public DateTime? FechaRetorno { get; set; }

        [StringLength(300)]
        [Column("Tras_Motiv")]
        public string? Motivo { get; set; }

        [Column("Tras_Est")]
        public string Estado { get; set; } = "planificado";

        [Range(0, 100)]
        [Column("Tras_PorcAv")]
        public int PorcentajeAvance { get; set; }
    }
}
