-- ============================================================================
-- Normalize Neighborhood Names from ALL CAPS to Dutch Title Case
--
-- Story L2: Agent Fine-Tuning Epic
--
-- Problem: Statbel provides neighborhood names in ALL CAPS (e.g., "DAMPOORT").
-- Solution: Create a function to normalize to proper Dutch title case.
--
-- Dutch-specific rules:
-- 1. Basic title case: DAMPOORT → Dampoort
-- 2. Hyphenated words: SINT-NIKLAAS → Sint-Niklaas
-- 3. Spaced compounds: AALST - STATION → Aalst - Station
-- 4. Dutch prefixes: 'S HERTOGENDIJK → 's Hertogendijk (keep 's, 't lowercase)
-- 5. Roman numerals: INDUSTRIEGEBIED I → Industriegebied I (keep uppercase)
-- 6. Abbreviations: STW. → Stw., STR. → Str.
-- 7. Parentheses: ALBERTPARK (OOSTWIJK) → Albertpark (Oostwijk)
-- ============================================================================

-- ============================================================================
-- STEP 1: Create the normalization function
-- ============================================================================

CREATE OR REPLACE FUNCTION normalize_dutch_name(input_text TEXT)
RETURNS TEXT AS $$
DECLARE
    result TEXT;
BEGIN
    -- Handle NULL input
    IF input_text IS NULL OR input_text = '' THEN
        RETURN input_text;
    END IF;

    -- Step 1: Apply PostgreSQL's initcap() which handles most cases well:
    -- - Basic title case: DAMPOORT → Dampoort
    -- - Hyphenated words: SINT-NIKLAAS → Sint-Niklaas
    -- - Spaced compounds: AALST - STATION → Aalst - Station
    -- - Parentheses: ALBERTPARK (OOSTWIJK) → Albertpark (Oostwijk)
    result := initcap(input_text);

    -- Step 2: Fix Dutch prefixes that initcap capitalizes incorrectly
    -- 'S → 's (at start of word)
    result := regexp_replace(result, '^''S ', '''s ', 'g');
    result := regexp_replace(result, ' ''S ', ' ''s ', 'g');
    -- 'T → 't (at start of word)
    result := regexp_replace(result, '^''T ', '''t ', 'g');
    result := regexp_replace(result, ' ''T ', ' ''t ', 'g');

    -- Step 3: Preserve Roman numerals that initcap lowercased
    -- Only at end of string (e.g., "Industriegebied I" not "Ii")
    result := regexp_replace(result, ' Ii$', ' II');
    result := regexp_replace(result, ' Iii$', ' III');
    result := regexp_replace(result, ' Iv$', ' IV');
    result := regexp_replace(result, ' Vi$', ' VI');
    -- Note: " I" stays as " I" (initcap doesn't change single uppercase I)

    RETURN result;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Add a comment explaining the function
COMMENT ON FUNCTION normalize_dutch_name(TEXT) IS
'Converts ALL CAPS neighborhood names to Dutch title case.
Handles: hyphenated words, Dutch prefixes (''s, ''t), Roman numerals, abbreviations, parentheses.
Examples: SINT-NIKLAAS → Sint-Niklaas, ''S HERTOGENDIJK → ''s Hertogendijk';

-- ============================================================================
-- STEP 2: Test the function with sample names
-- ============================================================================

-- Display test results (for manual verification before UPDATE)
DO $$
DECLARE
    test_cases TEXT[] := ARRAY[
        'DAMPOORT',
        'BEGIJNHOFDRIES',
        'AALST - STATION',
        'SINT-NIKLAAS',
        '''S HERTOGENDIJK',
        '''T HOEKSEN',
        '''S GRAVENTAFEL - GOUDBERG - KAZAKKE',
        'INDUSTRIEGEBIED I',
        'GERMINAL II',
        'ALBERTPARK (OOSTWIJK)',
        'DE KAZERNE',
        'ST.-ANDRIES',
        'MARIA-AALTER-KERN',
        '''T HOGE - DE MAAN',
        'BLOKKEN-KRUISHOFSTR. (WILR.PL)',
        'A. BUYLSTRAAT',
        'AARSCHOT LEUVENSESTWG.'
    ];
    test_name TEXT;
BEGIN
    RAISE NOTICE '=== Testing normalize_dutch_name() ===';
    RAISE NOTICE '';
    FOREACH test_name IN ARRAY test_cases LOOP
        RAISE NOTICE '% → %',
            rpad(test_name, 45),
            normalize_dutch_name(test_name);
    END LOOP;
    RAISE NOTICE '';
    RAISE NOTICE '=== End of tests ===';
END $$;

-- ============================================================================
-- STEP 3: Preview changes (SELECT before UPDATE)
-- ============================================================================

-- Show sample of what will change in neighborhoods table
SELECT
    name AS original,
    normalize_dutch_name(name) AS normalized,
    city
FROM neighborhoods
WHERE name !~ '[a-z]'  -- Only names without lowercase letters (i.e., ALL CAPS)
ORDER BY city, name
LIMIT 30;

-- ============================================================================
-- STEP 4: Update neighborhoods table
-- ============================================================================

UPDATE neighborhoods
SET
    name = normalize_dutch_name(name),
    updated_at = NOW()
WHERE name !~ '[a-z]';  -- Only update names without lowercase (ALL CAPS)

-- Report how many rows were updated
DO $$
DECLARE
    updated_count INT;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Updated % rows in neighborhoods table', updated_count;
END $$;

-- ============================================================================
-- STEP 5: Update statistical_sectors table
-- ============================================================================

UPDATE statistical_sectors
SET
    name = normalize_dutch_name(name),
    updated_at = NOW()
WHERE name !~ '[a-z]';  -- Only update names without lowercase (ALL CAPS)

-- Report how many rows were updated
DO $$
DECLARE
    updated_count INT;
BEGIN
    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Updated % rows in statistical_sectors table', updated_count;
END $$;

-- ============================================================================
-- STEP 6: Verification queries
-- ============================================================================

-- Verify no ALL CAPS names remain in neighborhoods
SELECT COUNT(*) AS remaining_all_caps_neighborhoods
FROM neighborhoods
WHERE name ~ '^[A-Z]{2,}$' AND name !~ '[a-z]';

-- Verify no ALL CAPS names remain in statistical_sectors
SELECT COUNT(*) AS remaining_all_caps_sectors
FROM statistical_sectors
WHERE name ~ '^[A-Z]{2,}$' AND name !~ '[a-z]';

-- Show sample of normalized names
SELECT name, city
FROM neighborhoods
WHERE city IN ('Gent', 'Antwerpen', 'Brugge')
ORDER BY city, name
LIMIT 20;

-- Show Dutch prefix examples
SELECT name, city
FROM neighborhoods
WHERE name LIKE '''s %' OR name LIKE '''t %'
ORDER BY name
LIMIT 10;

-- ============================================================================
-- DONE
-- ============================================================================
