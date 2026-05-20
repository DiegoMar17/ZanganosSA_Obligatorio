namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Ítem de inventario con control de stock.
    /// </summary>
    public class ItemInventario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Unidad { get; set; } = "u";
        public double CantidadActual { get; set; }
        public double CantidadMaxima { get; set; }
        public double CantidadMinima { get; set; }

        /// <summary>Porcentaje del stock actual respecto al máximo (0-100).</summary>
        public int PorcentajeStock =>
            CantidadMaxima > 0
                ? (int)Math.Round(CantidadActual / CantidadMaxima * 100)
                : 0;

        /// <summary>verde | amarillo | rojo</summary>
        public string EstadoStock =>
            PorcentajeStock >= 50 ? "verde" :
            PorcentajeStock >= 20 ? "amarillo" : "rojo";
    }
}
