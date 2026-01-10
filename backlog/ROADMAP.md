# Roadmap — Pipeline Optimization & Content Architecture

Serial execution plan for Epics L, M, and N.

**Goal:** Reduce per-neighborhood cost, enable $0 data refreshes, and prepare for unattended batch processing.

---

## Phase 1: Quick Wins (Current Pipeline)

Fix bugs and prepare for mass regeneration.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 1 | L2 | Agent Fine-Tuning | Normalize neighborhood name casing | Low |
| 2 | L4 | Agent Fine-Tuning | Fix Statbel NIS code mapping for merged municipalities | Low |
| 3 | L7 | Agent Fine-Tuning | Qualitative language in prose (no specific counts) | Medium |

**Milestone:** Content quality issues fixed. Prose will be stable after regeneration.

---

## Phase 2: Pipeline Efficiency

Reduce cost per neighborhood before mass regeneration.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 4 | L8 | Agent Fine-Tuning | Merge SEO + Brand Reviewers into single Quality Reviewer | Medium |
| 5 | L5 | Agent Fine-Tuning | Reduce prompt token usage via external references | Medium |
| 6 | L6 | Agent Fine-Tuning | Materialized views for POI aggregates | Low |

**Milestone:** Pipeline runs ~40% faster, ~30% cheaper per neighborhood.

---

## Checkpoint: Mass Regeneration

After Phase 2, regenerate all content with the optimized pipeline.

- Estimated cost: ~$300 (down from ~$450)
- All neighborhoods get qualitative prose (L7)
- All neighborhoods get correct house prices (L4)
- All neighborhoods get proper name casing (L2)

---

## Phase 3: Infrastructure Foundation

Set up the `pipeline/` project and content database.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 7 | M1 | API Infrastructure | PostGIS database on Lightsail | Medium |
| 8 | N1 | Content Architecture | Content database schema (EF Core migrations) | Medium |
| 9 | N2 | Content Architecture | Migrate existing content to database | Low |

**Milestone:** Content stored in database. Current pipeline still works.

---

## Phase 4: Build System

Replace AI-generated structured content with templates.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 10 | N3 | Content Architecture | Value card template system | Medium |
| 11 | N4 | Content Architecture | Label rules engine | Medium |
| 12 | N5 | Content Architecture | Content build script | High |

**Milestone:** $0 data refreshes achieved! OSM/Statbel updates no longer require AI.

---

## Phase 5: Full Migration

Connect the new pipeline to the content database.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 13 | M2 | API Infrastructure | ASP.NET Core data API | Medium |
| 14 | M3 | API Infrastructure | Hangfire job orchestration | Medium |
| 15 | M4 | API Infrastructure | Anthropic API integration | Medium |
| 16 | N6 | Content Architecture | Writer agent outputs to database | Medium |
| 17 | N7 | Content Architecture | Deprecate brand reviewer JSON as source | Low |

**Milestone:** Single source of truth (database). Build script is only publisher.

---

## Phase 6: Scale (Optional)

Unattended overnight batch processing.

| Order | Story | Epic | Description | Effort |
|-------|-------|------|-------------|--------|
| 18 | M5 | API Infrastructure | Batch API integration | Medium |
| 19 | M6 | API Infrastructure | CLI trigger for batch processing | Low |

**Milestone:** Fire-and-forget overnight processing for new neighborhoods.

---

## Summary

```
Phase 1: L2 → L4 → L7                    (Quick wins)
Phase 2: L8 → L5 → L6                    (Efficiency)
         ─── REGENERATE ALL CONTENT ───
Phase 3: M1 → N1 → N2                    (Infrastructure)
Phase 4: N3 → N4 → N5                    ($0 refreshes)
Phase 5: M2 → M3 → M4 → N6 → N7          (Full migration)
Phase 6: M5 → M6                         (Scale)
```

---

## Cost Impact Summary

| Phase | One-time Cost | Ongoing Benefit |
|-------|---------------|-----------------|
| After Phase 2 | ~$300 regeneration | -40% per neighborhood |
| After Phase 4 | — | $0 data refreshes |
| After Phase 6 | — | Unattended batch processing |

---

## Notes

- L1 (Dog Park Features) and L3 (Ralph Loop) are not on critical path; do when convenient
- Phase 3-5 can be developed locally before deploying to Lightsail
- Current Claude Code pipeline remains functional until Phase 5 complete
