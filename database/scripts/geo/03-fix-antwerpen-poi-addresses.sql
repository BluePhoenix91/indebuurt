-- ============================================================================
-- Fix Missing POI Addresses in Antwerpen
--
-- Issue: Several POIs in Antwerpen have NULL street/house_number because
-- the OSM source data lacks addr:street and addr:housenumber tags.
-- These addresses were manually looked up from business websites and directories.
--
-- Run against the GIS database after POI import.
-- ============================================================================

-- ============================================================================
-- Pet Stores - Antwerpen
-- ============================================================================

-- KATshop - Lange Koepoortstraat 54, 2000 Antwerpen
-- Source: https://www.katsign.be/katshop
UPDATE pois SET street = 'Lange Koepoortstraat', house_number = '54', updated_at = NOW() WHERE osm_id = '10657250960';

-- pet à porter - Schrijnwerkersstraat 19, 2000 Antwerpen
-- Source: https://www.petaporter.be
UPDATE pois SET street = 'Schrijnwerkersstraat', house_number = '19', updated_at = NOW() WHERE osm_id = '11261020400';

-- Maxi Zoo Merksem - Bredabaan 891-893, 2170 Merksem
-- Source: https://www.maxizoo.be/nl/stores/maxi-zoo-merksem/
UPDATE pois SET street = 'Bredabaan', house_number = '891-893', updated_at = NOW() WHERE osm_id = '2977432528';

-- Maxi Zoo Hoboken - Zeelandstraat 38, 2660 Hoboken
-- Source: https://www.maxizoo.be/nl/stores/maxi-zoo-hoboken/
UPDATE pois SET street = 'Zeelandstraat', house_number = '38', updated_at = NOW() WHERE osm_id = '4757047172';

-- Maxi Zoo Deurne - Ter Heydelaan 235, 2100 Deurne
-- Source: https://www.maxizoo.be/nl/stores/maxi-zoo-deurne/
UPDATE pois SET street = 'Ter Heydelaan', house_number = '235', updated_at = NOW() WHERE osm_id = '5550583500';

-- Tom & Co Berchem - Diksmuidelaan 79-83, 2600 Berchem
-- Source: https://www.openingsuren.vlaanderen/tom-en-co/2600-berchem/diksmuidelaan-79
UPDATE pois SET street = 'Diksmuidelaan', house_number = '79-83', updated_at = NOW() WHERE osm_id = '1701467766';

-- Petpret Wilrijk - Prins Boudewijnlaan 120, 2610 Wilrijk
-- Source: https://www.petpret.be/
UPDATE pois SET street = 'Prins Boudewijnlaan', house_number = '120', updated_at = NOW() WHERE osm_id = '5116922823';

-- ============================================================================
-- Veterinarians - Antwerpen
-- ============================================================================

-- Dierenarts Tony De Vries - Ballaarstraat 6, 2018 Antwerpen
-- Source: https://seety.co/nl/parkeerregeling/poi/dierenarts-dierenarts-tony-de-vries-antwerpen
UPDATE pois SET street = 'Ballaarstraat', house_number = '6', updated_at = NOW() WHERE osm_id = '4488335489';

-- Luc Lambrechts - Sint-Bavostraat 11, 2610 Wilrijk
-- Source: https://www.dierenartslambrechts.be/
UPDATE pois SET street = 'Sint-Bavostraat', house_number = '11', updated_at = NOW() WHERE osm_id = '1538266393';

-- Valérie Monster - Eduard van Steenbergenlaan 16, 2100 Deurne
-- Source: https://seety.co/nl/parkeerregeling/poi/dierenarts-valerie-monster-antwerpen
UPDATE pois SET street = 'Eduard van Steenbergenlaan', house_number = '16', updated_at = NOW() WHERE osm_id = '7358150617';

-- Veterinarian Animoretus - Jules Moretuslei 171, 2610 Wilrijk
-- Source: https://www.animoretus.be/
UPDATE pois SET street = 'Jules Moretuslei', house_number = '171', updated_at = NOW() WHERE osm_id = '9375670418';

-- Eddy Janssens - Generaal Eisenhowerlei 291, 2600 Berchem
-- Note: OSM has house_number=291 but no street; street determined from coordinates location
UPDATE pois SET street = 'Generaal Eisenhowerlei', updated_at = NOW() WHERE osm_id = '297296979';

-- ============================================================================
-- Verification
-- ============================================================================

SELECT name, street, house_number, postal_code, city
FROM pois
WHERE osm_id IN (
    '10657250960', '11261020400', '2977432528', '4757047172', '5550583500',
    '1701467766', '5116922823', '4488335489', '1538266393', '7358150617',
    '9375670418', '297296979'
)
ORDER BY name;
