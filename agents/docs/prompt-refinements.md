# Prompt Refinement Log

Track changes made to agent prompts based on testing results.

## Format

```markdown
## YYYY-MM-DD - [Agent] v[old] → v[new]
**Neighborhood**: Where the issue was discovered
**Issue**: What went wrong
**Fix**: What was changed
**Affected file**: Path and line numbers
**Verification**: How we confirmed the fix worked
```

---

## Log

*No refinements yet. This log will be populated during Story I6 testing.*

<!--
Example entry:

## 2026-01-05 - Researcher v1.0 → v1.1
**Neighborhood**: gent-mendonk
**Issue**: Missing pharmacies in poiCounts even though query returned 0
**Fix**: Added explicit instruction: "Include all POI categories in poiCounts, even if count is 0"
**Affected file**: agents/researcher/prompt-v1.md lines 94-96
**Verification**: Re-ran gent-mendonk, poiCounts now includes pharmacies: 0
-->
