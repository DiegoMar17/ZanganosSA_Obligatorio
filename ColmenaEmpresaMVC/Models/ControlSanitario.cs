namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Control sanitario aplicado a una o más colmenas.
    /// </summary>
    public class ControlSanitario
    {
        public int Id { get; set; }
        public int ApiarioId { get; set; }
        public string ApiarioNombre { get; set; } = string.Empty;
        public string ColmenasAfectadas { get; set; } = string.Empty; // CSV de códigos
        public string TipoControl { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty; // positivo | negativo | dudoso
        public string Tratamiento { get; set; } = string.Empty;
        public string Dosis { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "en_tratamiento"; // en_tratamiento | limpio
        public string Observaciones { get; set; } = string.Empty;
    }
}
