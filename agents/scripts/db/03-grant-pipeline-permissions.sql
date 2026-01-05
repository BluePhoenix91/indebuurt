-- ============================================================================
-- Pipeline Database Permissions
-- ============================================================================
-- Grants full CRUD (Create, Read, Update, Delete) permissions to the
-- buurtkompas_pipeline user on the pipeline database.
--
-- Prerequisites:
--   - Database buurtkompas_pipeline must exist (run 01-create-pipeline-database.sql)
--   - User buurtkompas_pipeline must exist (run 02-create-pipeline-user.sql)
--   - You must be connected to the buurtkompas_pipeline database
--
-- Usage:
--   psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/03-grant-pipeline-permissions.sql
--
-- Note: The -d buurtkompas_pipeline flag is REQUIRED because DEFAULT PRIVILEGES
-- are database-specific.
-- ============================================================================

-- Schema access
GRANT USAGE ON SCHEMA public TO buurtkompas_pipeline;
GRANT CREATE ON SCHEMA public TO buurtkompas_pipeline;

-- Table permissions for existing tables (if any)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO buurtkompas_pipeline;

-- Table permissions for future tables (created by migrations)
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO buurtkompas_pipeline;

-- Sequence permissions for existing sequences (auto-increment columns)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO buurtkompas_pipeline;

-- Sequence permissions for future sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO buurtkompas_pipeline;

-- ============================================================================
-- Verification Queries (run these after setup to confirm permissions)
-- ============================================================================
--
-- 1. Test write access (as buurtkompas_pipeline user):
--    CREATE TABLE test_permissions (id SERIAL PRIMARY KEY, name TEXT);
--    INSERT INTO test_permissions (name) VALUES ('test');
--    SELECT * FROM test_permissions;
--    UPDATE test_permissions SET name = 'updated' WHERE id = 1;
--    DELETE FROM test_permissions WHERE id = 1;
--    DROP TABLE test_permissions;
--
-- 2. Verify isolation (this should FAIL):
--    psql -U buurtkompas_pipeline -d buurtkompas -c "SELECT 1"
--    Expected error: FATAL: permission denied for database "buurtkompas"
-- ============================================================================
