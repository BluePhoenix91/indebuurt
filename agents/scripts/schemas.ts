/**
 * Schema Definitions for indebuurt.be Agent Pipeline
 *
 * This file defines the Zod schemas for each stage of the agent pipeline:
 * - ResearcherOutput: Factual data from database queries (no prose, no icons)
 * - WriterOutput: Adds narrative content and presentation (icons, intros)
 * - FinalOutput: Matches Astro Content Collections schema exactly
 *
 * Schema version: 1.0.0
 */

import { z } from "zod";

// =============================================================================
// SHARED PRIMITIVES
// =============================================================================

const coordinatesSchema = z.object({
  lat: z.number().describe("Latitude in WGS84"),
  lon: z.number().describe("Longitude in WGS84"),
});

// =============================================================================
// RESEARCHER OUTPUT SCHEMA
// =============================================================================
// The Researcher queries PostGIS and outputs structured factual data.
// No prose, no icons, no presentation logic — pure data.

const researcherPOISchema = z.object({
  name: z.string().describe("Name of the POI from OSM"),
  street: z.string().describe("Street name"),
  streetNumber: z.string().describe("House number"),
  bus: z.string().optional().describe("Bus/unit number if applicable"),
  municipality: z.string().describe("Municipality name"),
  postalCode: z.string().describe("Belgian postal code"),
  coordinates: coordinatesSchema,
  distanceMeters: z.number().describe("Distance from neighborhood center in meters"),
  walkingTimeMinutes: z.number().describe("Estimated walking time in minutes"),
  source: z.string().describe("Data source reference, e.g., 'OSM 2024-12'"),
});

const researcherDogParkSchema = z.object({
  name: z.string().describe("Name of the dog park or green space"),
  coordinates: coordinatesSchema,
  distanceMeters: z.number().describe("Distance from neighborhood center in meters"),
  walkingTimeMinutes: z.number().describe("Estimated walking time in minutes"),
  isFenced: z.boolean().describe("Whether the area is fenced/enclosed"),
  hasWater: z.boolean().describe("Whether there's water access for dogs"),
  surface: z.string().optional().describe("Surface type: grass, gravel, mixed, sand"),
  // Additional optional features (sparse OSM coverage)
  isAccessible: z.enum(["yes", "no", "limited"]).optional().describe("Wheelchair accessibility (19% coverage)"),
  isLit: z.boolean().optional().describe("Whether the area has lighting for evening use (2% coverage)"),
  openingHours: z.string().optional().describe("Opening hours if restricted, e.g., '24/7' or '08:00-21:00' (3% coverage)"),
  hasSmallDogArea: z.boolean().optional().describe("Whether there's a separate area for small dogs (2% coverage)"),
  source: z.string().describe("Data source reference"),
});

const researcherStatisticsSchema = z.object({
  inhabitants: z.number().describe("Number of inhabitants"),
  medianHousePrice: z.number().nullable().describe("Median house price in euros, null if unavailable"),
  pricePerSqm: z.number().nullable().describe("Price per square meter in euros, null if unavailable"),
  availableHomes: z.number().describe("Number of homes currently for sale"),
  populationDensity: z.number().nullable().describe("Population per km2"),
  source: z.string().describe("Data source reference, e.g., 'Statbel 2024-Q3'"),
});

const researcherNeighborhoodContextSchema = z.object({
  neighboringNeighborhoods: z.array(z.object({
    id: z.string().describe("Neighborhood slug/ID"),
    name: z.string().describe("Display name"),
    distanceMeters: z.number().describe("Distance to center"),
  })).describe("Adjacent neighborhoods"),
  cityContext: z.object({
    name: z.string().describe("City name"),
    totalNeighborhoods: z.number().describe("Total neighborhoods in this city"),
  }),
});

export const researcherOutputSchema = z.object({
  schemaVersion: z.literal("1.0.0").describe("Schema version for compatibility checking"),
  generatedAt: z.string().datetime().describe("ISO 8601 timestamp of generation"),

  // Identification
  neighborhoodId: z.string().describe("Unique neighborhood identifier/slug"),
  neighborhoodName: z.string().describe("Official neighborhood name"),
  city: z.string().describe("City the neighborhood belongs to"),
  postalCode: z.string().describe("Primary postal code"),

  // Geographic data
  centerCoordinates: coordinatesSchema.describe("Geographic center of the neighborhood"),
  boundingBox: z.object({
    north: z.number(),
    south: z.number(),
    east: z.number(),
    west: z.number(),
  }).optional().describe("Bounding box for map display"),

  // POI data - factual, no presentation
  vets: z.array(researcherPOISchema).describe("Veterinary practices within/near neighborhood"),
  petStores: z.array(researcherPOISchema).describe("Pet stores within/near neighborhood"),
  dogParks: z.array(researcherDogParkSchema).describe("Dog parks and suitable green spaces"),
  parks: z.array(z.object({
    name: z.string(),
    coordinates: coordinatesSchema,
    distanceMeters: z.number(),
    areaHectares: z.number().optional(),
    source: z.string(),
  })).describe("General parks and green spaces"),

  // Counts and summaries
  poiCounts: z.object({
    vets: z.number(),
    petStores: z.number(),
    dogParks: z.number(),
    parks: z.number(),
    supermarkets: z.number(),
    pharmacies: z.number(),
    schools: z.number(),
    busStops: z.number(),
    trainStations: z.number(),
  }).describe("Quick counts for each POI category"),

  // Statistics
  statistics: researcherStatisticsSchema,

  // Context
  context: researcherNeighborhoodContextSchema,

  // Data quality metadata
  dataSources: z.array(z.object({
    name: z.string().describe("Source name, e.g., 'OpenStreetMap'"),
    date: z.string().describe("Data extraction/update date"),
    coverage: z.string().optional().describe("Notes on coverage or limitations"),
  })).describe("All data sources used"),
});

export type ResearcherOutput = z.infer<typeof researcherOutputSchema>;

// =============================================================================
// WRITER OUTPUT SCHEMA
// =============================================================================
// The Writer receives ResearcherOutput and adds:
// - All narrative content (intros, descriptions, benefits)
// - Presentation logic (icons, formatted distances)
// - Editorial decisions (which POIs to highlight, tone)

const labelSchema = z.object({
  text: z.string().describe("Label text, e.g., 'Historisch centrum'"),
  icon: z.string().describe("FontAwesome icon class, e.g., 'fa-regular fa-monument'"),
});

const valueCardSchema = z.object({
  icon: z.string().describe("FontAwesome icon class"),
  title: z.string().describe("Card title, e.g., 'Hondenparken'"),
  distance: z.string().describe("Formatted distance, e.g., '9 mins'"),
  distanceIcon: z.string().optional().describe("Icon for distance type, e.g., 'fa-regular fa-person-walking'"),
  description: z.string().describe("Brief description of the amenity situation"),
  detail: z.string().describe("Additional detail or context"),
});

const featureSchema = z.object({
  text: z.string().describe("Feature description"),
  icon: z.string().describe("FontAwesome icon class"),
});

const writerDogParkSchema = z.object({
  icon: z.string().describe("FontAwesome icon class"),
  name: z.string(),
  distance: z.string().describe("Formatted distance, e.g., '5 mins'"),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
  features: z.array(featureSchema).describe("Notable features of this park"),
});

const writerVetSchema = z.object({
  icon: z.string().describe("FontAwesome icon class"),
  name: z.string(),
  street: z.string(),
  streetNumber: z.string(),
  bus: z.string().optional(),
  municipality: z.string(),
  postalCode: z.string(),
  distance: z.string().describe("Formatted distance, e.g., '4 mins'"),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
});

const writerPetStoreSchema = z.object({
  icon: z.string().describe("FontAwesome icon class"),
  name: z.string(),
  street: z.string(),
  streetNumber: z.string(),
  bus: z.string().optional(),
  municipality: z.string(),
  postalCode: z.string(),
  distance: z.string().describe("Formatted distance, e.g., '14 mins'"),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
});

export const writerOutputSchema = z.object({
  schemaVersion: z.literal("1.0.0"),
  generatedAt: z.string().datetime(),

  // Identity (passed through from Researcher)
  id: z.string().describe("Neighborhood slug for URLs"),
  city: z.string(),
  name: z.string().describe("Display name"),
  postalCode: z.string(),

  // Writer-generated content
  subtitle: z.string()
    .describe("Compelling subtitle for the neighborhood (target: 80-120 chars)"),

  dateAdded: z.string().describe("ISO date when page was created"),
  inhabitants: z.number(),

  labels: z.array(labelSchema)
    .min(2).max(5)
    .describe("2-5 characteristic labels for the neighborhood"),

  intro: z.string()
    .describe("Main introduction text (target: 400-800 words). Should cover character, dog-friendliness, trade-offs."),

  coordinates: coordinatesSchema.extend({
    zoom: z.number().describe("Default map zoom level"),
  }),

  valueCards: z.array(valueCardSchema)
    .min(4).max(8)
    .describe("4-8 value proposition cards highlighting key amenities"),

  facilities: z.object({
    intro: z.string().describe("Overview of dog-relevant facilities"),
  }),

  dogParks: z.object({
    intro: z.string().describe("Introduction to dog parks situation"),
    parks: z.array(writerDogParkSchema),
  }),

  vets: z.object({
    intro: z.string().describe("Introduction to veterinary options"),
    practices: z.array(writerVetSchema),
  }),

  petStores: z.object({
    intro: z.string().describe("Introduction to pet store options"),
    stores: z.array(writerPetStoreSchema),
  }),

  dailyLife: z.object({
    title: z.string().describe("Section title, typically about daily life with a dog"),
    intro: z.string().describe("What daily life looks like for dog owners here"),
    benefits: z.array(z.string())
      .min(3).max(7)
      .describe("3-7 key benefits/characteristics as bullet points"),
  }),

  contributionCTA: z.object({
    heading: z.string(),
    intro: z.string(),
    typeformId: z.string(),
  }),

  statistics: z.object({
    intro: z.string().describe("Introduction to the statistics section"),
    medianPrice: z.number().nullable().describe("Median house price, null if unavailable"),
    inhabitants: z.number(),
    availableHomes: z.number().nullable().describe("Available homes for sale, null if unavailable"),
    pricePerSqm: z.number().nullable().describe("Price per square meter, null if unavailable"),
  }),

  houses: z.object({
    intro: z.string().describe("Introduction to the housing search section"),
    hasOwnPostalCode: z.boolean(),
  }),

  neighboringNeighborhoods: z.array(z.string()).optional()
    .describe("IDs of neighboring neighborhoods for internal linking"),
});

export type WriterOutput = z.infer<typeof writerOutputSchema>;

// =============================================================================
// FINAL OUTPUT SCHEMA
// =============================================================================
// This must exactly match the Astro Content Collections schema.
// It's essentially the WriterOutput after SEO and Brand review passes.

export const finalOutputSchema = z.object({
  id: z.string(),
  city: z.string(),
  name: z.string(),
  subtitle: z.string(),
  dateAdded: z.string(),
  postalCode: z.string(),
  inhabitants: z.number(),
  labels: z.array(z.object({
    text: z.string(),
    icon: z.string(),
  })),
  intro: z.string(),
  coordinates: z.object({
    lat: z.number(),
    lon: z.number(),
    zoom: z.number(),
  }),
  valueCards: z.array(z.object({
    icon: z.string(),
    title: z.string(),
    distance: z.string(),
    distanceIcon: z.string().optional(),
    description: z.string(),
    detail: z.string(),
  })),
  facilities: z.object({
    intro: z.string(),
  }),
  dogParks: z.object({
    intro: z.string(),
    parks: z.array(z.object({
      icon: z.string(),
      name: z.string(),
      distance: z.string(),
      distanceIcon: z.string().optional(),
      coordinates: z.object({
        lat: z.number(),
        lon: z.number(),
      }),
      features: z.array(z.object({
        text: z.string(),
        icon: z.string(),
      })),
    })),
  }),
  vets: z.object({
    intro: z.string(),
    practices: z.array(z.object({
      icon: z.string(),
      name: z.string(),
      street: z.string(),
      streetNumber: z.string(),
      bus: z.string().optional(),
      municipality: z.string(),
      postalCode: z.string(),
      distance: z.string(),
      distanceIcon: z.string().optional(),
      coordinates: z.object({
        lat: z.number(),
        lon: z.number(),
      }),
    })),
  }),
  petStores: z.object({
    intro: z.string(),
    stores: z.array(z.object({
      icon: z.string(),
      name: z.string(),
      street: z.string(),
      streetNumber: z.string(),
      bus: z.string().optional(),
      municipality: z.string(),
      postalCode: z.string(),
      distance: z.string(),
      distanceIcon: z.string().optional(),
      coordinates: z.object({
        lat: z.number(),
        lon: z.number(),
      }),
    })),
  }),
  dailyLife: z.object({
    title: z.string(),
    intro: z.string(),
    benefits: z.array(z.string()),
  }),
  contributionCTA: z.object({
    heading: z.string(),
    intro: z.string(),
    typeformId: z.string(),
  }),
  statistics: z.object({
    intro: z.string(),
    medianPrice: z.number().nullable(),
    inhabitants: z.number(),
    availableHomes: z.number().nullable(),
    pricePerSqm: z.number().nullable(),
  }),
  houses: z.object({
    intro: z.string(),
    hasOwnPostalCode: z.boolean(),
  }),
  neighboringNeighborhoods: z.array(z.string()).optional(),
});

export type FinalOutput = z.infer<typeof finalOutputSchema>;

// =============================================================================
// SEO REVIEWER OUTPUT SCHEMA
// =============================================================================
// The SEO Reviewer receives WriterOutput and:
// - Optimizes text fields for search visibility
// - Validates internal links exist in database
// - Logs all changes made with before/after
// - Outputs a quality score and pass/fail status

const seoChangeLogSchema = z.object({
  field: z.string().describe("JSON path to the modified field, e.g., 'subtitle' or 'dogParks.intro'"),
  before: z.string().describe("Original text before modification"),
  after: z.string().describe("Modified text after SEO optimization"),
  reason: z.enum([
    "subtitle_length",        // Subtitle too short/long for meta description
    "keyword_density",        // Primary keywords missing or insufficient
    "intro_structure",        // Intro missing key SEO elements
    "section_intro_thin",     // Section intro too short for SEO value
    "local_keyword_missing",  // Missing city/neighborhood name where appropriate
    "readability",            // Sentence structure too complex
    "value_card_clarity",     // Value card description not clear
    "benefit_specificity",    // Benefit too vague, not searchable
    "cta_optimization",       // CTA text not compelling
    "label_clarity",          // Label text not clear or searchable
  ]).describe("Category of SEO improvement made"),
});

const seoValidationIssueSchema = z.object({
  field: z.string().describe("Field with validation issue"),
  issue: z.string().describe("Description of the issue"),
  severity: z.enum(["error", "warning", "info"]).describe("Issue severity"),
});

const seoScoreBreakdownSchema = z.object({
  subtitleScore: z.number().min(0).max(15).describe("Subtitle/meta description quality (0-15)"),
  introScore: z.number().min(0).max(25).describe("Main intro SEO quality (0-25)"),
  keywordScore: z.number().min(0).max(20).describe("Keyword usage and density (0-20)"),
  sectionIntrosScore: z.number().min(0).max(15).describe("Section intros SEO value (0-15)"),
  localRelevanceScore: z.number().min(0).max(15).describe("Local SEO signals (0-15)"),
  internalLinkingScore: z.number().min(0).max(10).describe("Internal link validity (0-10)"),
});

export const seoReviewerOutputSchema = writerOutputSchema.extend({
  seoReview: z.object({
    reviewedAt: z.string().datetime().describe("ISO 8601 timestamp of review"),
    qualityScore: z.number().min(0).max(100).describe("SEO quality score 0-100"),
    passedSEO: z.boolean().describe("True if qualityScore >= 70"),
    changesLog: z.array(seoChangeLogSchema).describe("All modifications made by SEO reviewer"),
    validationIssues: z.array(seoValidationIssueSchema).optional()
      .describe("Issues found that couldn't be auto-fixed (e.g., invalid internal links)"),
    scoreBreakdown: seoScoreBreakdownSchema.describe("Score breakdown by category"),
  }),
});

export type SEOReviewerOutput = z.infer<typeof seoReviewerOutputSchema>;

// =============================================================================
// BRAND REVIEWER OUTPUT SCHEMA
// =============================================================================
// The Brand Reviewer receives SEOReviewerOutput and:
// - Validates terminology compliance (baasjes, hondenspeelweide, etc.)
// - Checks tone consistency (friendly, second-person, not corporate)
// - Verifies local authenticity (specific names, insider details)
// - Assesses narrative naturalness (flows like prose, not a fact dump)
// - Evaluates sparse data handling (graceful acknowledgment pattern)
// - Logs all changes and outputs a quality score

const brandChangeLogSchema = z.object({
  field: z.string().describe("JSON path to the modified field, e.g., 'intro' or 'petStores.intro'"),
  before: z.string().describe("Original text before modification"),
  after: z.string().describe("Modified text after brand review"),
  reason: z.enum([
    "terminology_violation",      // Used avoided term (eigenaars instead of baasjes)
    "tone_formal",                // Too corporate or formal (u kunt, men dient)
    "tone_promotional",           // Too salesy (ontdek de mogelijkheden)
    "missing_local_detail",       // Generic, not specific to neighborhood
    "narrative_list_like",        // Reads like a list, not prose
    "sparse_data_unhandled",      // Gap not acknowledged gracefully
    "perspective_inconsistent",   // Mixed je/u/wij forms
    "english_term_used",          // English where Dutch preferred
  ]).describe("Category of brand improvement made"),
});

const brandValidationIssueSchema = z.object({
  field: z.string().describe("Field with validation issue"),
  issue: z.string().describe("Description of the issue"),
  severity: z.enum(["error", "warning", "info"]).describe("Issue severity"),
});

const brandScoreBreakdownSchema = z.object({
  terminologyScore: z.number().min(0).max(30).describe("Terminology compliance (0-30)"),
  toneVoiceScore: z.number().min(0).max(25).describe("Tone and voice consistency (0-25)"),
  localAuthenticityScore: z.number().min(0).max(20).describe("Local authenticity markers (0-20)"),
  narrativeNaturalnessScore: z.number().min(0).max(15).describe("Narrative naturalness (0-15)"),
  sparseDataHandlingScore: z.number().min(0).max(10).describe("Sparse data handling quality (0-10)"),
});

const brandTerminologyAnalysisSchema = z.object({
  avoidedTermsFound: z.array(z.object({
    term: z.string().describe("The avoided term that was found"),
    field: z.string().describe("JSON path where the term was found"),
    preferred: z.string().describe("The preferred term to use instead"),
  })).describe("List of terminology violations detected"),
  preferredTermsPresent: z.array(z.string())
    .describe("List of preferred terms correctly used in content"),
  allowedExceptionsUsed: z.array(z.string())
    .describe("List of allowed exception phrases used (e.g., 'buurtgevoel')"),
});

const brandToneAnalysisSchema = z.object({
  perspectiveForm: z.enum(["je_jouw", "u", "wij", "mixed"])
    .describe("Dominant perspective form used in content"),
  formalPhrasesFound: z.array(z.string())
    .describe("Formal/corporate phrases detected (e.g., 'u kunt', 'men dient')"),
  promotionalPhrasesFound: z.array(z.string())
    .describe("Promotional phrases detected (e.g., 'ontdek de mogelijkheden')"),
  friendlyMarkersCount: z.number()
    .describe("Count of friendly tone markers (je vindt, handig voor, etc.)"),
});

const brandLocalAuthenticityAnalysisSchema = z.object({
  uniquePlaceNamesCount: z.number()
    .describe("Count of unique POI/street names mentioned in narrative"),
  localTipsFound: z.array(z.string())
    .describe("Local tips detected (e.g., 'via de Coupure', 'richting de waterkant')"),
  neighborhoodObservations: z.array(z.string())
    .describe("Neighborhood-specific observations detected"),
});

const brandNarrativeNaturalnessAnalysisSchema = z.object({
  sentenceStartVariety: z.number().min(0).max(1)
    .describe("Score 0-1 indicating variety in sentence starters"),
  averageSentenceLength: z.number()
    .describe("Average sentence length in words"),
  listLikePatternsFound: z.number()
    .describe("Count of list-like patterns (e.g., 'Er zijn X. Er zijn Y.')"),
});

const brandSparseDataAnalysisSchema = z.object({
  gapsDetected: z.array(z.string())
    .describe("Data gaps detected (e.g., 'no pet store in neighborhood')"),
  gapsHandledGracefully: z.number()
    .describe("Count of gaps handled with acknowledgment + pivot + alternative"),
  gapsHandledPoorly: z.number()
    .describe("Count of gaps handled poorly (just 'helaas geen...')"),
});

const brandAnalysisSchema = z.object({
  terminology: brandTerminologyAnalysisSchema,
  tone: brandToneAnalysisSchema,
  localAuthenticity: brandLocalAuthenticityAnalysisSchema,
  narrativeNaturalness: brandNarrativeNaturalnessAnalysisSchema,
  sparseDataHandling: brandSparseDataAnalysisSchema,
});

export const brandReviewerOutputSchema = seoReviewerOutputSchema.extend({
  brandReview: z.object({
    reviewedAt: z.string().datetime().describe("ISO 8601 timestamp of review"),
    qualityScore: z.number().min(0).max(100).describe("Brand quality score 0-100"),
    passedBrand: z.boolean().describe("True if qualityScore >= 70"),
    changesLog: z.array(brandChangeLogSchema)
      .describe("All modifications made by Brand reviewer"),
    validationIssues: z.array(brandValidationIssueSchema).optional()
      .describe("Issues found that require human review"),
    scoreBreakdown: brandScoreBreakdownSchema
      .describe("Score breakdown by category"),
    analysis: brandAnalysisSchema
      .describe("Detailed analysis for debugging and feedback"),
  }),
});

export type BrandReviewerOutput = z.infer<typeof brandReviewerOutputSchema>;

// =============================================================================
// QUALITY REVIEWER OUTPUT SCHEMA
// =============================================================================
// The Quality Reviewer combines SEO and Brand review into a single stage.
// It receives WriterOutput and outputs a unified quality review.
// This reduces the pipeline from 4 stages to 3 stages.

// Combined change log that accepts both SEO and Brand change reasons
const qualityChangeLogSchema = z.object({
  field: z.string().describe("JSON path to the modified field, e.g., 'subtitle' or 'petStores.intro'"),
  before: z.string().describe("Original text before modification"),
  after: z.string().describe("Modified text after quality review"),
  reason: z.enum([
    // SEO reasons
    "subtitle_length",        // Subtitle too short/long for meta description
    "keyword_density",        // Primary keywords missing or insufficient
    "intro_structure",        // Intro missing key SEO elements
    "section_intro_thin",     // Section intro too short for SEO value
    "local_keyword_missing",  // Missing city/neighborhood name where appropriate
    "readability",            // Sentence structure too complex
    "value_card_clarity",     // Value card description not clear
    "benefit_specificity",    // Benefit too vague, not searchable
    "cta_optimization",       // CTA text not compelling
    "label_clarity",          // Label text not clear or searchable
    // Brand reasons
    "terminology_violation",      // Used avoided term (eigenaars instead of baasjes)
    "tone_formal",                // Too corporate or formal (u kunt, men dient)
    "tone_promotional",           // Too salesy (ontdek de mogelijkheden)
    "missing_local_detail",       // Generic, not specific to neighborhood
    "narrative_list_like",        // Reads like a list, not prose
    "sparse_data_unhandled",      // Gap not acknowledged gracefully
    "perspective_inconsistent",   // Mixed je/u/wij forms
    "english_term_used",          // English where Dutch preferred
  ]).describe("Category of quality improvement made"),
});

const qualityValidationIssueSchema = z.object({
  field: z.string().describe("Field with validation issue"),
  issue: z.string().describe("Description of the issue"),
  severity: z.enum(["error", "warning", "info"]).describe("Issue severity"),
  domain: z.enum(["seo", "brand"]).describe("Which review domain identified this issue"),
});

export const qualityReviewerOutputSchema = writerOutputSchema.extend({
  qualityReview: z.object({
    reviewedAt: z.string().datetime().describe("ISO 8601 timestamp of review"),
    qualityScore: z.number().min(0).max(100).describe("Combined quality score: (seoScore + brandScore) / 2"),
    passedQuality: z.boolean().describe("True if qualityScore >= 70"),

    // Individual domain scores
    seoScore: z.number().min(0).max(100).describe("SEO quality score 0-100"),
    brandScore: z.number().min(0).max(100).describe("Brand quality score 0-100"),

    // Breakdowns by category (reuse existing schemas)
    seoBreakdown: seoScoreBreakdownSchema.describe("SEO score breakdown by category"),
    brandBreakdown: brandScoreBreakdownSchema.describe("Brand score breakdown by category"),

    // Combined change log and validation issues
    changesLog: z.array(qualityChangeLogSchema)
      .describe("All modifications made by Quality reviewer (both SEO and Brand)"),
    validationIssues: z.array(qualityValidationIssueSchema).optional()
      .describe("Issues found that couldn't be auto-fixed or require human review"),

    // Detailed analysis (from Brand domain)
    analysis: brandAnalysisSchema
      .describe("Detailed analysis for debugging and feedback"),
  }),
});

export type QualityReviewerOutput = z.infer<typeof qualityReviewerOutputSchema>;
