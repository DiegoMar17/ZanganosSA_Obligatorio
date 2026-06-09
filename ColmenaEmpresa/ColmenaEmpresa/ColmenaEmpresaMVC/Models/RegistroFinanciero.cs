namespace ColmenaEmpresa.Models
{
    /// <summary>
    /// Registro de un movimiento financiero (ingreso, gasto o inversión).
    /// </summary>
    public class RegistroFinanciero
    {
        public int Id { get; set; }
        public string TipoMovimiento { get; set; } = "ingreso"; // ingreso | gasto | inversion
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string ApiarioNombre { get; set; } = "General";
    }
}
