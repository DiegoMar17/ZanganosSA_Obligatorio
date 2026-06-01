using Microsoft.AspNetCore.Identity;

namespace ColmenaEmpresa.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
