-- =============================================================
-- ZanganosSA · Colmena_Empresa_ZanganosDB  v5
-- Nomenclatura alineada al MER del proyecto
-- Campos extra (no en MER) marcados con -- [+]
-- Generado: 2026-07-01
-- =============================================================

USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Colmena_Empresa_ZanganosDB')
BEGIN
    ALTER DATABASE Colmena_Empresa_ZanganosDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Colmena_Empresa_ZanganosDB;
END
GO
CREATE DATABASE Colmena_Empresa_ZanganosDB;
GO
USE Colmena_Empresa_ZanganosDB;
GO

-- =============================================================
-- TABLAS
-- =============================================================

-- ─────────────────────────────────────────────────────────────
-- PROPIETARIO_PREDIO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Propietarios (
    ID_Propietario  INT           PRIMARY KEY IDENTITY,
    Nom_Propietario NVARCHAR(100) NOT NULL,
    Tel_Propietario VARCHAR(20)   NULL,
    Email_Propietario VARCHAR(100) NULL   -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- APIARIO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Apiarios (
    ID_Apiario        INT           PRIMARY KEY IDENTITY,
    Nom_Apiario       NVARCHAR(100) NOT NULL,
    Depto_Apiario     NVARCHAR(50)  NOT NULL,
    Dir_Apiario       NVARCHAR(200) NOT NULL,
    Lat_Apiario       DECIMAL(9,6)  NULL,
    Lon_Apiario       DECIMAL(9,6)  NULL,
    Flora_Apiario     NVARCHAR(100) NULL,
    Acceso_Apiario    NVARCHAR(200) NULL,
    Cap_Apiario       INT           NOT NULL DEFAULT 0 CHECK (Cap_Apiario >= 0),
    FuenteAgua_Apiario BIT          NOT NULL DEFAULT 0,   -- [+]
    EstSem_Apiario    VARCHAR(20)   NOT NULL DEFAULT 'verde'
                      CHECK (EstSem_Apiario IN ('verde','amarillo','rojo')),  -- [+]
    ID_Apicultor_Api  NVARCHAR(450) NULL,                 -- [+] FK → AspNetUsers
    ID_Propietario_Api INT          NULL
                      REFERENCES Propietarios(ID_Propietario) ON DELETE SET NULL  -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- COLMENA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Colmenas (
    ID_Colmena        INT           PRIMARY KEY IDENTITY,
    Cod_Colmena       VARCHAR(20)   NOT NULL UNIQUE,
    ID_Apiario_Col    INT           NOT NULL
                      REFERENCES Apiarios(ID_Apiario) ON DELETE CASCADE,
    Tipo_Colmena      NVARCHAR(50)  NOT NULL DEFAULT 'Langstroth'
                      CHECK (Tipo_Colmena IN ('Langstroth','Núcleo','Otro')),
    Origen_Colmena    NVARCHAR(100) NULL,
    FecIns_Colmena    DATE          NOT NULL,             -- [+]
    EstReina_Colmena  VARCHAR(20)   NOT NULL DEFAULT 'vista'
                      CHECK (EstReina_Colmena IN ('vista','no_vista','ausente')),  -- [+]
    CantAlzas_Colmena INT           NOT NULL DEFAULT 0
                      CHECK (CantAlzas_Colmena BETWEEN 0 AND 20),  -- [+]
    MarcosCria_Colmena INT          NOT NULL DEFAULT 0
                      CHECK (MarcosCria_Colmena BETWEEN 0 AND 30), -- [+]
    EstSem_Colmena    VARCHAR(20)   NOT NULL DEFAULT 'verde'
                      CHECK (EstSem_Colmena IN ('verde','amarillo','rojo','viaje')), -- [+]
    UltVis_Colmena    DATETIME      NULL,                 -- [+]
    Obs_Colmena       NVARCHAR(500) NULL,                 -- [+]
    ID_Apicultor_Col  NVARCHAR(450) NULL                  -- [+] FK → AspNetUsers
);

-- ─────────────────────────────────────────────────────────────
-- VISITA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Visitas (
    ID_Visita        INT            PRIMARY KEY IDENTITY,
    ID_Apiario_Vis   INT            NOT NULL
                     REFERENCES Apiarios(ID_Apiario) ON DELETE CASCADE,   -- [+]
    Fecha_Plan_Vis   DATE           NOT NULL,
    Fecha_Real_Vis   DATE           NULL,
    Mat_Sugeridos    NVARCHAR(300)  NULL,
    Completada_V     VARCHAR(20)    NOT NULL DEFAULT 'planificada'
                     CHECK (Completada_V IN ('planificada','completada')),
    Notas_Visita     NVARCHAR(500)  NULL
);

-- ─────────────────────────────────────────────────────────────
-- INSPECCION
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Inspecciones (
    ID_Inspeccion  INT            PRIMARY KEY IDENTITY,
    ID_Apiario_Insp INT           NOT NULL
                   REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,   -- [+]
    ID_Colmena_Insp INT           NULL
                   REFERENCES Colmenas(ID_Colmena) ON DELETE NO ACTION,   -- [+]
    ID_Visita_Insp  INT           NULL
                   REFERENCES Visitas(ID_Visita) ON DELETE NO ACTION,     -- [+]
    Fecha_Insp     DATE           NOT NULL,
    Clima_Insp     NVARCHAR(50)   NULL,
    Temp_Insp      DECIMAL(4,1)   NULL,
    Notas_Insp     NVARCHAR(1000) NULL,
    Tipo_Insp      VARCHAR(20)    NOT NULL DEFAULT 'apiario'
                   CHECK (Tipo_Insp IN ('apiario','colmena')),             -- [+]
    ColInsp_Insp   INT            NOT NULL DEFAULT 0,                     -- [+]
    TotCol_Insp    INT            NOT NULL DEFAULT 0,                     -- [+]
    Estado_Insp    VARCHAR(20)    NOT NULL DEFAULT 'pendiente'
                   CHECK (Estado_Insp IN ('pendiente','completa','vencida'))  -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- EVALUACION_COLMENA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE EvaluacionesColmena (
    ID_Eval              INT           PRIMARY KEY IDENTITY,
    ID_Inspeccion_Eval   INT           NOT NULL
                         REFERENCES Inspecciones(ID_Inspeccion) ON DELETE CASCADE,
    ID_Colmena_Eval      INT           NOT NULL
                         REFERENCES Colmenas(ID_Colmena)        ON DELETE CASCADE,
    Estado_Reina_Insp    NVARCHAR(50)  NULL,
    Obs_Colmena_Eval     NVARCHAR(500) NULL,
    UNIQUE (ID_Inspeccion_Eval, ID_Colmena_Eval)
);

-- ─────────────────────────────────────────────────────────────
-- CONTROL_SANITARIO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE ControlesSanitarios (
    ID_Control        INT            PRIMARY KEY IDENTITY,
    ID_Apiario_Control INT           NOT NULL
                       REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,  -- [+]
    Tipo_Control      NVARCHAR(100)  NOT NULL,
    Resultado_Control VARCHAR(20)    NULL
                      CHECK (Resultado_Control IN ('positivo','negativo','dudoso')),
    Nivel_Infest      NVARCHAR(50)   NULL,
    Tratamiento       NVARCHAR(200)  NULL,
    Dosis             NVARCHAR(100)  NULL,
    Notas_Control     NVARCHAR(1000) NULL,
    Fecha_Ini_Trat    DATE           NOT NULL,
    Fecha_Fin_Trat    DATE           NULL,
    ColAfect_Control  NVARCHAR(500)  NULL,                -- [+]
    Estado_Control    VARCHAR(20)    NOT NULL DEFAULT 'en_tratamiento'
                      CHECK (Estado_Control IN ('en_tratamiento','limpio'))  -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- TRASLADO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Traslados (
    ID_Traslado      INT            PRIMARY KEY IDENTITY,
    ID_Apiario_Orig  INT            NOT NULL
                     REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,
    ID_Apiario_Dest  INT            NOT NULL
                     REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,
    Fecha_Salida     DATE           NOT NULL,
    Fecha_Retorno    DATE           NULL,
    Distancia_Km     DECIMAL(10,2)  NULL     CHECK (Distancia_Km >= 0),
    Motivo_Traslado  NVARCHAR(300)  NULL,
    Estado_Traslado  VARCHAR(20)    NOT NULL DEFAULT 'planificado'
                     CHECK (Estado_Traslado IN ('planificado','en_curso','completado')),
    Nom_Traslado     NVARCHAR(100)  NULL,                 -- [+]
    CantCol_Traslado INT            NULL CHECK (CantCol_Traslado > 0),    -- [+]
    PorcAv_Traslado  INT            NOT NULL DEFAULT 0
                     CHECK (PorcAv_Traslado BETWEEN 0 AND 100)            -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- TRASLADOS_COLMENA (pivot)
-- ─────────────────────────────────────────────────────────────
CREATE TABLE TrasladosColmena (
    ID_Traslado INT NOT NULL REFERENCES Traslados(ID_Traslado)  ON DELETE CASCADE,
    ID_Colmena  INT NOT NULL REFERENCES Colmenas(ID_Colmena)    ON DELETE CASCADE,
    PRIMARY KEY (ID_Traslado, ID_Colmena)
);

-- ─────────────────────────────────────────────────────────────
-- COSECHA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Cosechas (
    ID_Cosecha      INT           PRIMARY KEY IDENTITY,
    ID_Apiario_Cos  INT           NOT NULL
                    REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,  -- [+]
    Fecha_Cosecha   DATE          NOT NULL,
    Tipo_Miel       NVARCHAR(100) NOT NULL DEFAULT 'Multifloral',
    Alzas_Cosecha   INT           NOT NULL CHECK (Alzas_Cosecha >= 0),
    Peso_Bruto      DECIMAL(10,2) NOT NULL CHECK (Peso_Bruto > 0),
    Merma           DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (Merma >= 0),
    Humedad         DECIMAL(5,2)  NULL     CHECK (Humedad BETWEEN 0 AND 100),
    HMF             DECIMAL(10,2) NULL     CHECK (HMF >= 0),
    Color_Pfund     NVARCHAR(50)  NULL,
    Destino_Cosecha NVARCHAR(100) NULL     DEFAULT 'Stock',
    Notas_Cosecha   NVARCHAR(500) NULL,                   -- [+]
    Vendida_Cosecha BIT           NOT NULL DEFAULT 0,     -- [+]
    PrecKg_Cosecha  DECIMAL(10,4) NOT NULL DEFAULT 0,     -- [+]
    CONSTRAINT CHK_PesoNeto CHECK (Peso_Bruto >= Merma)
);

-- ─────────────────────────────────────────────────────────────
-- FINANZA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE RegistrosFinancieros (
    ID_Finanza      INT            PRIMARY KEY IDENTITY,
    Tipo_Finanza    VARCHAR(20)    NOT NULL
                    CHECK (Tipo_Finanza IN ('ingreso','gasto','inversion')),
    Monto           DECIMAL(12,2)  NOT NULL CHECK (Monto > 0),
    Fecha_Finanza   DATE           NOT NULL,
    Desc_Finanza    NVARCHAR(300)  NOT NULL,
    Categ_Finanza   NVARCHAR(100)  NULL,                  -- [+]
    ID_Apiario_Fin  INT            NULL
                    REFERENCES Apiarios(ID_Apiario) ON DELETE SET NULL,   -- [+]
    ID_Cosecha_Fin  INT            NULL
                    REFERENCES Cosechas(ID_Cosecha) ON DELETE SET NULL    -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- ITEM_INVENTARIO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE ItemsInventario (
    ID_Item       INT            PRIMARY KEY IDENTITY,
    Nom_Item      NVARCHAR(100)  NOT NULL,
    Unidad_Item   NVARCHAR(20)   NOT NULL DEFAULT 'u',
    Stock_Min     DECIMAL(10,2)  NOT NULL DEFAULT 0,
    Stock_Act_Item DECIMAL(10,2) NOT NULL DEFAULT 0,      -- [+]
    Stock_Max_Item DECIMAL(10,2) NOT NULL DEFAULT 0       -- [+]
);

-- ─────────────────────────────────────────────────────────────
-- MOVIMIENTO_INVENTARIO
-- ─────────────────────────────────────────────────────────────
CREATE TABLE MovimientosInventario (
    ID_Movimiento    INT           PRIMARY KEY IDENTITY,
    ID_Item_Mov      INT           NOT NULL
                     REFERENCES ItemsInventario(ID_Item) ON DELETE CASCADE,
    Mov_Tipo         VARCHAR(10)   NOT NULL CHECK (Mov_Tipo IN ('Entrada','Salida')),
    Mov_Cant         DECIMAL(10,2) NOT NULL CHECK (Mov_Cant > 0),
    Mov_Fecha        DATE          NOT NULL,
    Mov_Motivo       NVARCHAR(200) NULL,
    ID_Apicultor_Mov NVARCHAR(450) NULL                   -- [+] FK → AspNetUsers
);

-- ─────────────────────────────────────────────────────────────
-- TAREA
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Tareas (
    ID_Tarea          INT           PRIMARY KEY IDENTITY,
    Desc_Tarea        NVARCHAR(200) NOT NULL,
    Tipo_Tarea        NVARCHAR(50)  NOT NULL DEFAULT 'General',
    Prioridad_Tarea   VARCHAR(10)   NOT NULL DEFAULT 'media'
                      CHECK (Prioridad_Tarea IN ('alta','media','baja')),
    Fecha_Prog_Tarea  DATE          NULL,
    Completada_Tarea  BIT           NOT NULL DEFAULT 0,   -- [+]
    FecCreac_Tarea    DATETIME      NOT NULL DEFAULT GETDATE(),  -- [+]
    ID_Apicultor_Tar  NVARCHAR(450) NULL                  -- [+] FK → AspNetUsers
);

-- ─────────────────────────────────────────────────────────────
-- ALERTAS_COMUNITARIAS  [tabla extra, no en MER]
-- ─────────────────────────────────────────────────────────────
CREATE TABLE AlertasComunitarias (
    ID_Alerta          INT            PRIMARY KEY IDENTITY,
    Tit_Alerta         NVARCHAR(150)  NOT NULL,
    Desc_Alerta        NVARCHAR(1000) NOT NULL,
    TipoAmen_Alerta    NVARCHAR(50)   NOT NULL DEFAULT 'sanitaria',
    Lat_Alerta         DECIMAL(9,6)   NOT NULL,
    Lon_Alerta         DECIMAL(9,6)   NOT NULL,
    RadioKm_Alerta     DECIMAL(6,2)   NOT NULL DEFAULT 10,
    Ubic_Alerta        NVARCHAR(200)  NULL,
    Estado_Alerta      VARCHAR(20)    NOT NULL DEFAULT 'activa',
    FecCreac_Alerta    DATETIME       NOT NULL DEFAULT GETDATE(),
    FecResol_Alerta    DATETIME       NULL,
    ID_Apicultor_Alerta NVARCHAR(450) NULL                -- FK → AspNetUsers
);

-- ─────────────────────────────────────────────────────────────
-- NOTIFICACIONES_ALERTA  [tabla extra, no en MER]
-- ─────────────────────────────────────────────────────────────
CREATE TABLE NotificacionesAlerta (
    ID_NotifAlerta  INT           PRIMARY KEY IDENTITY,
    ID_Alerta_Notif INT           NOT NULL
                    REFERENCES AlertasComunitarias(ID_Alerta) ON DELETE CASCADE,
    ID_Apiario_Notif INT          NOT NULL
                    REFERENCES Apiarios(ID_Apiario) ON DELETE NO ACTION,
    DistKm_Notif    DECIMAL(6,2)  NOT NULL,
    FecEnv_Notif    DATETIME      NOT NULL DEFAULT GETDATE(),
    Leida_Notif     BIT           NOT NULL DEFAULT 0
);

-- ─────────────────────────────────────────────────────────────
-- AUDITORIAS  [tabla extra, no en MER]
-- ─────────────────────────────────────────────────────────────
CREATE TABLE Auditorias (
    ID_Auditoria  INT            PRIMARY KEY IDENTITY,
    ID_User_Aud   NVARCHAR(450)  NOT NULL,
    NomUser_Aud   NVARCHAR(200)  NOT NULL,
    Accion_Aud    VARCHAR(20)    NOT NULL
                  CHECK (Accion_Aud IN ('CREATE','UPDATE','DELETE')),
    Tabla_Aud     NVARCHAR(50)   NOT NULL,
    FecHor_Aud    DATETIME       NOT NULL DEFAULT GETDATE(),
    Det_Aud       NVARCHAR(MAX)  NULL
);

-- ─────────────────────────────────────────────────────────────
-- HISTORIAL_ACCESO  [tabla extra, no en MER]
-- ─────────────────────────────────────────────────────────────
CREATE TABLE HistorialesAcceso (
    ID_HisAcceso  INT            PRIMARY KEY IDENTITY,
    ID_User_HisAcc NVARCHAR(450) NOT NULL,
    NomUser_HisAcc NVARCHAR(200) NOT NULL,
    FecHor_HisAcc  DATETIME      NOT NULL DEFAULT GETDATE(),
    IP_HisAcc      VARCHAR(45)   NULL,
    Dispos_HisAcc  NVARCHAR(300) NULL,
    Exitoso_HisAcc BIT           NOT NULL DEFAULT 1
);

GO

-- =============================================================
-- ÍNDICES
-- =============================================================

CREATE INDEX idx_col_apiario       ON Colmenas(ID_Apiario_Col);
CREATE INDEX idx_col_apicultor     ON Colmenas(ID_Apicultor_Col);
CREATE INDEX idx_vis_apiario       ON Visitas(ID_Apiario_Vis);
CREATE INDEX idx_vis_fecha         ON Visitas(Fecha_Plan_Vis, Completada_V);
CREATE INDEX idx_insp_apiario      ON Inspecciones(ID_Apiario_Insp);
CREATE INDEX idx_insp_visita       ON Inspecciones(ID_Visita_Insp);
CREATE INDEX idx_insp_estado       ON Inspecciones(Estado_Insp, Fecha_Insp);
CREATE INDEX idx_evalc_insp        ON EvaluacionesColmena(ID_Inspeccion_Eval);
CREATE INDEX idx_ctrl_apiario      ON ControlesSanitarios(ID_Apiario_Control, Fecha_Ini_Trat);
CREATE INDEX idx_ctrl_estado       ON ControlesSanitarios(Estado_Control);
CREATE INDEX idx_tras_orig         ON Traslados(ID_Apiario_Orig);
CREATE INDEX idx_tras_dest         ON Traslados(ID_Apiario_Dest);
CREATE INDEX idx_tras_estado       ON Traslados(Estado_Traslado);
CREATE INDEX idx_trasc_colmena     ON TrasladosColmena(ID_Colmena);
CREATE INDEX idx_cos_apiario       ON Cosechas(ID_Apiario_Cos);
CREATE INDEX idx_cos_fecha         ON Cosechas(Fecha_Cosecha);
CREATE INDEX idx_fin_apiario       ON RegistrosFinancieros(ID_Apiario_Fin);
CREATE INDEX idx_fin_tipo          ON RegistrosFinancieros(Tipo_Finanza, Fecha_Finanza);
CREATE INDEX idx_item_nom          ON ItemsInventario(Nom_Item);
CREATE INDEX idx_mov_item          ON MovimientosInventario(ID_Item_Mov, Mov_Fecha);
CREATE INDEX idx_tar_prioridad     ON Tareas(Completada_Tarea, Prioridad_Tarea);
CREATE INDEX idx_tar_fecha         ON Tareas(Fecha_Prog_Tarea);
CREATE INDEX idx_alt_estado        ON AlertasComunitarias(Estado_Alerta, FecCreac_Alerta);
CREATE INDEX idx_notif_alerta      ON NotificacionesAlerta(ID_Alerta_Notif);
CREATE INDEX idx_aud_user          ON Auditorias(ID_User_Aud, FecHor_Aud);
CREATE INDEX idx_hisacc_user       ON HistorialesAcceso(ID_User_HisAcc, FecHor_HisAcc);

GO

-- =============================================================
-- DATOS DE PRUEBA (1 juego por tabla, en orden de dependencias)
-- =============================================================

-- ── Propietarios ─────────────────────────────────────────────
INSERT INTO Propietarios (Nom_Propietario, Tel_Propietario, Email_Propietario)
VALUES (N'Juan Rodríguez', '099-123-456', 'juan.rodriguez@gmail.com');

-- ── Apiarios (2 filas para soportar FK origen ≠ destino en Traslados) ──
INSERT INTO Apiarios (Nom_Apiario, Depto_Apiario, Dir_Apiario, Lat_Apiario, Lon_Apiario,
                      Flora_Apiario, Acceso_Apiario, Cap_Apiario, FuenteAgua_Apiario,
                      EstSem_Apiario, ID_Apicultor_Api, ID_Propietario_Api)
VALUES (N'El Sauce',    'Canelones', 'Ruta 7 km 42',  -34.520000, -55.810000,
        'Eucaliptus, Trébol', 'Camino vecinal accesible', 50, 1, 'verde', NULL, 1),
       (N'Monte Nativo', 'Lavalleja', 'Ruta 81 km 12', -34.375000, -55.237000,
        'Monte nativo', 'Solo con buen tiempo', 30, 0, 'amarillo', NULL, 1);

-- ── Colmenas ─────────────────────────────────────────────────
INSERT INTO Colmenas (Cod_Colmena, ID_Apiario_Col, Tipo_Colmena, Origen_Colmena,
                      FecIns_Colmena, EstReina_Colmena, CantAlzas_Colmena,
                      MarcosCria_Colmena, EstSem_Colmena, UltVis_Colmena,
                      Obs_Colmena, ID_Apicultor_Col)
VALUES ('C-001', 1, 'Langstroth', 'División', '2024-09-15', 'vista',
        2, 7, 'verde', '2025-06-01', N'Colmena productiva, sin novedades.', NULL);

-- ── Visitas ──────────────────────────────────────────────────
INSERT INTO Visitas (ID_Apiario_Vis, Fecha_Plan_Vis, Fecha_Real_Vis,
                     Mat_Sugeridos, Completada_V, Notas_Visita)
VALUES (1, '2025-07-10', NULL,
        N'Velo, ahumador, espátula, cuadernillo',
        'planificada',
        N'Revisar estado general y aplicar tratamiento preventivo si corresponde.');

-- ── Inspecciones ─────────────────────────────────────────────
INSERT INTO Inspecciones (ID_Apiario_Insp, ID_Colmena_Insp, ID_Visita_Insp,
                          Fecha_Insp, Clima_Insp, Temp_Insp, Notas_Insp,
                          Tipo_Insp, ColInsp_Insp, TotCol_Insp, Estado_Insp)
VALUES (1, 1, 1,
        '2025-06-01', 'Soleado', 22.5,
        N'Reina vista en cuadro 3. Postura uniforme. Sin signos de enfermedad.',
        'colmena', 1, 1, 'completa');

-- ── EvaluacionesColmena ──────────────────────────────────────
INSERT INTO EvaluacionesColmena (ID_Inspeccion_Eval, ID_Colmena_Eval,
                                  Estado_Reina_Insp, Obs_Colmena_Eval)
VALUES (1, 1, 'vista', N'Postura uniforme, población alta, sin problemas sanitarios.');

-- ── ControlesSanitarios ──────────────────────────────────────
INSERT INTO ControlesSanitarios (ID_Apiario_Control, Tipo_Control, Resultado_Control,
                                  Nivel_Infest, Tratamiento, Dosis, Notas_Control,
                                  Fecha_Ini_Trat, Fecha_Fin_Trat,
                                  ColAfect_Control, Estado_Control)
VALUES (1, 'Varroasis', 'positivo',
        'Moderado', 'Ácido oxálico', '4 ml/colmena',
        N'Tratamiento completado sin reincidencia. Recuento post-trat: 0,3%.',
        '2025-05-20', '2025-06-03',
        'C-001', 'limpio');

-- ── Traslados ────────────────────────────────────────────────
INSERT INTO Traslados (ID_Apiario_Orig, ID_Apiario_Dest, Fecha_Salida, Fecha_Retorno,
                       Distancia_Km, Motivo_Traslado, Estado_Traslado,
                       Nom_Traslado, CantCol_Traslado, PorcAv_Traslado)
VALUES (1, 2, '2025-10-01', NULL,
        142.50, N'Floración de eucaliptus en zona Lavalleja',
        'en_curso',
        N'Primavera 2025', 15, 60);

-- ── TrasladosColmena ─────────────────────────────────────────
INSERT INTO TrasladosColmena (ID_Traslado, ID_Colmena) VALUES (1, 1);

-- ── Cosechas ─────────────────────────────────────────────────
INSERT INTO Cosechas (ID_Apiario_Cos, Fecha_Cosecha, Tipo_Miel, Alzas_Cosecha,
                      Peso_Bruto, Merma, Humedad, HMF, Color_Pfund,
                      Destino_Cosecha, Notas_Cosecha, Vendida_Cosecha, PrecKg_Cosecha)
VALUES (1, '2025-06-15', 'Multifloral', 8,
        210.00, 4.50, 17.80, 12.5, '25mm',
        'Exportación', N'Excelente color ámbar, aroma floral suave.', 0, 850.0000);

-- ── RegistrosFinancieros ─────────────────────────────────────
INSERT INTO RegistrosFinancieros (Tipo_Finanza, Monto, Fecha_Finanza, Desc_Finanza,
                                   Categ_Finanza, ID_Apiario_Fin, ID_Cosecha_Fin)
VALUES ('ingreso', 178500.00, '2025-07-01',
        N'Venta cosecha multifloral junio 2025',
        'Venta miel', 1, 1);

-- ── ItemsInventario ──────────────────────────────────────────
INSERT INTO ItemsInventario (Nom_Item, Unidad_Item, Stock_Min, Stock_Act_Item, Stock_Max_Item)
VALUES (N'Ácido oxálico', 'ml', 100.00, 350.00, 1000.00);

-- ── MovimientosInventario ────────────────────────────────────
INSERT INTO MovimientosInventario (ID_Item_Mov, Mov_Tipo, Mov_Cant,
                                    Mov_Fecha, Mov_Motivo, ID_Apicultor_Mov)
VALUES (1, 'Entrada', 500.00, '2025-05-18',
        N'Compra insumos para tratamiento de Varroa', NULL);

-- ── Tareas ───────────────────────────────────────────────────
INSERT INTO Tareas (Desc_Tarea, Tipo_Tarea, Prioridad_Tarea,
                    Fecha_Prog_Tarea, Completada_Tarea, FecCreac_Tarea, ID_Apicultor_Tar)
VALUES (N'Revisión semanal colmena C-001 y registro en libreta',
        'Inspección', 'alta',
        '2025-07-10', 0, GETDATE(), NULL);

-- ── AlertasComunitarias ──────────────────────────────────────
INSERT INTO AlertasComunitarias (Tit_Alerta, Desc_Alerta, TipoAmen_Alerta,
                                  Lat_Alerta, Lon_Alerta, RadioKm_Alerta,
                                  Ubic_Alerta, Estado_Alerta,
                                  FecCreac_Alerta, FecResol_Alerta, ID_Apicultor_Alerta)
VALUES (N'Loque americana — zona Canelones Norte',
        N'Casos confirmados de Loque americana en apiarios del corredor Ruta 7. Se recomienda inspección inmediata y notificación a MGAP.',
        'sanitaria',
        -34.420000, -55.720000, 30.00,
        'Canelones Norte, Ruta 7 km 35-55',
        'activa', GETDATE(), NULL, NULL);

-- ── NotificacionesAlerta ─────────────────────────────────────
INSERT INTO NotificacionesAlerta (ID_Alerta_Notif, ID_Apiario_Notif,
                                   DistKm_Notif, FecEnv_Notif, Leida_Notif)
VALUES (1, 1, 7.30, GETDATE(), 0);

-- ── Auditorias ───────────────────────────────────────────────
INSERT INTO Auditorias (ID_User_Aud, NomUser_Aud, Accion_Aud,
                         Tabla_Aud, FecHor_Aud, Det_Aud)
VALUES ('SYSTEM', 'Sistema', 'CREATE',
        'Apiarios', GETDATE(),
        N'Seed inicial: apiario El Sauce creado.');

-- ── HistorialesAcceso ────────────────────────────────────────
INSERT INTO HistorialesAcceso (ID_User_HisAcc, NomUser_HisAcc,
                                FecHor_HisAcc, IP_HisAcc,
                                Dispos_HisAcc, Exitoso_HisAcc)
VALUES ('SYSTEM', 'Carlos Bentancur',
        GETDATE(), '192.168.1.10',
        'Chrome 125 / Windows 11', 1);

GO

-- =============================================================
-- VERIFICACIÓN RÁPIDA: conteo de filas por tabla
-- =============================================================
SELECT 'Propietarios'          AS Tabla, COUNT(*) AS Filas FROM Propietarios         UNION ALL
SELECT 'Apiarios',                        COUNT(*)          FROM Apiarios             UNION ALL
SELECT 'Colmenas',                        COUNT(*)          FROM Colmenas             UNION ALL
SELECT 'Visitas',                         COUNT(*)          FROM Visitas              UNION ALL
SELECT 'Inspecciones',                    COUNT(*)          FROM Inspecciones         UNION ALL
SELECT 'EvaluacionesColmena',             COUNT(*)          FROM EvaluacionesColmena  UNION ALL
SELECT 'ControlesSanitarios',             COUNT(*)          FROM ControlesSanitarios  UNION ALL
SELECT 'Traslados',                       COUNT(*)          FROM Traslados            UNION ALL
SELECT 'TrasladosColmena',                COUNT(*)          FROM TrasladosColmena     UNION ALL
SELECT 'Cosechas',                        COUNT(*)          FROM Cosechas             UNION ALL
SELECT 'RegistrosFinancieros',            COUNT(*)          FROM RegistrosFinancieros UNION ALL
SELECT 'ItemsInventario',                 COUNT(*)          FROM ItemsInventario      UNION ALL
SELECT 'MovimientosInventario',           COUNT(*)          FROM MovimientosInventario UNION ALL
SELECT 'Tareas',                          COUNT(*)          FROM Tareas               UNION ALL
SELECT 'AlertasComunitarias',             COUNT(*)          FROM AlertasComunitarias  UNION ALL
SELECT 'NotificacionesAlerta',            COUNT(*)          FROM NotificacionesAlerta UNION ALL
SELECT 'Auditorias',                      COUNT(*)          FROM Auditorias           UNION ALL
SELECT 'HistorialesAcceso',               COUNT(*)          FROM HistorialesAcceso;
GO
