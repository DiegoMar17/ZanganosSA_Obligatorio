namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Representa un apiario (ubicación física con colmenas).
    /// </summary>
    public class Apiario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string Flora { get; set; } = string.Empty;
        public string Acceso { get; set; } = string.Empty;
        public bool FuenteAgua { get; set; }
        public int CapacidadColmenas { get; set; }
        public string EstadoSemaforo { get; set; } = "verde"; // verde | amarillo | rojo
        public int TotalColmenas { get; set; }
    }
}
