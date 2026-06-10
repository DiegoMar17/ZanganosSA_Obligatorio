# Prompt — Implementación Control de Acceso por Roles (ZanganosSA)

Implementá un sistema completo de Control de Acceso por Roles para el proyecto ZanganosSA (Sistema de Gestión Apícola en ASP.NET Core MVC + MySQL). A continuación se detallan todos los módulos, funcionalidades y características a desarrollar:

---

## 1. Modificación del modelo de datos

Extender la entidad `APICULTOR` con dos nuevos campos:

```sql
ALTER TABLE APICULTOR ADD COLUMN rol ENUM('ADMIN', 'EMPLEADO') NOT NULL DEFAULT 'EMPLEADO';
ALTER TABLE APICULTOR ADD COLUMN pin_hash VARCHAR(255) NULL;
```

Crear tabla de auditoría:

```sql
CREATE TABLE AUDITORIA (
  id_auditoria INT AUTO_INCREMENT PRIMARY KEY,
  id_apicultor INT NOT NULL,
  accion       VARCHAR(100) NOT NULL,
  tabla        VARCHAR(50)  NOT NULL,
  fecha_hora   DATETIME     NOT NULL DEFAULT NOW(),
  FOREIGN KEY (id_apicultor) REFERENCES APICULTOR(id_apicultor)
);
```

Crear tabla de historial de accesos:

```sql
CREATE TABLE HISTORIAL_ACCESO (
  id_acceso    INT AUTO_INCREMENT PRIMARY KEY,
  id_apicultor INT NOT NULL,
  fecha_hora   DATETIME NOT NULL DEFAULT NOW(),
  ip           VARCHAR(45),
  dispositivo  VARCHAR(100),
  FOREIGN KEY (id_apicultor) REFERENCES APICULTOR(id_apicultor)
);
```

---

## 2. Módulo de autenticación

- Pantalla de login con campo de PIN numérico de 6 dígitos
- Validación del PIN contra el hash almacenado (bcrypt)
- Según el rol detectado, redirigir a la vista de Administrador o de Empleado
- Registrar cada acceso exitoso en `HISTORIAL_ACCESO`
- En ASP.NET Core MVC usar `[Authorize(Roles = "ADMIN")]` y `[Authorize(Roles = "EMPLEADO")]` en los controllers correspondientes

---

## 3. Vistas y permisos por rol

### Administrador — acceso completo

- Todos los módulos existentes: COLMENA, COSECHA, FINANZA, APIARIO, VISITA, TAREA, CONTROL_SANITARIO, TRASLADO, INSPECCION, ITEM_INVENTARIO
- Dashboard con KPIs globales: producción total, tareas pendientes, estado sanitario general
- Panel de equipo: lista de todos los empleados con su sector asignado y actividad reciente
- Módulo de gestión de acceso: generar, renovar y revocar PINs
- Módulo de auditoría: historial completo de acciones por empleado
- Asignación de tareas y sectores a empleados específicos
- Vista completa del módulo FINANZA (oculto para empleados)

### Empleado — acceso restringido

- Solo sus tareas asignadas (filtradas por `id_apicultor` en sesión)
- Solo los apiarios y colmenas de su sector asignado
- Registrar: VISITA, CONTROL_SANITARIO, INSPECCION (solo en sus sectores)
- No puede ver datos de otros empleados
- No tiene acceso a FINANZA
- No puede modificar registros existentes de otros usuarios

---

## 4. Gestor de PINs (panel exclusivo del Administrador)

Panel separado del CRUD principal con las siguientes funciones:

- **Generar PIN**: crear un PIN aleatorio de 6 dígitos para un empleado nuevo, guardarlo como hash (bcrypt) en `APICULTOR.pin_hash`
- **Ver estado de PINs**: tabla con todos los empleados mostrando estado del PIN (activo / expirado / nunca usado)
- **Revocar acceso**: deshabilitar el PIN de un empleado (campo adicional `pin_activo TINYINT(1)`)
- **Regenerar PIN**: generar un nuevo PIN si el empleado lo olvidó
- El PIN generado se muestra una sola vez al administrador y no se vuelve a mostrar en texto plano

---

## 5. Panel de equipo (solo Administrador)

Pantalla con vista tipo tabla o tarjetas que muestra:

- Nombre del empleado
- Sector / apiario asignado
- Cantidad de registros realizados esta semana
- Última actividad (fecha y hora)
- Estado del acceso (activo / sin PIN / revocado)
- Acceso directo al gestor de PIN desde esta pantalla

---

## 6. Dashboard exclusivo del Administrador

Indicadores principales visibles al iniciar sesión como ADMIN:

- Total de colmenas activas
- Cosechas registradas en el mes actual
- Tareas pendientes en todo el sistema
- Empleados activos / con acceso habilitado
- Últimos 5 registros de auditoría
- Alertas de controles sanitarios vencidos

---

## 7. Funciones existentes potenciadas por los roles

| Entidad existente | Mejora que agrega el sistema de roles |
|---|---|
| VISITA | Queda asociada automáticamente al empleado que la registró |
| TAREA | El admin la crea y asigna; el empleado solo ve las propias |
| CONTROL_SANITARIO | El admin ve el historial completo; el empleado solo su sector |
| FINANZA | Visible únicamente para ADMIN |
| TRASLADO | El empleado solicita; el admin aprueba |
| INSPECCION | Solo registrable por el empleado asignado al sector |

---

## 8. Auditoría de registros

- Cada operación de INSERT, UPDATE o DELETE relevante debe registrar una entrada en `AUDITORIA`
- Campos: quién hizo la acción, qué acción, sobre qué tabla, cuándo
- El administrador puede filtrar el historial por empleado, por fecha o por tipo de acción
- Los empleados no tienen acceso a esta pantalla

---

## 9. Historial de accesos

- Registrar cada login exitoso con fecha, hora e IP
- El administrador puede consultar el historial de accesos por empleado
- Útil para detectar accesos fuera de horario o accesos no autorizados

---

## 10. Notificaciones (complementario)

- Al asignar una tarea nueva, el empleado ve una notificación al iniciar sesión
- Contador de tareas pendientes visible en el header de la vista del empleado
- Puede implementarse con una tabla `NOTIFICACION` o con un campo `leida` en `TAREA`

---

## Stack tecnológico de referencia

- **Backend:** C# · ASP.NET Core MVC
- **Base de datos:** MySQL
- **ORM:** Entity Framework Core (o ADO.NET directo)
- **Autenticación:** ASP.NET Core Identity o implementación custom con sesiones y cookies
- **Hash de PINs:** BCrypt.Net-Next
- **Control de versiones:** Git + GitHub
