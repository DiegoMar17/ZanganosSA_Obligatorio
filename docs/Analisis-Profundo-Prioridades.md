# Analisis Profundo + Prioridades

**Proyecto:** ZanganosSA — ColmenaEmpresa MVC
**Tipo:** Auditoría técnica (QA Senior) — funcional, accesibilidad y seguridad
**Metodología:** lectura de los 17 controllers + modelos + vistas, contrastada contra ejecución real de la app (build, `dotnet run`, sesiones autenticadas con cookies para admin y empleado de prueba, incluyendo intentos de escritura reales). Todo lo marcado como hallazgo fue reproducido, no es especulación.

---

## 1. Evaluación funcional por módulo

| Módulo | Viabilidad para apicultura real | Estado |
|---|---|---|
| Apiarios / Colmenas | Semáforo verde/amarillo/rojo, capacidad, estado de reina — modelo de dominio correcto y completo | ✅ Sólido |
| Inspecciones | Soporta inspección a nivel apiario o colmena puntual, auto-marca "vencida" si pasó la fecha, actualiza `UltimaVisita` en cascada | ✅ Sólido, con scoping por sector bien implementado |
| Sanidad / Transhumancia / Inventario | Lógica de negocio razonable (tratamientos, traslados, stock mín/máx) | ✅ Funcional, control de acceso corregido (ver §3) |
| **Producción/Cosechas** | Diseño correcto (venta opcional sincroniza ingreso en Finanzas) | ✅ **Corregido** — ver hallazgo crítico §2 |
| Finanzas | Balance, búsqueda, paginado, `[Authorize(Roles="ADMIN")]` correcto | ✅ Sólido |
| Planificación (Tareas/Visitas) | Asignación a empleado, scoping por sector en visitas | ✅ Sólido |
| Alertas comunitarias | Cálculo Haversine real para notificar apiarios en radio — buena idea de dominio | ✅ Acceso total para EMPLEADO confirmado como intencional |
| Equipo / PIN / Auditoría | Gestión de empleados, generación de PIN con BCrypt, auditoría de acciones | ✅ Sólido, bien gateado a ADMIN |
| Login (password + PIN) | Doble flujo: admin con contraseña, empleado con PIN de 6 dígitos — decisión de UX correcta para campo | ✅ Acertado |

---

## 2. Hallazgo crítico de funcionalidad — ✅ RESUELTO

**`Cosechas.CrearRegistroIngreso` — registrar una cosecha estaba roto al 100%, para cualquier rol.**

Reproducido en vivo (como ADMIN y como EMPLEADO): `POST /Produccion/Crear` con datos válidos devolvía **500**:
```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 19: 'NOT NULL constraint failed: Cosechas.CrearRegistroIngreso'.
```
Columna residual de una versión anterior del modelo `Cosecha` (previa al refactor a `Vendida`/`PrecioPorKg`/`SincronizarIngreso`). `Database.EnsureCreated()` no migra esquemas existentes, así que bases ya creadas quedaron con esa columna `NOT NULL` huérfana que EF Core ya no puebla.

**Impacto:** ninguna empresa podía registrar una cosecha — función núcleo del negocio.

**Fix aplicado:** `Program.cs` — `ALTER TABLE Cosechas DROP COLUMN CrearRegistroIngreso` idempotente, siguiendo el mismo patrón ya usado en el archivo para otros ajustes de esquema. Verificado en vivo: cosecha creada y visible en `/Produccion/Index`, luego eliminada (dato de prueba).

---

## 3. Riesgos de seguridad/datos — ✅ RESUELTOS (con decisión del equipo)

Verificado en vivo con un empleado autenticado **sin sector asignado** (el caso más restrictivo posible), antes del fix:

| Endpoint | Antes | Después del fix |
|---|---|---|
| `GET /Produccion/Crear` | 200 | 200 — **intencional**, EMPLEADO tiene acceso total a Producción |
| `GET /Alertas/Crear` | 200 | 200 — **intencional**, EMPLEADO tiene acceso total a Alertas |
| `GET /Transhumancia/Crear` | 200 | **302** (solo ADMIN) |
| `GET /Inventario/Crear` | 200 | **302** (solo ADMIN) |
| `GET /Calendario/Index` | mostraba eventos de todos los sectores | filtrado por sector asignado para EMPLEADO |

Decisión del equipo sobre el modelo de permisos: **Producción y Alertas → acceso total para EMPLEADO** (se deja como estaba, es el comportamiento deseado). **Transhumancia e Inventario → lectura para EMPLEADO, escritura solo ADMIN.** Calendario se alineó al mismo patrón de scoping por sector que ya usan Inspecciones/Sanidad/Colmenas (un empleado no debía ver inspecciones, cosechas, controles sanitarios o traslados de sectores ajenos).

**Fixes aplicados:**
- `TranshumanciaController.cs` / `InventarioController.cs` — `[Authorize(Roles="ADMIN")]` en `Crear`/`Editar`/`Eliminar` (GET y POST). `Index` queda abierto a cualquier usuario autenticado.
- `Views/Transhumancia/Index.cshtml` / `Views/Inventario/Index.cshtml` — botones de Editar/Eliminar/Crear ocultos para EMPLEADO (mismo patrón ya usado en Apiarios/Colmenas/Sanidad).
- `CalendarioController.cs` — inyectado `UserManager<ApplicationUser>`; cada tipo de evento (inspección, cosecha, control sanitario, traslado, tarea, visita) se filtra por el `ApiarioAsignadoId` del empleado (o por nombre de apiario para Transhumancia/Visita, que no tienen FK real) cuando el usuario no es ADMIN. Las tareas se filtran además por `AsignadoAId`.
- `AccountController.cs` — el `[Authorize]` en `Logout` quedaba muerto por el `[AllowAnonymous]` de la clase (warning del compilador `ASP0026`). Se movió `[AllowAnonymous]` a `Login`/`LoginPin`/`AccesoDenegado` individualmente, dejando `Logout` correctamente protegido. Verificado en vivo: `POST /Account/Logout` sin sesión ahora redirige a login (antes ejecutaba el sign-out de forma anónima).

Todos los fixes fueron verificados en vivo (curl con cookies, sesiones admin y empleado separadas) antes y después del cambio.

---

## 4. Auditoría de accesibilidad — perspectiva trabajador de campo (pendiente)

**Hallazgo más grave de UX, sin resolver:** en `wwwroot/css/site.css:838`, dentro de `@media (max-width: 768px)`:
```css
.sb-nav { display: none; }
```
**No hay menú hamburguesa ni navegación alternativa.** En cualquier celular (el dispositivo realista de un apicultor en el campo), el menú lateral completo desaparece — Apiarios, Colmenas, Inspecciones, Tareas, todo. Solo queda visible el logo y el botón de logout. Una vez cargada una pantalla, el usuario de campo no tiene forma de navegar a ninguna otra sección.

Otros puntos, en orden de impacto:
- **Sin soporte offline.** Apiarios del seed con accesos como "Solo con buen tiempo" o "Requiere 4x4" — zonas con conectividad pobre garantizada — pero la app depende de CDN (Google Fonts, Leaflet) sin Service Worker/PWA ni cacheo local.
- **Touch targets por debajo del mínimo recomendado** (~44px Apple HIG/WCAG): `.chk { width:20px; height:20px }` en checkboxes de tareas. Difíciles de tocar con guantes de apicultura.
- **Acierto real:** login dual con PIN de 6 dígitos para empleados — mucho más rápido que email+contraseña en un celular en el campo.
- **Captura de coordenadas manual** en `Apiarios/Crear` — falta integrar `navigator.geolocation` para autocompletar lat/long en sitio.

---

## 5. Resumen y prioridades

**Fortalezas**
- Modelo de dominio apícola coherente (semáforos, estados de reina, ciclo de inspección/cosecha/sanidad)
- Auditoría de acciones (`Auditoria`) y de accesos (`HistorialAcceso`) reales, no decorativas
- Scoping por sector bien implementado y ahora extendido de forma consistente a Calendario
- Login dual contraseña/PIN, pensado para uso en campo

**Prioridades, en orden**

| # | Acción | Estado |
|---|---|---|
| 1 | Arreglar columna huérfana `Cosechas.CrearRegistroIngreso` (bloqueaba registrar cosechas) | ✅ Hecho |
| 2 | `[Authorize]` en Transhumancia/Inventario (escritura ADMIN, lectura EMPLEADO) | ✅ Hecho |
| 3 | Scoping por sector en Calendario | ✅ Hecho |
| 4 | `[Authorize]` muerto en `AccountController.Logout` | ✅ Hecho |
| 5 | Navegación móvil (drawer/hamburguesa o bottom-nav) — la app es inutilizable en celular hoy | ⬜ Pendiente |
| 6 | Touch targets ≥44px en checkboxes/botones de acción frecuente | ⬜ Pendiente |
| 7 | Modo offline-first (al menos cachear assets estáticos), dado el contexto rural real del negocio | ⬜ Pendiente |
| 8 | Captura de coordenadas vía GPS del dispositivo en Apiarios/Crear | ⬜ Pendiente |

---

*Documento generado por Claude Code — 2026-06-22*
