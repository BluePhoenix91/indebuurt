-- Enrich POI addresses from spatial data and manual cross-border fixes
-- Run against the GIS database

-- Auto-fill Belgian POIs from statistical sectors
UPDATE pois p
SET
    postal_code = COALESCE(p.postal_code, ss.postal_code),
    city = COALESCE(p.city, ss.city),
    updated_at = NOW()
FROM statistical_sectors ss
WHERE ST_Within(p.location, ss.boundary)
AND (p.postal_code IS NULL OR p.city IS NULL);

-- ============================================================================
-- Cross-border POI fixes (matched by osm_id for stability across re-imports)
-- ============================================================================

-- NETHERLANDS - Veterinarians
UPDATE pois SET street = 'Zusterstraat', house_number = '3', postal_code = '4551 ER', city = 'Sas van Gent', updated_at = NOW() WHERE osm_id = '269052892'; -- Dierenkliniek Sas van Gent
UPDATE pois SET street = '''s-Gravenpolderseweg', house_number = '92', postal_code = '4462 CH', city = 'Goes', updated_at = NOW() WHERE osm_id = '284986360'; -- Dierenkliniek Goes
UPDATE pois SET street = 'Meemortel', house_number = '54', postal_code = '6021 AG', city = 'Budel', updated_at = NOW() WHERE osm_id = '13283272025'; -- Dierenartsenpraktijk De Meemortel
UPDATE pois SET street = 'Kalisstraat', house_number = '4b', postal_code = '5768 CX', city = 'Meijel', updated_at = NOW() WHERE osm_id = '4226361555'; -- Dierenarts praktijk Meijel
UPDATE pois SET street = 'Hoogveldsweg', house_number = '34', postal_code = '6102 CB', city = 'Echt', updated_at = NOW() WHERE osm_id = '1854785446'; -- Dierenkliniek 't Hoogveld
UPDATE pois SET street = 'Julianalaan', house_number = '7-A', postal_code = '6191 AL', city = 'Beek', updated_at = NOW() WHERE osm_id = '2100125021'; -- Dierenkliniek Boskamp

-- BELGIUM (Wallonia/Brussels) - Veterinarians outside Flanders statistical sectors
UPDATE pois SET street = 'Chaussée de Tubize', house_number = '234', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '11603062856'; -- Cabinet vétérinaire Fabienne Bedet
UPDATE pois SET street = 'Avenue Désiré Yernaux', house_number = '15', postal_code = '1300', city = 'Wavre', updated_at = NOW() WHERE osm_id = '674074346'; -- Centre Vétérinaire Vet2Care
UPDATE pois SET street = 'Chaussée de Tervuren', house_number = '174', postal_code = '1410', city = 'Waterloo', updated_at = NOW() WHERE osm_id = '7784559182'; -- Centre Vétérinaire du Lion
UPDATE pois SET street = 'Chaussée de Wavre', house_number = '190', postal_code = '1390', city = 'Grez-Doiceau', updated_at = NOW() WHERE osm_id = '5613359879'; -- Centre Vétérinaire Vetegrez
UPDATE pois SET street = 'Chaussée de Bruxelles', house_number = '165', postal_code = '7850', city = 'Petit-Enghien', updated_at = NOW() WHERE osm_id = '223896068'; -- Firmin Wéverberg
UPDATE pois SET street = 'Rue de Tubize', house_number = '61A', postal_code = '1440', city = 'Braine-le-Château', updated_at = NOW() WHERE osm_id = '6069744498'; -- MedicAnimalia
UPDATE pois SET street = 'Rue Latérale', house_number = '5', postal_code = '1440', city = 'Braine-le-Château', updated_at = NOW() WHERE osm_id = '9741268889'; -- Vétérinaire Druet
UPDATE pois SET street = 'Clos de la Houblonnière', house_number = '19', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '4260592959'; -- Cabinet vétérinaire Didier Eeckhout
UPDATE pois SET street = 'Rue Joseph Wauters', house_number = '5', postal_code = '1480', city = 'Tubize', updated_at = NOW() WHERE osm_id = '11358155908'; -- Vétérinaire Catherine Doppée

-- NETHERLANDS - Pet Stores
UPDATE pois SET street = 'N.C.B.-weg', house_number = '6', postal_code = '5684 PH', city = 'Best', updated_at = NOW() WHERE osm_id = '13040186083'; -- Pets Place - Boerenbond
UPDATE pois SET street = 'Ekkersrijt', house_number = '7418', postal_code = '5692 HB', city = 'Son en Breugel', updated_at = NOW() WHERE osm_id = '10862120677'; -- Vijvercentrum Ekkersrijt

-- BELGIUM (Wallonia) - Pet Stores
UPDATE pois SET street = 'Rue de Tubize', house_number = '102', postal_code = '1440', city = 'Braine-le-Château', updated_at = NOW() WHERE osm_id = '865609632'; -- Poils et Plumes
UPDATE pois SET street = 'Chaussée de Wavre', house_number = '312', postal_code = '1390', city = 'Grez-Doiceau', updated_at = NOW() WHERE osm_id = '95744560'; -- Poils et Plumes
UPDATE pois SET street = 'Chaussée de Louvain', house_number = '310', postal_code = '1300', city = 'Wavre', updated_at = NOW() WHERE osm_id = '8880064534'; -- Tom & Co
UPDATE pois SET street = 'Rue de Septembre', house_number = '6', postal_code = '1300', city = 'Wavre', updated_at = NOW() WHERE osm_id = '272997363'; -- Tom & Co

-- BELGIUM (Wallonia) - Pharmacies
UPDATE pois SET street = 'Chaussée de Tervuren', house_number = '22', postal_code = '1410', city = 'Waterloo', updated_at = NOW() WHERE osm_id = '13421463071'; -- Multipharma
UPDATE pois SET street = 'Place de la Gare', house_number = '13', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '13421461995'; -- BENU Pharmacie

-- BELGIUM (Wallonia) - Train Stations
UPDATE pois SET street = 'Place de la Gare', house_number = '1', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '13421501735'; -- Braine-l'Alleud
UPDATE pois SET street = 'Chaussée du Tilleul', house_number = NULL, postal_code = '1300', city = 'Wavre', updated_at = NOW() WHERE osm_id = '13421501652'; -- Basse-Wavre
UPDATE pois SET street = 'Rue de la Station', house_number = '40', postal_code = '1332', city = 'Genval', updated_at = NOW() WHERE osm_id = '13421501597'; -- Genval
UPDATE pois SET street = 'Rue de la Coopérative', house_number = NULL, postal_code = '7850', city = 'Enghien', updated_at = NOW() WHERE osm_id = '13421468584'; -- Enghien - Edingen

-- BELGIUM (Wallonia) - Supermarkets
UPDATE pois SET street = 'Rue de Tubize', house_number = '102B', postal_code = '1440', city = 'Braine-le-Château', updated_at = NOW() WHERE osm_id = '13421466961'; -- AD Delhaize
UPDATE pois SET street = 'Rue du Serment', house_number = '10', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '13421465840'; -- ALDI
UPDATE pois SET street = 'Boulevard de l''Europe', house_number = '3', postal_code = '1301', city = 'Bierges', updated_at = NOW() WHERE osm_id = '13421465963'; -- Carrefour
UPDATE pois SET street = 'Chaussée de Louvain', house_number = '314', postal_code = '1300', city = 'Wavre', updated_at = NOW() WHERE osm_id = '13421465969'; -- ALDI
UPDATE pois SET street = 'Chaussée d''Alsemberg', house_number = '437', postal_code = '1420', city = 'Braine-l''Alleud', updated_at = NOW() WHERE osm_id = '13421464284'; -- Bio-Planet

-- NETHERLANDS - Train Stations
UPDATE pois SET street = 'Stationsplein', house_number = NULL, postal_code = '4461 HP', city = 'Goes', updated_at = NOW() WHERE osm_id = '13421502580'; -- Goes
UPDATE pois SET street = 'Beukenlaan', house_number = '1', postal_code = '5617 AB', city = 'Eindhoven', updated_at = NOW() WHERE osm_id = '13421499901'; -- Eindhoven Strijp-S
UPDATE pois SET street = 'Spoorstraat', house_number = '1', postal_code = '5683 CK', city = 'Best', updated_at = NOW() WHERE osm_id = '13421501346'; -- Best
UPDATE pois SET street = 'Stationsplein', house_number = NULL, postal_code = '6245 AG', city = 'Eijsden', updated_at = NOW() WHERE osm_id = '13421502359'; -- Eijsden
UPDATE pois SET street = 'Stationsplein', house_number = '1', postal_code = '4611 AB', city = 'Bergen op Zoom', updated_at = NOW() WHERE osm_id = '13421502330'; -- Bergen op Zoom

-- NETHERLANDS - Supermarkets
UPDATE pois SET street = 'Rapportstraat', house_number = '2', postal_code = '5504 BP', city = 'Veldhoven', updated_at = NOW() WHERE osm_id = '13421465868'; -- Albert Heijn
