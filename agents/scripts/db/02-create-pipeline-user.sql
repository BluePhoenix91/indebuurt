-- ============================================================================
-- Pipeline Database User Creation
-- ============================================================================
-- Creates the buurtkompas_pipeline user with connection rights to the pipeline
-- database only. This user has NO access to the main buurtkompas (GIS) database.
--
-- Prerequisites:
--   - PostgreSQL 14+ running on localhost:5432
--   - Superuser access (postgres user)
--   - Database buurtkompas_pipeline must exist (run 01-create-pipeline-database.sql first)
--
-- Usage:
--   psql -U postgres -f agents/scripts/db/02-create-pipeline-user.sql
--
-- Security Note:
--   The password 'pipeline_local_dev' is for local development only.
--   For production, use a strong password and store it securely.
-- ============================================================================

-- Create the pipeline user
-- Password matches what's configured in .mcp.json for the pipeline server
CREATE USER buurtkompas_pipeline WITH PASSWORD 'pipeline_local_dev';

-- Grant connection rights to the pipeline database only
GRANT CONNECT ON DATABASE buurtkompas_pipeline TO buurtkompas_pipeline;

-- Explicitly do NOT grant any access to buurtkompas (GIS) database
-- This ensures data isolation between the two databases

-- Add descriptive comment
COMMENT ON ROLE buurtkompas_pipeline IS 'Read-write user for pipeline job tracking. No access to GIS data.';
