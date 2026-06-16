using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tabla { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; } = DateTime.Now;
        public string? Detalle { get; set; }
    }
}
