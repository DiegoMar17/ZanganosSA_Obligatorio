using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Tarea
    {
        [Column("Tar_ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200)]
        [Column("Tar_Nom")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("Tar_Categ")]
        public string Categoria { get; set; } = "General";

        [Column("Tar_Prior")]
        public string Prioridad { get; set; } = "media";

        [Column("Tar_FecVenc")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("Tar_Complet")]
        public bool Completada { get; set; }

        [Column("Tar_FecCreac")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("Tar_IDAsig")]
        public string? AsignadoAId { get; set; }
        public ApplicationUser? AsignadoA { get; set; }
    }
}
