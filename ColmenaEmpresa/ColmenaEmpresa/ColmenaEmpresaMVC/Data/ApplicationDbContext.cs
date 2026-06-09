using Microsoft.EntityFrameworkCore;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Apiario> Apiarios { get; set; }
        public DbSet<Colmena> Colmenas { get; set; }
        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<ControlSanitario> ControlesSanitarios { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<RegistroFinanciero> RegistrosFinancieros { get; set; }
        public DbSet<ItemInventario> ItemsInventario { get; set; }
        public DbSet<Transhumancia> Transhumancias { get; set; }
    }
}
