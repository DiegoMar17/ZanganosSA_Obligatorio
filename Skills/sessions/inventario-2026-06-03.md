# Corrección — Inventario

**Proyecto:** ZanganosSA — Sistema de gestión apícola
**Stack:** ASP.NET MVC · C# · .NET 9 · SQLite · Entity Framework Core
**Fecha:** 2026-06-03
**Participantes:** ericVignolo + Claude (Opus 4.8)

---

## Objetivo

Revisión página por página corrigiendo errores y haciendo que todas las pantallas se complementen entre sí. Esta página: **Inventario y Stock**.

---

## Errores encontrados y corregidos

| # | Error | Solución |
|---|-------|----------|
| 1 | El badge de estado siempre mostraba "OK" — `EstadoStock` devuelve "verde/amarillo/rojo" (español) pero el texto comparaba con "red"/"amber" (inglés). Además `p-@EstadoStock` generaba clases inexistentes | Mapeo correcto: rojo=Crítico, amarillo=Bajo, verde=OK, con clases `p-red/p-amber/p-green` |
| 2 | La barra de stock no tenía color — `var(--@EstadoStock)` generaba `var(--verde)`, inexistente | Mapeo a `var(--red/amber/green)` |
| 3 | "Movimientos hoy: 7" hardcodeado | Reemplazado por stat-cards reales: "Bajo stock" (amarillos) y "Agotados / críticos" (rojos) |
| 4 | Sin estado vacío — la tabla quedaba con solo el header si no había ítems | Mensaje "Sin ítems en el inventario" |

---

## Archivos modificados

| Archivo | Acción |
|---|---|
| `Controllers/InventarioController.cs` | `Index` calcula bajos (amarillo) y agotados (rojo) desde `EstadoStock` |
| `Views/Inventario/Index.cshtml` | Badge y barra con colores correctos, stat-cards reales, estado vacío, subtítulo coherente |

---

## Estado previo ya correcto

- CRUD completo de ítems
- Cálculo de `PorcentajeStock` y `EstadoStock` en el modelo
- Exportar (vista PDF premium) operativo

---

## Cómo se complementa con otras páginas

- Los estados de stock (crítico / bajo / ok) son coherentes con la lógica de semáforos usada en el resto de la app (Apiarios, Colmenas)
- El conteo de ítems agotados sirve como alerta operativa
