# Corrección — Calendario

**Proyecto:** ZanganosSA — Sistema de gestión apícola
**Stack:** ASP.NET MVC · C# · .NET 9 · SQLite · Entity Framework Core
**Fecha:** 2026-06-03
**Participantes:** ericVignolo + Claude (Opus 4.8)

---

## Objetivo

Revisión página por página corrigiendo errores y haciendo que todas las pantallas se complementen entre sí. Esta página: **Calendario**.

---

## Estado previo

El calendario ya estaba bien armado: usa datos reales de la BD, navegación entre meses funcional, eventos por tipo con colores, estado vacío. Faltaba integración con Planificación.

---

## Errores encontrados y corregidos

| # | Error | Solución |
|---|-------|----------|
| 1 | El calendario ignoraba el módulo **Planificación** — no mostraba Tareas (con fecha límite) ni Visitas planificadas | El controller ahora agrega eventos de `Tareas` (por `FechaVencimiento`) y `Visitas` (por `FechaPlanificada`) |
| 2 | Nombres de tipo internos confusos (el traslado usaba `Tipo = "tarea"`) | Tipos renombrados a nombres claros: `inspeccion`, `cosecha`, `sanidad`, `traslado`, `tarea`, `visita`. Cada uno con su clase CSS y color propios |
| 3 | El día de hoy no se auto-seleccionaba — el panel lateral arrancaba en blanco | Si el mes mostrado es el actual, se cargan automáticamente los eventos del día de hoy al abrir |

---

## Archivos modificados

| Archivo | Acción |
|---|---|
| `Controllers/CalendarioController.cs` | Agregados eventos de Tareas y Visitas; tipos renombrados |
| `Views/Calendario/Index.cshtml` | Mapeo de clases por tipo, leyenda y header con Tarea/Visita, colores JS, auto-selección del día de hoy |
| `wwwroot/css/site.css` | Clases nuevas `.ce-inspeccion`, `.ce-sanidad`, `.ce-traslado`, `.ce-visita` (purple), `.ce-tareap` (green) |

---

## Colores por tipo

| Tipo | Color | Origen |
|---|---|---|
| Inspección | Azul | módulo Inspecciones |
| Cosecha | Dorado | módulo Producción |
| Sanidad | Rojo | módulo Sanidad |
| Traslado | Ámbar | módulo Transhumancia |
| Tarea | Verde | módulo Planificación |
| Visita | Violeta | módulo Planificación |

---

## Cómo se complementa con otras páginas

- El calendario es el **punto de unión** de toda la app: muestra en un solo lugar las inspecciones, cosechas, controles sanitarios, traslados, tareas y visitas
- Cualquier registro que se cree en esos módulos con una fecha aparece automáticamente en el día correspondiente
