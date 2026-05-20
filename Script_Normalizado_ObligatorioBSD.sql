-- ============================================================
-- BASE DE DATOS: ObligatorioBSD (Esquema Normalizado a 3FN)
-- ============================================================

CREATE DATABASE ObligatorioBSD;
GO
USE ObligatorioBSD;
GO

-- ============================================================
-- ENTIDADES INDEPENDIENTES (Sin dependencias externas)
-- ============================================================

CREATE TABLE Ciudad (
    Id_Ciudad   INTEGER         PRIMARY KEY,
    Nom_Ciudad  VARCHAR(30)     NOT NULL
);

CREATE TABLE Clientes (
    Id_Clie     INTEGER         PRIMARY KEY,
    Nom_C       VARCHAR(30)     NOT NULL,
    Ape_C       VARCHAR(30)     NOT NULL,
    Tel_C       NUMERIC(12)     NOT NULL
);

CREATE TABLE Profesional (
    Id_P            INTEGER         PRIMARY KEY,
    Nom_P           VARCHAR(30)     NOT NULL,
    Especialidad    VARCHAR(30)     NOT NULL,
    Disponibilidad  VARCHAR(30)
);

CREATE TABLE Servicios (
    Id_S    INTEGER         PRIMARY KEY,
    Nom_S   VARCHAR(20)     NOT NULL,
    Desc_S  VARCHAR(50)
);

CREATE TABLE Marca (
    Id_Marca    INTEGER         PRIMARY KEY,
    Nom_Marca   VARCHAR(30)     NOT NULL,
    Tipo        VARCHAR(30)     NOT NULL
);

CREATE TABLE Productos (
    Id_Prod         INTEGER         PRIMARY KEY,
    Nom_Prod        VARCHAR(50)     NOT NULL,
    Precio_Unitario DECIMAL(10,2),
    Id_Marca        INTEGER         NOT NULL REFERENCES Marca(Id_Marca)
);

CREATE TABLE Ayudante (
    Id_Ayu          INTEGER         PRIMARY KEY,
    Nom_Ayu         VARCHAR(30)     NOT NULL,
    Ape_Ayu         VARCHAR(50)     NOT NULL,
    Categoria_Ayu   VARCHAR(30)
);

CREATE TABLE Promocion (
    Id_Prom         INTEGER         PRIMARY KEY,
    Porcentaje_Desc DECIMAL(5,2),
    Fecha_Inicio    DATE,
    Fecha_Fin       DATE
);

-- ============================================================
-- ENTIDADES CON DEPENDENCIAS (Claves foráneas 1:N)
-- ============================================================

CREATE TABLE Salon (
    Id_Salon    INTEGER         PRIMARY KEY,
    Dir_Salon   VARCHAR(50)     NOT NULL,
    Tel_Salon   NUMERIC(12)     NOT NULL,
    Nom_Salon   VARCHAR(30)     NOT NULL
);

CREATE TABLE Turno (
    Id_T    INTEGER         PRIMARY KEY,
    Fecha   DATE            NOT NULL,
    Hora    TIME            NOT NULL,
    Estado  VARCHAR(30)     NOT NULL,
    Id_Clie INTEGER         NOT NULL REFERENCES Clientes(Id_Clie),
    Id_P    INTEGER         NOT NULL REFERENCES Profesional(Id_P),
    Id_Ayu  INTEGER                  REFERENCES Ayudante(Id_Ayu)
);

CREATE TABLE Stock (
    Id_Stock    INTEGER     PRIMARY KEY,
    Cantidad    INTEGER     NOT NULL,
    Fecha_Venc  DATE        NOT NULL,
    Id_Prod     INTEGER     NOT NULL REFERENCES Productos(Id_Prod),
    Id_Salon    INTEGER     NOT NULL REFERENCES Salon(Id_Salon)
);

CREATE TABLE Pago (
    Id_Pago     INTEGER         PRIMARY KEY,
    Fecha_Pago  DATE            NOT NULL,
    Monto       DECIMAL(10,2),
    Metodo      VARCHAR(30),
    Id_Clie     INTEGER         NOT NULL REFERENCES Clientes(Id_Clie),
    Id_Prom     INTEGER                  REFERENCES Promocion(Id_Prom)
);

CREATE TABLE Valoracion (
    Id_V        INTEGER     PRIMARY KEY,
    Puntuacion  INTEGER     CHECK (Puntuacion BETWEEN 1 AND 5),
    Comentario  VARCHAR(200),
    Id_Clie     INTEGER     NOT NULL REFERENCES Clientes(Id_Clie)
);

-- ============================================================
-- TABLAS DE RELACIÓN (Relaciones N:M)
-- ============================================================

CREATE TABLE Turno_Servicio (
    Id_T    INTEGER NOT NULL REFERENCES Turno(Id_T),
    Id_S    INTEGER NOT NULL REFERENCES Servicios(Id_S),
    PRIMARY KEY (Id_T, Id_S)
);

CREATE TABLE Salon_Ciudad (
    Id_Salon    INTEGER NOT NULL REFERENCES Salon(Id_Salon),
    Id_Ciudad   INTEGER NOT NULL REFERENCES Ciudad(Id_Ciudad),
    PRIMARY KEY (Id_Salon, Id_Ciudad)
);

CREATE TABLE Salon_Profesional (
    Id_Salon    INTEGER NOT NULL REFERENCES Salon(Id_Salon),
    Id_P        INTEGER NOT NULL REFERENCES Profesional(Id_P),
    PRIMARY KEY (Id_Salon, Id_P)
);

CREATE TABLE Producto_Cliente (
    Id_Prod INTEGER NOT NULL REFERENCES Productos(Id_Prod),
    Id_Clie INTEGER NOT NULL REFERENCES Clientes(Id_Clie),
    PRIMARY KEY (Id_Prod, Id_Clie)
);

CREATE TABLE Productos_Servicios_Prof (
    Id_Prod INTEGER NOT NULL REFERENCES Productos(Id_Prod),
    Id_S    INTEGER NOT NULL REFERENCES Servicios(Id_S),
    Id_P    INTEGER NOT NULL REFERENCES Profesional(Id_P),
    PRIMARY KEY (Id_Prod, Id_S, Id_P)
);

CREATE TABLE Cliente_Profesional (
    Id_Clie INTEGER NOT NULL REFERENCES Clientes(Id_Clie),
    Id_P    INTEGER NOT NULL REFERENCES Profesional(Id_P),
    PRIMARY KEY (Id_Clie, Id_P)
);
