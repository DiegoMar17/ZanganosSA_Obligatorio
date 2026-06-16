using Microsoft.AspNetCore.Identity;

namespace ColmenaEmpresa.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = "EMPLEADO";
        public string? PinHash { get; set; }
        public bool PinActivo { get; set; }
        public int? ApiarioAsignadoId { get; set; }
    }
}
