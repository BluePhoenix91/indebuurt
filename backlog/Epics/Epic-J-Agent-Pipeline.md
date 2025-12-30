# Epic J — Agent Pipeline

**Goal:** Build the automated orchestration that runs agents in sequence, handles errors, and outputs validated content files without human intervention.

**Depends on:** Epic H (Infrastructure) and Epic I (Agent Development)

---

## Story J1: Python Orchestrator Script
> As a developer, I want a Python script that runs all agents in sequence for a given neighborhood, so that content generation is automated end-to-end.

**Context:** Orchestrator manages the pipeline: Researcher → Writer → SEO → Brand → output JSON. Each agent's output feeds the next.

**Acceptance Criteria:**
- [ ] Python script accepts neighborhood ID as command-line argument
- [ ] Script loads agent prompts from `/agents/prompts/`
- [ ] Agents executed in correct sequence via Claude API
- [ ] Each agent receives previous agent's output as context
- [ ] Intermediate outputs saved to temp files for debugging
- [ ] Final JSON written to `src/content/neighborhoods/{slug}.json`
- [ ] Script returns exit code 0 on success, non-zero with error details on failure
- [ ] Execution time logged for each agent stage

---

## Story J2: Error Handling and Retry Logic
> As a developer, I want the orchestrator to handle agent failures gracefully, so that transient errors don't require manual intervention.

**Context:** API calls can fail, agents can produce invalid output. Need resilient handling.

**Acceptance Criteria:**
- [ ] API errors (rate limits, timeouts) trigger automatic retry with exponential backoff
- [ ] Max 3 retries per agent before failing the neighborhood
- [ ] Schema validation after each agent catches malformed output
- [ ] Invalid output triggers agent re-run with error context
- [ ] All errors logged with: timestamp, neighborhood, agent, error type, attempt number
- [ ] Failed neighborhoods written to `failed_neighborhoods.log` for later retry
- [ ] Script continues to next neighborhood on failure (doesn't halt batch)

---

## Story J3: Quality Scoring System
> As a content team member, I want automated quality scores for generated content, so that low-quality output is flagged before publishing.

**Context:** Brand agent outputs a quality score. This story adds additional automated checks and aggregates into final score.

**Acceptance Criteria:**
- [ ] Quality score (0-100) calculated combining: brand score, SEO score, completeness
- [ ] Completeness check: all required fields present, minimum content lengths met
- [ ] SEO check: title/description lengths, heading structure, keyword presence
- [ ] Data accuracy check: POI counts match database query results
- [ ] Score breakdown saved with output: `{ total: 85, brand: 90, seo: 80, completeness: 85 }`
- [ ] Configurable threshold: content >= 80 auto-approved, < 80 flagged for review
- [ ] Flagged content saved to separate `review_queue/` directory

---

## Story J4: Review Queue Interface
> As a content team member, I want a simple interface to review flagged content, so that I can approve, reject, or request regeneration without editing JSON manually.

**Context:** Human-outside-the-loop: not blocking pipeline, but can review/fix low-quality content.

**Acceptance Criteria:**
- [ ] Simple web interface (can be basic HTML/Python Flask)
- [ ] Lists all content in review queue with quality scores
- [ ] Detail view shows: generated content, score breakdown, specific issues flagged
- [ ] Actions available: Approve (moves to content/), Reject (archives), Regenerate (re-runs pipeline)
- [ ] Regenerate accepts optional feedback to include in next run
- [ ] Approved content automatically moved to `src/content/neighborhoods/`
- [ ] Audit log tracks: who reviewed, what action, when

---

## Story J5: Revision Loop Between Agents
> As a developer, I want SEO and Brand agents to iterate on content when quality is low, so that more content passes automatically without human review.

**Context:** If Brand agent scores content < 80, SEO and Brand can iterate (max 3 rounds) to improve before flagging.

**Acceptance Criteria:**
- [ ] After Brand review, if score < threshold, content loops back to SEO agent
- [ ] SEO agent receives: current content + Brand feedback + instruction to address issues
- [ ] Brand agent re-reviews SEO's revisions
- [ ] Maximum 3 revision rounds before flagging for human review
- [ ] Each round's scores logged to track improvement
- [ ] Final round's content used even if still below threshold (human will review)
- [ ] Revision history saved for debugging: what changed each round

---

## Dependencies

```
J1 (Orchestrator)
  ├── J2 (Error Handling)
  ├── J3 (Quality Scoring) ── J4 (Review Interface)
  └── J5 (Revision Loop)
```

J1 is foundation. J2, J3, J5 can be added incrementally. J4 depends on J3.
