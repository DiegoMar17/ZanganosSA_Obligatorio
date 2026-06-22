-- ============================================================
-- ZanganosSA_DB — script SQL Server actualizado
-- Generado a partir de la base real de la app (colmena.db / EF Core)
-- Reemplaza a "BASE DE DATOS 2 ZANGANOS.sql" (diseño original, desactualizado)
-- ============================================================
--
-- QUÉ CAMBIÓ respecto al script original y POR QUÉ:
--
-- 1. Apicultor + Propietario_Predio -> Empleado (única tabla).
--    La app no maneja "dueño del predio" como entidad — el login real
--    es por Empleado (admin/empleado) con rol y PIN. Por pedido explícito
--    se simplifica a columnas de negocio (sin ruido de Identity:
--    SecurityStamp, ConcurrencyStamp, etc.).
--
-- 2. Eliminadas: Evaluacion_Colmena, Traslado_Colmena, Movimiento_Inventario,
--    Visita_Tarea, Tarea_Colmena, Tarea_Apiario.
--    La app real no implementa esas tablas intermedias — Inspeccion ya
--    soporta nivel "apiario" o "colmena" directo (sin junction), Tarea
--    se asigna a un Empleado (no a colmenas/apiarios/visitas), Transhumancia
--    no vincula colmenas puntuales, e Item_Inventario guarda cantidad
--    actual directa (sin historial de movimientos).
--
-- 3. Varias FK pasaron a ser texto plano (denormalizado) porque así está
--    implementado hoy en la app real — se mantiene fiel a esa realidad,
--    marcado con comentario "// DENORMALIZADO" en cada caso:
--      - RegistroFinanciero.Apiario_Nombre (no hay FK a Apiario)
--      - Transhumancia.Apiario_Origen / Apiario_Destino (no hay FK a Apiario)
--      - Visita.Apiario_Nombre (no hay FK a Apiario)
--      - Control_Sanitario.Colmenas_Afectadas (CSV de códigos, no FK)
--
-- 4. Cosecha ya NO tiene FK a Colmena ni a Finanza — la relación inversa
--    es Registro_Financiero.ID_Cosecha (nullable), para poder sincronizar
--    el ingreso al editar/eliminar una cosecha.
--
-- 5. Tablas nuevas que no existían en el diseño original (agregadas
--    durante el desarrollo real): Auditoria, Historial_Acceso,
--    Alerta_Comunitaria, Notificacion_Alerta.
--
-- 6. Fix de sintaxis: el script original declaraba las PK como
--    "int primary key" sin AUTO_INCREMENT/IDENTITY pero los INSERT
--    omitían el valor de ID — eso falla en SQL Server real. Acá todas
--    las PK son IDENTITY(1,1). También: tinyint(1) (MySQL) -> bit
--    (tipo correcto de SQL Server).
--
-- ============================================================

create database ZanganosSA_DB;
go
use ZanganosSA_DB;
go

-- ============================================================
-- tablas
-- ============================================================

create table Empleado (
    ID_Empleado int identity(1,1) primary key,
    Nom_Empleado nvarchar(100) not null,
    Email_Empleado nvarchar(100) not null unique,
    Password_Hash nvarchar(300) not null,

    Rol_Empleado varchar(20) not null default 'EMPLEADO'
        check (Rol_Empleado in ('ADMIN', 'EMPLEADO')),

    Pin_Hash nvarchar(300) null,
    Pin_Activo bit not null default 0,

    ID_Apiario int null  -- apiario/sector asignado (solo EMPLEADO, ADMIN = null)
);

create table Apiario (
    ID_Apiario int identity(1,1) primary key,
    Nom_Apiario nvarchar(100) not null,
    Depto_Apiario nvarchar(50) not null,
    Dir_Apiario nvarchar(200),
    Lat_Apiario float,
    Lon_Apiario float,
    Flora_Apiario nvarchar(100),
    Acceso_Apiario nvarchar(100),
    Fuente_Agua bit not null default 0,

    Cap_Apiario int not null default 0
        check (Cap_Apiario between 0 and 500),

    Estado_Semaforo varchar(10) not null default 'verde'
        check (Estado_Semaforo in ('verde', 'amarillo', 'rojo')),

    Total_Colmenas int not null default 0
);

alter table Empleado
    add constraint FK_Empleado_Apiario
    foreign key (ID_Apiario) references Apiario(ID_Apiario);

create table Colmena (
    ID_Colmena int identity(1,1) primary key,
    Cod_Colmena varchar(20) not null unique,

    ID_Apiario int not null,
    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO, copiado de Apiario.Nom_Apiario

    Tipo_Colmena varchar(20) not null default 'Langstroth'
        check (Tipo_Colmena in ('Langstroth', 'Núcleo', 'Otro')),

    Fecha_Instalacion date not null,
    Origen_Colmena nvarchar(100),

    Estado_Reina varchar(20) not null default 'vista'
        check (Estado_Reina in ('vista', 'no_vista', 'ausente')),

    Cantidad_Alzas int not null default 0
        check (Cantidad_Alzas between 0 and 20),

    Marcos_Con_Cria int not null default 0
        check (Marcos_Con_Cria between 0 and 30),

    Estado_Semaforo varchar(10) not null default 'verde'
        check (Estado_Semaforo in ('verde', 'amarillo', 'rojo', 'viaje')),

    Ultima_Visita date null,
    Observaciones nvarchar(500),

    foreign key (ID_Apiario) references Apiario(ID_Apiario)
);

create table Inspeccion (
    ID_Inspeccion int identity(1,1) primary key,

    Tipo_Inspeccion varchar(10) not null default 'apiario'
        check (Tipo_Inspeccion in ('apiario', 'colmena')),

    ID_Apiario int not null,
    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO

    ID_Colmena int null,
    Colmena_Codigo varchar(20),

    Fecha_Insp date not null,
    Clima_Insp nvarchar(50),

    Temp_Insp decimal(4,1)
        check (Temp_Insp between -10 and 50),

    Colmenas_Inspeccionadas int not null default 0
        check (Colmenas_Inspeccionadas between 0 and 1000),

    Total_Colmenas int not null default 0
        check (Total_Colmenas between 0 and 1000),

    Estado_Insp varchar(20) not null default 'pendiente'
        check (Estado_Insp in ('pendiente', 'completa', 'vencida', 'incompleta')),

    Notas_Campo nvarchar(1000),

    foreign key (ID_Apiario) references Apiario(ID_Apiario),
    foreign key (ID_Colmena) references Colmena(ID_Colmena)
);

create table Control_Sanitario (
    ID_Control int identity(1,1) primary key,

    ID_Apiario int not null,
    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO

    Colmenas_Afectadas nvarchar(500),  -- // DENORMALIZADO, CSV de códigos de Colmena

    Tipo_Control nvarchar(100) not null,

    Resultado_Control varchar(20)
        check (Resultado_Control in ('positivo', 'negativo', 'dudoso')),

    Tratamiento nvarchar(200),
    Dosis nvarchar(100),
    Fecha_Control date not null,

    Estado_Control varchar(20) not null default 'en_tratamiento'
        check (Estado_Control in ('en_tratamiento', 'limpio')),

    Observaciones nvarchar(1000),

    foreign key (ID_Apiario) references Apiario(ID_Apiario)
);

create table Cosecha (
    ID_Cosecha int identity(1,1) primary key,

    ID_Apiario int not null,
    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO

    Fecha_Cosecha date not null,
    Tipo_Miel nvarchar(100) not null default 'Multifloral',

    Alzas_Cosecha int not null
        check (Alzas_Cosecha between 1 and 200),

    Peso_Bruto float not null
        check (Peso_Bruto > 0 and Peso_Bruto <= 10000),

    Merma float not null default 0
        check (Merma between 0 and 10000),

    Humedad float
        check (Humedad between 0 and 100),

    HMF float
        check (HMF between 0 and 100),

    Destino_Cosecha nvarchar(100) not null default 'Stock',
    Notas_Cosecha nvarchar(500),

    Vendida bit not null default 0,
    Precio_Por_Kg decimal(12,2) not null default 0
        check (Precio_Por_Kg between 0 and 100000),

    check (Peso_Bruto > Merma),

    foreign key (ID_Apiario) references Apiario(ID_Apiario)

    -- nota: Peso_Neto y Monto_Venta NO son columnas — se calculan en la
    -- app (PesoBruto-Merma y PesoNeto*PrecioPorKg), igual que en colmena.db
);

create table Registro_Financiero (
    ID_Finanza int identity(1,1) primary key,

    Tipo_Movimiento varchar(20) not null
        check (Tipo_Movimiento in ('ingreso', 'gasto', 'inversion')),

    Categoria nvarchar(100) not null,
    Descripcion nvarchar(300) not null,
    Fecha_Finanza date not null,

    Monto decimal(12,2) not null
        check (Monto between 0.01 and 10000000),

    Apiario_Nombre nvarchar(100) not null default 'General',  -- // DENORMALIZADO, sin FK a Apiario

    ID_Cosecha int null,  -- sincroniza el ingreso si la cosecha se edita/elimina

    foreign key (ID_Cosecha) references Cosecha(ID_Cosecha)
);

create table Transhumancia (
    ID_Transhumancia int identity(1,1) primary key,
    Nom_Transhumancia nvarchar(100) not null,

    Apiario_Origen nvarchar(100) not null,    -- // DENORMALIZADO, sin FK a Apiario
    Apiario_Destino nvarchar(100) not null,   -- // DENORMALIZADO, sin FK a Apiario

    Cantidad_Colmenas int not null
        check (Cantidad_Colmenas between 1 and 500),

    Distancia_Km float not null
        check (Distancia_Km between 0.1 and 5000),

    Fecha_Salida date not null,
    Fecha_Retorno date null,
    Motivo_Transhumancia nvarchar(300),

    Estado_Transhumancia varchar(20) not null default 'en_curso'
        check (Estado_Transhumancia in ('planificado', 'en_curso', 'completado')),

    Porcentaje_Avance int not null default 0
        check (Porcentaje_Avance between 0 and 100)
);

create table Item_Inventario (
    ID_Item int identity(1,1) primary key,
    Nom_Item nvarchar(100) not null,
    Unidad_Item nvarchar(20) not null default 'u',

    Cantidad_Actual float not null default 0
        check (Cantidad_Actual between 0 and 100000),

    Cantidad_Maxima float not null default 0
        check (Cantidad_Maxima between 0 and 100000),

    Cantidad_Minima float not null default 0
        check (Cantidad_Minima between 0 and 100000)

    -- nota: Porcentaje_Stock y Estado_Stock NO son columnas — calculados
    -- en la app a partir de Cantidad_Actual/Cantidad_Maxima
);

create table Visita (
    ID_Visita int identity(1,1) primary key,

    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO, sin FK a Apiario

    Fecha_Plan_Visita date not null,
    Materiales nvarchar(300),

    Estado_Visita varchar(20) not null default 'planificada'
        check (Estado_Visita in ('planificada', 'completada'))

    -- nota: "vencida" se calcula en la app (planificada + fecha pasada),
    -- no es un valor guardado en Estado_Visita
);

create table Tarea (
    ID_Tarea int identity(1,1) primary key,
    Nom_Tarea nvarchar(200) not null,
    Categoria_Tarea nvarchar(50) not null default 'General',

    Prioridad_Tarea varchar(10) not null default 'media'
        check (Prioridad_Tarea in ('alta', 'media', 'baja')),

    Fecha_Vencimiento date null,
    Completada bit not null default 0,
    Fecha_Creacion datetime not null default getdate(),

    ID_Empleado int null,              -- a quién está asignada (nullable: sin asignar)
    Asignado_Nombre nvarchar(150) not null default '',  -- // DENORMALIZADO

    foreign key (ID_Empleado) references Empleado(ID_Empleado)
);

create table Auditoria (
    ID_Auditoria int identity(1,1) primary key,

    ID_Empleado int not null,
    Nombre_Usuario nvarchar(150) not null default '',

    Accion nvarchar(100) not null,
    Tabla_Afectada nvarchar(50) not null,
    Fecha_Hora datetime not null default getdate(),
    Detalle nvarchar(500) null,

    foreign key (ID_Empleado) references Empleado(ID_Empleado)
);

create table Historial_Acceso (
    ID_Historial int identity(1,1) primary key,

    ID_Empleado int not null,
    Nombre_Usuario nvarchar(150) not null default '',

    Fecha_Hora datetime not null default getdate(),
    Ip varchar(50) null,
    Dispositivo nvarchar(200) null,
    Exitoso bit not null default 1,

    foreign key (ID_Empleado) references Empleado(ID_Empleado)
);

create table Alerta_Comunitaria (
    ID_Alerta int identity(1,1) primary key,
    Titulo nvarchar(150) not null,
    Descripcion nvarchar(1000) not null,
    Tipo_Amenaza varchar(50) not null default 'sanitaria',

    Latitud float not null
        check (Latitud between -90 and 90),

    Longitud float not null
        check (Longitud between -180 and 180),

    Radio_Km float not null default 10
        check (Radio_Km between 1 and 200),

    Ubicacion nvarchar(200),

    Estado_Alerta varchar(20) not null default 'activa'
        check (Estado_Alerta in ('activa', 'resuelta')),

    Fecha_Creacion datetime not null default getdate(),
    Fecha_Resolucion datetime null,
    Reportado_Por nvarchar(150),  -- // DENORMALIZADO, email del empleado, sin FK
    Notas nvarchar(500)
);

create table Notificacion_Alerta (
    ID_Notificacion int identity(1,1) primary key,

    ID_Alerta int not null,
    ID_Apiario int not null,
    Apiario_Nombre nvarchar(100) not null,  -- // DENORMALIZADO

    Distancia_Km float not null,
    Fecha_Envio datetime not null default getdate(),
    Leida bit not null default 0,

    foreign key (ID_Alerta) references Alerta_Comunitaria(ID_Alerta),
    foreign key (ID_Apiario) references Apiario(ID_Apiario)
);

-- ============================================================
-- indices
-- ============================================================

create index idx_empleado_apiario on Empleado(ID_Apiario);
create index idx_colmena_apiario on Colmena(ID_Apiario);
create index idx_inspeccion_apiario on Inspeccion(ID_Apiario);
create index idx_inspeccion_colmena on Inspeccion(ID_Colmena);
create index idx_control_apiario on Control_Sanitario(ID_Apiario);
create index idx_cosecha_apiario on Cosecha(ID_Apiario);
create index idx_cosecha_fecha on Cosecha(Fecha_Cosecha);
create index idx_finanza_cosecha on Registro_Financiero(ID_Cosecha);
create index idx_tarea_empleado on Tarea(ID_Empleado);
create index idx_tarea_pendiente on Tarea(Completada, Fecha_Vencimiento);
create index idx_visita_fecha on Visita(Fecha_Plan_Visita, Estado_Visita);
create index idx_auditoria_fecha on Auditoria(Fecha_Hora);
create index idx_historial_empleado on Historial_Acceso(ID_Empleado, Fecha_Hora);
create index idx_notif_apiario on Notificacion_Alerta(ID_Apiario);
go

-- ============================================================
-- inserts — calcados de la data semilla real de Program.cs
-- ============================================================

insert into Empleado (Nom_Empleado, Email_Empleado, Password_Hash, Rol_Empleado, Pin_Hash, Pin_Activo, ID_Apiario)
values
(N'Carlos Bentancur', 'admin@colmena.com', N'[hash generado por la app — Identity/PBKDF2]', 'ADMIN', null, 0, null);
-- Empleados de ejemplo (la app real los crea desde "Gestión de PINs", no vienen seedeados):
insert into Empleado (Nom_Empleado, Email_Empleado, Password_Hash, Rol_Empleado, Pin_Hash, Pin_Activo, ID_Apiario)
values
(N'Ana Silva', 'empleado1@colmena.com', '[hash generado por la app]', 'EMPLEADO', N'[hash de PIN — BCrypt]', 1, null);

insert into Apiario
(Nom_Apiario, Depto_Apiario, Dir_Apiario, Lat_Apiario, Lon_Apiario, Flora_Apiario, Acceso_Apiario, Fuente_Agua, Cap_Apiario, Estado_Semaforo, Total_Colmenas)
values
(N'La Rinconada',  N'San José',  N'Ruta 3 km 85',  -34.337, -56.713, N'Eucaliptal',   N'Todo tiempo',          0, 30, 'rojo',     18),
(N'Monte Olivo',   N'Canelones', N'Ruta 8 km 48',  -34.523, -56.284, N'Monte nativo', N'Todo tiempo',          0, 40, 'verde',    24),
(N'El Eucaliptal', N'Lavalleja', N'Ruta 81 km 12', -34.375, -55.237, N'Eucaliptal',   N'Solo con buen tiempo', 0, 25, 'amarillo', 15),
(N'Paso Carrasco', N'Rocha',     N'Ruta 9 km 220', -34.483, -54.333, N'Pradera',      N'Requiere 4x4',         0, 35, 'amarillo', 21);

update Empleado set ID_Apiario = (select ID_Apiario from Apiario where Nom_Apiario = N'Monte Olivo')
where Email_Empleado = 'empleado1@colmena.com';

insert into Colmena
(Cod_Colmena, ID_Apiario, Apiario_Nombre, Tipo_Colmena, Fecha_Instalacion, Estado_Reina, Cantidad_Alzas, Marcos_Con_Cria, Estado_Semaforo, Ultima_Visita)
values
('C-01',  (select ID_Apiario from Apiario where Nom_Apiario = N'Monte Olivo'),   N'Monte Olivo',   'Langstroth', '2022-09-01', 'vista',    2, 8, 'verde',    dateadd(day, -4,  getdate())),
('C-47',  (select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'),  N'La Rinconada',  'Langstroth', '2021-03-15', 'no_vista', 1, 5, 'amarillo', dateadd(day, -18, getdate())),
('C-82',  (select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'),  N'La Rinconada',  'Langstroth', '2020-11-01', 'ausente',  0, 2, 'rojo',     dateadd(day, -25, getdate())),
('C-110', (select ID_Apiario from Apiario where Nom_Apiario = N'Paso Carrasco'), N'Paso Carrasco', N'Núcleo',    '2023-10-01', 'vista',    1, 6, 'viaje',    null);

insert into Inspeccion
(Tipo_Inspeccion, ID_Apiario, Apiario_Nombre, Fecha_Insp, Clima_Insp, Temp_Insp, Colmenas_Inspeccionadas, Total_Colmenas, Estado_Insp)
values
('apiario', (select ID_Apiario from Apiario where Nom_Apiario = N'Monte Olivo'),   N'Monte Olivo',   '2026-04-18', N'Nublado', 19, 18, 24, 'completa'),
('apiario', (select ID_Apiario from Apiario where Nom_Apiario = N'El Eucaliptal'), N'El Eucaliptal', '2026-04-14', N'Lluvia',  15, 12, 15, 'incompleta'),
('apiario', (select ID_Apiario from Apiario where Nom_Apiario = N'Paso Carrasco'), N'Paso Carrasco', '2026-04-10', N'Soleado', 24, 21, 21, 'completa');

insert into Registro_Financiero
(Tipo_Movimiento, Categoria, Descripcion, Fecha_Finanza, Monto, Apiario_Nombre)
values
('ingreso',   N'Cosecha miel', N'Venta primavera 2026',   '2026-01-15', 1850, N'Monte Olivo'),
('gasto',     N'Insumos',      N'Ácido oxálico + frames', '2026-01-20', 320,  N'General'),
('inversion', N'Equipamiento', N'Extractor nuevo',        '2026-02-01', 2100, N'General'),
('ingreso',   N'Polen',        N'Venta mercado local',    '2026-02-10', 480,  N'General');

insert into Cosecha
(ID_Apiario, Apiario_Nombre, Fecha_Cosecha, Tipo_Miel, Alzas_Cosecha, Peso_Bruto, Merma, Humedad, Destino_Cosecha)
values
((select ID_Apiario from Apiario where Nom_Apiario = N'Monte Olivo'),   N'Monte Olivo',   '2026-01-10', N'Multifloral',  12, 855, 15, 18.2, N'Exportación'),
((select ID_Apiario from Apiario where Nom_Apiario = N'Paso Carrasco'), N'Paso Carrasco', '2026-01-20', N'Eucalipto',    9,  640, 10, 17.8, N'Fraccionado local'),
((select ID_Apiario from Apiario where Nom_Apiario = N'El Eucaliptal'), N'El Eucaliptal', '2026-02-05', N'Eucalipto',    8,  515, 10, 18.0, N'Stock'),
((select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'),  N'La Rinconada',  '2026-02-18', N'Monte nativo', 7,  415, 10, 18.5, N'Exportación');

insert into Control_Sanitario
(ID_Apiario, Apiario_Nombre, Colmenas_Afectadas, Tipo_Control, Resultado_Control, Tratamiento, Fecha_Control, Estado_Control)
values
((select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'), N'La Rinconada', 'C-47', N'Varroa — recuento', 'positivo', N'Ácido oxálico', '2026-04-20', 'en_tratamiento'),
((select ID_Apiario from Apiario where Nom_Apiario = N'Monte Olivo'),  N'Monte Olivo',  'C-01', N'Nosema',            'negativo', N'—',             '2026-04-18', 'limpio'),
((select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'), N'La Rinconada', 'C-82', N'Varroa — recuento', 'positivo', N'Ácido fórmico', '2026-04-14', 'en_tratamiento');

insert into Transhumancia
(Nom_Transhumancia, Apiario_Origen, Apiario_Destino, Cantidad_Colmenas, Distancia_Km, Fecha_Salida, Fecha_Retorno, Estado_Transhumancia, Porcentaje_Avance)
values
(N'Verano 2026',    N'Paso Carrasco', N'El Trébol (temp.)', 21, 184, '2026-01-01', '2026-06-30', 'en_curso',   45),
(N'Primavera 2025', N'Paso Carrasco', N'La Rinconada',      18, 140, '2023-09-01', '2023-12-01', 'completado', 100);

insert into Item_Inventario (Nom_Item, Unidad_Item, Cantidad_Actual, Cantidad_Maxima, Cantidad_Minima)
values
(N'Alzas de madera',  'u',  80,  100, 20),
(N'Ácido oxálico',    'kg', 2,   8,   2),
(N'Marcos de cera',   'u',  120, 200, 40),
(N'Jarabe azucarado', 'L',  5,   50,  20),
(N'Trajes apícolas',  'u',  4,   4,   2);

insert into Alerta_Comunitaria
(Titulo, Descripcion, Tipo_Amenaza, Latitud, Longitud, Radio_Km, Ubicacion, Estado_Alerta, Fecha_Creacion, Reportado_Por, Notas)
values
(N'Brote de Varroa detectado — Zona San José',
 N'Se confirmó infestación elevada de Varroa destructor en apiarios de la zona. Se recomienda realizar recuentos inmediatos y aplicar tratamiento preventivo.',
 'sanitaria', -34.337, -56.713, 50, N'Ruta 3, San José', 'activa', dateadd(day, -3, getdate()), 'admin@colmena.com',
 N'Contactar al MGAP para registro oficial del brote.'),
(N'Fumigación aérea de agroquímicos — Canelones',
 N'Se reportó aplicación aérea de herbicidas en cultivos de soja adyacentes. Riesgo de intoxicación de colonias.',
 'ambiental', -34.523, -56.284, 60, N'Ruta 8, Canelones', 'resuelta', dateadd(day, -10, getdate()), 'admin@colmena.com', null);

insert into Notificacion_Alerta (ID_Alerta, ID_Apiario, Apiario_Nombre, Distancia_Km, Leida)
values
((select ID_Alerta from Alerta_Comunitaria where Titulo like N'Brote de Varroa%'),
 (select ID_Apiario from Apiario where Nom_Apiario = N'La Rinconada'), N'La Rinconada', 0.0, 0);

-- Visita y Tarea: la app no las seedea (se crean desde "Planificación"),
-- estos son ejemplos ilustrativos:
insert into Visita (Apiario_Nombre, Fecha_Plan_Visita, Materiales, Estado_Visita)
values
(N'Monte Olivo', '2026-07-03', null, 'planificada'),
(N'El Eucaliptal', '2026-06-14', N'Ácido oxálico, cuadernillo', 'planificada');

insert into Tarea (Nom_Tarea, Categoria_Tarea, Prioridad_Tarea, Fecha_Vencimiento, ID_Empleado, Asignado_Nombre)
values
(N'Aplicar tratamiento Varroa en La Rinconada', N'Sanidad', 'alta', '2026-06-25',
 (select ID_Empleado from Empleado where Email_Empleado = 'empleado1@colmena.com'), N'Ana Silva'),
(N'Revisar nivel de jarabe en Paso Carrasco', N'Alimentación', 'media', '2026-06-30', null, '');
go

-- ============================================================
-- consultas
-- ============================================================

-- 1. producción neta por apiario
select
    a.Nom_Apiario as Apiario,
    sum(c.Peso_Bruto - c.Merma) as Kg_Netos,
    round(avg(c.Humedad), 2) as Humedad_Promedio
from Apiario a
inner join Cosecha c on a.ID_Apiario = c.ID_Apiario
group by a.ID_Apiario, a.Nom_Apiario
order by Kg_Netos desc;

-- 2. colmenas sin inspección puntual registrada
select
    a.Nom_Apiario as Apiario,
    co.Cod_Colmena as Colmena,
    max(i.Fecha_Insp) as Ultima_Inspeccion_Colmena
from Colmena co
inner join Apiario a on co.ID_Apiario = a.ID_Apiario
left join Inspeccion i on i.ID_Colmena = co.ID_Colmena
group by co.ID_Colmena, co.Cod_Colmena, a.Nom_Apiario
having max(i.Fecha_Insp) is null
order by a.Nom_Apiario;

-- 3. balance financiero (Apiario_Nombre es texto libre, sin FK real)
select
    f.Apiario_Nombre as Apiario,
    sum(case when f.Tipo_Movimiento = 'ingreso' then f.Monto else 0 end) as Total_Ingresos,
    sum(case when f.Tipo_Movimiento = 'gasto'   then f.Monto else 0 end) as Total_Gastos
from Registro_Financiero f
group by f.Apiario_Nombre;

-- 4. historial sanitario
select
    cs.Apiario_Nombre as Apiario,
    cs.Colmenas_Afectadas as Colmenas,
    cs.Tipo_Control,
    cs.Resultado_Control,
    cs.Tratamiento
from Control_Sanitario cs
order by cs.Fecha_Control desc;

-- 5. tareas pendientes
select
    t.ID_Tarea,
    t.Nom_Tarea,
    t.Fecha_Vencimiento,
    t.Prioridad_Tarea,
    t.Asignado_Nombre
from Tarea t
where t.Completada = 0
order by t.Fecha_Vencimiento asc;

-- 6. transhumancias (sin FK a Apiario — texto libre en origen/destino)
select
    tr.ID_Transhumancia,
    tr.Apiario_Origen as Origen,
    tr.Apiario_Destino as Destino,
    tr.Distancia_Km,
    tr.Estado_Transhumancia
from Transhumancia tr;

-- 7. resumen por empleado (1 apiario asignado como máximo, no N:M)
select
    e.Nom_Empleado as Empleado,
    e.Rol_Empleado,
    a.Nom_Apiario as Sector_Asignado,
    count(distinct c.ID_Colmena) as Total_Colmenas_Sector
from Empleado e
left join Apiario a on e.ID_Apiario = a.ID_Apiario
left join Colmena c on c.ID_Apiario = a.ID_Apiario
group by e.ID_Empleado, e.Nom_Empleado, e.Rol_Empleado, a.Nom_Apiario;
go
