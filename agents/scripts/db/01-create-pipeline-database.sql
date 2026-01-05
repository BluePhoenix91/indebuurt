-- ============================================================================
-- Pipeline Database Creation
-- ============================================================================
-- Creates the buurtkompas_pipeline database for tracking content generation jobs.
-- This database is separate from the main buurtkompas (GIS) database to enforce
-- read/write isolation.
--
-- Prerequisites:
--   - PostgreSQL 14+ running on localhost:5432
--   - Superuser access (postgres user)
--
-- Usage:
--   psql -U postgres -f agents/scripts/db/01-create-pipeline-database.sql
--
-- Note: Run this BEFORE 02-create-pipeline-user.sql and 03-grant-pipeline-permissions.sql
-- ============================================================================

-- Create the pipeline database
CREATE DATABASE buurtkompas_pipeline;

-- Add descriptive comment
COMMENT ON DATABASE buurtkompas_pipeline IS 'Pipeline job tracking for neighborhood content generation. Part of Epic J (Agent Pipeline).';

-- Note: No extensions needed for this database (no PostGIS, just simple tables)
-- The pipeline_jobs schema will be created in Story J1
