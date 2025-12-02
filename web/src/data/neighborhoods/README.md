# Neighborhood Data Files

Each neighborhood has its own TypeScript file in this directory. This makes it easier to manage and maintain individual neighborhood data.

## Structure

- `antwerpen-zuid.ts` - Example neighborhood file
- `index.ts` - Aggregates all neighborhoods and exports them

## Adding a New Neighborhood

1. Create a new file: `{slug}.ts` (e.g., `gent-centrum.ts`)

2. Copy the structure from `antwerpen-zuid.ts` and fill in your data:

```typescript
import type { Neighborhood } from "../neighborhoods";

export const gentCentrum: Neighborhood = {
  id: "gent-centrum",
  name: "Gent Centrum",
  // ... rest of the data
};
```

3. Import and add it to `index.ts`:

```typescript
import { gentCentrum } from "./gent-centrum";

export const neighborhoods: Record<string, Neighborhood> = {
  "antwerpen-zuid": antwerpenZuid,
  "gent-centrum": gentCentrum, // Add here
};
```

4. The page will automatically be generated at `/buurt/gent-centrum` when you build!

## Converting to JSON (Optional)

If you prefer JSON files instead of TypeScript:

1. Create `{slug}.json` files
2. Update `index.ts` to import JSON files:

```typescript
import antwerpenZuidData from "./antwerpen-zuid.json";
import gentCentrumData from "./gent-centrum.json";

export const neighborhoods: Record<string, Neighborhood> = {
  "antwerpen-zuid": antwerpenZuidData as Neighborhood,
  "gent-centrum": gentCentrumData as Neighborhood,
};
```

Note: TypeScript files provide type safety and autocomplete, which is recommended.
