# Diagnóstico ColmenaEmpresa — ZanganosSA
**Fecha:** 2026-06-03  
**Proyecto:** ColmenaEmpresa MVC (ASP.NET Core / .NET 10)  
**Estado:** Pre-pull — análisis del código en rama local

---

## FUNCIONES ROTAS (bugs reales)

### 1. Mapeo ApiarioId → ApiarioNombre hardcodeado e incompleto
**Severidad:** Critico  
**Archivos afectados:**
- `Controllers/InspeccionesController.cs` lineas 32–38
- `Controllers/SanidadController.cs` lineas 33–37
- `Controllers/ColmenasController.cs` lineas 44–51

Los tres controllers tienen un `switch` con IDs fijos que **no usa la lista real de apiarios**:
- `InspeccionesController` — solo mapea IDs 1–3. "Paso Carrasco" (ID 4) siempre produce **"Desconocido"**
- `SanidadController` — solo mapea IDs 1–2. IDs 3 y 4 producen **"Desconocido"**
- `ColmenasController` — mapea 1–4, pero si se agrega un nuevo apiario falla igual

**Corrección esperada:** reemplazar el switch por `ApiariosController.Apiarios.FirstOrDefault(a => a.Id == id)?.Nombre`.

---

### 2. InspeccionesController sobreescribe los datos que el usuario ingresa
**Severidad:** Alto  
**Archivo:** `Controllers/InspeccionesController.cs` lineas 41–43

```csharp
inspeccion.TotalColmenas = 20;            // ignora lo real
inspeccion.ColmenasInspeccionadas = 18;   // ignora lo real
inspeccion.Estado = "completa";           // siempre completa
```

El checklist de colmenas en `Views/Inspecciones/Crear.cshtml` es **decorativo** (no envía datos al servidor, el toggle es solo visual con JS inline).

---

### 3. ProduccionController — ApiarioNombre no se asigna correctamente en el POST
**Severidad:** Medio  
**Archivo:** `Views/Produccion/Crear.cshtml` linea 15

La vista usa `asp-for="ApiarioNombre"` con texto directo en lugar de `ApiarioId`. La vista solo lista **2 de los 4 apiarios** (Monte Olivo y Paso Carrasco). El Eucaliptal y La Rinconada no aparecen como opciones.

---

### 4. TranshumanciaController sobreescribe el nombre y usa distancia aleatoria
**Severidad:** Medio  
**Archivo:** `Controllers/TranshumanciaController.cs` lineas 26–27

```csharp
// Sobreescribe el nombre que el apicultor pudo haber ingresado
traslado.Nombre = $"Traslado {traslado.ApiarioOrigen} a {traslado.ApiarioDestino}";

// Distancia aleatoria — dato inventado, no confiable
traslado.DistanciaKm = traslado.DistanciaKm > 0 ? traslado.DistanciaKm : new Random().Next(80, 200);
```

---

### 5. Dashboard completamente desconectado de los datos reales
**Severidad:** Critico  
**Archivo:** `Controllers/HomeController.cs` lineas 15–38

| KPI mostrado en pantalla | Valor hardcodeado | Valor real en memoria |
|---|---|---|
| TotalColmenas | 148 | 4 (lista estatica) |
| TotalApiarios | 12 | 4 (lista estatica) |
| ColmenasVerde | 88 | calculable desde colmenas |
| ColmenasAmarillo | 43 | calculable desde colmenas |
| ColmenasRojo | 17 | calculable desde colmenas |
| Alertas | texto fijo | no se generan dinamicamente |
| ProduccionMensual | datos fijos | no refleja cosechas reales |

---

### 6. Apiario/Detalle — comparacion de semaforo en idioma incorrecto
**Severidad:** Alto  
**Archivo:** `Views/Apiarios/Detalle.cshtml` linea 22

```csharp
// La vista compara en ingles
Model.EstadoSemaforo == "red"
Model.EstadoSemaforo == "amber"

// Pero el modelo guarda en espanol
// "rojo" | "amarillo" | "verde"
```

La etiqueta de estado nunca muestra el texto correcto ("Inspeccion Vencida" / "Visitar Pronto" / "Al Dia") para ningun apiario.

---

### 7. Cosecha — CrearRegistroIngreso no hace nada
**Severidad:** Medio  
**Archivo:** `Controllers/ProduccionController.cs` lineas 31–37

El checkbox "Crear registro de ingreso automaticamente" existe en la vista (`Views/Produccion/Crear.cshtml`) y en el modelo (`Models/Cosecha.cs`), pero el POST ignora completamente el valor. Nunca crea el `RegistroFinanciero` correspondiente en `FinanzasController._registros`.

---

### 8. Inventario sin ninguna accion de escritura
**Severidad:** Alto  
**Archivo:** `Controllers/InventarioController.cs`

Solo tiene `Index`. El boton **"+ Movimiento"** en `Views/Inventario/Index.cshtml` no esta vinculado a ninguna ruta. No existe accion `Crear`, `Editar` ni `AjustarStock`.

---

### 9. Generacion de IDs puede duplicar registros
**Severidad:** Bajo (riesgo futuro)  
**Afecta:** todos los controllers

```csharp
item.Id = List.Count + 1;  // patron en todos los controllers
```

Si se implementa eliminacion de registros, el siguiente ID chocara con uno existente (p.ej. lista con 3 items → se elimina uno → Count=2 → nuevo Id=3 ya existe).

---

## MEJORAS OPTIMAS PARA EL APICULTOR

### A. Semaforo de colmena calculado automaticamente
**Impacto:** Alto — evita errores de carga y mantiene el estado siempre actualizado

Reglas sugeridas para calcular `EstadoSemaforo` en `Colmena`:

| Condicion | Semaforo |
|---|---|
| Reina ausente | Rojo |
| Sin visita hace +30 dias | Rojo |
| Sin visita hace +14 dias | Amarillo |
| En transhumancia activa | Viaje |
| Resto | Verde |

---

### B. Inspeccion actualiza UltimaVisita de las colmenas del apiario
**Impacto:** Alto — la vista de semaforo y dias-sin-visita depende de este dato

Al guardar una inspeccion, el controller deberia actualizar `UltimaVisita = DateTime.Today` en todas las colmenas cuyo `ApiarioId` coincida con el apiario inspeccionado.

---

### C. Selects de apiarios generados desde datos reales
**Impacto:** Alto — los nuevos apiarios que se creen nunca aparecen en los formularios

Las vistas de Colmenas, Inspecciones y Sanidad tienen las opciones de apiario escritas a mano. Deben generarse desde `ApiariosController.Apiarios` pasado via `ViewBag` desde cada controller.

---

### D. Transhumancia activa cambia el estado de colmenas a "viaje"
**Impacto:** Medio — coherencia visual entre modulos

Al crear una transhumancia, las colmenas del apiario origen deberian pasar a `EstadoSemaforo = "viaje"` automaticamente. Al completar la transhumancia, volver al estado correcto calculado.

---

### E. Calendario y Planificacion — actualmente vacios
**Impacto:** Medio — modulos visibles en el sidebar que no hacen nada

`CalendarioController` y `PlanificacionController` retornan `View()` sin ningun dato.  
El calendario podria cruzar y mostrar:
- Fechas de inspecciones registradas
- Vencimientos de tratamientos sanitarios activos
- Fechas de salida/retorno de transhumancias

---

### F. Editar y Eliminar en todos los modulos
**Impacto:** Alto — sin edicion, cualquier error de carga es permanente

Ningun controller tiene acciones `Editar(int id)` ni `Eliminar(int id)`. Esto bloquea la correccion de errores y la actualizacion de estados (p.ej. marcar una transhumancia como completada).

---

### G. Perfil del apicultor
**Impacto:** Bajo — mejora de experiencia

`PerfilController` retorna `View()` sin ningun dato. El nombre "Carlos Bentancur" y el rol "Apicultor principal" estan hardcodeados en `Views/Shared/_Layout.cshtml`.

---

### H. Finanzas — separar inversiones de gastos operativos
**Impacto:** Bajo — precision contable

El balance calcula: `gastos = registros donde TipoMovimiento != "ingreso"`, agrupando inversiones y gastos. Seria mas util mostrar tres lineas: Ingresos / Gastos operativos / Inversiones.

---

## Resumen de prioridades

| Prioridad | Tipo | Descripcion |
|---|---|---|
| Critico | Bug | Dashboard desconectado de datos reales |
| Critico | Bug | Mapeo ApiarioId roto en Inspecciones y Sanidad |
| Alto | Bug | Semaforo en Detalle de Apiario nunca muestra texto correcto |
| Alto | Bug | Inspeccion sobreescribe datos del usuario |
| Alto | Bug | Inventario sin CRUD — boton "+ Movimiento" no funciona |
| Medio | Bug | CrearRegistroIngreso en cosechas no implementado |
| Medio | Bug | Produccion — faltan apiarios en el formulario |
| Medio | Bug | Transhumancia usa distancia aleatoria y sobreescribe nombre |
| Alto | Mejora | Semaforo de colmena calculado automaticamente |
| Alto | Mejora | Inspecciones actualizan UltimaVisita de colmenas |
| Alto | Mejora | Selects de apiarios generados dinamicamente |
| Alto | Mejora | Editar y Eliminar en todos los modulos |
| Medio | Mejora | Transhumancia actualiza estado de colmenas |
| Medio | Mejora | Calendario y Planificacion operativos |
| Bajo | Mejora | Perfil del apicultor con datos reales |
| Bajo | Mejora | Separar inversiones de gastos en Finanzas |

---

*Generado por Claude Code — 2026-06-03*
