-- =============================================================================
-- AquaScale: WEBS Cross-Database Views
-- =============================================================================
-- ⚠ DO NOT RUN THIS YET — this is for a future setup step.
--
-- Run this ONLY after:
--   1. The AquaScale database is created (CREATE DATABASE AquaScale)
--   2. EF Core migrations have been applied (dotnet ef database update)
--   3. The WEBS database is accessible on the same SQL Server instance
--
-- What this does:
--   Creates 7 read-only views inside the AquaScale database that forward
--   queries to the WEBS database. AquaScale.Api reads WEBS data through
--   these views — it never queries WEBS directly and never writes to it.
--
-- Safe to re-run (uses CREATE OR ALTER VIEW).
-- Replace 'WEBS' below with your actual WEBS database name if it differs.
--   (Check with: SELECT name FROM sys.databases)
-- =============================================================================

USE AquaScale;
GO

-- ── T_Account_Meter ───────────────────────────────────────────────────────
-- Physical meter master. PK: ID (uniqueidentifier).
-- Schema confirmed via INFORMATION_SCHEMA.COLUMNS against live WEBS, 2026-08-06.
-- Columns excluded (audit-only, not needed by AquaScale): Createdby, DateCreated, Editedby, DateEdited.
CREATE OR ALTER VIEW dbo.T_Account_Meter AS
    SELECT
        ID,
        RTRIM(AccountNo)    AS AccountNo,   -- nchar(10) NOT NULL
        MeterNo,                            -- nvarchar(15) NOT NULL
        DateInstalled,                      -- datetime NULL
        DepAmt,                             -- money NULL
        DepDate,                            -- datetime NULL
        RTRIM(MeterStatus)  AS MeterStatus, -- nchar(2) NULL — M_GenCodes Group 41
        StatusDate,                         -- datetime NULL
        Remarks,                            -- nvarchar(500) NULL
        Notes                               -- nvarchar(100) NULL
    FROM WEBS.dbo.T_Account_Meter;
GO

-- ── T_Billing_Account ─────────────────────────────────────────────────────
-- Billing account master. PK: AccountNo (nchar 10).
-- Schema confirmed via INFORMATION_SCHEMA.COLUMNS against live WEBS, 2026-08-06.
-- BillType: W = Water, E = Electric. AcctStatus is an M_GenCodes code.
CREATE OR ALTER VIEW dbo.T_Billing_Account AS
    SELECT
        RTRIM(AccountNo)        AS AccountNo,       -- nchar(10) NOT NULL
        DateReg,                                    -- datetime NOT NULL
        Entity_ID,                                  -- nvarchar(10) NOT NULL
        AccountName,                                -- varchar(50) NOT NULL
        Project_ID,                                 -- char(10) NULL
        RTRIM(ReservationNo)    AS ReservationNo,   -- nchar(8) NULL
        BillType,                                   -- varchar(2) NOT NULL
        ClassID,                                    -- varchar(2) NOT NULL
        RTRIM(AcctStatus)       AS AcctStatus,      -- nchar(2) NOT NULL
        Movein                                      -- datetime NULL
    FROM WEBS.dbo.T_Billing_Account;
GO

-- ── T_Consumption ─────────────────────────────────────────────────────────
-- Posted meter reading records. PK: ID (uniqueidentifier).
-- AcctMtr_ID links to T_Account_Meter.ID.
-- ReadingValidationService uses CurRead as the baseline for new captures.
CREATE OR ALTER VIEW dbo.T_Consumption AS
    SELECT
        ID,
        AcctMtr_ID,
        SeqNo,
        DateRead,
        DueDate,
        PrevRead,       -- float, NOT NULL in WEBS
        CurRead,        -- float, NOT NULL in WEBS
        UsedPerMtr,     -- float
        RatePerMtr,     -- money
        CurCharge,      -- money
        Remarks
    FROM WEBS.dbo.T_Consumption;
GO

-- ── T_Payment ─────────────────────────────────────────────────────────────
-- Payment transaction history per account-meter.
-- AcctMtr_ID links to T_Account_Meter.ID. ORDate is the OR date.
CREATE OR ALTER VIEW dbo.T_Payment AS
    SELECT
        ID,
        AcctMtr_ID,
        SeqNo,
        ORDate,
        DueDate,
        ORNo,
        CurCharge,
        Penalty,
        TotAmtDue,
        AmtPaid,
        Balance,
        PrevBalance,
        Remarks
    FROM WEBS.dbo.T_Payment;
GO

-- ── T_PM_Reservation ──────────────────────────────────────────────────────
-- Property ownership history. PK: ReservationNo (char 8).
-- Schema confirmed via INFORMATION_SCHEMA.COLUMNS against live WEBS, 2026-08-06.
-- AquaScale only needs 5 of the ~80 columns — others are real-estate financials.
-- BackoutType IS NULL = reservation is active (buyer currently owns the lot).
-- CompPBL matches Property.CompPbl in AquaScale (varchar 40 NOT NULL).
-- Buyer_ID (char 8 NOT NULL) links to M_Buyer.Buyer_ID.
CREATE OR ALTER VIEW dbo.T_PM_Reservation AS
    SELECT
        ReservationNo,              -- char(8) NOT NULL PK
        CompPBL,                    -- varchar(40) NOT NULL
        RTRIM(Buyer_ID) AS Buyer_ID, -- char(8) NOT NULL
        DateReserved,               -- smalldatetime NULL
        BackoutType                 -- char(2) NULL — NULL = active
    FROM WEBS.dbo.T_PM_Reservation;
GO

-- ── M_Buyer ───────────────────────────────────────────────────────────────
-- Buyer personal info master. PK: Buyer_ID (char 8).
-- Schema confirmed via INFORMATION_SCHEMA.COLUMNS against live WEBS, 2026-08-06.
-- M_Buyer has ~80 columns (biographical, employment, spouse info, etc.).
-- AquaScale only needs name fields. BuyerName is nullable in WEBS (nvarchar 200).
-- Profile.BuyerRef stores Buyer_ID for buyer-role AquaScale accounts.
CREATE OR ALTER VIEW dbo.M_Buyer AS
    SELECT
        RTRIM(Buyer_ID) AS Buyer_ID, -- char(8) NOT NULL PK
        BuyerName,                   -- nvarchar(200) NULL
        FirstName,                   -- nvarchar(100) NOT NULL
        LastName                     -- nvarchar(100) NOT NULL
    FROM WEBS.dbo.M_Buyer;
GO

-- ── M_Buyer_Contact ───────────────────────────────────────────────────────
-- Buyer contact info (mobile, email). Multiple rows per buyer (update history).
-- Composite PK: Buyer_ID + DateUpdated.
-- BuyerContactService always queries ORDER BY DateUpdated DESC.
CREATE OR ALTER VIEW dbo.M_Buyer_Contact AS
    SELECT
        RTRIM(Buyer_ID) AS Buyer_ID,    -- char(8)
        DateUpdated,
        MobileNo,
        Email
    FROM WEBS.dbo.M_Buyer_Contact;
GO

-- =============================================================================
-- After running this script, the following endpoints will start working
-- with live WEBS data:
--   GET  /api/customers          (reads T_Account_Meter, T_PM_Reservation, M_Buyer)
--   GET  /api/customers/{id}/ledger  (reads T_Consumption, T_Payment)
--   POST /dev/seed-buyer         (reads M_Buyer)
--   OCR validation               (reads T_Consumption for prev reading baseline)
-- =============================================================================
