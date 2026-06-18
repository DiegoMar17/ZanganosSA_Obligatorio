# ZanganosSA — Requisitos para correr el proyecto

Checklist de todo lo que hace falta descargar/tener instalado para correr `ColmenaEmpresaMVC` en cualquier máquina, de cero.

## 1. Requisito obligatorio

| Recurso | Versión | Link |
|---|---|---|
| .NET SDK | 9.0+ (probado con SDK 9.0.x y 10.0.300) | https://dotnet.microsoft.com/download/dotnet/9.0 |

Verificar con:
```
dotnet --version
```

No se necesita instalar Node.js ni npm — el proyecto no tiene `package.json`, las libs de cliente (Bootstrap, jQuery) ya están vendorizadas en `ColmenaEmpresaMVC/wwwroot/lib/`.

## 2. Paquetes NuGet (se restauran solos)

Definidos en `ColmenaEmpresaMVC/ColmenaEmpresa.csproj`. No requieren paso manual — `dotnet build`/`dotnet run` los descarga automáticamente la primera vez (necesita internet esa primera vez).

| Paquete | Versión |
|---|---|
| BCrypt.Net-Next | 4.0.3 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.5 |
| Microsoft.EntityFrameworkCore.Sqlite | 9.0.5 |
| Microsoft.EntityFrameworkCore.Tools | 9.0.5 |

Si hiciera falta restaurar manual:
```
cd ColmenaEmpresaMVC
dotnet restore
```

## 3. Librerías de cliente (ya incluidas, no instalar nada)

Vendorizadas en `wwwroot/lib/` — vienen en el repo, no se descargan aparte:

| Librería | Versión |
|---|---|
| Bootstrap | 5.3.3 |
| jQuery | 3.7.1 |
| jQuery Validation | 1.21.0 |
| jQuery Validation Unobtrusive | (oficial .NET Foundation) |

## 4. Recursos externos por CDN (necesitan internet para verse bien, no para compilar)

| Recurso | Dónde se usa | URL |
|---|---|---|
| Google Fonts (Fraunces, Plus Jakarta Sans, DM Sans, DM Serif Display) | `_Layout.cshtml`, `Login.cshtml`, `Landing.cshtml`, `AccesoDenegado.cshtml` | fonts.googleapis.com |
| Leaflet 1.9.4 (mapas) | `Alertas/Crear`, `Alertas/Editar`, `Alertas/Index`, `Alertas/Detalle` | cdn.jsdelivr.net/npm/leaflet@1.9.4 |

Sin internet, la app funciona igual — solo se ven sin fuentes custom y sin mapas.

## 5. Base de datos

SQLite local, **no requiere instalar ningún motor de base de datos**. El archivo `colmena.db` se crea solo al arrancar la app (`Database.EnsureCreated()` en `Program.cs`), con roles, usuario admin y datos de ejemplo precargados.

- Admin seed: `admin@colmena.com` / `colmena123`

## 6. Correr el proyecto

```
cd ColmenaEmpresaMVC
dotnet restore
dotnet run
```

Abre en `https://localhost:7120` (o `http://localhost:5058`) — definidos en `Properties/launchSettings.json`.
