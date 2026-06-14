-- =====================================================================
-- schema.sql -- subset of a fund administration schema (SQL Server).
-- Canonical entity names used across the workshop:
--   Fund, ShareClass, Investor, Instrument, PriceEOD, Holding,
--   Subscription, Redemption, Valuation (NAV), FeeAccrual.
--
-- This is a deliberately small subset, sufficient for the NAV / fee module.
-- Instrument / PriceEOD / Holding are present for domain realism (lab 2);
-- the NAV-per-unit / management-fee module only needs ShareClass,
-- Subscription, Redemption and Valuation.
-- =====================================================================

IF OBJECT_ID('dbo.FeeAccrual', 'U')   IS NOT NULL DROP TABLE dbo.FeeAccrual;
IF OBJECT_ID('dbo.Valuation', 'U')    IS NOT NULL DROP TABLE dbo.Valuation;
IF OBJECT_ID('dbo.Redemption', 'U')   IS NOT NULL DROP TABLE dbo.Redemption;
IF OBJECT_ID('dbo.Subscription', 'U') IS NOT NULL DROP TABLE dbo.Subscription;
IF OBJECT_ID('dbo.Holding', 'U')      IS NOT NULL DROP TABLE dbo.Holding;
IF OBJECT_ID('dbo.PriceEOD', 'U')     IS NOT NULL DROP TABLE dbo.PriceEOD;
IF OBJECT_ID('dbo.Instrument', 'U')   IS NOT NULL DROP TABLE dbo.Instrument;
IF OBJECT_ID('dbo.Investor', 'U')     IS NOT NULL DROP TABLE dbo.Investor;
IF OBJECT_ID('dbo.ShareClass', 'U')   IS NOT NULL DROP TABLE dbo.ShareClass;
IF OBJECT_ID('dbo.Fund', 'U')         IS NOT NULL DROP TABLE dbo.Fund;
GO

CREATE TABLE dbo.Fund (
    FundId        VARCHAR(20)  NOT NULL PRIMARY KEY,
    Name          NVARCHAR(120) NOT NULL,
    BaseCurrency  CHAR(3)      NOT NULL,
    InceptionDate DATE         NOT NULL
);
GO

CREATE TABLE dbo.ShareClass (
    ShareClassId     VARCHAR(20)  NOT NULL PRIMARY KEY,
    FundId           VARCHAR(20)  NOT NULL REFERENCES dbo.Fund(FundId),
    Name             NVARCHAR(120) NOT NULL,
    Currency         CHAR(3)      NOT NULL,
    ManagementFeeBps INT          NOT NULL   -- annual management fee in basis points
);
GO

CREATE TABLE dbo.Investor (
    InvestorId  VARCHAR(20)  NOT NULL PRIMARY KEY,
    Name        NVARCHAR(120) NOT NULL,
    Email       NVARCHAR(160) NOT NULL,
    CountryCode CHAR(2)      NOT NULL
);
GO

CREATE TABLE dbo.Instrument (
    InstrumentId VARCHAR(20)  NOT NULL PRIMARY KEY,
    Isin         CHAR(12)     NOT NULL,
    Name         NVARCHAR(120) NOT NULL,
    Currency     CHAR(3)      NOT NULL,
    AssetClass   VARCHAR(20)  NOT NULL
);
GO

CREATE TABLE dbo.PriceEOD (
    InstrumentId VARCHAR(20)    NOT NULL REFERENCES dbo.Instrument(InstrumentId),
    PriceDate    DATE           NOT NULL,
    ClosePrice   DECIMAL(18, 2) NOT NULL,
    CONSTRAINT PK_PriceEOD PRIMARY KEY (InstrumentId, PriceDate)
);
GO

CREATE TABLE dbo.Holding (
    HoldingId    VARCHAR(20)    NOT NULL PRIMARY KEY,
    FundId       VARCHAR(20)    NOT NULL REFERENCES dbo.Fund(FundId),
    InstrumentId VARCHAR(20)    NOT NULL REFERENCES dbo.Instrument(InstrumentId),
    HoldingDate  DATE           NOT NULL,
    Quantity     DECIMAL(18, 4) NOT NULL
);
GO

CREATE TABLE dbo.Subscription (
    SubscriptionId VARCHAR(20)    NOT NULL PRIMARY KEY,
    ShareClassId   VARCHAR(20)    NOT NULL REFERENCES dbo.ShareClass(ShareClassId),
    InvestorId     VARCHAR(20)    NOT NULL REFERENCES dbo.Investor(InvestorId),
    TradeDate      DATE           NOT NULL,
    Units          DECIMAL(18, 4) NOT NULL,
    GrossAmount    DECIMAL(18, 2) NOT NULL
);
GO

CREATE TABLE dbo.Redemption (
    RedemptionId VARCHAR(20)    NOT NULL PRIMARY KEY,
    ShareClassId VARCHAR(20)    NOT NULL REFERENCES dbo.ShareClass(ShareClassId),
    InvestorId   VARCHAR(20)    NOT NULL REFERENCES dbo.Investor(InvestorId),
    TradeDate    DATE           NOT NULL,
    Units        DECIMAL(18, 4) NOT NULL,
    GrossAmount  DECIMAL(18, 2) NOT NULL
);
GO

-- Official daily NAV per share class (NetAssetValue is the total fund value
-- attributable to the class on AsOfDate). The module derives NavPerUnit from it.
CREATE TABLE dbo.Valuation (
    ValuationId   VARCHAR(20)    NOT NULL PRIMARY KEY,
    ShareClassId  VARCHAR(20)    NOT NULL REFERENCES dbo.ShareClass(ShareClassId),
    AsOfDate      DATE           NOT NULL,
    NetAssetValue DECIMAL(18, 2) NOT NULL,
    CONSTRAINT UQ_Valuation UNIQUE (ShareClassId, AsOfDate)
);
GO

-- Daily management fee accrual. Written by the NAV/fee module; empty on seed.
CREATE TABLE dbo.FeeAccrual (
    FeeAccrualId      VARCHAR(20)    NOT NULL PRIMARY KEY,
    ShareClassId      VARCHAR(20)    NOT NULL REFERENCES dbo.ShareClass(ShareClassId),
    AccrualDate       DATE           NOT NULL,
    NetAssetValue     DECIMAL(18, 2) NOT NULL,
    DailyManagementFee DECIMAL(18, 2) NOT NULL,
    CONSTRAINT UQ_FeeAccrual UNIQUE (ShareClassId, AccrualDate)
);
GO
