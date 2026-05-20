# Normalización — Sistema de Gestión de Salón

## Datos de la Materia
- **Proyecto:** ObligatorioGeneral — ColmenaEmpresa  
- **Base de datos:** ObligatorioBSD  
- **Fecha:** Mayo 2026  

---

## Esquema Original (Sin Normalizar)

A continuación se listan las relaciones tal como fueron entregadas:

| Relación | Atributos |
|---|---|
| Cliente | ID_C, Nom-C, Ape-C, Tel-C |
| Profesional | ID_P, Nom-P, Especialidad, Disponibilidad |
| Servicios | ID_S, Nom-S, Desc-S |
| Turno | ID_T, Fecha, Hora, Estado, id-c, id_p, id_s, id-Ayudante |
| Salón | ID_Salon, Dir-S, Tel-S, Nom-S, id-t |
| Ciudad | ID-Ciudad, Nombre |
| Productos | ID_Prod, nom-Producto, precio-Unitario, tipo, marca |
| Stock | ID_Stock, cantidad, fecha-Vencimiento, ID-Producto, ID_Salon |
| Pago | ID_Pago, Fecha_Pago, Monto, Metodo, ID_C, ID_Prom |
| Promoción | ID_Prom, porcentaje-desc, fecha-Inicio, Fecha-Fin |
| Valoración | ID_V, puntuacion, Comentario, Id_C |
| Ayudante | Id-Ayudante, Nom-Ayudante, Ape-Ayudante, Cl-Ayudante |
| Producto-Cliente | ID_Producto, ID_C |
| Productos-Servicios-Profesional | ID_Producto, ID_S, ID_P |
| Clientes-Productos | ID_Productos, ID_C |
| Cliente-Pago | ID_C, ID_Pago |
| Cliente-Profesional | ID_P, ID_C |
| Turno-Servicio | ID_T, ID_S |
| Salon-Ciudad | ID_Salon, ID_Ciudad |
| Salón-Profesional | ID_Salon, ID_P |

---

## PRIMERA FORMA NORMAL (1FN)

> **Definición:** Una relación está en 1FN si todos sus atributos son atómicos (no se repiten grupos ni valores multivaluados).

### Análisis de violaciones 1FN

#### ❌ `Turno (ID_T, Fecha, Hora, Estado, id-c, id_p, id_s, id-Ayudante)`
**Problema:** El atributo `id_s` (ID del servicio) puede representar múltiples servicios por turno, lo cual viola la atomicidad. Además, `id-Ayudante` es un grupo repetido potencial.  
**Acción:** Se separa la relación Turno de sus servicios mediante la tabla intermedia `Turno_Servicio`, y se extrae el ayudante con FK en Turno.

#### ❌ `Salón (ID_Salon, Dir-S, Tel-S, Nom-S, id-t)`
**Problema:** `id-t` es una FK a Turno dentro de Salón, lo que implica que un salón solo puede tener un turno. Esto es un grupo repetido inverso — deberían existir múltiples turnos por salón.  
**Acción:** Se elimina `id-t` de Salón. La relación se maneja mediante una FK `Id_Salon` dentro de `Turno`.

#### ⚠️ `Clientes_Productos` y `Producto_Cliente`
**Problema:** Son **tablas duplicadas** que representan la misma relación (Producto ↔ Cliente). Esto viola el principio de no redundancia implícito en 1FN.  
**Acción:** Se conserva una sola tabla intermedia: `Producto_Cliente(Id_Prod, Id_Clie)`.

#### ⚠️ `Ayudante (Id-Ayu, Nom-Ayu, Ape-Ayu, Cl-Ayudante)`
**Problema:** `Cl-Ayudante` no tiene semántica clara — puede referirse a una "clave" o "clase", lo cual podría ser un atributo compuesto o ambiguo.  
**Acción:** Se renombra a `Categoria_Ayu` para dar claridad semántica. Se asume que es atómico.

### ✅ Esquema en 1FN

```
Cliente        (Id_Clie, Nom_C, Ape_C, Tel_C)
Profesional    (Id_P, Nom_P, Especialidad, Disponibilidad)
Servicios      (Id_S, Nom_S, Desc_S)
Turno          (Id_T, Fecha, Hora, Estado, Id_Clie, Id_P, Id_Ayu)
Salon          (Id_Salon, Dir_Salon, Tel_Salon, Nom_Salon)
Ciudad         (Id_Ciudad, Nom_Ciudad)
Productos      (Id_Prod, Nom_Prod, Precio_Unitario, Tipo, Marca)
Stock          (Id_Stock, Cantidad, Fecha_Venc, Id_Prod, Id_Salon)
Promocion      (Id_Prom, Porcentaje_Desc, Fecha_Inicio, Fecha_Fin)
Pago           (Id_Pago, Fecha_Pago, Monto, Metodo, Id_Clie, Id_Prom)
Valoracion     (Id_V, Puntuacion, Comentario, Id_Clie)
Ayudante       (Id_Ayu, Nom_Ayu, Ape_Ayu, Categoria_Ayu)

-- Tablas de relación
Turno_Servicio             (Id_T, Id_S)
Salon_Ciudad               (Id_Salon, Id_Ciudad)
Salon_Profesional          (Id_Salon, Id_P)
Producto_Cliente           (Id_Prod, Id_Clie)
Productos_Servicios_Prof   (Id_Prod, Id_S, Id_P)
Cliente_Pago               (Id_Clie, Id_Pago)
Cliente_Profesional        (Id_Clie, Id_P)
```

---

## SEGUNDA FORMA NORMAL (2FN)

> **Definición:** Una relación está en 2FN si está en 1FN y **todos los atributos no clave dependen funcionalmente de la clave primaria completa** (no de una parte de ella). Aplica solo a claves compuestas.

### Análisis de dependencias parciales

#### ❌ `Turno_Servicio (Id_T, Id_S)`
Solo tiene PK compuesta, sin atributos descriptivos → **ya está en 2FN** (no hay atributos que dependan parcialmente).

#### ❌ `Productos_Servicios_Profesional (Id_Prod, Id_S, Id_P)`
PK compuesta de 3 atributos. No hay atributos descriptivos adicionales → **ya está en 2FN**.

#### ❌ `Stock (Id_Stock, Cantidad, Fecha_Venc, Id_Prod, Id_Salon)`
PK es `Id_Stock` (simple). `Cantidad` y `Fecha_Venc` dependen de `Id_Stock`. `Id_Prod` e `Id_Salon` son FKs → **ya está en 2FN**.

#### ❌ `Pago (Id_Pago, Fecha_Pago, Monto, Metodo, Id_Clie, Id_Prom)`
PK es `Id_Pago` (simple). Todos los atributos dependen de `Id_Pago` → **ya está en 2FN**.  
> Nota: `Id_Clie` también aparece en `Cliente_Pago`. Si `Pago` ya tiene `Id_Clie`, la tabla `Cliente_Pago` es **redundante** → se elimina `Cliente_Pago`.

#### ❌ `Valoracion (Id_V, Puntuacion, Comentario, Id_Clie)`
PK es `Id_V` (simple). Todo depende de `Id_V` → **ya está en 2FN**.

#### ✅ Resultado: Ninguna violación de 2FN detectada (todas las claves simples o tablas intermedias sin atributos descriptivos).

### ✅ Esquema en 2FN

```
Cliente        (Id_Clie, Nom_C, Ape_C, Tel_C)
Profesional    (Id_P, Nom_P, Especialidad, Disponibilidad)
Servicios      (Id_S, Nom_S, Desc_S)
Turno          (Id_T, Fecha, Hora, Estado, Id_Clie, Id_P, Id_Ayu)
Salon          (Id_Salon, Dir_Salon, Tel_Salon, Nom_Salon)
Ciudad         (Id_Ciudad, Nom_Ciudad)
Productos      (Id_Prod, Nom_Prod, Precio_Unitario, Tipo, Marca)
Stock          (Id_Stock, Cantidad, Fecha_Venc, Id_Prod, Id_Salon)
Promocion      (Id_Prom, Porcentaje_Desc, Fecha_Inicio, Fecha_Fin)
Pago           (Id_Pago, Fecha_Pago, Monto, Metodo, Id_Clie, Id_Prom)
Valoracion     (Id_V, Puntuacion, Comentario, Id_Clie)
Ayudante       (Id_Ayu, Nom_Ayu, Ape_Ayu, Categoria_Ayu)

-- Tablas de relación
Turno_Servicio             (Id_T, Id_S)
Salon_Ciudad               (Id_Salon, Id_Ciudad)
Salon_Profesional          (Id_Salon, Id_P)
Producto_Cliente           (Id_Prod, Id_Clie)
Productos_Servicios_Prof   (Id_Prod, Id_S, Id_P)
Cliente_Profesional        (Id_Clie, Id_P)

-- Eliminada: Cliente_Pago (redundante con Id_Clie en Pago)
-- Eliminada: Clientes_Productos (duplicado de Producto_Cliente)
```

---

## TERCERA FORMA NORMAL (3FN)

> **Definición:** Una relación está en 3FN si está en 2FN y **no existen dependencias transitivas**: ningún atributo no clave depende de otro atributo no clave.

### Análisis de dependencias transitivas

#### ❌ `Productos (Id_Prod, Nom_Prod, Precio_Unitario, Tipo, Marca)`
**Posible dependencia transitiva:**  
`Marca → Tipo`  
Si cada marca pertenece siempre al mismo tipo de producto (por ejemplo, "L'Oréal" siempre es "Capillar"), entonces:  
`Id_Prod → Marca → Tipo`  
Esto es una **dependencia transitiva**.  

**Corrección:**
```
Productos  (Id_Prod, Nom_Prod, Precio_Unitario, Id_Marca)
Marca      (Id_Marca, Nom_Marca, Tipo)
```

#### ❌ `Turno (Id_T, Fecha, Hora, Estado, Id_Clie, Id_P, Id_Ayu)`
No se detecta dependencia transitiva entre atributos no clave. `Fecha`, `Hora`, `Estado` dependen directamente de `Id_T`. Las FKs son referencias a otras entidades → **no hay violación**.

#### ❌ `Pago (Id_Pago, Fecha_Pago, Monto, Metodo, Id_Clie, Id_Prom)`
`Monto` podría derivarse de `Id_Prom` (descuento aplicado). Sin embargo, el monto ya es el valor final pagado (post-descuento), por lo que **no es transitivo** — depende directamente del registro de pago.  
→ **No hay violación.**

#### ❌ `Ayudante (Id_Ayu, Nom_Ayu, Ape_Ayu, Categoria_Ayu)`
Si `Categoria_Ayu` define un rango de especialización con más atributos (sueldo base, permisos, etc.), convendría extraerla. Pero como se presenta solo como un atributo simple → **se mantiene en 3FN por ahora.**

#### ❌ `Salon (Id_Salon, Dir_Salon, Tel_Salon, Nom_Salon)`
La dirección de un salón podría implicar una ciudad. Ya que la relación `Salon_Ciudad` existe como tabla intermedia, no hay transitiva interna en Salón → **no hay violación.**

### ✅ Esquema Final en 3FN

```
Cliente        (Id_Clie, Nom_C, Ape_C, Tel_C)
Profesional    (Id_P, Nom_P, Especialidad, Disponibilidad)
Servicios      (Id_S, Nom_S, Desc_S)
Turno          (Id_T, Fecha, Hora, Estado, Id_Clie, Id_P, Id_Ayu)
Salon          (Id_Salon, Dir_Salon, Tel_Salon, Nom_Salon)
Ciudad         (Id_Ciudad, Nom_Ciudad)
Marca          (Id_Marca, Nom_Marca, Tipo)          ← NUEVA
Productos      (Id_Prod, Nom_Prod, Precio_Unitario, Id_Marca)
Stock          (Id_Stock, Cantidad, Fecha_Venc, Id_Prod, Id_Salon)
Promocion      (Id_Prom, Porcentaje_Desc, Fecha_Inicio, Fecha_Fin)
Pago           (Id_Pago, Fecha_Pago, Monto, Metodo, Id_Clie, Id_Prom)
Valoracion     (Id_V, Puntuacion, Comentario, Id_Clie)
Ayudante       (Id_Ayu, Nom_Ayu, Ape_Ayu, Categoria_Ayu)

-- Tablas de relación (todas en 3FN por no tener atributos no clave)
Turno_Servicio             (Id_T, Id_S)
Salon_Ciudad               (Id_Salon, Id_Ciudad)
Salon_Profesional          (Id_Salon, Id_P)
Producto_Cliente           (Id_Prod, Id_Clie)
Productos_Servicios_Prof   (Id_Prod, Id_S, Id_P)
Cliente_Profesional        (Id_Clie, Id_P)
```

---

## Resumen de Cambios Realizados

| # | Forma Normal | Cambio |
|---|---|---|
| 1 | 1FN | Se eliminó `id-t` de `Salón` (grupo repetido inverso) |
| 2 | 1FN | Se eliminó `id_s` del turno como atómico → ya existía `Turno_Servicio` |
| 3 | 1FN | Se renombró `Cl-Ayudante` → `Categoria_Ayu` (atributo ambiguo) |
| 4 | 1FN | Se fusionaron `Clientes_Productos` y `Producto_Cliente` (tablas duplicadas) |
| 5 | 2FN | Se eliminó `Cliente_Pago` (redundante: `Pago` ya tiene `Id_Clie`) |
| 6 | 3FN | Se extrajo `Marca` como entidad independiente para eliminar `Marca → Tipo` |
