namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Registro de una inspección realizada en un apiario.
    /// </summary>
    public class Inspeccion
    {
        public int Id { get; set; }
        public int ApiarioId { get; set; }
        public string ApiarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Clima { get; set; } = string.Empty;
        public double Temperatura { get; set; }
        public int ColmenasInspeccionadas { get; set; }
        public int TotalColmenas { get; set; }
        public string Estado { get; set; } = "completa"; // completa | incompleta | pendiente
        public string NotasCampo { get; set; } = string.Empty;
    }
}
