using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class Auditoria
    {
        [Column("Aud_ID")]
        public int Id { get; set; }

        [Column("Aud_IDUser")]
        public string UserId { get; set; } = string.Empty;

        [Column("Aud_NomUser")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("Aud_Accion")]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("Aud_Tabla")]
        public string Tabla { get; set; } = string.Empty;

        [Column("Aud_FecHor")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Column("Aud_Det")]
        public string? Detalle { get; set; }
    }
}
