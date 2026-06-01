using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ColmenaEmpresa.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Apiario> Apiarios { get; set; }
        public DbSet<Colmena> Colmenas { get; set; }
        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<ControlSanitario> ControlesSanitarios { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<RegistroFinanciero> RegistrosFinancieros { get; set; }
        public DbSet<Transhumancia> Transhumancias { get; set; }
        public DbSet<ItemInventario> ItemsInventario { get; set; }
    }
}
