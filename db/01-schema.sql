/* =====================================================================
   LOCAL DEV SCHEMA  —  runs against (localdb)\MSSQLLocalDB
   ---------------------------------------------------------------------
   Stand-in for the real database in the AVD:
       [SIGSSAMI_OS].[MTL].[MR]
       [SIGSSAMI_OS].[MTL].[MRDetail]

   Column names, data types and nullability are replicated EXACTLY as they
   exist in production — including names that look like typos
   (e.g. OrderRecjectedBy). Do not "fix" them here; the API must stay in
   sync with the real schema.

   Not visible in the source screenshots, so assumed (confirmed with the
   team): MRId / MRDetailId are IDENTITY(1,1); FK MRDetail.MRId -> MR.MRId
   with no cascade; no DEFAULT constraints.

   Rebuild:  powershell -ExecutionPolicy Bypass -File tools\db-reset.ps1
   ===================================================================== */

IF DB_ID('SSAMMobileAppLocal') IS NULL
    CREATE DATABASE SSAMMobileAppLocal;
GO

USE SSAMMobileAppLocal;
GO

IF SCHEMA_ID('MTL') IS NULL
    EXEC('CREATE SCHEMA [MTL]');
GO

IF OBJECT_ID('MTL.MRDetail', 'U') IS NOT NULL DROP TABLE MTL.MRDetail;
IF OBJECT_ID('MTL.MR', 'U')       IS NOT NULL DROP TABLE MTL.MR;
GO

-- =====================================================================
-- MTL.MR
-- =====================================================================
CREATE TABLE MTL.MR
(
    MRId                        INT             IDENTITY(1,1) NOT NULL,
    TenantId                    INT             NOT NULL,
    RequestType                 NVARCHAR(50)    NOT NULL,
    MonthYear                   DATE            NOT NULL,
    CustomerId                  INT             NOT NULL,
    LocationId                  INT             NOT NULL,
    RefId                       NVARCHAR(50)    NULL,
    RefDate                     DATE            NULL,
    Notes                       NVARCHAR(MAX)   NULL,
    Field1                      NVARCHAR(MAX)   NULL,
    Field2                      NVARCHAR(MAX)   NULL,
    Field3                      NVARCHAR(MAX)   NULL,
    Field4                      NVARCHAR(MAX)   NULL,
    Field5                      NVARCHAR(MAX)   NULL,
    Field6                      NVARCHAR(MAX)   NULL,
    Field7                      NVARCHAR(MAX)   NULL,
    Field8                      NVARCHAR(MAX)   NULL,
    Status                      INT             NOT NULL,
    RequestedBy                 INT             NOT NULL,
    RequestedOn                 DATETIME        NOT NULL,
    CreatedBy                   INT             NOT NULL,
    CreatedOn                   DATETIME        NOT NULL,
    UpdatedBy                   INT             NOT NULL,
    UpdatedOn                   DATETIME        NOT NULL,
    ACKStatus                   VARCHAR(20)     NULL,
    OrderCreatedBy              VARCHAR(1000)   NULL,
    OrderCreatedDate            DATETIME        NULL,
    OrderSubmittedBy            VARCHAR(1000)   NULL,
    OrderSubmittedOn            DATETIME        NULL,
    OrderRecjectedBy            VARCHAR(1000)   NULL,
    OrderRejectedDate           DATETIME        NULL,
    ShipToId                    BIGINT          NULL,
    IsOnlineOrder               BIT             NULL,
    DistRemarks                 VARCHAR(1000)   NULL,
    InvLocId                    INT             NULL,
    Dist_MSP_Approval_Status    VARCHAR(20)     NULL,
    SalesUserRemarks            VARCHAR(1000)   NULL,
    FreightTerms                VARCHAR(100)    NULL,
    CONSTRAINT PK_MR PRIMARY KEY CLUSTERED (MRId)
);
GO

-- =====================================================================
-- MTL.MRDetail
-- =====================================================================
CREATE TABLE MTL.MRDetail
(
    MRDetailId                      INT             IDENTITY(1,1) NOT NULL,
    MRId                            INT             NOT NULL,
    RequestType                     NVARCHAR(50)    NOT NULL,
    MonthYear                       DATE            NOT NULL,
    IsComputed                      BIT             NOT NULL,
    CustomerId                      INT             NOT NULL,
    LocationId                      INT             NOT NULL,
    RefId                           NVARCHAR(50)    NULL,
    RefDate                         DATE            NULL,
    Product                         NVARCHAR(1000)  NOT NULL,
    ItemId                          INT             NOT NULL,
    InvLocId                        INT             NULL,
    AQ1                             DECIMAL(10,2)   NULL,
    AQ2                             DECIMAL(10,2)   NULL,
    AQ3                             DECIMAL(10,2)   NULL,
    Q1                              DECIMAL(10,2)   NULL,
    Q2                              DECIMAL(10,2)   NULL,
    Q3                              DECIMAL(10,2)   NULL,
    TotalQty                        DECIMAL(10,2)   NULL,
    STP                             DECIMAL(10,2)   NULL,
    DelDate                         DATE            NULL,
    Remarks                         NVARCHAR(MAX)   NULL,
    Instructions                    NVARCHAR(MAX)   NULL,
    Length                          DECIMAL(10,2)   NULL,
    Quantity                        DECIMAL(10,2)   NULL,
    L1                              DECIMAL(10,2)   NULL,
    L2                              DECIMAL(10,2)   NULL,
    L3                              DECIMAL(10,2)   NULL,
    IsBoard                         BIT             NOT NULL,
    Field1                          NVARCHAR(MAX)   NULL,
    Field2                          NVARCHAR(MAX)   NULL,
    Field3                          NVARCHAR(MAX)   NULL,
    Field4                          NVARCHAR(MAX)   NULL,
    Field5                          NVARCHAR(MAX)   NULL,
    Field6                          NVARCHAR(MAX)   NULL,
    Field7                          NVARCHAR(MAX)   NULL,
    Field8                          NVARCHAR(MAX)   NULL,
    STAG_HDR_ID                     INT             NULL,
    STAG_LINE_ID                    INT             NULL,
    Status                          INT             NOT NULL,
    RequestedBy                     INT             NOT NULL,
    RequestedOn                     DATETIME        NOT NULL,
    CreatedBy                       INT             NOT NULL,
    CreatedOn                       DATETIME        NOT NULL,
    UpdatedBy                       INT             NOT NULL,
    UpdatedOn                       DATETIME        NOT NULL,
    ACKStatus                       VARCHAR(20)     NULL,
    ModStatus                       VARCHAR(20)     NULL,
    ModUpdatedBy                    VARCHAR(20)     NULL,
    ModUpdatedDate                  DATETIME        NULL,
    OrderCreatedBy                  VARCHAR(1000)   NULL,
    OrderCreatedDate                DATETIME        NULL,
    OrderSubmittedBy                VARCHAR(1000)   NULL,
    OrderSubmittedOn                DATETIME        NULL,
    OrderRecjectedBy                VARCHAR(1000)   NULL,
    OrderRejectedDate               DATETIME        NULL,
    ShipToId                        BIGINT          NULL,
    IsOnlineOrder                   BIT             NULL,
    HSNCode                         VARCHAR(50)     NULL,
    STPFromOS                       DECIMAL(10,2)   NULL,
    Price                           DECIMAL(10,2)   NULL,
    PriceOnOS                       DECIMAL(10,2)   NULL,
    SalesUserRemarks                VARCHAR(1000)   NULL,
    AcceptedPrice                   DECIMAL(20,2)   NULL,
    MSPPrice                        DECIMAL(20,2)   NULL,
    Distributor_Accepted_Remarks    VARCHAR(300)    NULL,
    MSP_Accepted_Remarks            VARCHAR(300)    NULL,
    ProcessedVia                    VARCHAR(50)     NULL,
    CONSTRAINT PK_MRDetail PRIMARY KEY CLUSTERED (MRDetailId),
    CONSTRAINT FK_MRDetail_MR FOREIGN KEY (MRId) REFERENCES MTL.MR (MRId)
);
GO

CREATE INDEX IX_MRDetail_MRId ON MTL.MRDetail (MRId);
GO
