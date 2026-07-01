using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ColmenaEmpresa.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Apiario> Apiarios { get; set; }
        public DbSet<Colmena> Colmenas { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<ControlSanitario> ControlesSanitarios { get; set; }
        public DbSet<Traslado> Traslados { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<RegistroFinanciero> RegistrosFinancieros { get; set; }
        public DbSet<ItemInventario> ItemsInventario { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<AlertaComunitaria> AlertasComunitarias { get; set; }
        public DbSet<NotificacionAlerta> NotificacionesAlerta { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<HistorialAcceso> HistorialesAcceso { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apiario → ResponsableId (ApplicationUser) sin cascade de Identity
            builder.Entity<Apiario>()
                .HasOne(a => a.Responsable)
                .WithMany()
                .HasForeignKey(a => a.ResponsableId)
                .OnDelete(DeleteBehavior.SetNull);

            // Colmena → AsignadoA (ApplicationUser)
            builder.Entity<Colmena>()
                .HasOne(c => c.AsignadoA)
                .WithMany()
                .HasForeignKey(c => c.AsignadoAId)
                .OnDelete(DeleteBehavior.SetNull);

            // Tarea → AsignadoA (ApplicationUser)
            builder.Entity<Tarea>()
                .HasOne(t => t.AsignadoA)
                .WithMany()
                .HasForeignKey(t => t.AsignadoAId)
                .OnDelete(DeleteBehavior.SetNull);

            // AlertaComunitaria → ReportadoPor (ApplicationUser)
            builder.Entity<AlertaComunitaria>()
                .HasOne(a => a.ReportadoPor)
                .WithMany()
                .HasForeignKey(a => a.ReportadoPorId)
                .OnDelete(DeleteBehavior.SetNull);

            // MovimientoInventario → User (ApplicationUser)
            builder.Entity<MovimientoInventario>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Inspecciones: NO ACTION para evitar ciclos de cascade
            builder.Entity<Inspeccion>()
                .HasOne(i => i.Apiario)
                .WithMany()
                .HasForeignKey(i => i.ApiarioId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Inspeccion>()
                .HasOne(i => i.Colmena)
                .WithMany()
                .HasForeignKey(i => i.ColmenaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Inspeccion>()
                .HasOne(i => i.Visita)
                .WithMany()
                .HasForeignKey(i => i.VisitaId)
                .OnDelete(DeleteBehavior.NoAction);

            // Traslados: dos FKs a Apiarios → NO ACTION
            builder.Entity<Traslado>()
                .HasOne(t => t.ApiarioOrigen)
                .WithMany()
                .HasForeignKey(t => t.ApiarioOrigenId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Traslado>()
                .HasOne(t => t.ApiarioDestino)
                .WithMany()
                .HasForeignKey(t => t.ApiarioDestinoId)
                .OnDelete(DeleteBehavior.NoAction);

            // ControlesSanitarios → Apiario NO ACTION
            builder.Entity<ControlSanitario>()
                .HasOne(c => c.Apiario)
                .WithMany()
                .HasForeignKey(c => c.ApiarioId)
                .OnDelete(DeleteBehavior.NoAction);

            // NotificacionAlerta → Apiario NO ACTION
            builder.Entity<NotificacionAlerta>()
                .HasOne(n => n.Apiario)
                .WithMany()
                .HasForeignKey(n => n.ApiarioId)
                .OnDelete(DeleteBehavior.NoAction);

            // Precisión explícita para columnas decimal (evita truncamiento silencioso en SQL Server)
            builder.Entity<Cosecha>()
                .Property(c => c.PrecioPorKg)
                .HasPrecision(18, 4);

            builder.Entity<RegistroFinanciero>()
                .Property(r => r.Monto)
                .HasPrecision(18, 2);

            // Mapeo explícito de tabla para Traslado → Traslados
            builder.Entity<Traslado>().ToTable("Traslados");
            builder.Entity<MovimientoInventario>().ToTable("MovimientosInventario");
        }
    }
}
