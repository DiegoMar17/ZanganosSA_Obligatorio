using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Cosecha
    {
        [Column("Cos_ID")]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        [Column("Cos_IDApi")]
        public int ApiarioId { get; set; }
        public Apiario? Apiario { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Column("Cos_Fec")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El tipo de miel es obligatorio.")]
        [StringLength(100)]
        [Column("Cos_TipoMiel")]
        public string TipoMiel { get; set; } = "Multifloral";

        [Range(1, 200, ErrorMessage = "Alzas cosechadas entre 1 y 200.")]
        [Column("Cos_AlzCos")]
        public int AlzasCosechadas { get; set; }

        [Range(0.01, 10000, ErrorMessage = "Peso bruto debe ser mayor a 0.")]
        [Column("Cos_PesBrut")]
        public double PesoBruto { get; set; }

        [Range(0, 10000, ErrorMessage = "Merma entre 0 y 10000 kg.")]
        [Column("Cos_Merma")]
        public double Merma { get; set; }

        public double PesoNeto => PesoBruto - Merma;

        [Range(0, 100, ErrorMessage = "Humedad entre 0 y 100%.")]
        [Column("Cos_Humid")]
        public double Humedad { get; set; }

        [Range(0, 100, ErrorMessage = "HMF entre 0 y 100.")]
        [Column("Cos_HMF")]
        public double HMF { get; set; }

        [StringLength(100)]
        [Column("Cos_Dest")]
        public string Destino { get; set; } = "Stock";

        [StringLength(500)]
        [Column("Cos_Notas")]
        public string? Notas { get; set; }

        [Column("Cos_Vend")]
        public bool Vendida { get; set; }

        [Range(0, 100000, ErrorMessage = "Precio por kg entre 0 y 100000.")]
        [Column("Cos_PrecKg")]
        public decimal PrecioPorKg { get; set; }

        public decimal MontoVenta => Math.Round((decimal)PesoNeto * PrecioPorKg, 2);
    }
}
