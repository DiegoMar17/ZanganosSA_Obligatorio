# Corrección — Dashboard

**Proyecto:** ZanganosSA — Sistema de gestión apícola
**Stack:** ASP.NET MVC · C# · .NET 9 · SQLite · Entity Framework Core
**Fecha:** 2026-06-03
**Participantes:** ericVignolo + Claude (Opus 4.8)

---

## Objetivo

Revisión página por página corrigiendo errores y haciendo que todas las pantallas se complementen entre sí. Esta página: **Dashboard** (`Home/Index`).

---

## Errores encontrados y corregidos

| # | Error | Solución |
|---|-------|----------|
| 1 | Las "Alertas activas" siempre estaban vacías — el controller enviaba `Alertas = new List<AlertaDashboard>()` fija | Se generan alertas reales desde la BD: colmenas en estado rojo, colmenas sin reina e inspecciones pendientes. Si no hay nada, muestra "Todo en orden — sin alertas activas" |
| 2 | Datos hardcodeados que nunca cambiaban: "↑ 4 este mes" y "2 en transhumancia" | "↑ N este mes" ahora cuenta colmenas instaladas en el mes actual; "N en transhumancia" cuenta los traslados con estado `en_curso` reales |
| 3 | "Cosecha estimada" engañaba — el valor era la suma de cosechas ya registradas | Renombrado a "Cosecha total" |
| 4 | El gráfico de producción mensual quedaba vacío sin mensaje si no había cosechas | Estado vacío: "Sin cosechas registradas todavía" |
| 5 | Botón "+ Nuevo" usaba `onclick="location.href=..."` | Cambiado a un `<a>` con `asp-action`, navegación correcta |

---

## Archivos modificados

| Archivo | Acción |
|---|---|
| `Models/DashboardViewModel.cs` | Agregadas propiedades `ColmenasNuevasMes` y `EnTranshumancia`; renombrado `CosechaEstimada` → `CosechaTotal` |
| `Controllers/HomeController.cs` | `Index` genera alertas reales desde colmenas/inspecciones, calcula colmenas nuevas del mes y traslados en curso |
| `Views/Home/Index.cshtml` | Stat-cards con datos dinámicos, gráfico con estado vacío, botón como link |

---

## Cómo se complementa con otras páginas

- Las **alertas** se alimentan de Colmenas (estado rojo, reina ausente) e Inspecciones (pendientes)
- El contador **"en transhumancia"** refleja los traslados reales del módulo Transhumancia
- La **cosecha total** suma lo registrado en Producción
