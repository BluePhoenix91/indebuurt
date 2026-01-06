-- ============================================================================
-- Migration: Add started_at column (Story J3)
-- ============================================================================
-- Adds the started_at column to track when job processing began.
-- Used for stale job detection (jobs in_progress for >30 minutes).
--
-- Run this ONLY if you have an existing pipeline_jobs table from before J3.
-- New installations should use the updated 04-init-pipeline-schema.sql.
--
-- Prerequisites:
--   - You must be connected to the buurtkompas_pipeline database
--   - The pipeline_jobs table must already exist
--
-- Usage:
--   psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/05-add-started-at.sql
-- ============================================================================

-- Add started_at column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'pipeline_jobs' AND column_name = 'started_at'
    ) THEN
        ALTER TABLE pipeline_jobs ADD COLUMN started_at TIMESTAMP;
        RAISE NOTICE 'Added started_at column to pipeline_jobs';
    ELSE
        RAISE NOTICE 'Column started_at already exists, skipping';
    END IF;
END $$;

-- Add partial index for stale job detection if it doesn't exist
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_stale
ON pipeline_jobs(status, started_at)
WHERE status = 'in_progress';

-- ============================================================================
-- Verification
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Migration completed successfully!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Added: started_at column for stale job detection';
    RAISE NOTICE 'Added: idx_pipeline_jobs_stale partial index';
    RAISE NOTICE '============================================';
END $$;
