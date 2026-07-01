# ZanganosSA — Funciones, Procedimientos y Triggers

Base de datos: `Colmena_Empresa_ZanganosDB`  
Motor: SQL Server Express

---

## Funciones

### `fn_PesoNeto`

**Tipo:** Función escalar  
**Propósito:** Calcula el peso neto de una cosecha descontando la merma del peso bruto.

**Parámetros:**

| Parámetro | Tipo | Descripción |
|---|---|---|
| `@peso_bruto` | DECIMAL(10,2) | Peso total extraído en kg |
| `@merma` | DECIMAL(10,2) | Pérdida por evaporación, filtrado u otros |

**Retorna:** `DECIMAL(10,2)` — peso neto en kg.

**Funcionamiento:** Resta `@merma` de `@peso_bruto` manejando nulos con `ISNULL`. Se utiliza en consultas de ranking de cosechas y en el cálculo de montos de venta.

**Uso:**
```sql
SELECT dbo.fn_PesoNeto(210.00, 4.50) AS Kg_Netos;
-- Resultado: 205.50
```

---

### `fn_BalanceApiario`

**Tipo:** Función escalar  
**Propósito:** Calcula el balance financiero neto de un apiario en un rango de fechas. Suma los ingresos y resta los gastos e inversiones.

**Parámetros:**

| Parámetro | Tipo | Descripción |
|---|---|---|
| `@id_apiario` | INT | ID del apiario a consultar |
| `@fecha_desde` | DATE | Inicio del período |
| `@fecha_hasta` | DATE | Fin del período |

**Retorna:** `DECIMAL(12,2)` — balance neto. Positivo indica ganancia, negativo indica déficit.

**Funcionamiento:** Consulta `RegistrosFinancieros` filtrando por apiario y rango de fechas. Aplica `CASE` para sumar ingresos y restar gastos e inversiones. Si no hay registros devuelve 0 (via `ISNULL`).

**Uso:**
```sql
SELECT dbo.fn_BalanceApiario(1, '2025-01-01', '2025-12-31') AS Balance;
-- Resultado: 178500.00
```

---

### `fn_ColmenasApiario`

**Tipo:** Función de tabla en línea (inline TVF)  
**Propósito:** Retorna el detalle de todas las colmenas de un apiario, incluyendo los días transcurridos desde la última visita. Permite identificar colmenas que requieren atención.

**Parámetros:**

| Parámetro | Tipo | Descripción |
|---|---|---|
| `@id_apiario` | INT | ID del apiario a consultar |

**Retorna:** Tabla con columnas `ID_Colmena`, `Cod_Colmena`, `Tipo_Colmena`, `EstReina_Colmena`, `CantAlzas_Colmena`, `EstSem_Colmena`, `UltVis_Colmena`, `DiasDesdeVisita`.

**Funcionamiento:** Filtra `Colmenas` por `ID_Apiario_Col` y calcula `DiasDesdeVisita` con `DATEDIFF(DAY, UltVis_Colmena, GETDATE())`. Al ser TVF en línea se puede usar en JOIN y aplicar filtros adicionales.

**Uso:**
```sql
SELECT * FROM dbo.fn_ColmenasApiario(1)
WHERE DiasDesdeVisita > 21;
```

---

## Procedimientos Almacenados

### `sp_RegistrarInspeccion`

**Propósito:** Centraliza el registro de una inspección. Si es de tipo colmena, crea automáticamente la evaluación correspondiente en `EvaluacionesColmena` y actualiza el estado de la reina en `Colmenas` si se informa.

**Parámetros:**

| Parámetro | Tipo | Default | Descripción |
|---|---|---|---|
| `@id_apiario` | INT | — | Apiario inspeccionado |
| `@id_colmena` | INT | NULL | Colmena específica (si aplica) |
| `@id_visita` | INT | NULL | Visita planificada asociada |
| `@fecha` | DATE | — | Fecha de la inspección |
| `@clima` | NVARCHAR(50) | NULL | Condición climática |
| `@temp` | DECIMAL(4,1) | NULL | Temperatura en °C |
| `@notas` | NVARCHAR(1000) | NULL | Observaciones de campo |
| `@tipo` | VARCHAR(20) | 'apiario' | `'apiario'` o `'colmena'` |
| `@estado_reina` | NVARCHAR(50) | NULL | Estado de la reina observado |
| `@obs_colmena` | NVARCHAR(500) | NULL | Observaciones de la colmena |
| `@id_inspeccion` | INT OUTPUT | — | ID generado de la inspección |

**Funcionamiento:**
1. Calcula `TotCol_Insp`: 1 si es inspección de colmena, o el conteo total del apiario.
2. Inserta en `Inspecciones` con estado `'completa'`.
3. Si `@tipo = 'colmena'`: inserta en `EvaluacionesColmena` y actualiza `EstReina_Colmena` en `Colmenas` si se proporcionó `@estado_reina`.
4. El trigger `trg_ActualizarUltVisita` se activa automáticamente para actualizar `UltVis_Colmena`.

**Uso:**
```sql
DECLARE @id INT;
EXEC sp_RegistrarInspeccion
    @id_apiario   = 1,
    @id_colmena   = 1,
    @fecha        = '2025-08-01',
    @clima        = 'Despejado',
    @temp         = 21.0,
    @tipo         = 'colmena',
    @estado_reina = 'vista',
    @id_inspeccion = @id OUTPUT;
```

---

### `sp_RegistrarCosecha`

**Propósito:** Registra una cosecha de miel. Si se indica un precio por kg, genera automáticamente un registro de ingreso en `RegistrosFinancieros` y marca la cosecha como vendida.

**Parámetros:**

| Parámetro | Tipo | Default | Descripción |
|---|---|---|---|
| `@id_apiario` | INT | — | Apiario de origen |
| `@fecha` | DATE | — | Fecha de cosecha |
| `@tipo_miel` | NVARCHAR(100) | — | Ej: Multifloral, Eucalipto |
| `@alzas` | INT | — | Cantidad de alzas cosechadas |
| `@peso_bruto` | DECIMAL(10,2) | — | Peso total en kg |
| `@merma` | DECIMAL(10,2) | 0 | Pérdida en kg |
| `@humedad` | DECIMAL(5,2) | NULL | Porcentaje de humedad |
| `@hmf` | DECIMAL(10,2) | NULL | Índice HMF |
| `@color_pfund` | NVARCHAR(50) | NULL | Color en escala Pfund |
| `@destino` | NVARCHAR(100) | 'Stock' | Destino de la miel |
| `@notas` | NVARCHAR(500) | NULL | Observaciones |
| `@precio_kg` | DECIMAL(10,4) | 0 | Precio de venta por kg |
| `@id_cosecha` | INT OUTPUT | — | ID generado |

**Funcionamiento:**
1. Valida que `@peso_bruto > @merma` (si no, lanza error con `RAISERROR`).
2. Inserta en `Cosechas`.
3. Si `@precio_kg > 0`: calcula `Kg_Netos * @precio_kg`, inserta en `RegistrosFinancieros` como ingreso y actualiza `Vendida_Cosecha = 1`.

**Uso:**
```sql
DECLARE @id INT;
EXEC sp_RegistrarCosecha
    @id_apiario  = 1,
    @fecha       = '2025-09-10',
    @tipo_miel   = 'Eucalipto',
    @alzas       = 6,
    @peso_bruto  = 180.00,
    @merma       = 3.50,
    @precio_kg   = 900.00,
    @id_cosecha  = @id OUTPUT;
```

---

### `sp_GestionarTraslado`

**Propósito:** Controla el ciclo de vida de un traslado. Permite iniciarlo (cambia estado a `en_curso` y marca colmenas como `viaje`) o completarlo (cambia estado a `completado`, registra fecha de retorno y devuelve semáforo a `verde`).

**Parámetros:**

| Parámetro | Tipo | Default | Descripción |
|---|---|---|---|
| `@id_traslado` | INT | — | ID del traslado a gestionar |
| `@accion` | VARCHAR(20) | — | `'iniciar'` o `'completar'` |
| `@fecha_retorno` | DATE | NULL | Fecha real de retorno (para completar) |
| `@porcentaje` | INT | NULL | Porcentaje de avance inicial (para iniciar) |

**Funcionamiento:**
- **`iniciar`:** Actualiza `Estado_Traslado = 'en_curso'` y `PorcAv_Traslado`. Cambia `EstSem_Colmena = 'viaje'` en todas las colmenas del traslado (vía `TrasladosColmena`).
- **`completar`:** Actualiza `Estado_Traslado = 'completado'`, registra `Fecha_Retorno` y `PorcAv_Traslado = 100`. Restaura `EstSem_Colmena = 'verde'` en las colmenas involucradas.
- Si el traslado no existe, lanza `RAISERROR`.

**Uso:**
```sql
EXEC sp_GestionarTraslado @id_traslado = 1, @accion = 'iniciar';
EXEC sp_GestionarTraslado @id_traslado = 1, @accion = 'completar',
                          @fecha_retorno = '2026-01-15';
```

---

### `sp_MovimientoInventario`

**Propósito:** Registra entradas o salidas de ítems del inventario con validación de stock disponible. El trigger `trg_AjustarStock` actualiza automáticamente `Stock_Act_Item` tras la inserción.

**Parámetros:**

| Parámetro | Tipo | Default | Descripción |
|---|---|---|---|
| `@id_item` | INT | — | ID del ítem de inventario |
| `@tipo` | VARCHAR(10) | — | `'Entrada'` o `'Salida'` |
| `@cantidad` | DECIMAL(10,2) | — | Cantidad a mover |
| `@motivo` | NVARCHAR(200) | NULL | Descripción del movimiento |
| `@id_apicultor` | NVARCHAR(450) | NULL | ID del usuario que ejecuta |

**Funcionamiento:**
1. Consulta stock actual. Si el ítem no existe, lanza error.
2. Si es `'Salida'` y `Stock_Act_Item < @cantidad`, lanza error con el stock disponible.
3. Inserta en `MovimientosInventario`. El trigger actualiza el stock.
4. Retorna el estado actual del ítem con alerta si el stock cayó por debajo del mínimo.

**Uso:**
```sql
EXEC sp_MovimientoInventario
    @id_item  = 1,
    @tipo     = 'Salida',
    @cantidad = 50.00,
    @motivo   = N'Uso en tratamiento colmena C-001';
```

---

### `sp_ResumenApiario`

**Propósito:** Panel de información completo de un apiario. Retorna cuatro conjuntos de resultados: datos generales con conteo de colmenas por semáforo, última inspección, balance financiero del año en curso y colmenas en riesgo.

**Parámetros:**

| Parámetro | Tipo | Descripción |
|---|---|---|
| `@id_apiario` | INT | ID del apiario a resumir |

**Retorna (4 result sets):**

1. **Info general:** nombre, departamento, capacidad, estado semáforo, conteo de colmenas por color (verde/amarillo/rojo) y sin reina.
2. **Última inspección:** fecha, clima, temperatura, estado y notas.
3. **Balance del año:** total ingresos, gastos, inversiones y balance neto del año en curso.
4. **Colmenas en riesgo:** colmenas con semáforo rojo, sin reina o con más de 21 días sin visita.

**Uso:**
```sql
EXEC sp_ResumenApiario @id_apiario = 1;
```

---

## Triggers

### `trg_ActualizarUltVisita`

**Tabla:** `Inspecciones`  
**Evento:** `AFTER INSERT`  
**Propósito:** Mantiene sincronizado el campo `UltVis_Colmena` en `Colmenas` cada vez que se registra una inspección, evitando actualización manual.

**Funcionamiento:**
- Si la inspección es de tipo `'colmena'` (`ID_Colmena_Insp IS NOT NULL`): actualiza `UltVis_Colmena` únicamente en esa colmena.
- Si es de tipo `'apiario'`: actualiza `UltVis_Colmena` en **todas** las colmenas del apiario inspeccionado.

Usa la tabla virtual `inserted` para acceder a los datos recién insertados.

---

### `trg_AjustarStock`

**Tabla:** `MovimientosInventario`  
**Evento:** `AFTER INSERT`  
**Propósito:** Actualiza automáticamente `Stock_Act_Item` en `ItemsInventario` tras cada movimiento, garantizando que el stock refleje la realidad sin necesidad de actualización manual.

**Funcionamiento:**  
Hace JOIN entre `inserted` y `ItemsInventario` por `ID_Item_Mov`. Aplica:
- `+Mov_Cant` si `Mov_Tipo = 'Entrada'`
- `-Mov_Cant` si `Mov_Tipo = 'Salida'`

Opera sobre todos los registros del batch en una sola instrucción `UPDATE`, soportando inserciones múltiples.

---

### `trg_AuditoriaColmena`

**Tabla:** `Colmenas`  
**Evento:** `AFTER UPDATE`  
**Propósito:** Registra automáticamente en `Auditorias` cualquier cambio en el estado de la reina (`EstReina_Colmena`) o el semáforo (`EstSem_Colmena`) de una colmena, creando trazabilidad completa del estado sanitario.

**Funcionamiento:**  
Compara los valores de `inserted` (nuevo) con `deleted` (anterior). Si alguno de los dos campos cambió, inserta una fila en `Auditorias` con el detalle del cambio en formato:

```
Colmena C-001 | EstReina: vista -> ausente | EstSem: verde -> rojo
```

Solo registra cuando hay cambio real; si los valores son idénticos no genera auditoría.

---

### `trg_ValidarOrigenDestino`

**Tabla:** `Traslados`  
**Evento:** `INSTEAD OF INSERT`  
**Propósito:** Impide registrar traslados donde el apiario de origen y destino sean el mismo, ya que no tiene sentido operativo mover colmenas al mismo lugar.

**Funcionamiento:**  
Al ser `INSTEAD OF INSERT`, intercepta el `INSERT` **antes** de que se ejecute. Si detecta que `ID_Apiario_Orig = ID_Apiario_Dest` en alguna fila de `inserted`, lanza `RAISERROR` y no inserta nada. Si la validación pasa, ejecuta el `INSERT` real en `Traslados` con los datos de `inserted`.

Esta validación complementa la restricción de aplicación (`TranshumanciaController`) y garantiza integridad a nivel de base de datos, independientemente del origen del INSERT.
