import type { Neighborhood } from "../neighborhoods";
import { gentBinnenstad } from "./gent-binnenstad";
import { gentBloemekenswijk } from "./gent-bloemekenswijk";
import { gentBrugsePoort } from "./gent-brugse-poort";
import { gentDampoort } from "./gent-dampoort";
import { gentElisabethbegijnhof } from "./gent-elisabethbegijnhof";
import { gentKanaaldorpen } from "./gent-kanaaldorpen";
import { gentMachariusHeirnis } from "./gent-macharius-heirnis";
import { gentMuide } from "./gent-muide";
import { gentRabot } from "./gent-rabot";
import { gentSluizekenTolhuisHam } from "./gent-sluizeken-tolhuis-ham";
import { gentWatersportbaanEkkergem } from "./gent-watersportbaan-ekkergem";
import { gentWondelgem } from "./gent-wondelgem";

// Import all neighborhood files here
// When adding a new neighborhood, create a new file and add it to this list
export const neighborhoods: Record<string, Neighborhood> = {
  "gent-binnenstad": gentBinnenstad,
  "gent-bloemekenswijk": gentBloemekenswijk,
  "gent-brugse-poort": gentBrugsePoort,
  "gent-dampoort": gentDampoort,
  "gent-elisabethbegijnhof": gentElisabethbegijnhof,
  "gent-kanaaldorpen": gentKanaaldorpen,
  "gent-macharius-heirnis": gentMachariusHeirnis,
  "gent-muide": gentMuide,
  "gent-rabot": gentRabot,
  "gent-sluizeken-tolhuis-ham": gentSluizekenTolhuisHam,
  "gent-watersportbaan-ekkergem": gentWatersportbaanEkkergem,
  "gent-wondelgem": gentWondelgem,
  // Add more neighborhoods here as you create them:
  // "gent-centrum": gentCentrum,
  // etc.
};
