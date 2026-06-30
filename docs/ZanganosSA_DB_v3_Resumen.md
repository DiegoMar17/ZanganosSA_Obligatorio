# ZanganosSA — Base de datos unificada v3
## Resumen completo: tablas, columnas y tipos

> **Motor:** SQL Server / SQL Server Express (T-SQL)  
> **Origen:** Combinación de `colmena.db` (SQLite/EF Core) + SQL Server original  
> **Fecha:** Junio 2026

---

## Totales

| Categoría | Cantidad |
|---|---|
| Tablas Identity (ASP.NET) | 7 |
| Tablas propias | 21 |
| **Total tablas** | **28** |
| Índices | 13 |
| Relaciones FK | 24 |
| Consultas adaptadas | 7 |

---

## BLOQUE 1 — Identity (7 tablas, gestionadas por ASP.NET)

| Tabla | Descripción |
|---|---|
| `AspNetUsers` | Usuarios del sistema |
| `AspNetRoles` | Roles (`ADMIN`, `EMPLEADO`) |
| `AspNetUserRoles` | Pivot usuario ↔ rol |
| `AspNetUserClaims` | Claims de usuario |
| `AspNetRoleClaims` | Claims de rol |
| `AspNetUserLogins` | Logins externos |
| `AspNetUserTokens` | Tokens de reset/verificación |

### Columnas extra en `AspNetUsers`

| Columna | Tipo | Notas |
|---|---|---|
| `NombreCompleto` | NVARCHAR(200) NOT NULL | |
| `Rol` | NVARCHAR(50) NOT NULL | `ADMIN` / `EMPLEADO` |
| `PinHash` | NVARCHAR(MAX) NULL | |
| `PinActivo` | BIT NOT NULL | |
| `ApiarioAsignadoId` | INT NULL | FK → Apiarios.Id (sector del empleado) |

---

## BLOQUE 2 — Territorio (3 tablas)

### `Propietarios`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Nombre` | NVARCHAR(100) NOT NULL | |
| `Telefono` | VARCHAR(20) NULL | |
| `Email` | VARCHAR(100) NULL | |

### `Apiarios`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Nombre` | NVARCHAR(100) NOT NULL | |
| `Departamento` | NVARCHAR(50) NOT NULL | |
| `Ubicacion` | NVARCHAR(200) NOT NULL | |
| `Latitud` | DECIMAL(9,6) NULL | |
| `Longitud` | DECIMAL(9,6) NULL | |
| `Flora` | NVARCHAR(100) NULL | |
| `Acceso` | NVARCHAR(200) NULL | |
| `FuenteAgua` | BIT NOT NULL | |
| `CapacidadColmenas` | INT NOT NULL | CHECK >= 0 |
| `EstadoSemaforo` | VARCHAR(20) NOT NULL | `verde` / `amarillo` / `rojo` |
| `ResponsableId` | NVARCHAR(450) NULL | FK → AspNetUsers.Id |
| `PropietarioId` | INT NULL | FK → Propietarios.Id |

### `Colmenas`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Codigo` | VARCHAR(20) NOT NULL UNIQUE | |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `Tipo` | NVARCHAR(50) NOT NULL | `Langstroth` / `Núcleo` / `Otro` |
| `FechaInstalacion` | DATE NOT NULL | |
| `Origen` | NVARCHAR(100) NULL | |
| `EstadoReina` | VARCHAR(20) NOT NULL | `vista` / `no_vista` / `ausente` |
| `CantidadAlzas` | INT NOT NULL | 0–20 |
| `MarcosConCria` | INT NOT NULL | 0–30 |
| `EstadoSemaforo` | VARCHAR(20) NOT NULL | `verde` / `amarillo` / `rojo` / `viaje` |
| `UltimaVisita` | DATETIME NULL | |
| `Observaciones` | NVARCHAR(500) NULL | |
| `AsignadoAId` | NVARCHAR(450) NULL | FK → AspNetUsers.Id |

---

## BLOQUE 3 — Visitas e Inspecciones (3 tablas)

### `Visitas`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `FechaPlanificada` | DATE NOT NULL | |
| `FechaReal` | DATE NULL | Fecha efectiva de la visita |
| `Materiales` | NVARCHAR(300) NULL | |
| `Estado` | VARCHAR(20) NOT NULL | `planificada` / `completada` |

### `Inspecciones`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `TipoInspeccion` | VARCHAR(20) NOT NULL | `apiario` / `colmena` |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `ColmenaId` | INT NULL | FK → Colmenas.Id |
| `VisitaId` | INT NULL | FK → Visitas.Id |
| `Fecha` | DATE NOT NULL | |
| `Clima` | NVARCHAR(50) NULL | |
| `Temperatura` | DECIMAL(4,1) NULL | |
| `ColmenasInspeccionadas` | INT NOT NULL | |
| `TotalColmenas` | INT NOT NULL | |
| `Estado` | VARCHAR(20) NOT NULL | `pendiente` / `completa` / `vencida` |
| `NotasCampo` | NVARCHAR(1000) NULL | |

### `EvaluacionesColmena`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `InspeccionId` | INT NOT NULL | FK → Inspecciones.Id |
| `ColmenaId` | INT NOT NULL | FK → Colmenas.Id |
| `EstadoReina` | NVARCHAR(50) NULL | Estado de la reina en esa inspección |
| `Observaciones` | NVARCHAR(500) NULL | |
| UNIQUE | `(InspeccionId, ColmenaId)` | |

---

## BLOQUE 4 — Sanidad (1 tabla)

### `ControlesSanitarios`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `ColmenasAfectadas` | NVARCHAR(500) NULL | CSV de códigos de colmenas |
| `TipoControl` | NVARCHAR(100) NOT NULL | |
| `Resultado` | VARCHAR(20) NULL | `positivo` / `negativo` / `dudoso` |
| `Tratamiento` | NVARCHAR(200) NULL | |
| `Dosis` | NVARCHAR(100) NULL | |
| `Fecha` | DATE NOT NULL | |
| `Estado` | VARCHAR(20) NOT NULL | `en_tratamiento` / `limpio` |
| `Observaciones` | NVARCHAR(1000) NULL | |

---

## BLOQUE 5 — Traslados (2 tablas)

### `Traslados`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Nombre` | NVARCHAR(100) NOT NULL | |
| `ApiarioOrigenId` | INT NOT NULL | FK → Apiarios.Id |
| `ApiarioDestinoId` | INT NOT NULL | FK → Apiarios.Id |
| `CantidadColmenas` | INT NOT NULL | CHECK > 0 |
| `DistanciaKm` | DECIMAL(10,2) NULL | CHECK >= 0 |
| `FechaSalida` | DATE NOT NULL | |
| `FechaRetorno` | DATE NULL | |
| `Motivo` | NVARCHAR(300) NULL | |
| `Estado` | VARCHAR(20) NOT NULL | `planificado` / `en_curso` / `completado` |
| `PorcentajeAvance` | INT NOT NULL | 0–100 |

### `TrasladosColmena`

| Columna | Tipo | Notas |
|---|---|---|
| `TrasladoId` | INT NOT NULL | FK → Traslados.Id |
| `ColmenaId` | INT NOT NULL | FK → Colmenas.Id |
| PK compuesta | `(TrasladoId, ColmenaId)` | |

---

## BLOQUE 6 — Cosecha y Finanzas (2 tablas)

### `Cosechas`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `Fecha` | DATE NOT NULL | |
| `TipoMiel` | NVARCHAR(100) NOT NULL | |
| `AlzasCosechadas` | INT NOT NULL | CHECK >= 0 |
| `PesoBruto` | DECIMAL(10,2) NOT NULL | CHECK > 0 |
| `Merma` | DECIMAL(10,2) NOT NULL | CHECK >= 0 |
| `Humedad` | DECIMAL(5,2) NULL | 0–100 |
| `HMF` | DECIMAL(10,2) NULL | CHECK >= 0 |
| `Destino` | NVARCHAR(100) NULL | Default `Stock` |
| `Notas` | NVARCHAR(500) NULL | |
| `Vendida` | BIT NOT NULL | |
| `PrecioPorKg` | DECIMAL(10,2) NOT NULL | |
| CHECK | `PesoBruto >= Merma` | |
| *`PesoNeto`* | *calculado* | *PesoBruto − Merma, no persiste* |

### `RegistrosFinancieros`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `TipoMovimiento` | VARCHAR(20) NOT NULL | `ingreso` / `gasto` / `inversion` |
| `Categoria` | NVARCHAR(100) NOT NULL | |
| `Descripcion` | NVARCHAR(300) NOT NULL | |
| `Fecha` | DATE NOT NULL | |
| `Monto` | DECIMAL(12,2) NOT NULL | CHECK > 0 |
| `ApiarioId` | INT NULL | FK → Apiarios.Id |
| `CosechaId` | INT NULL | FK → Cosechas.Id |

---

## BLOQUE 7 — Inventario (2 tablas)

### `ItemsInventario`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Nombre` | NVARCHAR(100) NOT NULL | |
| `Unidad` | NVARCHAR(20) NOT NULL | Default `u` |
| `CantidadActual` | DECIMAL(10,2) NOT NULL | |
| `CantidadMinima` | DECIMAL(10,2) NOT NULL | |
| `CantidadMaxima` | DECIMAL(10,2) NOT NULL | |

### `MovimientosInventario`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `ItemId` | INT NOT NULL | FK → ItemsInventario.Id |
| `Tipo` | VARCHAR(10) NOT NULL | `Entrada` / `Salida` |
| `Cantidad` | DECIMAL(10,2) NOT NULL | CHECK > 0 |
| `Fecha` | DATE NOT NULL | |
| `Motivo` | NVARCHAR(200) NULL | |
| `UserId` | NVARCHAR(450) NULL | FK → AspNetUsers.Id |

---

## BLOQUE 8 — Tareas (1 tabla)

### `Tareas`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Nombre` | NVARCHAR(200) NOT NULL | |
| `Categoria` | NVARCHAR(50) NOT NULL | `Sanidad` / `Inspección` / `General` / etc. |
| `Prioridad` | VARCHAR(10) NOT NULL | `alta` / `media` / `baja` |
| `FechaVencimiento` | DATE NULL | |
| `Completada` | BIT NOT NULL | |
| `FechaCreacion` | DATETIME NOT NULL | Default `GETDATE()` |
| `AsignadoAId` | NVARCHAR(450) NULL | FK → AspNetUsers.Id |

---

## BLOQUE 9 — Alertas comunitarias (2 tablas)

### `AlertasComunitarias`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `Titulo` | NVARCHAR(150) NOT NULL | |
| `Descripcion` | NVARCHAR(1000) NOT NULL | |
| `TipoAmenaza` | NVARCHAR(50) NOT NULL | `sanitaria` / etc. |
| `Latitud` | DECIMAL(9,6) NOT NULL | |
| `Longitud` | DECIMAL(9,6) NOT NULL | |
| `RadioKm` | DECIMAL(6,2) NOT NULL | Default 10 |
| `Ubicacion` | NVARCHAR(200) NULL | |
| `Estado` | VARCHAR(20) NOT NULL | `activa` / `cerrada` |
| `FechaCreacion` | DATETIME NOT NULL | Default `GETDATE()` |
| `FechaResolucion` | DATETIME NULL | |
| `ReportadoPorId` | NVARCHAR(450) NULL | FK → AspNetUsers.Id |

### `NotificacionesAlerta`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `AlertaId` | INT NOT NULL | FK → AlertasComunitarias.Id |
| `ApiarioId` | INT NOT NULL | FK → Apiarios.Id |
| `DistanciaKm` | DECIMAL(6,2) NOT NULL | |
| `FechaEnvio` | DATETIME NOT NULL | Default `GETDATE()` |
| `Leida` | BIT NOT NULL | |

---

## BLOQUE 10 — Auditoría (2 tablas)

### `Auditorias`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `UserId` | NVARCHAR(450) NOT NULL | |
| `NombreUsuario` | NVARCHAR(200) NOT NULL | Denorm. intencional — preserva historial si se elimina el usuario |
| `Accion` | VARCHAR(20) NOT NULL | `CREATE` / `UPDATE` / `DELETE` |
| `Tabla` | NVARCHAR(50) NOT NULL | |
| `FechaHora` | DATETIME NOT NULL | Default `GETDATE()` |
| `Detalle` | NVARCHAR(MAX) NULL | |

### `HistorialesAcceso`

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | INT PK IDENTITY | |
| `UserId` | NVARCHAR(450) NOT NULL | |
| `NombreUsuario` | NVARCHAR(200) NOT NULL | Denorm. intencional — ídem Auditorias |
| `FechaHora` | DATETIME NOT NULL | Default `GETDATE()` |
| `Ip` | VARCHAR(45) NULL | |
| `Dispositivo` | NVARCHAR(300) NULL | |
| `Exitoso` | BIT NOT NULL | |

---

## Índices (13 en total)

| Índice | Tabla | Columnas | Origen |
|---|---|---|---|
| `idx_colmena_apiario` | Colmenas | ApiarioId | Adaptado de SQL Server original |
| `idx_inspeccion_apiario` | Inspecciones | ApiarioId | Adaptado de SQL Server original |
| `idx_eval_inspeccion` | EvaluacionesColmena | InspeccionId | Adaptado de SQL Server original |
| `idx_control_apiario` | ControlesSanitarios | ApiarioId, Fecha | Adaptado (era por colmena, ahora por apiario) |
| `idx_cosecha_fecha` | Cosechas | Fecha | Adaptado de SQL Server original |
| `idx_tarea_pendiente` | Tareas | Completada, FechaVencimiento | Adaptado de SQL Server original |
| `idx_visita_fecha` | Visitas | FechaPlanificada, Estado | Adaptado de SQL Server original |
| `idx_colmena_asignada` | Colmenas | AsignadoAId | Nuevo — panel de empleados |
| `idx_traslado_colmena` | TrasladosColmena | ColmenaId | Nuevo — colmenas en traslados |
| `idx_movimiento_item` | MovimientosInventario | ItemId, Fecha | Nuevo — historial de stock |
| `idx_auditoria_usuario` | Auditorias | UserId, FechaHora | Nuevo — log por usuario |
| `idx_alerta_estado` | AlertasComunitarias | Estado, FechaCreacion | Nuevo — alertas activas |
| `idx_inspeccion_visita` | Inspecciones | VisitaId | Nuevo — inspecciones por visita |

---

## Relaciones FK principales (24 en total)

| Tabla hija | Columna | Tabla padre |
|---|---|---|
| AspNetUsers | ApiarioAsignadoId | Apiarios |
| Apiarios | ResponsableId | AspNetUsers |
| Apiarios | PropietarioId | Propietarios |
| Colmenas | ApiarioId | Apiarios |
| Colmenas | AsignadoAId | AspNetUsers |
| Visitas | ApiarioId | Apiarios |
| Inspecciones | ApiarioId | Apiarios |
| Inspecciones | ColmenaId | Colmenas |
| Inspecciones | VisitaId | Visitas |
| EvaluacionesColmena | InspeccionId | Inspecciones |
| EvaluacionesColmena | ColmenaId | Colmenas |
| ControlesSanitarios | ApiarioId | Apiarios |
| Traslados | ApiarioOrigenId | Apiarios |
| Traslados | ApiarioDestinoId | Apiarios |
| TrasladosColmena | TrasladoId | Traslados |
| TrasladosColmena | ColmenaId | Colmenas |
| Cosechas | ApiarioId | Apiarios |
| RegistrosFinancieros | ApiarioId | Apiarios |
| RegistrosFinancieros | CosechaId | Cosechas |
| ItemsInventario | — | — |
| MovimientosInventario | ItemId | ItemsInventario |
| MovimientosInventario | UserId | AspNetUsers |
| Tareas | AsignadoAId | AspNetUsers |
| AlertasComunitarias | ReportadoPorId | AspNetUsers |
| NotificacionesAlerta | AlertaId | AlertasComunitarias |
| NotificacionesAlerta | ApiarioId | Apiarios |

---

## Origen de cada componente

| Componente | Tomado de |
|---|---|
| Tablas base (estructura general) | colmena.db |
| Estilo de normalización (FKs reales, sin denorm.) | SQL Server original |
| Gestión de usuarios / autenticación | colmena.db (ASP.NET Identity) |
| Auditoría y accesos | colmena.db |
| Inventario con historial de movimientos | SQL Server original |
| Control sanitario (nivel apiario) | colmena.db |
| Cosecha (con Vendida / PrecioPorKg) | colmena.db |
| EvaluacionesColmena (por inspección) | SQL Server original |
| TrasladosColmena (pivot detallado) | SQL Server original |
| Propietarios (dueños de predio) | SQL Server original |
| Apicultor → ResponsableId en Apiarios | Solución híbrida (ver nota) |
| Alertas comunitarias | colmena.db (exclusivo) |

> **Nota Apicultor:** La tabla `Apicultor` del SQL Server original fue reemplazada por
> `Apiarios.ResponsableId FK → AspNetUsers.Id`. El admin del sistema cumple el rol de
> apicultor responsable, evitando duplicar entidades para la misma persona.
