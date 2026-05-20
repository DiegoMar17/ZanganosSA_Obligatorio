namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Registro de una cosecha de miel en un apiario.
    /// </summary>
    public class Cosecha
    {
        public int Id { get; set; }
        public int ApiarioId { get; set; }
        public string ApiarioNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string TipoMiel { get; set; } = "Multifloral";
        public int AlzasCosechadas { get; set; }
        public double PesoBruto { get; set; }
        public double Merma { get; set; }
        public double PesoNeto => PesoBruto - Merma;
        public double Humedad { get; set; }
        public double HMF { get; set; }
        public string Destino { get; set; } = "Stock";
        public string Notas { get; set; } = string.Empty;
        public bool CrearRegistroIngreso { get; set; } = true;
    }
}
