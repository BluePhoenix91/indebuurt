/**
 * Schema Generation Script
 *
 * Generates JSON Schema files from Zod definitions for use by AI agents.
 * Run with: npm run schemas:generate
 *
 * Output:
 * - shared/final-output-schema.json (matches Astro Content Collections)
 * - researcher/output-schema.json (factual data from database)
 * - writer/output-schema.json (adds narrative and presentation)
 */

import { writeFileSync, mkdirSync } from "fs";
import { dirname, join } from "path";
import { fileURLToPath } from "url";
import { zodToJsonSchema } from "zod-to-json-schema";
import {
  researcherOutputSchema,
  writerOutputSchema,
  finalOutputSchema,
  seoReviewerOutputSchema,
  brandReviewerOutputSchema,
} from "./schemas.js";
import { humanReviewSchema } from "./review/review-schema.js";

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
  {
    schema: seoReviewerOutputSchema,
    outputPath: "seo-reviewer/output-schema.json",
    title: "SEOReviewerOutput",
    description:
      "Output schema for the SEO Reviewer agent. Extends WriterOutput with changes log, quality score, and SEO validation metadata.",
    version: "1.0.0",
  },
  {
    schema: brandReviewerOutputSchema,
    outputPath: "brand-reviewer/output-schema.json",
    title: "BrandReviewerOutput",
    description:
      "Output schema for the Brand Reviewer agent. Extends SEOReviewerOutput with terminology compliance, tone analysis, and brand quality score.",
    version: "1.0.0",
  },
  {
    schema: humanReviewSchema,
    outputPath: "shared/human-review-schema.json",
    title: "HumanReview",
    description:
      "Schema for human quality reviews of agent-generated content. Used during manual testing (Story I6) to rate accuracy, readability, and brand compliance.",
    version: "1.0.0",
  },
];

function generateSchema(config: SchemaConfig): void {
  const jsonSchema = zodToJsonSchema(config.schema, {
    name: config.title,
    $refStrategy: "none", // Inline all definitions for easier agent consumption
  });

  // Add metadata
  const enrichedSchema = {
    $schema: "http://json-schema.org/draft-07/schema#",
    $id: `https://indebuurt.be/schemas/${config.outputPath}`,
    title: config.title,
    description: config.description,
    version: config.version,
    ...jsonSchema,
  };

  const outputPath = join(agentsDir, config.outputPath);

  // Ensure directory exists
  mkdirSync(dirname(outputPath), { recursive: true });

  writeFileSync(outputPath, JSON.stringify(enrichedSchema, null, 2) + "\n");

  console.log(`Generated: ${config.outputPath}`);
}

// Main execution
console.log("Generating JSON schemas from Zod definitions...\n");

for (const config of schemas) {
  generateSchema(config);
}

console.log("\nDone! Schemas generated successfully.");
console.log("\nNext steps:");
console.log("1. Review the generated schemas");
console.log("2. Commit them to git");
console.log("3. Run 'npm run schemas:validate' to verify they match Zod definitions");
