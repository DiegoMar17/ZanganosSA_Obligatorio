using ColmenaEmpresa.Models;

namespace ColmenaEmpresa.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext db)
        {
            if (db.Apiarios.Any()) return;

            db.Apiarios.AddRange(
                new Apiario { Id = 1, Nombre = "La Rinconada",  Departamento = "San José",  Ubicacion = "34°23'S 56°41'W", EstadoSemaforo = "rojo",     TotalColmenas = 18, Flora = "Eucaliptal",   Acceso = "Todo tiempo" },
                new Apiario { Id = 2, Nombre = "Monte Olivo",   Departamento = "Canelones", Ubicacion = "34°28'S 56°15'W", EstadoSemaforo = "verde",    TotalColmenas = 24, Flora = "Monte nativo", Acceso = "Todo tiempo" },
                new Apiario { Id = 3, Nombre = "El Eucaliptal", Departamento = "Lavalleja", Ubicacion = "34°19'S 55°38'W", EstadoSemaforo = "amarillo", TotalColmenas = 15, Flora = "Eucaliptal",   Acceso = "Solo con buen tiempo" },
                new Apiario { Id = 4, Nombre = "Paso Carrasco", Departamento = "Rocha",     Ubicacion = "34°08'S 54°55'W", EstadoSemaforo = "amarillo", TotalColmenas = 21, Flora = "Pradera",      Acceso = "Requiere 4x4" }
            );

            db.Colmenas.AddRange(
                new Colmena { Id = 1, Codigo = "C-01",  ApiarioId = 2, ApiarioNombre = "Monte Olivo",   EstadoReina = "vista",    EstadoSemaforo = "verde",    CantidadAlzas = 2, UltimaVisita = DateTime.Now.AddDays(-4) },
                new Colmena { Id = 2, Codigo = "C-47",  ApiarioId = 1, ApiarioNombre = "La Rinconada",  EstadoReina = "no_vista", EstadoSemaforo = "amarillo", CantidadAlzas = 1, UltimaVisita = DateTime.Now.AddDays(-18) },
                new Colmena { Id = 3, Codigo = "C-82",  ApiarioId = 1, ApiarioNombre = "La Rinconada",  EstadoReina = "ausente",  EstadoSemaforo = "rojo",     CantidadAlzas = 0, UltimaVisita = DateTime.Now.AddDays(-25) },
                new Colmena { Id = 4, Codigo = "C-110", ApiarioId = 4, ApiarioNombre = "Paso Carrasco", EstadoReina = "vista",    EstadoSemaforo = "viaje",    CantidadAlzas = 1, UltimaVisita = null }
            );

            db.Inspecciones.AddRange(
                new Inspeccion { Id = 1, ApiarioId = 2, ApiarioNombre = "Monte Olivo",   ColmenasInspeccionadas = 18, TotalColmenas = 24, Fecha = new DateTime(2024,4,18), Clima = "⛅ Nublado",  Temperatura = 19, Estado = "completa"   },
                new Inspeccion { Id = 2, ApiarioId = 3, ApiarioNombre = "El Eucaliptal", ColmenasInspeccionadas = 12, TotalColmenas = 15, Fecha = new DateTime(2024,4,14), Clima = "🌧 Lluvia",   Temperatura = 15, Estado = "incompleta" },
                new Inspeccion { Id = 3, ApiarioId = 4, ApiarioNombre = "Paso Carrasco", ColmenasInspeccionadas = 21, TotalColmenas = 21, Fecha = new DateTime(2024,4,10), Clima = "☀ Soleado",   Temperatura = 24, Estado = "completa"   }
            );

            db.ControlesSanitarios.AddRange(
                new ControlSanitario { Id = 1, ApiarioId = 1, ApiarioNombre = "La Rinconada", ColmenasAfectadas = "C-47", TipoControl = "Varroa",  Resultado = "positivo", Tratamiento = "Ácido oxálico",  Fecha = new DateTime(2024,4,20), Estado = "en_tratamiento" },
                new ControlSanitario { Id = 2, ApiarioId = 2, ApiarioNombre = "Monte Olivo",  ColmenasAfectadas = "C-12", TipoControl = "Nosema",  Resultado = "negativo", Tratamiento = "—",              Fecha = new DateTime(2024,4,18), Estado = "limpio"         },
                new ControlSanitario { Id = 3, ApiarioId = 1, ApiarioNombre = "La Rinconada", ColmenasAfectadas = "C-82", TipoControl = "Varroa",  Resultado = "positivo", Tratamiento = "Ácido fórmico",  Fecha = new DateTime(2024,4,14), Estado = "en_tratamiento" }
            );

            db.Cosechas.AddRange(
                new Cosecha { Id = 1, ApiarioId = 2, ApiarioNombre = "Monte Olivo",   Fecha = new DateTime(2024,11,10), TipoMiel = "Multifloral",  AlzasCosechadas = 12, PesoBruto = 855, Merma = 15, Destino = "Exportación"       },
                new Cosecha { Id = 2, ApiarioId = 4, ApiarioNombre = "Paso Carrasco", Fecha = new DateTime(2024,11,20), TipoMiel = "Eucalipto",    AlzasCosechadas = 9,  PesoBruto = 640, Merma = 10, Destino = "Fraccionado local" },
                new Cosecha { Id = 3, ApiarioId = 3, ApiarioNombre = "El Eucaliptal", Fecha = new DateTime(2024,12,5),  TipoMiel = "Eucalipto",    AlzasCosechadas = 8,  PesoBruto = 515, Merma = 10, Destino = "Stock"             },
                new Cosecha { Id = 4, ApiarioId = 1, ApiarioNombre = "La Rinconada",  Fecha = new DateTime(2024,12,18), TipoMiel = "Monte nativo", AlzasCosechadas = 7,  PesoBruto = 415, Merma = 10, Destino = "Exportación"       }
            );

            db.RegistrosFinancieros.AddRange(
                new RegistroFinanciero { Id = 1, TipoMovimiento = "ingreso",   Categoria = "Cosecha miel",  Descripcion = "Venta primavera 2024",   Fecha = new DateTime(2024,11,15), Monto = 1850, ApiarioNombre = "Monte Olivo" },
                new RegistroFinanciero { Id = 2, TipoMovimiento = "gasto",     Categoria = "Insumos",       Descripcion = "Ácido oxálico + frames", Fecha = new DateTime(2024,11,20), Monto = 320,  ApiarioNombre = "General"     },
                new RegistroFinanciero { Id = 3, TipoMovimiento = "inversion", Categoria = "Equipamiento",  Descripcion = "Extractor nuevo",        Fecha = new DateTime(2024,12,1),  Monto = 2100, ApiarioNombre = "General"     },
                new RegistroFinanciero { Id = 4, TipoMovimiento = "ingreso",   Categoria = "Polen",         Descripcion = "Venta mercado local",    Fecha = new DateTime(2024,12,10), Monto = 480,  ApiarioNombre = "General"     }
            );

            db.ItemsInventario.AddRange(
                new ItemInventario { Id = 1, Nombre = "Alzas de madera",  Unidad = "u",  CantidadActual = 80,  CantidadMaxima = 100, CantidadMinima = 20 },
                new ItemInventario { Id = 2, Nombre = "Ácido oxálico",    Unidad = "kg", CantidadActual = 2,   CantidadMaxima = 8,   CantidadMinima = 2  },
                new ItemInventario { Id = 3, Nombre = "Marcos de cera",   Unidad = "u",  CantidadActual = 120, CantidadMaxima = 200, CantidadMinima = 40 },
                new ItemInventario { Id = 4, Nombre = "Jarabe azucarado", Unidad = "L",  CantidadActual = 5,   CantidadMaxima = 50,  CantidadMinima = 20 },
                new ItemInventario { Id = 5, Nombre = "Trajes apícolas",  Unidad = "u",  CantidadActual = 4,   CantidadMaxima = 4,   CantidadMinima = 2  }
            );

            db.Transhumancias.AddRange(
                new Transhumancia { Id = 1, Nombre = "Verano 2024",    ApiarioOrigen = "Paso Carrasco", ApiarioDestino = "El Trébol (temp.)", CantidadColmenas = 21, DistanciaKm = 184, FechaSalida = new DateTime(2024,1,1),   FechaRetorno = new DateTime(2024,6,30),  Estado = "en_curso",   PorcentajeAvance = 45  },
                new Transhumancia { Id = 2, Nombre = "Primavera 2023", ApiarioOrigen = "Paso Carrasco", ApiarioDestino = "La Rinconada",      CantidadColmenas = 18, DistanciaKm = 140, FechaSalida = new DateTime(2023,9,1),   FechaRetorno = new DateTime(2023,12,1),  Estado = "completado", PorcentajeAvance = 100 },
                new Transhumancia { Id = 3, Nombre = "Verano 2023",    ApiarioOrigen = "Monte Olivo",   ApiarioDestino = "El Eucaliptal",     CantidadColmenas = 12, DistanciaKm = 95,  FechaSalida = new DateTime(2023,12,15), FechaRetorno = new DateTime(2024,3,15), Estado = "completado", PorcentajeAvance = 100 }
            );

            db.SaveChanges();
        }
    }
}
