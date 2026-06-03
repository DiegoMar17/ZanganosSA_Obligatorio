# Corrección — Producción

**Proyecto:** ZanganosSA — Sistema de gestión apícola
**Stack:** ASP.NET MVC · C# · .NET 9 · SQLite · Entity Framework Core
**Fecha:** 2026-06-03
**Participantes:** ericVignolo + Claude (Opus 4.8)

---

## Objetivo

Revisión página por página corrigiendo errores y haciendo que todas las pantallas se complementen entre sí. Esta página: **Producción** (cosechas).

---

## Errores encontrados y corregidos

| # | Error | Solución |
|---|-------|----------|
| 1 | **No se podía registrar ninguna cosecha** — el modelo exige `ApiarioId` (`[Range(1,...)]`) pero el form solo cargaba `ApiarioNombre`. `ApiarioId` quedaba en 0 y la validación fallaba siempre | El form ahora usa `ApiarioId` con dropdown real (`asp-items`); el POST deriva el `ApiarioNombre` desde el Id |
| 2 | Apiarios hardcodeados en Crear (2 fijos) y Editar (4 fijos) | Dropdown con apiarios reales de la BD vía `CargarApiarios()` |
| 3 | Stat-cards sin dato: "Prom. colmena" (`ViewBag.PromColmena` nunca seteado) y "vs temporada ant." (`"—"` fijo) | Reemplazados por "Promedio por cosecha" (kg) y "N° de cosechas", ambos calculados |
| 4 | Año hardcodeado inconsistente — stat-meta decía "Temporada 24–25" y el subtítulo "2025–26" | Unificado a "Temporada 2025–26" |
| 5 | Checkbox "Crear registro de ingreso automáticamente" (`CrearRegistroIngreso`) no hacía nada | Removido del formulario — no se puede generar un ingreso válido sin un precio/monto, y agregar ese campo excede el alcance de corrección. La integración Producción→Finanzas queda como mejora futura (requiere campo precio) |

---

## Archivos modificados

| Archivo | Acción |
|---|---|
| `Controllers/ProduccionController.cs` | `CargarApiarios()`; Crear/Editar derivan `ApiarioNombre` desde `ApiarioId`; Index calcula promedio y cantidad |
| `Views/Produccion/Crear.cshtml` | Dropdown de apiario por Id; checkbox de ingreso removido |
| `Views/Produccion/Editar.cshtml` | Dropdown de apiario por Id |
| `Views/Produccion/Index.cshtml` | Stat-cards reales, año coherente |

---

## Cómo se complementa con otras páginas

- Las cosechas alimentan la "Cosecha total" del **Dashboard** y aparecen en el **Calendario**
- El dropdown de apiarios se sincroniza con **Apiarios**
- `PesoNeto` (bruto − merma) se usa de forma consistente en los totales

## Pendiente / observación

- Integración **Producción → Finanzas**: para generar un ingreso automático al vender una cosecha se necesita un campo de precio/monto en el modelo `Cosecha`. Es una mejora futura, no una corrección.
