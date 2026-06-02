---
name: session-documenter
description: >
  Especialista en documentación de sesiones de trabajo. Registra todo lo que se
  trabajó en una conversación: prompts del usuario, herramientas usadas, archivos
  generados, decisiones tomadas y artefactos producidos. Al finalizar genera un
  documento Markdown estructurado de la sesión y opcionalmente lo sube a GitHub
  dentro de la carpeta Skills/sessions/ del repositorio del usuario, numerando
  cada sesión automáticamente.

  Activar cuando el usuario diga: "documentar sesión", "guardar lo que hicimos",
  "crear log de la sesión", "subir sesión a GitHub", "registrar la conversación",
  "crear sesión", "nueva sesión", "archivar sesión", o cualquier variante de querer
  dejar registro de lo trabajado. También activar si el usuario menciona "skill de
  documentación" o "guardar historial de trabajo".
---

# Session Documenter

Especialista que registra y archiva el trabajo realizado en una sesión de conversación.
Genera un documento Markdown estructurado y puede subirlo a GitHub automáticamente.

## Cuándo activar

- Usuario pide documentar, guardar o registrar la sesión actual
- Usuario quiere subir el historial a GitHub
- Al inicio de una sesión si el usuario dice "iniciar documentación"
- Al final de una sesión larga de trabajo colaborativo
- Cuando el usuario dice "esto quedó como skill" o "guardá esto"

---

## Flujo principal

### PASO 1 — Recopilar contexto de la sesión

Antes de generar nada, lee el historial completo de la conversación y extrae:

1. **Proyecto o tema central** de la sesión
2. **Lista de prompts clave** del usuario (los que marcaron dirección o decisión)
3. **Herramientas usadas** (bash, create_file, docx, SQL, Canva, etc.)
4. **Artefactos producidos**: archivos .sql, .docx, .html, .ps1, .md, presentaciones, etc.
5. **Decisiones técnicas tomadas** (ej: "se eligió MySQL sobre PostgreSQL", "se adoptó el formato minúsculas")
6. **Módulos o funcionalidades definidas**
7. **Problemas encontrados y cómo se resolvieron**
8. **Estado al cierre**: qué quedó completo, qué quedó pendiente

Si hay información ambigua o incompleta, preguntá al usuario antes de continuar.

### PASO 2 — Generar el documento de sesión

Genera un archivo Markdown siguiendo el formato definido en `references/session-template.md`.

El nombre del archivo sigue el patrón: `sesion-NNN-YYYY-MM-DD.md`
donde NNN es el número de sesión con cero adelante (001, 002, etc.)

Guardá el archivo en `/home/claude/session-FECHA/sesion-NNN-YYYY-MM-DD.md`

### PASO 3 — Verificar con el usuario

Mostrá un resumen de lo que se documentó y preguntá:
- ¿Hay algo que faltó registrar?
- ¿Hay algo incorrecto?
- ¿Querés subir esto a GitHub?

Esperá confirmación antes de proceder al Paso 4.

### PASO 4 — Subir a GitHub (si el usuario lo pide)

Lee `references/github-upload.md` para el proceso detallado.

En resumen:
1. Pedí el token de GitHub si no está disponible
2. Verificá/creá la carpeta `Skills/sessions/` en el repo
3. Determiná el número de sesión siguiente (contando archivos existentes)
4. Subí el archivo con un commit descriptivo

---

## Reglas de documentación

- **Nunca inventés** información que no esté en la conversación
- **Sé específico**: mencioná nombres de archivos, tablas, módulos reales
- **Prompts clave**: no transcribís todo, elegís los que marcaron decisiones
- **Código**: incluí snippets cortos solo si son decisiones relevantes (máx 10 líneas)
- **Tono técnico pero claro**: legible para el equipo del proyecto
- **Pendientes**: siempre cerrá con qué falta hacer

---

## Numeración de sesiones

- La primera sesión es `001`
- Si ya existen sesiones en GitHub, contás los archivos `.md` en `Skills/sessions/` y sumás 1
- Si es la primera vez y no hay sesiones previas, empezás en `001`
- Si el usuario dice el número, respetalo

---

## Manejo de errores comunes

**Token inválido**: Informar claramente y pedir uno nuevo con los permisos correctos (`repo` scope).

**Repositorio no encontrado**: Confirmar usuario y nombre del repo antes de reintentar.

**Carpeta Skills/sessions/ no existe**: Crearla automáticamente con el primer archivo.

**Sesión muy larga**: Si la conversación tiene más de 50 turnos, enfocarse en los hitos principales y los artefactos producidos, no en cada mensaje individual.
