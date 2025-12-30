# Epic F — Developer Tooling & Code Quality

## Story F1: CSS Property Ordering with Stylelint
> As a developer, I want automated CSS property ordering so that styles are consistent, readable, and easier to maintain across the codebase.

**Context:**
- SCSS lives in multiple places: global files (`src/styles/`) and potentially scoped `<style>` blocks in `.astro` components
- ~~Currently no CSS linting in place~~ **Done**
- Using "outside-in" ordering (Recess/Bootstrap style) groups related properties together

**Property Order Groups:**
1. **Position** — position, top, right, bottom, left, z-index
2. **Display/Box Model** — display, flex, grid, width, height, margin, padding
3. **Typography** — font, line-height, text-align, color
4. **Visual** — background, border, border-radius, box-shadow, opacity
5. **Animation** — transition, animation, transform
6. **Misc** — cursor, pointer-events, user-select

**Acceptance Criteria:**
- [x] Stylelint installed and configured in `web/`
- [x] Property ordering enforced via `stylelint-config-recess-order`
- [x] SCSS-specific rules enabled via `stylelint-config-standard-scss`
- [x] Linting works for both `.scss` files and `<style>` blocks in `.astro` files
- [x] npm scripts added: `lint:css` and `lint:css:fix`
- [x] All existing SCSS files pass linting (auto-fixed)
- [x] VS Code integration configured (fix on save)

**Implementation Steps:**

### Step 1: Install Dependencies
```bash
cd web
npm install --save-dev stylelint stylelint-order stylelint-config-recess-order stylelint-config-standard-scss postcss-html
```

### Step 2: Create Stylelint Configuration
Create `web/.stylelintrc.json`:
```json
{
  "extends": [
    "stylelint-config-standard-scss",
    "stylelint-config-recess-order"
  ],
  "overrides": [
    {
      "files": ["**/*.astro"],
      "customSyntax": "postcss-html"
    }
  ],
  "rules": {
    "selector-class-pattern": null
  }
}
```

### Step 3: Add npm Scripts
Update `web/package.json` scripts:
```json
{
  "scripts": {
    "lint:css": "stylelint \"src/**/*.{scss,astro}\"",
    "lint:css:fix": "stylelint \"src/**/*.{scss,astro}\" --fix"
  }
}
```

### Step 4: VS Code Integration (Optional)
Create/update `web/.vscode/settings.json`:
```json
{
  "stylelint.validate": ["css", "scss", "astro"],
  "editor.codeActionsOnSave": {
    "source.fixAll.stylelint": "explicit"
  }
}
```

### Step 5: Auto-fix Existing Files
```bash
npm run lint:css:fix
```

**Files to Create/Modify:**
| File | Action |
|------|--------|
| `web/package.json` | Add devDependencies + scripts |
| `web/.stylelintrc.json` | Create new config |
| `web/.vscode/settings.json` | Create/update for IDE integration |
| `web/src/styles/**/*.scss` | Auto-fix property order |
| `web/src/**/*.astro` | Auto-fix any scoped styles |

**References:**
- [stylelint-config-recess-order](https://github.com/stormwarning/stylelint-config-recess-order) — Actively maintained (v7.4.0)
- [stylelint-order](https://github.com/hudochenkov/stylelint-order) — Property ordering plugin
- [Happy Potter and the Order of CSS](https://dev.to/thekashey/happy-potter-and-the-order-of-css-5ec) — Background on "outside-in" ordering

**Notes:**
- `stylelint-config-rational-order` is 7+ years outdated and missing modern CSS properties — do not use
- `stylelint-config-recess-order` supports CSS logical properties, container queries, and Stylelint 16

---

## Story F2: Migrate to Component-Scoped Styles (Future)
> As a developer, I want styles co-located with components so that I can reason about styling locally without hunting through global SCSS files.

**Status:** Backlog — depends on F1

**Notes:**
- Current architecture uses global SCSS with BEM naming
- Stylelint (from F1) will lint both global and scoped styles
- Migration can be incremental: move styles as components are touched
- Consider keeping shared variables/mixins global, move component-specific rules to `<style>` blocks
