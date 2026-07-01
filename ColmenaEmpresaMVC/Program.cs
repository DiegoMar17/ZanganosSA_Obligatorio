using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using ColmenaEmpresa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' no encontrada. " +
        "Configurala en appsettings.Development.json o como variable de entorno.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = false;
    options.Password.RequireLowercase       = false;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccesoDenegado";
});

builder.Services.AddScoped<AuditoriaService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    db.Database.EnsureCreated();

    if (!await roleManager.RoleExistsAsync("ADMIN"))
        await roleManager.CreateAsync(new IdentityRole("ADMIN"));
    if (!await roleManager.RoleExistsAsync("EMPLEADO"))
        await roleManager.CreateAsync(new IdentityRole("EMPLEADO"));

    if (!userManager.Users.Any())
    {
        var admin = new ApplicationUser
        {
            UserName       = "admin@colmena.com",
            Email          = "admin@colmena.com",
            NombreCompleto = "Carlos Bentancur",
            EmailConfirmed = true,
            Rol            = "ADMIN"
        };
        await userManager.CreateAsync(admin, "colmena123");
        await userManager.AddToRoleAsync(admin, "ADMIN");

        var empleado = new ApplicationUser
        {
            UserName       = "empleado@colmena.com",
            Email          = "empleado@colmena.com",
            NombreCompleto = "Laura Rodríguez",
            EmailConfirmed = true,
            Rol            = "EMPLEADO"
        };
        await userManager.CreateAsync(empleado, "colmena123");
        await userManager.AddToRoleAsync(empleado, "EMPLEADO");
    }
    else
    {
        var admin = await userManager.FindByEmailAsync("admin@colmena.com");
        if (admin is not null && !await userManager.IsInRoleAsync(admin, "ADMIN"))
        {
            admin.Rol = "ADMIN";
            await userManager.UpdateAsync(admin);
            await userManager.AddToRoleAsync(admin, "ADMIN");
        }
    }

    if (!db.Apiarios.Any())
    {
        var rinconada    = new Apiario { Nombre="La Rinconada",  Departamento="San José",  Ubicacion="Ruta 3 km 85",  Latitud=-34.337, Longitud=-56.713, Flora="Eucaliptal",   Acceso="Todo tiempo",          CapacidadColmenas=30, EstadoSemaforo="rojo" };
        var monteOlivo   = new Apiario { Nombre="Monte Olivo",   Departamento="Canelones", Ubicacion="Ruta 8 km 48",  Latitud=-34.523, Longitud=-56.284, Flora="Monte nativo", Acceso="Todo tiempo",          CapacidadColmenas=40, EstadoSemaforo="verde" };
        var eucaliptal   = new Apiario { Nombre="El Eucaliptal", Departamento="Lavalleja", Ubicacion="Ruta 81 km 12", Latitud=-34.375, Longitud=-55.237, Flora="Eucaliptal",   Acceso="Solo con buen tiempo", CapacidadColmenas=25, EstadoSemaforo="amarillo" };
        var pasoCarrasco = new Apiario { Nombre="Paso Carrasco", Departamento="Rocha",     Ubicacion="Ruta 9 km 220", Latitud=-34.483, Longitud=-54.333, Flora="Pradera",      Acceso="Requiere 4x4",         CapacidadColmenas=35, EstadoSemaforo="amarillo" };
        db.Apiarios.AddRange(rinconada, monteOlivo, eucaliptal, pasoCarrasco);
        db.SaveChanges();

        db.Colmenas.AddRange(
            new Colmena { Codigo="C-01",  ApiarioId=monteOlivo.Id,   Tipo="Langstroth", FechaInstalacion=new DateTime(2022,9,1),  EstadoReina="vista",    CantidadAlzas=2, MarcosConCria=8, EstadoSemaforo="verde",    UltimaVisita=DateTime.Now.AddDays(-4) },
            new Colmena { Codigo="C-47",  ApiarioId=rinconada.Id,    Tipo="Langstroth", FechaInstalacion=new DateTime(2021,3,15), EstadoReina="no_vista", CantidadAlzas=1, MarcosConCria=5, EstadoSemaforo="amarillo", UltimaVisita=DateTime.Now.AddDays(-18) },
            new Colmena { Codigo="C-82",  ApiarioId=rinconada.Id,    Tipo="Langstroth", FechaInstalacion=new DateTime(2020,11,1), EstadoReina="ausente",  CantidadAlzas=0, MarcosConCria=2, EstadoSemaforo="rojo",     UltimaVisita=DateTime.Now.AddDays(-25) },
            new Colmena { Codigo="C-110", ApiarioId=pasoCarrasco.Id, Tipo="Núcleo",     FechaInstalacion=new DateTime(2023,10,1), EstadoReina="vista",    CantidadAlzas=1, MarcosConCria=6, EstadoSemaforo="viaje",    UltimaVisita=null }
        );
        db.Inspecciones.AddRange(
            new Inspeccion { ApiarioId=monteOlivo.Id,   Fecha=new DateTime(2026,4,18), Clima="Nublado",  Temperatura=19, ColmenasInspeccionadas=18, TotalColmenas=24, Estado="completa" },
            new Inspeccion { ApiarioId=eucaliptal.Id,   Fecha=new DateTime(2026,4,14), Clima="Lluvia",   Temperatura=15, ColmenasInspeccionadas=12, TotalColmenas=15, Estado="incompleta" },
            new Inspeccion { ApiarioId=pasoCarrasco.Id, Fecha=new DateTime(2026,4,10), Clima="Soleado",  Temperatura=24, ColmenasInspeccionadas=21, TotalColmenas=21, Estado="completa" }
        );
        db.RegistrosFinancieros.AddRange(
            new RegistroFinanciero { TipoMovimiento="ingreso",   Categoria="Cosecha miel", Descripcion="Venta primavera 2026",   Fecha=new DateTime(2026,1,15), Monto=1850, ApiarioId=monteOlivo.Id },
            new RegistroFinanciero { TipoMovimiento="gasto",     Categoria="Insumos",      Descripcion="Ácido oxálico + frames", Fecha=new DateTime(2026,1,20), Monto=320 },
            new RegistroFinanciero { TipoMovimiento="inversion", Categoria="Equipamiento", Descripcion="Extractor nuevo",        Fecha=new DateTime(2026,2,1),  Monto=2100 },
            new RegistroFinanciero { TipoMovimiento="ingreso",   Categoria="Polen",        Descripcion="Venta mercado local",    Fecha=new DateTime(2026,2,10), Monto=480 }
        );
        db.Cosechas.AddRange(
            new Cosecha { ApiarioId=monteOlivo.Id,   Fecha=new DateTime(2026,1,10), TipoMiel="Multifloral",  AlzasCosechadas=12, PesoBruto=855, Merma=15, Humedad=18.2, Destino="Exportación" },
            new Cosecha { ApiarioId=pasoCarrasco.Id, Fecha=new DateTime(2026,1,20), TipoMiel="Eucalipto",    AlzasCosechadas=9,  PesoBruto=640, Merma=10, Humedad=17.8, Destino="Fraccionado local" },
            new Cosecha { ApiarioId=eucaliptal.Id,   Fecha=new DateTime(2026,2,5),  TipoMiel="Eucalipto",    AlzasCosechadas=8,  PesoBruto=515, Merma=10, Humedad=18.0, Destino="Stock" },
            new Cosecha { ApiarioId=rinconada.Id,    Fecha=new DateTime(2026,2,18), TipoMiel="Monte nativo", AlzasCosechadas=7,  PesoBruto=415, Merma=10, Humedad=18.5, Destino="Exportación" }
        );
        db.ControlesSanitarios.AddRange(
            new ControlSanitario { ApiarioId=rinconada.Id,  ColmenasAfectadas="C-47", TipoControl="Varroa — recuento", Resultado="positivo", Tratamiento="Ácido oxálico", Fecha=new DateTime(2026,4,20), Estado="en_tratamiento" },
            new ControlSanitario { ApiarioId=monteOlivo.Id, ColmenasAfectadas="C-01", TipoControl="Nosema",            Resultado="negativo", Tratamiento="—",             Fecha=new DateTime(2026,4,18), Estado="limpio" },
            new ControlSanitario { ApiarioId=rinconada.Id,  ColmenasAfectadas="C-82", TipoControl="Varroa — recuento", Resultado="positivo", Tratamiento="Ácido fórmico", Fecha=new DateTime(2026,4,14), Estado="en_tratamiento" }
        );
        db.Traslados.AddRange(
            new Traslado { Nombre="Verano 2026",    ApiarioOrigenId=pasoCarrasco.Id, ApiarioDestinoId=monteOlivo.Id,   CantidadColmenas=21, DistanciaKm=184, FechaSalida=new DateTime(2026,1,1),  FechaRetorno=new DateTime(2026,6,30), Estado="en_curso",   PorcentajeAvance=45 },
            new Traslado { Nombre="Primavera 2025", ApiarioOrigenId=pasoCarrasco.Id, ApiarioDestinoId=rinconada.Id,    CantidadColmenas=18, DistanciaKm=140, FechaSalida=new DateTime(2023,9,1),  FechaRetorno=new DateTime(2023,12,1), Estado="completado", PorcentajeAvance=100 }
        );
        db.ItemsInventario.AddRange(
            new ItemInventario { Nombre="Alzas de madera",  Unidad="u",  CantidadActual=80,  CantidadMaxima=100, CantidadMinima=20 },
            new ItemInventario { Nombre="Ácido oxálico",    Unidad="kg", CantidadActual=2,   CantidadMaxima=8,   CantidadMinima=2 },
            new ItemInventario { Nombre="Marcos de cera",   Unidad="u",  CantidadActual=120, CantidadMaxima=200, CantidadMinima=40 },
            new ItemInventario { Nombre="Jarabe azucarado", Unidad="L",  CantidadActual=5,   CantidadMaxima=50,  CantidadMinima=20 },
            new ItemInventario { Nombre="Trajes apícolas",  Unidad="u",  CantidadActual=4,   CantidadMaxima=4,   CantidadMinima=2 }
        );
        db.SaveChanges();

        var adminUser = await userManager.FindByEmailAsync("admin@colmena.com");
        var adminId   = adminUser?.Id ?? throw new InvalidOperationException(
            "No se pudo obtener el ID del admin para el seed de alertas.");

        static double SeedHav(double la1, double lo1, double la2, double lo2)
        {
            const double R = 6371;
            var dL = (la2 - la1) * Math.PI / 180;
            var dO = (lo2 - lo1) * Math.PI / 180;
            var a  = Math.Sin(dL/2)*Math.Sin(dL/2) + Math.Cos(la1*Math.PI/180)*Math.Cos(la2*Math.PI/180)*Math.Sin(dO/2)*Math.Sin(dO/2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
        var apiosGeo = db.Apiarios.Where(a => a.Latitud.HasValue && a.Longitud.HasValue).ToList();

        var al1 = new AlertaComunitaria {
            Titulo        = "Brote de Varroa detectado — Zona San José",
            Descripcion   = "Se confirmó infestación elevada de Varroa destructor en apiarios de la zona. Se recomienda realizar recuentos inmediatos y aplicar tratamiento preventivo con ácido oxálico o ácido fórmico según temperatura.",
            TipoAmenaza   = "sanitaria", Latitud=-34.337, Longitud=-56.713, RadioKm=50,
            Ubicacion     = "Ruta 3, San José", Estado="activa",
            FechaCreacion = DateTime.Now.AddDays(-3), ReportadoPorId=adminId
        };
        foreach (var ap in apiosGeo)
        {
            var d = SeedHav(al1.Latitud, al1.Longitud, ap.Latitud!.Value, ap.Longitud!.Value);
            if (d <= al1.RadioKm) al1.Notificaciones.Add(new NotificacionAlerta { ApiarioId=ap.Id, DistanciaKm=Math.Round(d,2), FechaEnvio=al1.FechaCreacion });
        }

        var al2 = new AlertaComunitaria {
            Titulo          = "Fumigación aérea de agroquímicos — Canelones",
            Descripcion     = "Se reportó aplicación aérea de herbicidas en cultivos de soja adyacentes. Riesgo de intoxicación de colonias por deriva del producto. Se recomienda evitar floración cercana durante 72 horas.",
            TipoAmenaza     = "ambiental", Latitud=-34.523, Longitud=-56.284, RadioKm=60,
            Ubicacion       = "Ruta 8, Canelones", Estado="resuelta",
            FechaCreacion   = DateTime.Now.AddDays(-10), FechaResolucion=DateTime.Now.AddDays(-7),
            ReportadoPorId  = adminId
        };
        foreach (var ap in apiosGeo)
        {
            var d = SeedHav(al2.Latitud, al2.Longitud, ap.Latitud!.Value, ap.Longitud!.Value);
            if (d <= al2.RadioKm) al2.Notificaciones.Add(new NotificacionAlerta { ApiarioId=ap.Id, DistanciaKm=Math.Round(d,2), FechaEnvio=al2.FechaCreacion, Leida=true });
        }

        db.AlertasComunitarias.AddRange(al1, al2);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}")
    .WithStaticAssets();

app.Run();
