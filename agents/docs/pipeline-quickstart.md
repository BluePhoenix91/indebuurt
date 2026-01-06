# Pipeline Quickstart

Get your first neighborhood processed in 5 minutes.

---

## Prerequisites

Before running the pipeline:

- [ ] Claude Code CLI installed and working
- [ ] PostgreSQL running with both databases:
  - `buurtkompas` (GIS data)
  - `buurtkompas_pipeline` (job tracking)
- [ ] MCP servers configured in `.mcp.json`:
  - `gis` - read-only access to GIS database
  - `pipeline` - read-write access to pipeline database
- [ ] Database schema initialized (see `agents/scripts/db/README.md`)

---

## Step 1: Verify Setup

Start Claude Code and check the pipeline status:

```
/pipeline status
```

**Expected output:**

```
## Pipeline Status

### Overall
- Pending: 0
- In Progress: 0
- Completed: 0
- Failed: 0
```

If you see database errors, check your MCP configuration.

---

## Step 2: Find a Neighborhood

Look up a NIS code to process. NIS codes are 7 characters (5 digits + letter + digit).

**Example municipalities:**
| City | NIS Prefix | Example Neighborhood |
|------|------------|---------------------|
| Gent | 44021 | `44021A1` |
| Aalst | 41002 | `41002A0` |
| Antwerpen | 11002 | `11002A1` |

Or query the GIS database:
```sql
SELECT nis_code, name, city FROM neighborhoods WHERE city = 'Gent' LIMIT 5;
```

---

## Step 3: Process Your First Neighborhood

Run the pipeline for a single neighborhood:

```
/pipeline 41002A0
```

The pipeline will:
1. Create a job record
2. Run the **Researcher** agent (queries PostGIS for POI data)
3. Run the **Writer** agent (transforms to Dutch prose)
4. Run the **SEO Reviewer** agent (optimizes for search)
5. Run the **Brand Reviewer** agent (validates voice)
6. Auto-publish if score >= 70

**Output:**

```
Completed 41002A0 (Aalst - Station)
- SEO Score: 79
- Brand Score: 91
- Final Score: 85
- Published: Yes -> web/src/content/neighborhoods/aalst-aalst-station.json
```

---

## Step 4: Find Your Output

**Intermediate outputs** (always preserved):
```
agents/pipeline-outputs/41002A0/
├── 1-researcher.json
├── 2-writer.json
├── 3-seo-reviewer.json
└── 4-brand-reviewer.json
```

**Published content** (if score >= 70):
```
web/src/content/neighborhoods/aalst-aalst-station.json
```

---

## What's Next?

- Process a whole city: `/pipeline municipality 44021` (Gent)
- Check progress: `/pipeline status`
- Handle failures: `/pipeline retry-failed`

See [pipeline-commands.md](pipeline-commands.md) for the full command reference.

---

## If Something Goes Wrong

- **Session interrupted?** Just re-run `/pipeline <nis_code>` - it resumes from where it left off
- **Agent failed?** Check the error, then `/pipeline retry-failed`
- **Score too low?** Review the output, then `/pipeline publish <nis_code>` to publish anyway

See [pipeline-troubleshooting.md](pipeline-troubleshooting.md) for detailed solutions.
