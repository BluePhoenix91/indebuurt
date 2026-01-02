/**
 * JSON Validation Script
 *
 * Validates JSON files against agent schemas.
 * Use to test agent outputs before saving to Content Collections.
 *
 * Usage:
 *   npm run validate:json -- <schema> <file>
 *
 * Examples:
 *   npm run validate:json -- researcher researcher/examples/gent-binnenstad.json
 *   npm run validate:json -- writer writer/examples/gent-binnenstad.json
 *   npm run validate:json -- final web/src/content/neighborhoods/gent-binnenstad.json
 *
 * Exit codes:
 * - 0: Valid
 * - 1: Invalid or error
 */

import { readFileSync } from "fs";
import { dirname, join, resolve } from "path";
import { fileURLToPath } from "url";
import {
  researcherOutputSchema,
  writerOutputSchema,
  finalOutputSchema,
} from "./schemas.js";
import type { ZodSchema } from "zod";

const __dirname = dirname(fileURLToPath(import.meta.url));
const agentsDir = join(__dirname, "..");
const repoRoot = join(agentsDir, "..");

// Schema mapping
const schemas: Record<string, ZodSchema> = {
  researcher: researcherOutputSchema,
  writer: writerOutputSchema,
  final: finalOutputSchema,
};

function printUsage(): void {
  console.log(`
Usage: npm run validate:json -- <schema> <file>

Schemas:
  researcher  - Validate against ResearcherOutput schema
  writer      - Validate against WriterOutput schema
  final       - Validate against FinalOutput schema (Content Collections)

Examples:
  npm run validate:json -- researcher researcher/examples/gent-binnenstad.json
  npm run validate:json -- final ../web/src/content/neighborhoods/gent-binnenstad.json
`);
}

function main(): void {
  const args = process.argv.slice(2);

  if (args.length < 2) {
    printUsage();
    process.exit(1);
  }

  const [schemaName, filePath] = args;

  // Get schema
  const schema = schemas[schemaName];
  if (!schema) {
    console.error(`Unknown schema: ${schemaName}`);
    console.error(`Available schemas: ${Object.keys(schemas).join(", ")}`);
    process.exit(1);
  }

  // Resolve file path (relative to agents dir or absolute)
  const resolvedPath = filePath.startsWith("/") || filePath.includes(":")
    ? filePath
    : resolve(agentsDir, filePath);

  // Read file
  let content: string;
  try {
    content = readFileSync(resolvedPath, "utf-8");
  } catch (e) {
    console.error(`Could not read file: ${resolvedPath}`);
    process.exit(1);
  }

  // Parse JSON
  let data: unknown;
  try {
    data = JSON.parse(content);
  } catch (e) {
    console.error(`Invalid JSON in file: ${resolvedPath}`);
    process.exit(1);
  }

  // Strip $comment and _annotation fields (used for documentation in examples)
  const cleanData = stripAnnotations(data);

  // Validate
  const result = schema.safeParse(cleanData);

  if (result.success) {
    console.log(`VALID: ${filePath}`);
    console.log(`Schema: ${schemaName}`);
    process.exit(0);
  } else {
    console.error(`INVALID: ${filePath}`);
    console.error(`Schema: ${schemaName}`);
    console.error(`\nErrors:`);

    for (const error of result.error.errors) {
      const path = error.path.length > 0 ? error.path.join(".") : "(root)";
      console.error(`  - ${path}: ${error.message}`);
    }

    process.exit(1);
  }
}

function stripAnnotations(data: unknown): unknown {
  if (Array.isArray(data)) {
    return data.map(stripAnnotations);
  }

  if (data !== null && typeof data === "object") {
    const result: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(data)) {
      // Skip annotation fields
      if (key === "$comment" || key.startsWith("_annotation")) {
        continue;
      }
      result[key] = stripAnnotations(value);
    }
    return result;
  }

  return data;
}

main();
