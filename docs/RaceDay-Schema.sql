/*
RaceDay Database Schema
Part 1: System Planning and Database

This script creates the RaceDay database and all six tables from the ERD
It also seeds the database with sample data so the schema can be tested right away
Run this on a clean SQL Server instance using SSMS, the whole script can be run in one go
*/

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
    UserId              INT IDENTITY(1,1) NOT NULL,
    RoleId              INT NOT NULL,
    FirstName           VARCHAR(100) NOT NULL,
    LastName            VARCHAR(100) NOT NULL,
    Email               VARCHAR(150) NOT NULL,
    PasswordHash        VARCHAR(256) NOT NULL,
    PhoneNumber         VARCHAR(20) NULL,
    ProfilePictureUrl   VARCHAR(300) NULL,
    CreatedAt           DATETIME NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()) FOR CreatedAt,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%_@__%.__%')
);
GO

-- Events table, each event is owned by one Organiser
CREATE TABLE Events (
    EventId         INT IDENTITY(1,1) NOT NULL,
    OrganiserId     INT NOT NULL,
    Name            VARCHAR(150) NOT NULL,
    Description     VARCHAR(1000) NULL,
    EventDate       DATETIME NOT NULL,
    Location        VARCHAR(200) NOT NULL,
    DistanceKm      DECIMAL(6,2) NULL,
    EventType       VARCHAR(20) NOT NULL,
    BannerImageUrl  VARCHAR(300) NULL,
    CreatedAt       DATETIME NOT NULL,
    CONSTRAINT PK_Events PRIMARY KEY (EventId),
    CONSTRAINT DF_Events_CreatedAt DEFAULT (GETDATE()) FOR CreatedAt,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run','Walk','Cycle')),
    CONSTRAINT CK_Events_DistanceKm CHECK (DistanceKm IS NULL OR DistanceKm > 0)
);
GO

-- Categories table, age or distance categories that belong to a single event
CREATE TABLE Categories (
    CategoryId      INT IDENTITY(1,1) NOT NULL,
    EventId         INT NOT NULL,
    Name            VARCHAR(100) NOT NULL,
    Description     VARCHAR(300) NULL,
    MinAge          INT NULL,
    MaxAge          INT NULL,
    DistanceKm      DECIMAL(6,2) NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT CK_Categories_AgeRange CHECK (MinAge IS NULL OR MaxAge IS NULL OR MinAge <= MaxAge),
    CONSTRAINT CK_Categories_DistanceKm CHECK (DistanceKm IS NULL OR DistanceKm > 0)
);
GO