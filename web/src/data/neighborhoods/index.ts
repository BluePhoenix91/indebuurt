import type { Neighborhood } from "../neighborhoods";
import { antwerpenZuid } from "./antwerpen-zuid";
import { gentMuide } from "./gent-muide";
import { gentRabot } from "./gent-rabot";

// Import all neighborhood files here
// When adding a new neighborhood, create a new file and add it to this list
export const neighborhoods: Record<string, Neighborhood> = {
  "antwerpen-zuid": antwerpenZuid,
  "gent-muide": gentMuide,
  "gent-rabot": gentRabot,
  // Add more neighborhoods here as you create them:
  // "gent-centrum": gentCentrum,
  // etc.
};
