/* =====================================================================
   LOCAL DEV SCHEMA  —  runs against (localdb)\MSSQLLocalDB
   ---------------------------------------------------------------------
   This is a STAND-IN for the real database that lives in the AVD.

   To make local dev match production:
     1. In the AVD, open SSMS -> right-click the real database ->
        Tasks -> Generate Scripts...
          - Choose "schema only" (NOT data)
          - Script the objects you need (tables, views, functions,
            stored procs). This is just structure — no client data.
     2. Copy that generated .sql out of the AVD.
     3. Replace the body of this file with it (keep the CREATE DATABASE
        / USE lines below, or adjust names).
     4. Run:  powershell -File tools\db-reset.ps1

   Until then, the sample "Distributors" table below matches the
   placeholder entity in src/SsamMobileApi/Data/Entities/Distributor.cs
   so the API runs end to end locally.
   ===================================================================== */

IF DB_ID('SsamMobileApiLocal') IS NULL
    CREATE DATABASE SsamMobileApiLocal;
GO

USE SsamMobileApiLocal;
GO

IF OBJECT_ID('dbo.Distributors', 'U') IS NOT NULL
    DROP TABLE dbo.Distributors;
GO

CREATE TABLE dbo.Distributors
(
    Id       INT           IDENTITY(1,1) NOT NULL CONSTRAINT PK_Distributors PRIMARY KEY,
    Name     NVARCHAR(200) NOT NULL,
    Region   NVARCHAR(100) NULL,
    IsActive BIT           NOT NULL CONSTRAINT DF_Distributors_IsActive DEFAULT (1)
);
GO
