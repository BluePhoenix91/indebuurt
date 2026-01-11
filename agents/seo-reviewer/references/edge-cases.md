# SEO Reviewer Edge Cases

## Already Good Content
If input scores >= 85 on initial analysis:
- Make no or minimal changes
- Output with empty or near-empty `changesLog`
- Note that content was already well-optimized

## Invalid Neighboring Neighborhoods
If a `neighboringNeighborhoods` ID doesn't exist in database:
- Log as `validationIssue` with severity "warning"
- Deduct points in scoring (-2 per invalid)
- Do NOT remove from array — may be future neighborhoods

## Content Too Short
If section intros are very short (< 20 words):
- Flag in `validationIssues`
- Make best-effort improvements
- Score will naturally be lower

## Sparse Data Neighborhood
For rural neighborhoods with limited amenities:
- Accept shorter section intros if data doesn't support more
- Focus on what IS available
- Honest sparse-data handling is acceptable

## Database Unavailable
If PostgreSQL connection fails:
- Continue without internal link validation
- Set `internalLinkingScore` to 0
- Log as `validationIssue` with severity "info"
