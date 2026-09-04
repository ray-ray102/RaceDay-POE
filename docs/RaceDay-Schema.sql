/*
RaceDay Database Schema
Part 1: System Planning and Database

This script creates the RaceDay database and all six tables from the ERD
It also seeds the database with sample data so the schema can be tested right away
Run this on a clean SQL Server instance using SSMS, the whole script can be run in one go
*/

-- Switch to master first, you cannot drop a database while your own
-- session is connected to it, even in single-user mode
USE master;
GO

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

PRINT 'Database created, building tables...';
GO

-- TABLES

-- Roles lookup table, holds the two roles used throughout the system
CREATE TABLE Roles (
    RoleId      INT IDENTITY(1,1) NOT NULL,
    RoleName    VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

-- Users table, stores both Organisers and Participants, role is set at registration
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) NOT NULL,
    RoleId INT NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    PasswordHash VARCHAR(256) NOT NULL,
    PhoneNumber  VARCHAR(20) NULL,
    ProfilePictureUrl VARCHAR(300) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%_@__%.__%')
);
GO

-- Events table, each event is owned by one Organiser
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Description VARCHAR(1000) NULL,
    EventDate DATETIME NOT NULL,
    Location VARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(6,2) NULL,
    EventType VARCHAR(20) NOT NULL,
    BannerImageUrl VARCHAR(300) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Events PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run','Walk','Cycle')),
    CONSTRAINT CK_Events_DistanceKm CHECK (DistanceKm IS NULL OR DistanceKm > 0)
);
GO

-- Categories table, age or distance categories that belong to a single event
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(300) NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    DistanceKm DECIMAL(6,2) NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT CK_Categories_AgeRange CHECK (MinAge IS NULL OR MaxAge IS NULL OR MinAge <= MaxAge),
    CONSTRAINT CK_Categories_DistanceKm CHECK (DistanceKm IS NULL OR DistanceKm > 0)
);
GO

-- Enrolments table, links a Participant to an event and the category they chose
CREATE TABLE Enrolments (
    EnrolmentId     INT IDENTITY(1,1) NOT NULL,
    ParticipantId   INT NOT NULL,
    EventId         INT NOT NULL,
    CategoryId      INT NOT NULL,
    EnrolmentDate   DATETIME NOT NULL CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT (GETDATE()),
    Status          VARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT ('Pending'),
    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentId),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_ParticipantEvent UNIQUE (ParticipantId, EventId),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending','Confirmed','Cancelled'))
);
GO

-- Results table, one result per enrolment, captured by the Organiser after the event
CREATE TABLE Results (
    ResultsId       INT IDENTITY(1,1) NOT NULL,
    EnrolmentId     INT NOT NULL,
    FinishTime      TIME NOT NULL,
    FinishPosition  INT NOT NULL,
    CONSTRAINT PK_Results PRIMARY KEY (ResultsId),
    CONSTRAINT UQ_Results_EnrolmentId UNIQUE (EnrolmentId),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE,
    CONSTRAINT CK_Results_FinishPosition CHECK (FinishPosition > 0)
);
GO

PRINT 'Tables created, adding indexes...';
GO

-- INDEXES
-- Speeds up the lookups the API will run most often, such as fetching
-- all events for an organiser, all enrolments for a participant, or
-- upcoming events sorted by date

CREATE INDEX IX_Users_RoleId ON Users(RoleId);
CREATE INDEX IX_Events_OrganiserId ON Events(OrganiserId);
CREATE INDEX IX_Events_EventDate ON Events(EventDate);
CREATE INDEX IX_Categories_EventId ON Categories(EventId);
CREATE INDEX IX_Enrolments_EventId ON Enrolments(EventId);
CREATE INDEX IX_Enrolments_ParticipantId ON Enrolments(ParticipantId);
CREATE INDEX IX_Enrolments_CategoryId ON Enrolments(CategoryId);
CREATE INDEX IX_Enrolments_Status ON Enrolments(Status);
GO

PRINT 'Indexes created, seeding sample data...';
GO

-- SEED DATA
-- Covers every table, 2 Organisers, 2 Participants, 3 Events,
-- categories for each event, sample enrolments and sample results

-- Roles
INSERT INTO Roles (RoleName) VALUES
('Organiser'),
('Participant');
GO

-- Users, two Organisers and two Participants
-- Password hashes below are placeholders, the real API will hash passwords properly in Part 2
INSERT INTO Users (RoleId, FirstName, LastName, Email, PasswordHash, PhoneNumber, ProfilePictureUrl, CreatedAt)
VALUES
(1, 'Thabo', 'Nkosi', 'thabo.nkosi@raceday.co.za', 'AQAAAAIAAYagAAAAEHashPlaceholder1==', '0821234567', NULL, GETDATE()),
(1, 'Amanda', 'Botha', 'amanda.botha@raceday.co.za', 'AQAAAAIAAYagAAAAEHashPlaceholder2==', '0837654321', NULL, GETDATE()),
(2, 'Sipho', 'Dlamini', 'sipho.dlamini@gmail.com', 'AQAAAAIAAYagAAAAEHashPlaceholder3==', '0721112222', NULL, GETDATE()),
(2, 'Lerato', 'Matlala', 'lerato.matlala@gmail.com', 'AQAAAAIAAYagAAAAEHashPlaceholder4==', '0739998888', NULL, GETDATE());
GO

-- Events, three events owned by the two Organisers, one of each event type
INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, DistanceKm, EventType, BannerImageUrl, CreatedAt)
VALUES
(1, 'Joburg City Run', 'A morning road run through the streets of Johannesburg with routes for every level', '2026-10-04 06:00:00', 'Sandton, Johannesburg', 10.0, 'Run', NULL, GETDATE()),
(1, 'Cape Winelands Cycle Challenge', 'A scenic cycling event through the vineyards outside Stellenbosch', '2026-11-15 06:30:00', 'Stellenbosch, Western Cape', 60.0, 'Cycle', NULL, GETDATE()),
(2, 'Durban Beachfront Park Walk', 'A family friendly walk along the Durban beachfront in support of local charities', '2026-09-20 07:00:00', 'North Beach, Durban', 5.0, 'Walk', NULL, GETDATE());
GO

-- Categories, at least two per event, six in total
INSERT INTO Categories (EventId, Name, Description, MinAge, MaxAge, DistanceKm)
VALUES
(1, '10km Open', 'Open category for the full 10km route', 16, NULL, 10.0),
(1, '5km Fun Run', 'Shorter route for beginners and families', NULL, NULL, 5.0),
(2, '60km Challenge', 'Full distance for experienced cyclists', 18, NULL, 60.0),
(2, '30km Social Ride', 'Shorter, more relaxed distance', 14, NULL, 30.0),
(3, 'Under 20', 'Youth category', NULL, 19, 5.0),
(3, 'Senior', 'Participants aged 50 and older', 50, NULL, 5.0);
GO

-- Enrolments, sample enrolments linking Participants, events and categories
INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, EnrolmentDate, Status)
VALUES
(3, 1, 1, GETDATE(), 'Confirmed'),
(4, 1, 2, GETDATE(), 'Confirmed'),
(3, 2, 3, GETDATE(), 'Pending'),
(4, 3, 5, GETDATE(), 'Confirmed');
GO

-- Results, sample finish times for the enrolments that have already taken place
-- only confirmed enrolments that finished get a result, this matches the real workflow
INSERT INTO Results (EnrolmentId, FinishTime, FinishPosition)
VALUES
(1, '00:52:18', 47),
(2, '00:31:05', 12);
GO

PRINT 'Seed data inserted, schema build complete.';
GO

-- VERIFICATION
-- !!Run this on its own after the script finishes to confirm every table has rows

SELECT 'Roles' AS TableName, COUNT(*) AS TotalRows FROM Roles
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM Results;
GO