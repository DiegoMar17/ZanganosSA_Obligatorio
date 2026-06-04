using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Cosecha
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un apiario.")]
        public int ApiarioId { get; set; }

        public string ApiarioNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El tipo de miel es obligatorio.")]
        [StringLength(100)]
        public string TipoMiel { get; set; } = "Multifloral";

        [Range(1, 200, ErrorMessage = "Alzas cosechadas entre 1 y 200.")]
        public int AlzasCosechadas { get; set; }

        [Range(0.01, 10000, ErrorMessage = "Peso bruto debe ser mayor a 0.")]
        public double PesoBruto { get; set; }

        [Range(0, 10000, ErrorMessage = "Merma entre 0 y 10000 kg.")]
        public double Merma { get; set; }

        public double PesoNeto => PesoBruto - Merma;

        [Range(0, 100, ErrorMessage = "Humedad entre 0 y 100%.")]
        public double Humedad { get; set; }

        [Range(0, 100, ErrorMessage = "HMF entre 0 y 100.")]
        public double HMF { get; set; }

        [StringLength(100)]
        public string Destino { get; set; } = "Stock";

        [StringLength(500)]
        public string Notas { get; set; } = string.Empty;

        // ── Venta (integración con Finanzas) ──
        public bool Vendida { get; set; }

        [Range(0, 100000, ErrorMessage = "Precio por kg entre 0 y 100000.")]
        public decimal PrecioPorKg { get; set; }

        // Monto total de la venta (no se persiste, se calcula)
        public decimal MontoVenta => Math.Round((decimal)PesoNeto * PrecioPorKg, 2);
    }
}
