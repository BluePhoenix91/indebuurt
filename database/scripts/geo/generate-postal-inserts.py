#!/usr/bin/env python3
"""
Generate SQL INSERT statements from be-dictionary.csv
Run: python database/scripts/geo/generate-postal-inserts.py > database/data/postal-inserts.sql
"""

import csv

def escape_sql(val):
    if val is None:
        return 'NULL'
    return "'" + val.replace("'", "''") + "'"

with open('database/data/be-dictionary.csv', 'r', encoding='latin-1') as f:
    reader = csv.DictReader(f)
    for row in reader:
        # We only need NIS9 and PostCode for our update
        nis9 = escape_sql(row['NIS9'])
        postcode = escape_sql(row['PostCode'])
        print(f"INSERT INTO staging_postal_codes (nis9, postcode) VALUES ({nis9}, {postcode});")
