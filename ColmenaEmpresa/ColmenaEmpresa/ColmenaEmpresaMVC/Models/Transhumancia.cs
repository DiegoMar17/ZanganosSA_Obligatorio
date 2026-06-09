namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Representa un evento de transhumancia (traslado de colmenas).
    /// </summary>
    public class Transhumancia
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApiarioOrigen { get; set; } = string.Empty;
        public string ApiarioDestino { get; set; } = string.Empty;
        public int CantidadColmenas { get; set; }
        public double DistanciaKm { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime? FechaRetorno { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = "en_curso"; // planificado | en_curso | completado
        public int PorcentajeAvance { get; set; }
    }
}
