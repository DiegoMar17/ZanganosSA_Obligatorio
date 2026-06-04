# Corrección — Transhumancia

**Proyecto:** ZanganosSA — Sistema de gestión apícola
**Stack:** ASP.NET MVC · C# · .NET 9 · SQLite · Entity Framework Core
**Fecha:** 2026-06-03
**Participantes:** ericVignolo + Claude (Opus 4.8)

---

## Objetivo

Revisión página por página corrigiendo errores y haciendo que todas las pantallas se complementen entre sí. Esta página: **Transhumancia**.

---

## Errores encontrados y corregidos

| # | Error | Solución |
|---|-------|----------|
| 1 | Los traslados se creaban con estado `planificado` pero el Index solo mostraba `en_curso` y `completado` — un traslado recién creado desaparecía | El Index muestra planificados y en curso juntos como "eventos activos" |
| 2 | El Crear reventaba si fallaba la validación — no recargaba `ViewBag.NombresApiarios` usado en el `foreach` | Se agregó `CargarApiarios()` antes de devolver la vista |
| 3 | Los dropdowns Origen/Destino en Editar tenían opciones hardcodeadas | Ahora usan `ViewBag.NombresApiarios` reales + `CargarApiarios()` en GET y POST |
| 4 | Subtítulo "1 evento activo" estaba fijo | Cuenta los eventos activos reales (singular/plural) |
| 5 | El estado mostraba texto crudo (`en_curso`) | Mapeado a "En curso" (azul) / "Planificado" (amber) |
| 6 | La card Historial quedaba vacía sin mensaje si no había completados | Estado vacío: "Sin traslados completados todavía" |

Bonus: se agregaron botones Editar y Eliminar en las tarjetas activas (antes solo por URL).

---

## Archivos modificados

| Archivo | Acción |
|---|---|
| `Controllers/TranshumanciaController.cs` | `CargarApiarios()` en Crear/Editar al fallar validación y en el GET de Editar |
| `Views/Transhumancia/Index.cshtml` | Muestra planificados + en curso; subtítulo real; estados legibles; estado vacío; botones Editar/Eliminar |
| `Views/Transhumancia/Editar.cshtml` | Dropdowns Origen/Destino con apiarios reales de la BD |

---

## Cómo se complementa con otras páginas

- Los traslados `en_curso` alimentan el contador "en transhumancia" del **Dashboard**
- Los dropdowns Origen/Destino se sincronizan con el módulo **Apiarios**
- Los traslados ya no se pierden al crearse (estado planificado visible)
