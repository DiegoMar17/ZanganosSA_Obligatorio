using System.ComponentModel.DataAnnotations;

namespace ColmenaEmpresa.Models
{
    public class ItemInventario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La unidad es obligatoria.")]
        [StringLength(20)]
        public string Unidad { get; set; } = "u";

        [Range(0, 100000, ErrorMessage = "Cantidad entre 0 y 100000.")]
        public double CantidadActual { get; set; }

        [Range(0, 100000, ErrorMessage = "Cantidad entre 0 y 100000.")]
        public double CantidadMaxima { get; set; }

        [Range(0, 100000, ErrorMessage = "Cantidad entre 0 y 100000.")]
        public double CantidadMinima { get; set; }

        public int PorcentajeStock =>
            CantidadMaxima > 0
                ? (int)Math.Round(CantidadActual / CantidadMaxima * 100)
                : 0;

        public string EstadoStock =>
            PorcentajeStock >= 50 ? "verde" :
            PorcentajeStock >= 20 ? "amarillo" : "rojo";
    }
}
