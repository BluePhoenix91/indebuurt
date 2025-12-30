# Backlog Instructions for Claude

This file provides guidance when creating, editing, or managing backlog items in this repository.

## Backlog Structure

```
backlog/
├── CLAUDE.md           # This file
└── Epics/
    └── Epic-X-Name.md  # Epic files with stories
```

## Epic File Format

### Naming Convention
- File name: `Epic-{LETTER}-{Short-Name}.md`
- Letters are sequential: A, B, C, D, etc.
- Use kebab-case for the name part
- Examples: `Epic-A-SEO-Foundations.md`, `Epic-B-Core-Content.md`

### Epic Structure
```markdown
# Epic {LETTER} — {Epic Title}

## Story {LETTER}{NUMBER}: {Story Title} {STATUS_EMOJI}

> As a {persona}, I want {goal}, so {benefit}.

**Acceptance Criteria:**

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

**Implementation Notes:** (added after completion)

- Note about implementation detail
- Technical decisions made
- Files created or modified
```

## User Story Format

### Story Numbering
- Stories are numbered within their epic: A1, A2, A3... B1, B2, B3...
- Numbers are sequential within the epic

### Story Title
- Format: `Story {LETTER}{NUMBER}: {Descriptive Title}`
- Add status emoji after title when complete: `✅`
- Example: `Story B1: Hero Section With Static Map Image ✅`

### User Story Statement
- Use blockquote format: `> As a {persona}, I want {goal}, so {benefit}.`
- Keep personas consistent across the project
- Common personas for this project:
  - `developer` - for technical/infrastructure stories
  - `dog owner` - primary end user persona
  - `user` - generic end user
  - `site owner` - for analytics/business stories
  - `search engine` - for SEO-related stories

### Acceptance Criteria
- Use checkbox format: `- [ ]` (unchecked) or `- [x]` (completed)
- Each criterion should be independently testable
- Be specific and measurable
- Include both functional and technical criteria where relevant
- Nest sub-criteria with indentation when needed:
  ```markdown
  - [x] Main criterion
      - [x] Sub-criterion 1
      - [x] Sub-criterion 2
  ```

### Implementation Notes
- Add this section **after** the story is completed
- Document:
  - Key files created or modified
  - Technical decisions and rationale
  - Patterns or components introduced
  - Any deviations from original acceptance criteria
- Keep notes concise but informative for future reference

## Testing Notes Section (Optional)

Add a `**Testing Note:**` section when manual testing steps are needed:

```markdown
**Testing Note:** To verify this feature:
1. Step one
2. Step two
3. Expected result
```

## Writing Guidelines

### Language
- Write stories and acceptance criteria in English
- Technical terms and code references stay in English
- User-facing content examples can be in Dutch (nl-BE) as that's the target audience

### Tone
- Be specific and actionable
- Avoid vague terms like "should work well" or "looks good"
- Use measurable criteria: "renders in under 100ms" not "renders quickly"

### Story Independence
- Each story should be independently deliverable
- Avoid stories that are purely technical layers (e.g., "create database schema")
- Stories should deliver user-visible value when possible

### Story Size
- Aim for stories that can be completed in a single session
- If a story has more than 8-10 acceptance criteria, consider splitting it
- Large features should be broken into multiple stories

## Completion Workflow

1. When starting a story: Leave checkboxes unchecked `- [ ]`
2. As criteria are met: Check them off `- [x]`
3. When all criteria complete: Add `✅` to story title
4. Add `**Implementation Notes:**` section with details

## Creating New Epics

When creating a new epic:
1. Determine the next available letter
2. Create file with proper naming convention
3. Add epic title as H1
4. Add stories following the format above
5. Keep stories focused on a cohesive theme/feature area
