/**
 * Neighborhood data utilities
 *
 * This module provides a unified interface for accessing neighborhood data
 * from Astro Content Collections.
 */

import { getCollection, getEntry } from "astro:content";
import type { NeighborhoodData } from "../content/config";

// Re-export the type for convenience
export type { NeighborhoodData };

// Type alias for backward compatibility
export type Neighborhood = NeighborhoodData;

/**
 * Get all neighborhood slugs (IDs)
 */
export async function getAllNeighborhoodSlugs(): Promise<string[]> {
  const collectionEntries = await getCollection("neighborhoods");
  return collectionEntries.map((entry) => entry.data.id);
}

/**
 * Get a single neighborhood by slug
 */
export async function getNeighborhood(
  slug: string
): Promise<NeighborhoodData | null> {
  try {
    const entry = await getEntry("neighborhoods", slug);
    if (entry) {
      return entry.data;
    }
  } catch {
    // Entry not found
  }
  return null;
}

/**
 * Get all neighborhoods as a record
 */
export async function getAllNeighborhoods(): Promise<
  Record<string, NeighborhoodData>
> {
  const result: Record<string, NeighborhoodData> = {};
  const collectionEntries = await getCollection("neighborhoods");

  for (const entry of collectionEntries) {
    result[entry.data.id] = entry.data;
  }

  return result;
}
