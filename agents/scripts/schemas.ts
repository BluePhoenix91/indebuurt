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
  surface: z.string().optional().describe("Surface type: grass, gravel, mixed"),
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
