/**
 * Human Review Schema for Agent Pipeline Testing
 *
 * This schema defines the structure for human quality reviews of agent-generated
 * neighborhood content. Used during Story I6 manual testing to evaluate:
 * - Data accuracy (Researcher)
 * - Content quality (Writer)
 * - SEO effectiveness (SEO Reviewer)
 * - Brand compliance (Brand Reviewer)
 *
 * Schema version: 1.0.0
 */

import { z } from "zod";

// =============================================================================
// STAGE REVIEW SCHEMAS
// =============================================================================

const researcherReviewSchema = z.object({
  dataAccuracy: z
    .number()
    .min(1)
    .max(5)
    .describe("1=major errors, 5=fully accurate data"),
  dataCompleteness: z
    .number()
    .min(1)
    .max(5)
    .describe("1=missing key data, 5=comprehensive coverage"),
  notes: z.string().optional().describe("Specific observations about data quality"),
});

const writerReviewSchema = z.object({
  readability: z
    .number()
    .min(1)
    .max(5)
    .describe("1=hard to read, 5=flows naturally"),
  engagement: z
    .number()
    .min(1)
    .max(5)
    .describe("1=boring/generic, 5=compelling and specific"),
  brandVoice: z
    .number()
    .min(1)
    .max(5)
    .describe("1=off-brand, 5=perfectly matches brand voice"),
  notes: z.string().optional().describe("Specific observations about content quality"),
});

const seoReviewerReviewSchema = z.object({
  keywordNaturalness: z
    .number()
    .min(1)
    .max(5)
    .describe("1=feels stuffed, 5=keywords feel natural"),
  metaQuality: z
    .number()
    .min(1)
    .max(5)
    .describe("1=poor subtitle/meta, 5=compelling and optimized"),
  notes: z.string().optional().describe("Specific observations about SEO quality"),
});

const brandReviewerReviewSchema = z.object({
  terminologyCorrect: z
    .number()
    .min(1)
    .max(5)
    .describe("1=wrong terms used, 5=perfect terminology"),
  toneConsistent: z
    .number()
    .min(1)
    .max(5)
    .describe("1=inconsistent tone, 5=consistent friendly tone"),
  localAuthenticity: z
    .number()
    .min(1)
    .max(5)
    .describe("1=generic content, 5=feels authentically local"),
  notes: z.string().optional().describe("Specific observations about brand compliance"),
});

// =============================================================================
// ISSUE TRACKING SCHEMA
// =============================================================================

const issueSchema = z.object({
  stage: z
    .enum(["researcher", "writer", "seo-reviewer", "brand-reviewer"])
    .describe("Which agent stage the issue was found in"),
  severity: z
    .enum(["minor", "major", "blocker"])
    .describe("minor=cosmetic, major=needs fix, blocker=can't publish"),
  description: z.string().describe("Clear description of the issue"),
  suggestedFix: z.string().optional().describe("How to fix this issue"),
  promptFixApplied: z
    .boolean()
    .optional()
    .describe("True if a prompt change was made to address this"),
});

// =============================================================================
// MAIN HUMAN REVIEW SCHEMA
// =============================================================================

export const humanReviewSchema = z.object({
  schemaVersion: z.literal("1.0.0").describe("Schema version for compatibility"),
  reviewedAt: z.string().datetime().describe("ISO 8601 timestamp of review"),
  reviewer: z.string().describe("Name or identifier of the reviewer"),

  neighborhoodId: z.string().describe("ID of the neighborhood being reviewed"),
  pipelineRunDate: z
    .string()
    .describe("Date when the pipeline was run (for traceability)"),

  stageReviews: z
    .object({
      researcher: researcherReviewSchema.optional(),
      writer: writerReviewSchema.optional(),
      seoReviewer: seoReviewerReviewSchema.optional(),
      brandReviewer: brandReviewerReviewSchema.optional(),
    })
    .describe("Per-stage quality ratings"),

  overallScore: z
    .number()
    .min(1)
    .max(5)
    .describe("Overall quality: 1=poor, 3=acceptable, 5=excellent"),
  publishReady: z
    .boolean()
    .describe("True if content is ready to publish without changes"),

  issuesFound: z
    .array(issueSchema)
    .describe("List of issues discovered during review"),

  generalNotes: z
    .string()
    .optional()
    .describe("Any additional observations or recommendations"),
});

export type HumanReview = z.infer<typeof humanReviewSchema>;
export type ResearcherReview = z.infer<typeof researcherReviewSchema>;
export type WriterReview = z.infer<typeof writerReviewSchema>;
export type SeoReviewerReview = z.infer<typeof seoReviewerReviewSchema>;
export type BrandReviewerReview = z.infer<typeof brandReviewerReviewSchema>;
export type Issue = z.infer<typeof issueSchema>;
