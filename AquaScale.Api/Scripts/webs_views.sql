-- =============================================================================
-- AquaScale: WEBS Cross-Database Views
-- =============================================================================
-- Run this script ONCE against the AquaScale database (not the WEBS database).
-- These views forward queries from AquaScaleDbContext to the WEBS database on
-- the same SQL Server instance, keeping the two databases fully separate.
--
-- Prerequisites:
--   1. Both AquaScale and WEBS databases exist on the same SQL Server instance.
--   2. The SQL Server login used by AquaScale.Api has SELECT permission on the
--      relevant WEBS tables (GRANT SELECT ON WEBS.dbo.T_Account_Meter TO [user]).
--
-- To re-run after a WEBS schema change, just execute this script again.
-- CREATE OR ALTER is used so it is safe to re-run without dropping views first.
-- =============================================================================

-- IMPORTANT: Replace 'WEBS' below with the exact database name on your server
-- if it differs (check with: SELECT name FROM sys.databases).

-- ── T_Account_Meter ────────────────────────────────────────────────────────
-- Account-meter master record. Primary key: ID (uniqueidentifier).
-- AccountNo is nchar(10) — RTRIM applied here to strip trailing spaces.
CREATE OR ALTER VIEW dbo.T_Account_Meter AS
    SELECT
        ID,
        RTRIM(AccountNo)  AS AccountNo,
        MeterNo,
        DateInstalled,
        DepAmt,
        DepDate,
        MeterStatus,
        StatusDate,
        Remarks,
        Notes,
        WithIssue,
        Hold
    FROM WEBS.dbo.T_Account_Meter;
GO

-- ── T_Billing_Account ──────────────────────────────────────────────────────
-- Billing account master per utility account. Primary key: AccountNo (nchar 10).
-- AccountNo and ReservationNo are nchar — RTRIM applied.
CREATE OR ALTER VIEW dbo.T_Billing_Account AS
    SELECT
        RTRIM(AccountNo)      AS AccountNo,
        DateReg,
        Entity_ID,
        AccountName,
        Project_ID,
        RTRIM(ReservationNo)  AS ReservationNo,
        BillType,
        ClassID,
        RTRIM(AcctStatus)     AS AcctStatus,
        Movein
    FROM WEBS.dbo.T_Billing_Account;
GO

-- ── T_Consumption ──────────────────────────────────────────────────────────
-- Posted meter reading with computed billing amounts. PK: ID (uniqueidentifier).
-- AcctMtr_ID is FK to T_Account_Meter.ID.
CREATE OR ALTER VIEW dbo.T_Consumption AS
    SELECT
        ID,
        AcctMtr_ID,
        SeqNo,
        DateRead,
        DueDate,
        PrevRead,
        CurRead,
        UsedPerMtr,
        RatePerMtr,
        CurCharge,
        Remarks
    FROM WEBS.dbo.T_Consumption;
GO

-- ── T_Payment ─────────────────────────────────────────────────────────────
-- Payment transactions per account-meter. PK: ID (uniqueidentifier).
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
-- Buyer ownership history per property. PK: ReservationNo (char 8).
-- BackoutType IS NULL means the reservation is active (buyer still owns lot).
-- Buyer_ID is char(8) — RTRIM applied.
CREATE OR ALTER VIEW dbo.T_PM_Reservation AS
    SELECT
        ReservationNo,
        CompPBL,
        RTRIM(Buyer_ID)   AS Buyer_ID,
        DateReserved,
        BackoutType
    FROM WEBS.dbo.T_PM_Reservation;
GO

-- ── M_Buyer ───────────────────────────────────────────────────────────────
-- Buyer personal information master. PK: Buyer_ID (char 8, RTRIM applied).
CREATE OR ALTER VIEW dbo.M_Buyer AS
    SELECT
        RTRIM(Buyer_ID)   AS Buyer_ID,
        BuyerName,
        FirstName,
        LastName
    FROM WEBS.dbo.M_Buyer;
GO

-- ── M_Buyer_Contact ───────────────────────────────────────────────────────
-- Buyer contact info (mobile, email). Composite PK: Buyer_ID + DateUpdated.
-- Multiple rows per buyer — callers always ORDER BY DateUpdated DESC.
-- Buyer_ID is char(8) — RTRIM applied.
CREATE OR ALTER VIEW dbo.M_Buyer_Contact AS
    SELECT
        RTRIM(Buyer_ID)   AS Buyer_ID,
        DateUpdated,
        MobileNo,
        Email
    FROM WEBS.dbo.M_Buyer_Contact;
GO

-- =============================================================================
-- Grant SELECT on all views to the AquaScale.Api database user.
-- Replace 'AquaScaleUser' with the actual SQL Server login name.
-- =============================================================================
-- GRANT SELECT ON dbo.T_Account_Meter   TO [AquaScaleUser];
-- GRANT SELECT ON dbo.T_Billing_Account TO [AquaScaleUser];
-- GRANT SELECT ON dbo.T_Consumption     TO [AquaScaleUser];
-- GRANT SELECT ON dbo.T_Payment         TO [AquaScaleUser];
-- GRANT SELECT ON dbo.T_PM_Reservation  TO [AquaScaleUser];
-- GRANT SELECT ON dbo.M_Buyer           TO [AquaScaleUser];
-- GRANT SELECT ON dbo.M_Buyer_Contact   TO [AquaScaleUser];
