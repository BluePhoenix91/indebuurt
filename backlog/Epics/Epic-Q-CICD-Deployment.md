# Epic Q — CI/CD & Automated Deployment

**Goal:** Automate the build and deployment pipeline so the site rebuilds and deploys without requiring a local machine.

**Depends on:** Epic N (Content Architecture), Epic M (Server Infrastructure)

---

## Context

Currently, building and deploying requires manual steps on a local machine:

1. Run `dotnet run -- build` (future: Epic N) to generate JSON from database
2. Run `npm run build` in `web/` to compile Astro to static HTML
3. Manually deploy `dist/` folder to hosting

This creates friction:
- Can't deploy when laptop is off
- Manual steps are error-prone
- No automatic rebuilds when data changes

**Solution: GitHub Actions + AWS Amplify**

```
Push to main
    │
    ▼
GitHub Actions
    ├── SSH tunnel to Lightsail DB
    ├── dotnet run -- build (generates JSON from live data)
    ├── git commit + push JSON files to main
    │
    ▼
Amplify detects commit
    ├── npm run build (Astro compiles HTML)
    ├── Deploy to CDN
    │
    ▼
Live at buurtkompas.be
```

**Benefits:**
- JSON files versioned in git (easy to track changes)
- Amplify handles static site build + CDN
- GitHub Actions handles database access via SSH tunnel
- Scheduled rebuilds for weekly data refreshes

---

## Story Q1: Hosting Platform ✅

> As a developer, I want a hosting platform selected, so that we know where static files will be deployed.

**Status:** Already using **AWS Amplify**

**Current Setup:**
- Site live at `https://www.buurtkompas.be`
- AWS Amplify Hosting (static site)
- Built-in CI/CD from GitHub
- CloudFront CDN included
- SSL certificate managed by AWS

**Acceptance Criteria:**

- [x] Hosting platform selected: AWS Amplify
- [x] Domain `buurtkompas.be` configured
- [x] SSL certificate provisioned
- [x] Site is live and accessible

---

## Story Q2: Build Pipeline ✅

> As a developer, I want the site to build automatically, so that builds don't depend on any local machine.

**Status:** Already configured in AWS Amplify Console

**Current Amplify Build Config:**
```yaml
version: 1
applications:
  - appRoot: web
    frontend:
      phases:
        preBuild:
          commands:
            - npm ci
        build:
          commands:
            - npm run build
      artifacts:
        baseDirectory: dist
        files:
          - "**/*"
      cache:
        paths:
          - node_modules/**/*
```

**Acceptance Criteria:**

- [x] Build triggers on push to main
- [x] Dependencies cached (node_modules)
- [x] Build artifacts deployed to CDN
- [x] Build fails if Zod validation fails

**Optional improvement:** Copy this config to `amplify.yml` in repo root for version control.

---

## Story Q3: Automated Deployment ✅

> As a developer, I want successful builds to deploy automatically, so that changes go live without manual intervention.

**Status:** Already working via Amplify

**Acceptance Criteria:**

- [x] Deployment triggers after successful build on main branch
- [x] Site is live at `https://www.buurtkompas.be` after deploy
- [x] Failed deployments don't affect current live site (Amplify keeps previous version)
- [x] Rollback possible via Amplify Console (redeploy previous build)

---

## Story Q4: Content Build Workflow

> As a developer, I want GitHub Actions to generate JSON content from the database and commit it to the repo, so that Amplify can build from versioned content.

**Workflow: `.github/workflows/build-content.yml`**

```yaml
name: Build Content

on:
  push:
    branches: [main]
    paths:
      - 'pipeline/**'
      - '.github/workflows/build-content.yml'
  schedule:
    # Run every Monday at 6 AM UTC (after weekend ETL)
    - cron: '0 6 * * 1'
  workflow_dispatch:  # Manual trigger

jobs:
  build-content:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Setup SSH tunnel to database
        run: |
          mkdir -p ~/.ssh
          echo "${{ secrets.LIGHTSAIL_SSH_KEY }}" > ~/.ssh/id_rsa
          chmod 600 ~/.ssh/id_rsa
          ssh -f -N -L 5433:localhost:5432 Administrator@${{ secrets.LIGHTSAIL_HOST }} -o StrictHostKeyChecking=no

      - name: Build content from database
        run: dotnet run -- build
        working-directory: pipeline/src/Pipeline.Cli
        env:
          ConnectionStrings__ContentDb: "Host=localhost;Port=5433;Database=buurtkompas;Username=buurtkompas_readonly;Password=${{ secrets.DB_PASSWORD }}"
          ConnectionStrings__GisDb: "Host=localhost;Port=5433;Database=buurtkompas;Username=buurtkompas_readonly;Password=${{ secrets.DB_PASSWORD }}"

      - name: Commit and push JSON files
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "github-actions[bot]@users.noreply.github.com"
          git add web/src/content/neighborhoods/*.json
          git diff --staged --quiet || git commit -m "chore: update neighborhood content from database"
          git push
```

**Flow:**

```
Trigger (push/schedule/manual)
    │
    ▼
GitHub Actions runner
    ├── Checkout repo
    ├── SSH tunnel to Lightsail (port 5433 → 5432)
    ├── dotnet run -- build
    │     └── Queries GIS + Content DBs
    │     └── Writes JSON to web/src/content/neighborhoods/
    ├── git commit + push (if changes)
    │
    ▼
Amplify detects commit
    ├── npm run build
    ├── Deploy to CDN
    │
    ▼
Live at buurtkompas.be
```

**Acceptance Criteria:**

- [ ] SSH key stored in GitHub Secrets (`LIGHTSAIL_SSH_KEY`)
- [ ] Database password stored in GitHub Secrets (`DB_PASSWORD`)
- [ ] Lightsail host stored in GitHub Secrets (`LIGHTSAIL_HOST`)
- [ ] Workflow triggers on schedule (weekly)
- [ ] Workflow triggers manually via workflow_dispatch
- [ ] JSON changes committed with descriptive message
- [ ] No commit if no changes (idempotent)
- [ ] Secrets never exposed in logs

---

## Story Q5: Scheduled Rebuilds

> As a pipeline operator, I want the site to rebuild on a schedule, so that POI data changes are reflected without manual action.

**Status:** Included in Q4 workflow via `schedule` trigger

**Schedule:**

```yaml
on:
  schedule:
    # Run every Monday at 6 AM UTC (after weekend ETL)
    - cron: '0 6 * * 1'
```

**Acceptance Criteria:**

- [ ] Scheduled workflow runs weekly (Monday 6 AM UTC)
- [ ] Coordinates with Epic O ETL schedule (runs Sunday night)
- [ ] Notification on build failure (GitHub Actions default)
- [ ] Can be triggered manually via workflow_dispatch

---

## Story Q6: Build Notifications

> As a developer, I want to be notified of build/deploy failures, so that issues are caught quickly.

**Acceptance Criteria:**

- [ ] Failed builds send notification
- [ ] Notification includes: branch, commit, error summary, link to logs
- [ ] Channel configurable (email, Slack, Discord)
- [ ] Successful deploys optionally notified (off by default)

**Implementation:**

```yaml
- name: Notify on failure
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    payload: |
      {
        "text": "Build failed for ${{ github.ref }}",
        "blocks": [
          {
            "type": "section",
            "text": {
              "type": "mrkdwn",
              "text": "*Build Failed* :x:\n<${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}|View logs>"
            }
          }
        ]
      }
  env:
    SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK }}
```

---

## Dependencies

```
Epic M (Server Infrastructure)
  └── M1 (PostgreSQL on Lightsail) — Q4 connects here via SSH

Epic N (Content Architecture)
  └── N5 (Build Script) — Q4 runs this in CI

Epic Q (CI/CD)
  ├── Q1 (Hosting Platform) ✅
  ├── Q2 (Build Pipeline) ✅
  ├── Q3 (Auto Deploy) ✅
  ├── Q4 (Content Build Workflow) — needs N5 first
  │     └── Q5 (Scheduled Rebuilds)
  └── Q6 (Notifications) — add anytime
```

**Current status:**

- ✅ **Q1, Q2, Q3** — Already working via AWS Amplify
- ⏳ **Q4, Q5** — After Epic N build script exists
- ⏳ **Q6** — Add when pipeline is stable

---

## Cost Estimate

| Component | Cost |
|-----------|------|
| AWS Amplify Hosting | Free tier: 5 GB storage, 15 GB/month transfer |
| AWS Amplify Build | Free tier: 1,000 build mins/month |
| GitHub Actions (if needed) | Free: 2,000 mins/month |

**Current cost:** Within free tier for MVP traffic. At scale, Amplify is ~$0.01/GB served.

---

## Security Considerations

- [ ] Database credentials in GitHub Secrets (never in code)
- [ ] SSH keys rotated periodically
- [ ] Least-privilege database user for CI (`buurtkompas_readonly`)
- [ ] Firewall rules reviewed if opening direct DB access
- [ ] Deploy tokens scoped to specific project

---

## Out of Scope

- Blue-green deployments (overkill for static site)
- Multiple environments (staging/production) — consider for future
- Container-based builds (not needed for static site)
- Self-hosted runners (GitHub-hosted sufficient)
