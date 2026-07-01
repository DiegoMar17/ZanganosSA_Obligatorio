using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public ItemInventario? Item { get; set; }

        [Required]
        public string Tipo { get; set; } = "Entrada"; // Entrada | Salida

        [Range(0.01, 100000)]
        public double Cantidad { get; set; }

        public DateTime Fecha { get; set; }

        [StringLength(200)]
        public string? Motivo { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
