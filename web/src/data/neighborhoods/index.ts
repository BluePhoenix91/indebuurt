import type { Neighborhood } from "../neighborhoods";
import { antwerpenZuid } from "./antwerpen-zuid";
import { gentBloemekenswijk } from "./gent-bloemekenswijk";
import { gentMuide } from "./gent-muide";
import { gentRabot } from "./gent-rabot";
import { gentSluizekenTolhuisHam } from "./gent-sluizeken-tolhuis-ham";
import { gentWondelgem } from "./gent-wondelgem";

// Import all neighborhood files here
// When adding a new neighborhood, create a new file and add it to this list
export const neighborhoods: Record<string, Neighborhood> = {
  "antwerpen-zuid": antwerpenZuid,
  "gent-bloemekenswijk": gentBloemekenswijk,
  "gent-muide": gentMuide,
  "gent-rabot": gentRabot,
  "gent-sluizeken-tolhuis-ham": gentSluizekenTolhuisHam,
  "gent-wondelgem": gentWondelgem,
  // Add more neighborhoods here as you create them:
  // "gent-centrum": gentCentrum,
  // etc.
};
