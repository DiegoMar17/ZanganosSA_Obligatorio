using ColmenaEmpresa.Data;
using ColmenaEmpresa.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=colmena.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

// Require authentication globally — use [AllowAnonymous] para excluir rutas
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

var app = builder.Build();

// Crear BD, aplicar Identity y sembrar datos
using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    db.Database.EnsureCreated();

    // Seed usuario admin
    if (!userManager.Users.Any())
    {
        var admin = new ApplicationUser
        {
            UserName      = "admin@colmena.com",
            Email         = "admin@colmena.com",
            NombreCompleto = "Carlos Bentancur",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "colmena123");
    }

    // Seed datos de ejemplo
    if (!db.Apiarios.Any())
    {
        db.Apiarios.AddRange(
            new Apiario { Nombre="La Rinconada",  Departamento="San José",  Ubicacion="Ruta 3 km 85",  Latitud=-34.38, Longitud=-56.68, Flora="Eucaliptal",   Acceso="Todo tiempo",          CapacidadColmenas=30, TotalColmenas=18, EstadoSemaforo="rojo" },
            new Apiario { Nombre="Monte Olivo",   Departamento="Canelones", Ubicacion="Ruta 8 km 48",  Latitud=-34.47, Longitud=-56.25, Flora="Monte nativo", Acceso="Todo tiempo",          CapacidadColmenas=40, TotalColmenas=24, EstadoSemaforo="verde" },
            new Apiario { Nombre="El Eucaliptal", Departamento="Lavalleja", Ubicacion="Ruta 81 km 12", Latitud=-34.32, Longitud=-55.63, Flora="Eucaliptal",   Acceso="Solo con buen tiempo", CapacidadColmenas=25, TotalColmenas=15, EstadoSemaforo="amarillo" },
            new Apiario { Nombre="Paso Carrasco", Departamento="Rocha",     Ubicacion="Ruta 9 km 220", Latitud=-34.13, Longitud=-54.92, Flora="Pradera",      Acceso="Requiere 4x4",         CapacidadColmenas=35, TotalColmenas=21, EstadoSemaforo="amarillo" }
        );
        db.Colmenas.AddRange(
            new Colmena { Codigo="C-01",  ApiarioId=2, ApiarioNombre="Monte Olivo",   Tipo="Langstroth", FechaInstalacion=new DateTime(2022,9,1),  EstadoReina="vista",    CantidadAlzas=2, MarcosConCria=8, EstadoSemaforo="verde",    UltimaVisita=DateTime.Now.AddDays(-4) },
            new Colmena { Codigo="C-47",  ApiarioId=1, ApiarioNombre="La Rinconada",  Tipo="Langstroth", FechaInstalacion=new DateTime(2021,3,15), EstadoReina="no_vista", CantidadAlzas=1, MarcosConCria=5, EstadoSemaforo="amarillo", UltimaVisita=DateTime.Now.AddDays(-18) },
            new Colmena { Codigo="C-82",  ApiarioId=1, ApiarioNombre="La Rinconada",  Tipo="Langstroth", FechaInstalacion=new DateTime(2020,11,1), EstadoReina="ausente",  CantidadAlzas=0, MarcosConCria=2, EstadoSemaforo="rojo",     UltimaVisita=DateTime.Now.AddDays(-25) },
            new Colmena { Codigo="C-110", ApiarioId=4, ApiarioNombre="Paso Carrasco", Tipo="Núcleo",     FechaInstalacion=new DateTime(2023,10,1), EstadoReina="vista",    CantidadAlzas=1, MarcosConCria=6, EstadoSemaforo="viaje",    UltimaVisita=null }
        );
        db.Inspecciones.AddRange(
            new Inspeccion { ApiarioId=2, ApiarioNombre="Monte Olivo",   Fecha=new DateTime(2024,4,18), Clima="⛅ Nublado", Temperatura=19, ColmenasInspeccionadas=18, TotalColmenas=24, Estado="completa" },
            new Inspeccion { ApiarioId=3, ApiarioNombre="El Eucaliptal", Fecha=new DateTime(2024,4,14), Clima="🌧 Lluvia",  Temperatura=15, ColmenasInspeccionadas=12, TotalColmenas=15, Estado="incompleta" },
            new Inspeccion { ApiarioId=4, ApiarioNombre="Paso Carrasco", Fecha=new DateTime(2024,4,10), Clima="☀ Soleado", Temperatura=24, ColmenasInspeccionadas=21, TotalColmenas=21, Estado="completa" }
        );
        db.RegistrosFinancieros.AddRange(
            new RegistroFinanciero { TipoMovimiento="ingreso",   Categoria="Cosecha miel",  Descripcion="Venta primavera 2024",   Fecha=new DateTime(2024,11,15), Monto=1850, ApiarioNombre="Monte Olivo" },
            new RegistroFinanciero { TipoMovimiento="gasto",     Categoria="Insumos",       Descripcion="Ácido oxálico + frames", Fecha=new DateTime(2024,11,20), Monto=320,  ApiarioNombre="General" },
            new RegistroFinanciero { TipoMovimiento="inversion", Categoria="Equipamiento",  Descripcion="Extractor nuevo",        Fecha=new DateTime(2024,12,1),  Monto=2100, ApiarioNombre="General" },
            new RegistroFinanciero { TipoMovimiento="ingreso",   Categoria="Polen",         Descripcion="Venta mercado local",    Fecha=new DateTime(2024,12,10), Monto=480,  ApiarioNombre="General" }
        );
        db.Cosechas.AddRange(
            new Cosecha { ApiarioId=2, ApiarioNombre="Monte Olivo",   Fecha=new DateTime(2024,11,10), TipoMiel="Multifloral",  AlzasCosechadas=12, PesoBruto=855, Merma=15, Humedad=18.2, Destino="Exportación" },
            new Cosecha { ApiarioId=4, ApiarioNombre="Paso Carrasco", Fecha=new DateTime(2024,11,20), TipoMiel="Eucalipto",    AlzasCosechadas=9,  PesoBruto=640, Merma=10, Humedad=17.8, Destino="Fraccionado local" },
            new Cosecha { ApiarioId=3, ApiarioNombre="El Eucaliptal", Fecha=new DateTime(2024,12,5),  TipoMiel="Eucalipto",    AlzasCosechadas=8,  PesoBruto=515, Merma=10, Humedad=18.0, Destino="Stock" },
            new Cosecha { ApiarioId=1, ApiarioNombre="La Rinconada",  Fecha=new DateTime(2024,12,18), TipoMiel="Monte nativo", AlzasCosechadas=7,  PesoBruto=415, Merma=10, Humedad=18.5, Destino="Exportación" }
        );
        db.ControlesSanitarios.AddRange(
            new ControlSanitario { ApiarioId=1, ApiarioNombre="La Rinconada", ColmenasAfectadas="C-47", TipoControl="Varroa — recuento", Resultado="positivo", Tratamiento="Ácido oxálico", Fecha=new DateTime(2024,4,20), Estado="en_tratamiento" },
            new ControlSanitario { ApiarioId=2, ApiarioNombre="Monte Olivo",  ColmenasAfectadas="C-01", TipoControl="Nosema",            Resultado="negativo", Tratamiento="—",             Fecha=new DateTime(2024,4,18), Estado="limpio" },
            new ControlSanitario { ApiarioId=1, ApiarioNombre="La Rinconada", ColmenasAfectadas="C-82", TipoControl="Varroa — recuento", Resultado="positivo", Tratamiento="Ácido fórmico", Fecha=new DateTime(2024,4,14), Estado="en_tratamiento" }
        );
        db.Transhumancias.AddRange(
            new Transhumancia { Nombre="Verano 2024",    ApiarioOrigen="Paso Carrasco", ApiarioDestino="El Trébol (temp.)", CantidadColmenas=21, DistanciaKm=184, FechaSalida=new DateTime(2024,1,1),  FechaRetorno=new DateTime(2024,6,30), Estado="en_curso",   PorcentajeAvance=45 },
            new Transhumancia { Nombre="Primavera 2023", ApiarioOrigen="Paso Carrasco", ApiarioDestino="La Rinconada",      CantidadColmenas=18, DistanciaKm=140, FechaSalida=new DateTime(2023,9,1),  FechaRetorno=new DateTime(2023,12,1), Estado="completado", PorcentajeAvance=100 }
        );
        db.ItemsInventario.AddRange(
            new ItemInventario { Nombre="Alzas de madera",  Unidad="u",  CantidadActual=80,  CantidadMaxima=100, CantidadMinima=20 },
            new ItemInventario { Nombre="Ácido oxálico",    Unidad="kg", CantidadActual=2,   CantidadMaxima=8,   CantidadMinima=2 },
            new ItemInventario { Nombre="Marcos de cera",   Unidad="u",  CantidadActual=120, CantidadMaxima=200, CantidadMinima=40 },
            new ItemInventario { Nombre="Jarabe azucarado", Unidad="L",  CantidadActual=5,   CantidadMaxima=50,  CantidadMinima=20 },
            new ItemInventario { Nombre="Trajes apícolas",  Unidad="u",  CantidadActual=4,   CantidadMaxima=4,   CantidadMinima=2 }
        );
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
