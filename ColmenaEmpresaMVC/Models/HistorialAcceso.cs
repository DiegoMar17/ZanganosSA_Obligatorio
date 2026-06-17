namespace ColmenaEmpresa.Models
{
    public class HistorialAcceso
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public string? Ip { get; set; }
        public string? Dispositivo { get; set; }
        public bool Exitoso { get; set; } = true;
    }
}
