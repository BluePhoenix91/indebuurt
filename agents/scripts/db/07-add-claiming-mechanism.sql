-- ============================================================================
-- Migration: Add claiming mechanism for parallel processing (Story K1)
-- ============================================================================
-- Enables multiple Claude Code sessions to process neighborhoods in parallel
-- without conflicts. Uses municipality-level claiming with row-level heartbeat.
--
-- Prerequisites:
--   - You must be connected to the buurtkompas_pipeline database
--   - The pipeline_jobs table must already exist (04-init-pipeline-schema.sql)
--
-- Usage:
--   psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/07-add-claiming-mechanism.sql
--
-- Design decisions:
--   - Municipality-level claims prevent conflicts (327 municipalities vs 2,800 neighborhoods)
--   - Row-level heartbeat enables stale claim detection (30 min timeout)
--   - Session names allow operator visibility into who is processing what
--   - Automatic recovery: stale claims released on next claim operation
--   - Idempotent: safe to run multiple times
-- ============================================================================

-- ============================================================================
-- Add claiming columns to pipeline_jobs
-- ============================================================================

-- Add claimed_by column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'pipeline_jobs' AND column_name = 'claimed_by'
    ) THEN
        ALTER TABLE pipeline_jobs ADD COLUMN claimed_by VARCHAR(50);
        RAISE NOTICE 'Added claimed_by column to pipeline_jobs';
    ELSE
        RAISE NOTICE 'Column claimed_by already exists, skipping';
    END IF;
END $$;

COMMENT ON COLUMN pipeline_jobs.claimed_by IS 'Session identifier that claimed this job (e.g., terminal-1, alice-laptop)';

-- Add heartbeat_at column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'pipeline_jobs' AND column_name = 'heartbeat_at'
    ) THEN
        ALTER TABLE pipeline_jobs ADD COLUMN heartbeat_at TIMESTAMP;
        RAISE NOTICE 'Added heartbeat_at column to pipeline_jobs';
    ELSE
        RAISE NOTICE 'Column heartbeat_at already exists, skipping';
    END IF;
END $$;

COMMENT ON COLUMN pipeline_jobs.heartbeat_at IS 'Last heartbeat from claiming session; used for stale detection (30 min timeout)';

-- Index for efficient claim queries
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_claimed
ON pipeline_jobs(claimed_by, status)
WHERE claimed_by IS NOT NULL;

-- ============================================================================
-- Table: pipeline_claims
-- ============================================================================
-- Tracks municipality-level claims for parallel processing coordination.
-- Each session claims entire municipalities to reduce contention.
-- ============================================================================

CREATE TABLE IF NOT EXISTS pipeline_claims (
    -- Identity
    municipality_nis VARCHAR(5) PRIMARY KEY,  -- e.g., '44021' (Gent)

    -- Session tracking
    claimed_by VARCHAR(50) NOT NULL,          -- Session name (e.g., 'terminal-1')
    claimed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    heartbeat_at TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Progress tracking
    neighborhoods_total INTEGER NOT NULL,     -- Total neighborhoods in municipality
    neighborhoods_completed INTEGER NOT NULL DEFAULT 0,

    -- Constraint: municipality_nis must be 5 digits
    CONSTRAINT chk_municipality_nis_format CHECK (municipality_nis ~ '^[0-9]{5}$')
);

COMMENT ON TABLE pipeline_claims IS 'Tracks which session has claimed each municipality for parallel processing';
COMMENT ON COLUMN pipeline_claims.municipality_nis IS '5-character Belgian municipality NIS code';
COMMENT ON COLUMN pipeline_claims.claimed_by IS 'Session identifier that claimed this municipality';
COMMENT ON COLUMN pipeline_claims.heartbeat_at IS 'Last heartbeat; claims older than 30 min are considered stale';
COMMENT ON COLUMN pipeline_claims.neighborhoods_total IS 'Total neighborhoods in this municipality (for progress display)';
COMMENT ON COLUMN pipeline_claims.neighborhoods_completed IS 'Number of neighborhoods completed in this claim session';

-- Indexes for pipeline_claims
CREATE INDEX IF NOT EXISTS idx_pipeline_claims_session ON pipeline_claims(claimed_by);
CREATE INDEX IF NOT EXISTS idx_pipeline_claims_stale ON pipeline_claims(heartbeat_at);

-- ============================================================================
-- Verification
-- ============================================================================

DO $$
DECLARE
    jobs_cols INTEGER;
    claims_exists BOOLEAN;
BEGIN
    -- Count new columns in pipeline_jobs
    SELECT COUNT(*) INTO jobs_cols
    FROM information_schema.columns
    WHERE table_name = 'pipeline_jobs'
    AND column_name IN ('claimed_by', 'heartbeat_at');

    -- Check if pipeline_claims exists
    SELECT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'pipeline_claims'
    ) INTO claims_exists;

    RAISE NOTICE '============================================';
    RAISE NOTICE 'Migration completed successfully!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'pipeline_jobs: % claiming columns present', jobs_cols;
    RAISE NOTICE 'pipeline_claims table: %', CASE WHEN claims_exists THEN 'created' ELSE 'ERROR' END;
    RAISE NOTICE '============================================';
    RAISE NOTICE 'New columns on pipeline_jobs:';
    RAISE NOTICE '  - claimed_by (VARCHAR(50))';
    RAISE NOTICE '  - heartbeat_at (TIMESTAMP)';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'New table pipeline_claims:';
    RAISE NOTICE '  - municipality_nis (PK, VARCHAR(5))';
    RAISE NOTICE '  - claimed_by, claimed_at, heartbeat_at';
    RAISE NOTICE '  - neighborhoods_total, neighborhoods_completed';
    RAISE NOTICE '============================================';
END $$;
