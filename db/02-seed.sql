/* Fake data for local development only. Never real client data. */
USE SSAMMobileAppLocal;
GO

DELETE FROM MTL.MRDetail;
DELETE FROM MTL.MR;
DBCC CHECKIDENT ('MTL.MRDetail', RESEED, 0);
DBCC CHECKIDENT ('MTL.MR', RESEED, 0);
GO

SET IDENTITY_INSERT MTL.MR ON;
INSERT INTO MTL.MR
    (MRId, TenantId, RequestType, MonthYear, CustomerId, LocationId, RefId, RefDate, Notes,
     Status, RequestedBy, RequestedOn, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, ACKStatus)
VALUES
    (1, 1, N'Monthly', '2026-09-01', 1001, 5001, N'MR-2026-0001', '2026-08-28', N'September indent',
     1, 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), N'ACK'),
    (2, 1, N'AdHoc',   '2026-09-01', 1002, 5002, N'MR-2026-0002', NULL, NULL,
     0, 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), NULL);
SET IDENTITY_INSERT MTL.MR OFF;
GO

SET IDENTITY_INSERT MTL.MRDetail ON;
INSERT INTO MTL.MRDetail
    (MRDetailId, MRId, RequestType, MonthYear, IsComputed, CustomerId, LocationId, Product, ItemId,
     Q1, Q2, Q3, TotalQty, STP, IsBoard, Status, RequestedBy, RequestedOn, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
VALUES
    (1, 1, N'Monthly', '2026-09-01', 0, 1001, 5001, N'Steel Strap 19mm', 9001,
     100.00, 120.00, 90.00, 310.00, 55.25, 0, 1, 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME()),
    (2, 1, N'Monthly', '2026-09-01', 1, 1001, 5001, N'Poly Strap 12mm', 9002,
     200.00, 180.00, 210.00, 590.00, 12.10, 0, 1, 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME()),
    (3, 2, N'AdHoc',   '2026-09-01', 0, 1002, 5002, N'Edge Board 50mm',  9003,
     NULL, NULL, NULL, 75.00, NULL, 1, 0, 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME(), 42, SYSUTCDATETIME());
SET IDENTITY_INSERT MTL.MRDetail OFF;
GO
