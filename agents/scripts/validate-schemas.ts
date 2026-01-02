/**
 * Schema Validation Script
 *
 * Validates that committed JSON schemas match the Zod source definitions.
 * Use in CI to catch drift between Zod schemas and generated JSON schemas.
 *
 * Run with: npm run schemas:validate
 *
 * Exit codes:
 * - 0: All schemas match
 * - 1: Schema mismatch detected (regenerate with npm run schemas:generate)
 */

import { readFileSync, existsSync } from "fs";
import { dirname, join } from "path";
import { fileURLToPath } from "url";
import { zodToJsonSchema } from "zod-to-json-schema";
import {
  researcherOutputSchema,
  writerOutputSchema,
  finalOutputSchema,
} from "./schemas.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const agentsDir = join(__dirname, "..");

interface SchemaConfig {
  schema: Parameters<typeof zodToJsonSchema>[0];
  outputPath: string;
  title: string;
  description: string;
  version: string;
}

const schemas: SchemaConfig[] = [
  {
    schema: researcherOutputSchema,
    outputPath: "researcher/output-schema.json",
    title: "ResearcherOutput",
    description:
      "Output schema for the Researcher agent. Contains factual data from PostGIS queries: POIs, statistics, distances. No prose or presentation logic.",
    version: "1.0.0",
  },
  {
    schema: writerOutputSchema,
    outputPath: "writer/output-schema.json",
    title: "WriterOutput",
    description:
      "Output schema for the Writer agent. Transforms Researcher data into engaging Dutch content with icons, intros, and editorial decisions.",
    version: "1.0.0",
  },
  {
    schema: finalOutputSchema,
    outputPath: "shared/final-output-schema.json",
    title: "NeighborhoodPage",
    description:
      "Final output schema matching Astro Content Collections. This is what gets saved to /web/src/content/neighborhoods/*.json",
    version: "1.0.0",
  },
];

function generateExpectedSchema(config: SchemaConfig): object {
  const jsonSchema = zodToJsonSchema(config.schema, {
    name: config.title,
    $refStrategy: "none",
  });

  return {
    $schema: "http://json-schema.org/draft-07/schema#",
    $id: `https://indebuurt.be/schemas/${config.outputPath}`,
    title: config.title,
    description: config.description,
    version: config.version,
    ...jsonSchema,
  };
}

function validateSchema(config: SchemaConfig): boolean {
  const filePath = join(agentsDir, config.outputPath);

  // Check if file exists
  if (!existsSync(filePath)) {
    console.error(`MISSING: ${config.outputPath}`);
    console.error(`  Run 'npm run schemas:generate' to create it.\n`);
    return false;
  }

  // Read committed schema
  const committedContent = readFileSync(filePath, "utf-8");
  let committedSchema: object;
  try {
    committedSchema = JSON.parse(committedContent);
  } catch (e) {
    console.error(`INVALID JSON: ${config.outputPath}`);
    console.error(`  File contains invalid JSON.\n`);
    return false;
  }

  // Generate expected schema
  const expectedSchema = generateExpectedSchema(config);

  // Compare (normalize by stringifying)
  const committedNormalized = JSON.stringify(committedSchema, null, 2);
  const expectedNormalized = JSON.stringify(expectedSchema, null, 2);

  if (committedNormalized !== expectedNormalized) {
    console.error(`MISMATCH: ${config.outputPath}`);
    console.error(`  Committed schema does not match Zod definition.`);
    console.error(`  Run 'npm run schemas:generate' to update.\n`);
    return false;
  }

  console.log(`OK: ${config.outputPath}`);
  return true;
}

// Main execution
console.log("Validating JSON schemas against Zod definitions...\n");

let allValid = true;

for (const config of schemas) {
  if (!validateSchema(config)) {
    allValid = false;
  }
}

console.log("");

if (allValid) {
  console.log("All schemas are valid and up to date.");
  process.exit(0);
} else {
  console.error("Schema validation failed!");
  console.error("Run 'npm run schemas:generate' to regenerate schemas.");
  process.exit(1);
}
