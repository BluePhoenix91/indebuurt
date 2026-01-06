-- ============================================================================
-- Migration: Add publish tracking columns (Story J5)
-- ============================================================================
-- Adds columns to track whether content has been published to the Astro
-- content collection directory (src/content/neighborhoods/).
--
-- Prerequisites:
--   - You must be connected to the buurtkompas_pipeline database
--   - The pipeline_jobs table must already exist (04-init-pipeline-schema.sql)
--
-- Usage:
--   psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/06-add-publish-tracking.sql
--
-- Design decisions:
--   - `published` boolean tracks whether content has been copied to content dir
--   - `published_at` timestamp records when publishing occurred
--   - Partial index optimizes queries for unpublished completed content
--   - Idempotent: safe to run multiple times
-- ============================================================================

-- Add published column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'pipeline_jobs' AND column_name = 'published'
    ) THEN
        ALTER TABLE pipeline_jobs ADD COLUMN published BOOLEAN NOT NULL DEFAULT FALSE;
        RAISE NOTICE 'Added published column to pipeline_jobs';
    ELSE
        RAISE NOTICE 'Column published already exists, skipping';
    END IF;
END $$;

-- Add published_at column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'pipeline_jobs' AND column_name = 'published_at'
    ) THEN
        ALTER TABLE pipeline_jobs ADD COLUMN published_at TIMESTAMP;
        RAISE NOTICE 'Added published_at column to pipeline_jobs';
    ELSE
        RAISE NOTICE 'Column published_at already exists, skipping';
    END IF;
END $$;

-- Partial index for querying unpublished completed content
-- Optimizes: SELECT * FROM pipeline_jobs WHERE status = 'completed' AND published = FALSE
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_unpublished
ON pipeline_jobs(status, published)
WHERE status = 'completed' AND published = FALSE;

-- ============================================================================
-- Verification
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Migration completed successfully!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Added: published (BOOLEAN, default FALSE)';
    RAISE NOTICE 'Added: published_at (TIMESTAMP)';
    RAISE NOTICE 'Added: idx_pipeline_jobs_unpublished partial index';
    RAISE NOTICE '============================================';
END $$;
