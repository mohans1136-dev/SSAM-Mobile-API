/* Fake data for local development only. Never real client data. */
USE SsamMobileApiLocal;
GO

TRUNCATE TABLE dbo.Distributors;   -- clears rows and resets IDENTITY to its seed
GO

INSERT INTO dbo.Distributors (Name, Region, IsActive) VALUES
    (N'Acme Connectors',        N'APAC',   1),
    (N'Northwind Fasteners',    N'EMEA',   1),
    (N'Contoso Industrial',     N'NA',     1),
    (N'Globex Distribution',    N'LATAM',  0),
    (N'Initech Supply Co',      N'APAC',   1);
GO
