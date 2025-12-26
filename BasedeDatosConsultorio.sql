USE master;
GO

-- ============================================================
-- CREAR BASE DE DATOS
-- ============================================================
CREATE DATABASE Consultorio
COLLATE Modern_Spanish_CI_AI;
GO

USE Consultorio;
GO

-- ============================================================
-- TABLA ROL
-- ============================================================
CREATE TABLE ROL (
    Id_Rol TINYINT IDENTITY(1,1) PRIMARY KEY,
    Descripcion_Rol VARCHAR(30) NOT NULL UNIQUE
);
INSERT INTO ROL (Descripcion_Rol)
VALUES ('Administrador'), ('Medico'), ('Secretario');
GO

-- ============================================================
-- SECUENCIA GLOBAL PARA Id_Usuario
-- ============================================================
CREATE SEQUENCE dbo.Seq_IdUsuario
    AS INT
    START WITH 1
    INCREMENT BY 1;
GO

-- ============================================================
-- TABLA USUARIOS (solo roles internos)
-- ============================================================
CREATE TABLE USUARIOS (
    Id_Usuario CHAR(8) PRIMARY KEY, -- generado por trigger
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL CONSTRAINT UQ_USUARIOS_Email UNIQUE,
    Cedula VARCHAR(30) NOT NULL CONSTRAINT UQ_USUARIOS_Cedula UNIQUE,
    Telefono VARCHAR(30) NULL,
    Contrasena VARCHAR(128) NOT NULL, -- hash (SHA2_256 HEX)
    Id_Rol TINYINT NOT NULL,
    Fecha_Registro DATETIME NOT NULL CONSTRAINT DF_Usuarios_Fecha DEFAULT GETDATE(),
	PedirContraseña BIT DEFAULT 1,
    CONSTRAINT FK_Usuarios_Rol FOREIGN KEY (Id_Rol) REFERENCES ROL(Id_Rol),
    CONSTRAINT CHK_EmailFormatoBasico CHECK (Email LIKE '_%@_%._%' AND Email NOT LIKE '% %')
);
GO

-- ============================================================
-- TABLA ACTIVIDAD_USUARIOS
-- ============================================================
CREATE TABLE ACTIVIDAD_USUARIOS (
    Id_Usuario CHAR(8) PRIMARY KEY,
    Activo BIT NOT NULL DEFAULT 1,
    Bloqueado BIT NOT NULL DEFAULT 0,
    Intentos_Fallidos INT NOT NULL DEFAULT 0,
    Fecha_Bloqueo DATETIME NULL,
    Ultima_Actividad DATETIME NULL,
    CONSTRAINT FK_Actividad_Usuarios FOREIGN KEY (Id_Usuario)
        REFERENCES USUARIOS(Id_Usuario)
        ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLAS COMPLEMENTARIAS
-- ============================================================
CREATE TABLE ESPECIALIDADES(
    ID_Especialidad INT IDENTITY PRIMARY KEY,
    Nombre_Especialidad VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200) NULL
);
GO

CREATE TABLE TIPO_CONTRATO(
    ID_Contrato INT IDENTITY PRIMARY KEY,
    Descripcion VARCHAR(30) NOT NULL UNIQUE
);
INSERT INTO TIPO_CONTRATO(Descripcion) VALUES ('eventual'), ('permanente');
GO

CREATE TABLE ESTADO_CITA(
    ID_Estado_Cita INT IDENTITY PRIMARY KEY,
    Descripcion VARCHAR(30) NOT NULL UNIQUE
);
INSERT INTO ESTADO_CITA VALUES ('agendada'), ('atendida'), ('cancelada');
GO

-- ============================================================
-- TABLA PACIENTES (independiente de USUARIOS)
-- ============================================================
CREATE TABLE PACIENTES(
    ID_Paciente INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Cedula VARCHAR(30) NOT NULL UNIQUE,
    Email NVARCHAR(255) NULL,
    Telefono VARCHAR(30) NULL,
    Fecha_Nacimiento DATE NOT NULL,
    Sexo VARCHAR(10) NULL,
    Direccion VARCHAR(200) NULL,
    ContactoEmergencia VARCHAR(100) NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- TABLA MEDICOS
-- ============================================================
CREATE TABLE MEDICOS(
    ID_Medico INT IDENTITY PRIMARY KEY,
    Id_Usuario CHAR(8) NOT NULL UNIQUE,
    ID_Especialidad INT NOT NULL,
    ID_Contrato INT NOT NULL,
    Horario_Atencion VARCHAR(200) NULL,
    Telefono_Consulta VARCHAR(30) NULL,
    CONSTRAINT FK_MEDICOS_USUARIOS FOREIGN KEY (Id_Usuario)
        REFERENCES USUARIOS(Id_Usuario)
        ON DELETE CASCADE,
    CONSTRAINT FK_MEDICOS_ESPECIALIDAD FOREIGN KEY (ID_Especialidad) REFERENCES ESPECIALIDADES(ID_Especialidad),
    CONSTRAINT FK_MEDICOS_CONTRATO FOREIGN KEY (ID_Contrato) REFERENCES TIPO_CONTRATO(ID_Contrato)
);
GO

-- ============================================================
-- CITAS
-- ============================================================
CREATE TABLE CITAS(
    ID_Cita INT IDENTITY PRIMARY KEY,
    ID_Paciente INT NOT NULL,
    ID_Medico INT NOT NULL,
    Fecha_Cita DATE NOT NULL,
    Hora_Cita TIME NOT NULL,
    ID_Estado_Cita INT NOT NULL,
    CONSTRAINT FK_CITAS_PACIENTE FOREIGN KEY (ID_Paciente) REFERENCES PACIENTES(ID_Paciente),
    CONSTRAINT FK_CITAS_MEDICO FOREIGN KEY (ID_Medico) REFERENCES MEDICOS(ID_Medico),
    CONSTRAINT FK_CITAS_ESTADO FOREIGN KEY (ID_Estado_Cita) REFERENCES ESTADO_CITA(ID_Estado_Cita)
);
GO

-- ============================================================
-- Atención médica
-- ============================================================
CREATE TABLE ATENCION_MEDICA(
    ID_Atencion INT IDENTITY PRIMARY KEY,
    ID_Cita INT NOT NULL,
    Fecha_Atencion DATETIME NOT NULL DEFAULT GETDATE(),
    Motivo_Consulta VARCHAR(300) NOT NULL,
    Diagnostico VARCHAR(300) NULL,
    Observaciones VARCHAR(400) NULL,
    CONSTRAINT FK_ATENCION_CITA FOREIGN KEY (ID_Cita)
        REFERENCES CITAS(ID_Cita)
);
GO


CREATE TABLE ANTECEDENTES_MEDICOS(
    ID_Antecedente INT IDENTITY PRIMARY KEY,
    ID_Paciente INT NOT NULL UNIQUE,
    Alergias VARCHAR(200) NULL,
    Enfermedades_Cronicas VARCHAR(200) NULL,
    Observaciones_Generales VARCHAR(300) NULL,
    Fecha_Registro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ANTECEDENTES_PACIENTE FOREIGN KEY (ID_Paciente)
        REFERENCES PACIENTES(ID_Paciente)
);
GO

-- ============================================================
-- TRIGGER: Insert usuario → genera Id_Usuario (sin pacientes)
-- ============================================================
CREATE OR ALTER TRIGGER dbo.TRG_Insert_Usuario_Clinica
ON dbo.USUARIOS
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tmp TABLE (
        Nombre NVARCHAR(100),
        Apellido NVARCHAR(100),
        Email NVARCHAR(255),
        Cedula VARCHAR(30),
        Telefono VARCHAR(30),
        Contrasena VARCHAR(128),
        Id_Rol TINYINT,
        Id_Usuario CHAR(8)
    );

    -- Recorremos cada fila insertada
    INSERT INTO @tmp (Nombre, Apellido, Email, Cedula, Telefono, Contrasena, Id_Rol, Id_Usuario)
    SELECT 
        i.Nombre,
        i.Apellido,
        i.Email,
        i.Cedula,
        i.Telefono,
        i.Contrasena,
        i.Id_Rol,

        -- 1️⃣ Primero generamos un número de secuencia (fuera del CASE)
        (CASE i.Id_Rol
            WHEN 1 THEN 'A'
            WHEN 2 THEN 'M'
            WHEN 3 THEN 'S'
            ELSE 'U'
        END)
        + RIGHT('0000000' + CAST( NEXT VALUE FOR dbo.Seq_IdUsuario AS VARCHAR(7) ), 7)

    FROM inserted i;

    -- 2️⃣ Insertamos en la tabla final
    INSERT INTO dbo.USUARIOS (Nombre, Apellido, Email, Cedula, Telefono, Contrasena, Id_Rol, Id_Usuario)
    SELECT Nombre, Apellido, Email, Cedula, Telefono, Contrasena, Id_Rol, Id_Usuario
    FROM @tmp;

	INSERT INTO dbo.ACTIVIDAD_USUARIOS (Id_Usuario, Activo, Bloqueado, Intentos_Fallidos, Ultima_Actividad)
    SELECT Id_Usuario, 1, 0, 0, GETDATE()
    FROM @tmp;

END;
GO


--Llenado de tablas

USE Consultorio;
GO

-- ========================================
-- INSERTAR ESPECIALIDADES
-- ========================================
INSERT INTO ESPECIALIDADES (Nombre_Especialidad, Descripcion)
VALUES 
('Medicina General', 'Atención médica general de adultos y niños'),
('Pediatría', 'Atención de niños y adolescentes'),
('Cardiología', 'Diagnóstico y tratamiento de enfermedades del corazón'),
('Dermatología', 'Enfermedades de la piel, cabello y uñas'),
('Ginecología', 'Salud femenina y controles obstétricos'),
('Neurología', 'Enfermedades del sistema nervioso'),
('Ortopedia', 'Lesiones y enfermedades de huesos y articulaciones'),
('Oftalmología', 'Salud ocular y visión'),
('Psiquiatría', 'Salud mental y emocional'),
('Endocrinología', 'Sistema hormonal y metabolismo');
GO

-- ========================================
-- INSERTAR USUARIOS (roles internos)
-- ========================================

INSERT INTO USUARIOS (Nombre, Apellido, Email, Cedula, Telefono, Contrasena, Id_Rol)
VALUES
(N'Ana', N'González', 'ana.gonzalez@mail.com', '8-123-4567', '+507 6001 0020', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1),
(N'Luis', N'Martínez', 'luis.martinez@mail.com', '4-567-8910', '+507 6002 0030', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2),
(N'Carla', N'Ramírez', 'carla.ramirez@mail.com', '3-445-6677', '+507 6003 0040', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 3),
(N'Jorge', N'López', 'jorge.lopez@mail.com', '2-112-3344', '+507 6004 0050', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2),
(N'Sofía', N'Pérez', 'sofia.perez@mail.com', '9-988-7766', '+507 6005 0060', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2),
(N'Mario', N'Díaz', 'mario.diaz@mail.com', '1-233-4455', '+507 6006 0070', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2),
(N'Laura', N'Méndez', 'laura.mendez@mail.com', '6-334-5566', '+507 6007 0080', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 3),
(N'Pedro', N'Vega', 'pedro.vega@mail.com', '7-445-6677', '+507 6008 0090', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 2),
(N'Elena', N'Torres', 'elena.torres@mail.com', '5-556-7788', '+507 6009 0100', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 3),
(N'Carlos', N'Santos', 'carlos.santos@mail.com', '8-677-8899', '+507 6010 0200', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1);
GO


-- ========================================
-- PACIENTES
-- ========================================
INSERT INTO PACIENTES (Nombre, Apellido, Cedula, Email, Telefono, Fecha_Nacimiento, Sexo, Direccion, ContactoEmergencia)
VALUES
('Pedro', 'Santos', '8-112-233', 'pedro.santos@mail.com', '+507 6101 0020', '1985-02-15', 'Masculino', 'Calle 1, Ciudad de Panamá', 'Maria Santos'),
('Lucia', 'Vega', '4-223-344', 'lucia.vega@mail.com', '+507 6102 0030', '1990-06-30', 'Femenino', 'Calle 2, Ciudad de Panamá', 'Jose Vega'),
('Mario', 'Torres', '3-334-455', 'mario.torres@mail.com', '+507 6103 0040', '1978-11-20', 'Masculino', 'Calle 3, Ciudad de Panamá', 'Ana Torres'),
('Elena', 'Diaz', '2-445-566', 'elena.diaz@mail.com', '+507 6104 0050', '2000-03-12', 'Femenino', 'Calle 4, Ciudad de Panamá', 'Luis Diaz'),
('Carlos', 'Mendez', '1-556-677', 'carlos.mendez@mail.com', '+507 6105 0060', '1995-08-25', 'Masculino', 'Calle 5, Ciudad de Panamá', 'Sofia Mendez'),
('Laura', 'Pérez', '6-667-788', 'laura.perez@mail.com', '+507 6106 0070', '1988-01-05', 'Femenino', 'Calle 6, Ciudad de Panamá', 'Pedro Pérez'),
('Diego', 'Santos', '7-778-899', 'diego.santos@mail.com', '+507 6107 0080', '1975-09-12', 'Masculino', 'Calle 7, Ciudad de Panamá', 'Ana Santos'),
('Mariana', 'Gomez', '5-889-900', 'mariana.gomez@mail.com', '+507 6108 0090', '1992-07-20', 'Femenino', 'Calle 8, Ciudad de Panamá', 'Luis Gomez'),
('Fernando', 'Rojas', '8-990-011', 'fernando.rojas@mail.com', '+507 6109 0100', '1980-12-10', 'Masculino', 'Calle 9, Ciudad de Panamá', 'Carla Rojas'),
('Valeria', 'Molina', '9-101-213', 'valeria.molina@mail.com', '+507 6110 0200', '1997-05-15', 'Femenino', 'Calle 10, Ciudad de Panamá', 'Juan Molina');
GO


-- ========================================
-- MEDICOS (IDs correctos según trigger)
-- ========================================
-- Médicos: Luis, Jorge, Sofia, Mario, Pedro (todos rol 2)
INSERT INTO MEDICOS (Id_Usuario, ID_Especialidad, ID_Contrato, Horario_Atencion, Telefono_Consulta)
VALUES
('M0000002', 1, 2, 'Lunes-Viernes 08:00-16:00', '600111222'), -- Luis
('M0000004', 2, 1, 'Lunes-Viernes 09:00-17:00', '600222333'), -- Jorge
('M0000005', 3, 2, 'Martes-Jueves 10:00-18:00', '600333444'), -- Sofia
('M0000006', 4, 2, 'Lunes-Miércoles 08:00-14:00', '600444555'), -- Mario
('M0000008', 5, 1, 'Martes-Viernes 12:00-20:00', '600555666'); -- Pedro
GO

-- ========================================
-- CITAS (15 citas variadas)
-- ========================================
INSERT INTO CITAS (ID_Paciente, ID_Medico, Fecha_Cita, Hora_Cita, ID_Estado_Cita)
VALUES
(1, 1, '2025-12-01', '09:00', 1),
(2, 2, '2025-12-02', '10:30', 1),
(3, 3, '2025-12-03', '11:00', 1),
(4, 4, '2025-12-04', '08:30', 1),
(5, 5, '2025-12-05', '14:00', 1),
(6, 1, '2025-12-06', '09:30', 1),
(7, 2, '2025-12-07', '10:00', 1),
(8, 3, '2025-12-08', '11:30', 1),
(9, 4, '2025-12-09', '08:45', 1),
(10, 5, '2025-12-10', '15:00', 1),
(1, 2, '2025-12-11', '12:00', 1),
(3, 4, '2025-12-12', '09:15', 1),
(5, 1, '2025-12-13', '13:00', 1),
(7, 3, '2025-12-14', '10:30', 1),
(9, 5, '2025-12-15', '11:45', 1);
GO

-- ========================================
-- ATENCION_MEDICA (15 registros)
-- ========================================
INSERT INTO ATENCION_MEDICA (ID_Cita, Motivo_Consulta, Diagnostico, Observaciones)
VALUES
(1, 'Dolor de cabeza', 'Migraña', 'Paciente con historial previo'),
(2, 'Fiebre y tos', 'Gripe común', 'Recetar medicamentos'),
(3, 'Revisión general', 'Sano', 'Sin observaciones'),
(4, 'Erupción en piel', 'Dermatitis', 'Recomendar crema tópica'),
(5, 'Control ginecológico', 'Normal', 'Todo dentro de parámetros'),
(6, 'Dolor lumbar', 'Lumbalgia', 'Ejercicios recomendados'),
(7, 'Tos persistente', 'Bronquitis', 'Antibióticos'),
(8, 'Chequeo pediátrico', 'Saludable', 'Vacunas al día'),
(9, 'Dolor en articulaciones', 'Artritis', 'Control mensual'),
(10, 'Chequeo general', 'Normal', 'Paciente sano'),
(11, 'Dolor abdominal', 'Gastritis', 'Recomendar dieta'),
(12, 'Revisión neurológica', 'Normal', 'Sin hallazgos'),
(13, 'Control cardiológico', 'Hipertensión leve', 'Revisar medicación'),
(14, 'Revisión dermatológica', 'Acné', 'Tratamiento tópico'),
(15, 'Control ginecológico', 'Normal', 'Sin observaciones');
GO

-- ========================================
-- ANTECEDENTES_MEDICOS
-- ========================================
INSERT INTO ANTECEDENTES_MEDICOS (ID_Paciente, Alergias, Enfermedades_Cronicas, Observaciones_Generales)
VALUES
(1, 'Ninguna', 'Hipertensión', 'Control cada 6 meses'),
(2, 'Penicilina', 'Asma', 'Evitar alérgenos'),
(3, 'Polen', 'Diabetes', 'Monitoreo diario'),
(4, 'Ninguna', 'Ninguna', 'Paciente saludable'),
(5, 'Ninguna', 'Anemia', 'Control mensual'),
(6, 'Polvo', 'Asma', 'Inhalador disponible'),
(7, 'Mariscos', 'Hipotiroidismo', 'Dieta especial'),
(8, 'Penicilina', 'Diabetes', 'Control endocrino'),
(9, 'Ninguna', 'Hipertensión', 'Control trimestral'),
(10, 'Ninguna', 'Ninguna', 'Paciente sano');
GO