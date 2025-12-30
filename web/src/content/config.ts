import { defineCollection, z } from "astro:content";
import { glob } from "astro/loaders";

// Shared schemas for nested objects
const coordinatesSchema = z.object({
  lat: z.number(),
  lon: z.number(),
});

const coordinatesWithZoomSchema = coordinatesSchema.extend({
  zoom: z.number(),
});

const labelSchema = z.object({
  text: z.string(),
  icon: z.string(),
});

const valueCardSchema = z.object({
  icon: z.string(),
  title: z.string(),
  distance: z.string(),
  distanceIcon: z.string().optional(),
  description: z.string(),
  detail: z.string(),
});

const featureSchema = z.object({
  text: z.string(),
  icon: z.string(),
});

const dogParkSchema = z.object({
  icon: z.string(),
  name: z.string(),
  distance: z.string(),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
  features: z.array(featureSchema),
});

const vetSchema = z.object({
  icon: z.string(),
  name: z.string(),
  street: z.string(),
  streetNumber: z.string(),
  bus: z.string().optional(),
  municipality: z.string(),
  postalCode: z.string(),
  distance: z.string(),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
});

const petStoreSchema = z.object({
  icon: z.string(),
  name: z.string(),
  street: z.string(),
  streetNumber: z.string(),
  bus: z.string().optional(),
  municipality: z.string(),
  postalCode: z.string(),
  distance: z.string(),
  distanceIcon: z.string().optional(),
  coordinates: coordinatesSchema,
});

// Main neighborhood schema
const neighborhoodSchema = z.object({
  id: z.string(),
  city: z.string(),
  name: z.string(),
  subtitle: z.string(),
  dateAdded: z.string(),
  postalCode: z.string(),
  inhabitants: z.number(),
  labels: z.array(labelSchema),
  intro: z.string(),
  coordinates: coordinatesWithZoomSchema,
  valueCards: z.array(valueCardSchema),
  facilities: z.object({
    intro: z.string(),
  }),
  dogParks: z.object({
    intro: z.string(),
    parks: z.array(dogParkSchema),
  }),
  vets: z.object({
    intro: z.string(),
    practices: z.array(vetSchema),
  }),
  petStores: z.object({
    intro: z.string(),
    stores: z.array(petStoreSchema),
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
    medianPrice: z.number(),
    inhabitants: z.number(),
    availableHomes: z.number(),
    pricePerSqm: z.number(),
  }),
  houses: z.object({
    intro: z.string(),
    hasOwnPostalCode: z.boolean(),
  }),
  neighboringNeighborhoods: z.array(z.string()).optional(),
});

// Define the neighborhoods collection
const neighborhoods = defineCollection({
  loader: glob({ pattern: "**/*.json", base: "./src/content/neighborhoods" }),
  schema: neighborhoodSchema,
});

export const collections = {
  neighborhoods,
};

// Export the schema type for use in components
export type NeighborhoodData = z.infer<typeof neighborhoodSchema>;
