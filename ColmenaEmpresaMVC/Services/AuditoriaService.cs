using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Services
{
    public class AuditoriaService
    {
        private readonly AppDbContext _ctx;

        public AuditoriaService(AppDbContext ctx) => _ctx = ctx;

        public void Registrar(string userId, string nombreUsuario, string accion, string tabla, string? detalle = null)
        {
            _ctx.Auditorias.Add(new Auditoria
            {
                UserId        = userId,
                NombreUsuario = nombreUsuario,
                Accion        = accion,
                Tabla         = tabla,
                FechaHora     = DateTime.Now,
                Detalle       = detalle
            });
            _ctx.SaveChanges();
        }
    }
}
