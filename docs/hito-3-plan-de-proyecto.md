# Hito 3 — Plan de Proyecto

**Proyecto:** ZanganosSA — Sistema de Gestión Apícola
**Repositorio:** https://github.com/DiegoMar17/ZanganosSA_Obligatorio.git
**Fecha:** Junio 2026

> Nota: los campos marcados con **〚completar〛** deben ajustarse con los datos reales del equipo antes de la entrega.

---

## 1. Descripción del entorno

ZanganosSA es una empresa apícola que gestiona la producción de miel y subproductos a partir de múltiples apiarios distribuidos en el territorio. Actualmente la operación se administra de forma manual (planillas, anotaciones en papel y comunicación informal), lo que genera pérdida de información, dificultad para hacer trazabilidad sanitaria y falta de visibilidad sobre la producción y las finanzas.

**Problema a resolver:** centralizar en un único sistema la gestión de apiarios, colmenas, inspecciones, sanidad, producción, transhumancia, inventario y finanzas, con información confiable y en tiempo real.

**Entorno de operación:**
- Usuarios: apicultor principal (administrador) y empleados de campo.
- Uso desde computadora de escritorio y notebook; acceso vía navegador web.
- Datos sensibles: registros productivos, sanitarios y financieros de la empresa.

**Entorno técnico:**
- Aplicación web **ASP.NET Core MVC** sobre **.NET 9**.
- Persistencia con **Entity Framework Core** sobre base **SQLite** (archivo local `colmena.db`).
- Autenticación con **ASP.NET Core Identity**.
- Interfaz web responsiva (HTML/CSS/JS propio, sin framework pesado de front).

---

## 2. Análisis de alternativas

Se evaluaron distintas opciones para resolver la necesidad de la empresa:

| Alternativa | Ventajas | Desventajas | Decisión |
|---|---|---|---|
| **A. Planilla de cálculo (Excel/Sheets)** | Costo cero, conocido por el usuario | Sin validaciones, sin multiusuario real, propenso a errores, sin trazabilidad | Descartada |
| **B. Software apícola comercial** | Funcionalidad lista | Costo de licencia, poco adaptable a ZanganosSA, datos en servidores de terceros | Descartada |
| **C. Aplicación de escritorio (WinForms/WPF)** | Buen rendimiento local | Instalación por equipo, difícil de actualizar, no accesible desde varios dispositivos | Descartada |
| **D. Aplicación web a medida (ASP.NET Core MVC)** | Adaptada al negocio, accesible desde navegador, centraliza datos, evolutiva | Requiere desarrollo propio y mantenimiento | **Seleccionada** |

**Justificación de la alternativa D:** es la que mejor se ajusta al proceso real de ZanganosSA. Permite que varias personas trabajen desde distintos equipos de la empresa, junta toda la información en un solo lugar y se puede ir agrandando de a poco (por ejemplo, el módulo de roles que queda pendiente). También nos sirve porque trabajamos con C# y .NET, que es lo que ya veníamos usando.

---

## 3. Estudio de factibilidad

**Factibilidad técnica:** ALTA.
El equipo maneja C#, ASP.NET Core MVC y Entity Framework. La arquitectura MVC + EF Core + SQLite está probada y es suficiente para el volumen de datos esperado. No requiere infraestructura compleja: corre con el SDK de .NET y un archivo de base SQLite.

**Factibilidad económica:** ALTA.
Todo el stack es gratuito y de código abierto (.NET, EF Core, SQLite, Identity). No hay costo de licencias ni de servidores en esta etapa (la base es local). El único costo es el tiempo de desarrollo del equipo.

**Factibilidad operativa:** MEDIA-ALTA.
Los usuarios ya conocen el proceso apícola; la curva de aprendizaje del sistema es baja gracias a una interfaz simple y en español. Requiere una breve capacitación inicial (ver Plan de capacitación).

**Factibilidad temporal:** ALTA.
El alcance se divide en incrementos entregables, lo que permite cumplir con los hitos del obligatorio sin dependencias bloqueantes.

**Conclusión:** el proyecto es **factible** en las cuatro dimensiones evaluadas.

---

## 4. Integrantes y roles

El equipo está formado por dos integrantes:

| Integrante | Usuario GitHub |
|---|---|
| 〚Diego Martinez〛 | DiegoMar17 |
| 〚Eric Vignolo〛 | ericVignolo |
<<<<<<< HEAD
=======

> Completar con los nombres reales antes de la entrega.
>>>>>>> 5089ef73de9490ecdf3b00e821568a48a103a3b6

No nos repartimos el trabajo en roles fijos. Vamos resolviendo las tareas a medida que aparecen: lo que haya pendiente lo toma el que esté disponible, y muchas veces lo hacemos juntos sobre la misma parte (uno escribe, el otro revisa o prueba). Tanto el backend (controllers, EF Core, base de datos) como el frontend (vistas, estilos, interacción) y las pruebas las hicimos los dos, según lo que pidiera cada etapa.

De la misma forma, las tareas de calidad, manejo del repositorio y testing las llevamos entre ambos en vez de asignarlas a una sola persona: revisamos el código antes de subirlo, cuidamos las ramas y el historial en Git, y vamos probando los módulos a medida que los terminamos.

---

## 5. Descripción y selección de herramientas

| Categoría | Herramienta | Justificación |
|---|---|---|
| Lenguaje | **C#** | Tipado fuerte, robusto, dominado por el equipo |
| Framework web | **ASP.NET Core MVC (.NET 9)** | Patrón MVC claro, maduro, multiplataforma |
| ORM | **Entity Framework Core 9** | Acceso a datos por objetos, migraciones, menos SQL manual |
| Base de datos | **SQLite** | Sin servidor, archivo local, ideal para el alcance actual |
| Autenticación | **ASP.NET Core Identity** | Gestión de usuarios, login y (a futuro) roles, ya integrado |
| Control de versiones | **Git + GitHub** | Estándar de la industria, trabajo colaborativo, historial |
| IDE | **Visual Studio / VS Code** | Soporte nativo .NET, depuración integrada |
| Documentación | **Markdown** | Liviano, versionable junto al código en `docs/` y `Skills/sessions/` |
| Diseño de datos | **draw.io** | Modelo entidad-relación (ver `docs/ObligatorioMER.pdf`) |

---

## 6. Plan de SQA (Software Quality Assurance)

**Objetivo:** asegurar que el software cumpla los requisitos y mantenga calidad de código consistente.

**Estándares de código:**
- Nomenclatura consistente (PascalCase para clases y métodos, camelCase para variables locales).
- Controllers delgados; lógica de negocio agrupada y reutilizable.
- Validaciones de datos mediante Data Annotations en los modelos (`[Required]`, `[Range]`, `[StringLength]`).
- Vistas sin lógica de negocio; solo presentación.

**Actividades de aseguramiento:**
- **Revisión de código** entre integrantes antes de integrar a la rama principal.
- **Compilación sin errores ni warnings** como condición para integrar (`dotnet build` → 0 errores).
- **Revisión de coherencia entre módulos** (que los datos de un módulo se reflejen en los demás: dashboard, alertas, semáforos).
- Registro de cada sesión de trabajo y correcciones en `Skills/sessions/`.

**Cómo medimos la calidad:**
- Que la rama principal compile siempre sin errores.
- Que todos los formularios validen los datos del lado del servidor.
- Que en la demo no aparezcan funcionalidades rotas.

---

## 7. Plan de testing

**Estrategia:** testing manual por módulo + pruebas de integración entre módulos, dado el alcance académico del proyecto.

**Niveles de prueba:**
1. **Pruebas unitarias de validación:** verificar que cada formulario rechaza datos inválidos (campos obligatorios, rangos, formatos).
2. **Pruebas funcionales (CRUD):** por cada módulo, probar Crear, Listar, Editar y Eliminar.
3. **Pruebas de integración:** verificar que un cambio en un módulo se refleja en los demás (ej.: registrar una inspección actualiza la "última visita" de las colmenas; una cosecha vendida genera un ingreso en Finanzas; el estado de una colmena afecta el semáforo del apiario y las alertas del dashboard).
4. **Pruebas de seguridad/acceso:** que las páginas requieran login; que el manejo de errores muestre la página de error.

**Casos de prueba representativos:**

| # | Caso | Resultado esperado |
|---|---|---|
| T1 | Crear apiario sin nombre | Rechaza con mensaje "El nombre es obligatorio" |
| T2 | Crear colmena y asignar apiario | La colmena aparece en el apiario y suma a su conteo |
| T3 | Registrar inspección de un apiario | Se actualiza la última visita de sus colmenas |
| T4 | Registrar cosecha marcada como vendida | Se genera un ingreso automático en Finanzas |
| T5 | Inspección pendiente con fecha pasada | Se marca como "vencida" y aparece como alerta roja en el dashboard |
| T6 | Acceder a una página sin estar logueado | Redirige al login |

**Registro de resultados:** cada ronda de pruebas se documenta en `Skills/sessions/` indicando qué se probó y qué se corrigió.

---

## 8. Plan de SCM (Software Configuration Management)

**Repositorio:** GitHub — `ZanganosSA_Obligatorio`.

**Estrategia de ramas:**
- `main` — rama estable; solo recibe código compilable y probado.
- Ramas de trabajo por funcionalidad o corrección cuando aplique.

**Política de commits:**
- Mensajes descriptivos en español, con prefijo del tipo de cambio (`feat:`, `fix:`, `docs:`).
- Un commit por unidad lógica de cambio.

**Gestión de configuración:**
- Código fuente en `ColmenaEmpresaMVC/`.
- Documentación del obligatorio en `docs/` (hitos, MER, este plan).
- Bitácora de trabajo y correcciones en `Skills/sessions/`.
- La base `colmena.db` no se versiona como dato productivo; se regenera con datos de ejemplo (seed) al iniciar.

**Control de cambios:** todo cambio relevante queda registrado en el historial de Git y en la bitácora de sesiones, lo que permite trazabilidad de qué se modificó, cuándo y por qué.

---

## 9. Plan de capacitación

**Destinatarios:** apicultor principal (administrador) y empleados de campo.

**Modalidad:** capacitación presencial breve + manual de uso.

**Contenidos:**
1. **Sesión 1 — Introducción (30 min):** ingreso al sistema, navegación general, dashboard.
2. **Sesión 2 — Gestión diaria (45 min):** alta de apiarios y colmenas, registro de inspecciones y controles sanitarios.
3. **Sesión 3 — Producción y finanzas (30 min):** registro de cosechas, movimientos financieros, exportación de reportes.
4. **Sesión 4 — Planificación (15 min):** tareas, visitas y calendario.

**Material de apoyo:**
- Manual de usuario con capturas de pantalla.
- Datos de ejemplo cargados para practicar sin afectar datos reales.

**Evaluación:** cada usuario realiza una operación completa de cada módulo de forma autónoma al finalizar la capacitación.

---

## 10. Incrementos o iteraciones definidas

El proyecto se desarrolla de forma **incremental**, entregando funcionalidad utilizable en cada iteración.

| Incremento | Contenido | Estado |
|---|---|---|
| **Inc. 1 — Base** | Estructura MVC, modelos, base de datos, CRUD de Apiarios y Colmenas | ✅ Completado |
| **Inc. 2 — Operación de campo** | Inspecciones, Sanidad, Transhumancia, Calendario, Planificación | ✅ Completado |
| **Inc. 3 — Gestión económica** | Producción, Finanzas, Inventario, exportación de reportes | ✅ Completado |
| **Inc. 4 — Autenticación y perfil** | Login con Identity, perfil de usuario, protección de rutas | ✅ Completado |
| **Inc. 5 — Calidad y coherencia** | Revisión página por página, corrección de errores, integración entre módulos | ✅ Completado |
| **Inc. 6 — Control de acceso por roles** | Roles ADMIN/EMPLEADO, PIN, auditoría, panel de equipo, sectores | 🔜 Planificado |

---

## 11. Cronograma de trabajo y criticidad

**Ritmo de trabajo:** trabajamos de lunes a viernes, dedicándole entre una y dos horas por día cada uno. No avanzamos en bloques largos ni los fines de semana, sino un rato cada día durante la semana. Por eso las duraciones de abajo están pensadas en semanas y no en horas por jornada: reflejan cuánto nos llevó cada etapa con ese ritmo.

| Etapa | Actividad | Duración estimada | Criticidad | Dependencias |
|---|---|---|---|---|
| E1 | Análisis y modelo de datos (MER) | 1 semana | **Alta** | — |
| E2 | Estructura base MVC + EF Core + CRUD inicial | 1 semana | **Alta** | E1 |
| E3 | Módulos de operación de campo | 2 semanas | Media | E2 |
| E4 | Módulos de producción, finanzas e inventario | 2 semanas | Media | E2 |
| E5 | Autenticación y perfil | 1 semana | **Alta** | E2 |
| E6 | Revisión de calidad e integración entre módulos | 1 semana | **Alta** | E3, E4, E5 |
| E7 | Documentación de hitos y plan de proyecto | Transversal | Media | — |
| E8 | Control de acceso por roles | 2 semanas | **Alta** | E5, E6 |

**Análisis de criticidad:**
- **Camino crítico:** E1 → E2 → E5 → E6 → E8. Un retraso en estas etapas impacta directamente en la entrega final, porque el resto de los módulos dependen de la base de datos, la estructura MVC y la autenticación.
- **Etapas de criticidad alta:** las que sostienen al resto del sistema (modelo de datos, estructura base, autenticación, integración y roles).
- **Etapas de criticidad media:** los módulos funcionales (E3, E4) pueden desarrollarse en paralelo una vez lista la base, y un retraso acotado en ellos no bloquea al resto.
- **Riesgo principal:** la etapa E8 (roles) es la más compleja pendiente; se mitiga dividiéndola en fases (roles base → permisos → auditoría → gestor de PINs → sectores).
