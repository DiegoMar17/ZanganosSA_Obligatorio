namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Representa una colmena individual dentro de un apiario.
    /// </summary>
    public class Colmena
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int ApiarioId { get; set; }
        public string ApiarioNombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Langstroth"; // Langstroth | Núcleo | Otro
        public DateTime FechaInstalacion { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string EstadoReina { get; set; } = "vista"; // vista | no_vista | ausente
        public int CantidadAlzas { get; set; }
        public int MarcosConCria { get; set; }
        public string EstadoSemaforo { get; set; } = "verde"; // verde | amarillo | rojo | viaje
        public DateTime? UltimaVisita { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
}
