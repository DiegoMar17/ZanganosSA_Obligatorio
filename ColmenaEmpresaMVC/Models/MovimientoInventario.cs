using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class MovimientoInventario
    {
        [Column("MovI_ID")]
        public int Id { get; set; }

        [Column("MovI_IDItem")]
        public int ItemId { get; set; }
        public ItemInventario? Item { get; set; }

        [Required]
        [Column("MovI_Tipo")]
        public string Tipo { get; set; } = "Entrada";

        [Range(0.01, 100000)]
        [Column("MovI_Cant")]
        public double Cantidad { get; set; }

        [Column("MovI_Fec")]
        public DateTime Fecha { get; set; }

        [StringLength(200)]
        [Column("MovI_Motiv")]
        public string? Motivo { get; set; }

        [Column("MovI_IDUser")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
