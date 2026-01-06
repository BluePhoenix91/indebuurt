-- ============================================================================
-- Pipeline Jobs Schema (Story J1)
-- ============================================================================
-- Creates the pipeline_jobs table for tracking neighborhood content generation
-- progress across Claude Code sessions.
--
-- Prerequisites:
--   - Database buurtkompas_pipeline must exist (run 01-create-pipeline-database.sql)
--   - User buurtkompas_pipeline must exist (run 02-create-pipeline-user.sql)
--   - Permissions must be granted (run 03-grant-pipeline-permissions.sql)
--   - You must be connected to the buurtkompas_pipeline database
--
-- Usage:
--   psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/04-init-pipeline-schema.sql
--
-- Design decisions:
--   - Uses nis_code (7-char) as primary identifier instead of neighborhood slugs
--   - municipality_nis (5-char) derived from nis_code for city-level filtering
--   - On-demand job creation (no bulk seeding)
--   - Database trigger for automatic updated_at timestamps
--   - Configuration (paths, thresholds) lives in code, not database
-- ============================================================================

-- ============================================================================
-- Table: pipeline_jobs
-- ============================================================================
-- Tracks the progress of each neighborhood through the content generation
-- pipeline (researcher -> writer -> seo_reviewer -> brand_reviewer).
-- ============================================================================

CREATE TABLE IF NOT EXISTS pipeline_jobs (
    -- Identity
    id SERIAL PRIMARY KEY,
    nis_code VARCHAR(7) NOT NULL UNIQUE,           -- e.g., '41002A0' (neighborhood NIS)
    municipality_nis VARCHAR(5) NOT NULL,          -- e.g., '41002' (derived, for city filtering)

    -- Status tracking
    status VARCHAR(20) NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'in_progress', 'completed', 'failed')),
    current_stage VARCHAR(20)
        CHECK (current_stage IS NULL OR current_stage IN ('researcher', 'writer', 'seo_reviewer', 'brand_reviewer')),

    -- Stage completion timestamps
    researcher_completed_at TIMESTAMP,
    writer_completed_at TIMESTAMP,
    seo_reviewer_completed_at TIMESTAMP,
    brand_reviewer_completed_at TIMESTAMP,

    -- Quality scores (0-100, nullable until reviewed)
    seo_score DECIMAL(5,2) CHECK (seo_score IS NULL OR (seo_score >= 0 AND seo_score <= 100)),
    brand_score DECIMAL(5,2) CHECK (brand_score IS NULL OR (brand_score >= 0 AND brand_score <= 100)),
    final_score DECIMAL(5,2) CHECK (final_score IS NULL OR (final_score >= 0 AND final_score <= 100)),

    -- Error handling
    error_message TEXT,
    retry_count INTEGER NOT NULL DEFAULT 0,
    last_error_at TIMESTAMP,

    -- Audit timestamps
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    started_at TIMESTAMP,                          -- When processing began (for stale job detection)
    completed_at TIMESTAMP,

    -- Constraint: municipality_nis must match first 5 chars of nis_code
    CONSTRAINT chk_municipality_nis CHECK (municipality_nis = LEFT(nis_code, 5))
);

COMMENT ON TABLE pipeline_jobs IS 'Tracks neighborhood content generation progress through the 4-agent pipeline';
COMMENT ON COLUMN pipeline_jobs.nis_code IS '7-character Belgian NIS code identifying the neighborhood (wijk)';
COMMENT ON COLUMN pipeline_jobs.municipality_nis IS '5-character municipality NIS prefix for city-level filtering';
COMMENT ON COLUMN pipeline_jobs.status IS 'Job status: pending, in_progress, completed, or failed';
COMMENT ON COLUMN pipeline_jobs.current_stage IS 'Current pipeline stage: researcher, writer, seo_reviewer, or brand_reviewer';
COMMENT ON COLUMN pipeline_jobs.final_score IS 'Average of seo_score and brand_score';

-- ============================================================================
-- Trigger: Auto-update updated_at
-- ============================================================================
-- Automatically sets updated_at to NOW() on any row update.
-- ============================================================================

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS update_pipeline_jobs_updated_at ON pipeline_jobs;
CREATE TRIGGER update_pipeline_jobs_updated_at
    BEFORE UPDATE ON pipeline_jobs
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- Indexes
-- ============================================================================
-- Optimized for common query patterns in the /pipeline slash command.
-- ============================================================================

-- Primary query patterns
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_status ON pipeline_jobs(status);
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_nis_code ON pipeline_jobs(nis_code);
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_municipality ON pipeline_jobs(municipality_nis);

-- Composite for /pipeline municipality <nis5> queries
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_municipality_status ON pipeline_jobs(municipality_nis, status);

-- For retry-failed queries (partial index)
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_failed_retry ON pipeline_jobs(status, retry_count)
    WHERE status = 'failed';

-- For stale job detection (jobs in_progress for too long)
CREATE INDEX IF NOT EXISTS idx_pipeline_jobs_stale ON pipeline_jobs(status, started_at)
    WHERE status = 'in_progress';

-- ============================================================================
-- Verification
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Pipeline schema created successfully!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Table: pipeline_jobs (19 columns)';
    RAISE NOTICE 'Trigger: update_pipeline_jobs_updated_at';
    RAISE NOTICE 'Indexes: 6 indexes for query optimization';
    RAISE NOTICE '============================================';
END $$;
