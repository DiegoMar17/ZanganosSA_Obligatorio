using System.ComponentModel.DataAnnotations.Schema;

namespace ColmenaEmpresa.Models
{
    public class HistorialAcceso
    {
        [Column("HisA_ID")]
        public int Id { get; set; }

        [Column("HisA_IDUser")]
        public string UserId { get; set; } = string.Empty;

        [Column("HisA_NomUser")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Column("HisA_FecHor")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Column("HisA_IP")]
        public string? Ip { get; set; }

        [Column("HisA_Dispos")]
        public string? Dispositivo { get; set; }

        [Column("HisA_Exitoso")]
        public bool Exitoso { get; set; } = true;
    }
}
