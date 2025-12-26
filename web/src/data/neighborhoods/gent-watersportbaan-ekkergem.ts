import type { Neighborhood } from "../neighborhoods";

export const gentWatersportbaanEkkergem: Neighborhood = {
  id: "gent-watersportbaan-ekkergem",
  name: "Watersportbaan - Ekkergem",
  subtitle:
    "Voor baasjes die houden van groen, water én een jong, dynamisch wijkleven met dorpsgevoel",
  dateAdded: "2025-12-14",
  postalCode: "9000",
  inhabitants: 13500,
  labels: [
    { text: "Groen recreatiegebied", icon: "fa-regular fa-water" },
    { text: "Jong en dynamisch", icon: "fa-regular fa-people-group" },
    { text: "Dorpsgevoel", icon: "fa-regular fa-home-heart" },
  ],
  intro: `Watersportbaan - Ekkergem is een wijk met vier gezichten: het dorpsgevoel van Ekkergem, de sociale hoogbouw aan de Watersportbaan (Neermeersen), de historische Bijlokesite én het groene recreatiegebied De Blaarmeersen. Wat deze wijk uniek maakt voor hondenbaasjes, is dat De Blaarmeersen — een 86 hectare groot recreatiegebied met water, wandelpaden en sportvelden — letterlijk in het hart van de wijk ligt (23 meter van het wijkcentrum).

Voor wie droomt van uitgebreide wandelingen met hun hond door groen en langs water, is dit een paradijs. De Blaarmeersen alleen al biedt eindeloos wandelplezier, maar de wijk telt maar liefst 19 parken binnen haar grenzen — van het Natuurpark Overmeers tot het Ekkergempark, van de Groenzone ruimlijk Malem tot het Daskalidèspark. Variatie genoeg voor dagelijkse uitlaatrondjes zonder ooit dezelfde route twee keer te lopen.

Wat betreft hondenvoorzieningen: een omheinde hondenspeelweide in de wijk zelf (op 4 minuten wandelen), Tom & Co ook in de wijk (12 minuten wandelen), maar voor de dierenarts moet je naar het naburige Stationsbuurt-Noord (19 minuten wandelen). De combinatie van jong wijkleven (1 op 4 bewoners is twintigjarig), dorpsgevoel, historisch erfgoed én het grootste stedelijke recreatiegebied van Gent maakt deze wijk bijzonder — ideaal voor baasjes die groen, water en gemeenschap zoeken.`,
  coordinates: {
    lat: 51.0494,
    lon: 3.6935,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "4 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Hondenspeelweide in de wijk zelf",
      detail: "Ongeveer 330 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "19 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Dierenarts in naburig Stationsbuurt-Noord",
      detail: "Ongeveer 1,5 kilometer",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "12 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Tom & Co in de wijk zelf",
      detail: "Ongeveer 940 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "De Blaarmeersen (86 hectare!) letterlijk in het hart",
      detail: "19 parken, dichtstbijzijnde op 23 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Goede verbindingen via tramlijnen",
      detail: "Tram 1 en 4 rijden door de wijk",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau rond het Gentse gemiddelde",
      detail: "€ 310.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "Watersportbaan - Ekkergem heeft een omheinde hondenspeelweide in de wijk zelf (op 4 minuten wandelen) en Tom & Co op 12 minuten. Voor dierenarts moet je naar het naburige Stationsbuurt-Noord (19 minuten wandelen). Maar het echte verhaal draait om groen: De Blaarmeersen (86 hectare groot) ligt letterlijk in het hart van de wijk (23 meter), en daarnaast telt de wijk nog 18 andere parken — van het intieme Natuurpark Overmeers tot het groene Ekkergempark.",
  },
  dogParks: {
    intro:
      "Watersportbaan - Ekkergem heeft een omheinde hondenspeelweide bij de Yachtdreef, op slechts 4 minuten wandelen. Hier kan je hond veilig los lopen en socialiseren. Voor dagelijkse uitlaatrondjes heb je de keuze uit 19 verschillende parken — maar de meeste baasjes kiezen voor De Blaarmeersen: 86 hectare groen, water en wandelpaden waar je urenlang kunt dwalen zonder ooit dezelfde route twee keer te lopen.",
    parks: [
      {
        name: "Dog Park near Yachtdreef",
        icon: "fa-solid fa-bench-tree",
        distance: "4 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.046494,
          lon: 3.700066,
        },
        features: [
          {
            text: "Volledig omheind terrein",
            icon: "fa-regular fa-shield-check",
          },
          { text: "Los lopen toegestaan", icon: "fa-regular fa-dog-leashed" },
          {
            text: "Open van zonsopgang tot zonsondergang",
            icon: "fa-regular fa-clock",
          },
        ],
      },
    ],
  },
  vets: {
    intro:
      "Watersportbaan - Ekkergem heeft geen eigen dierenarts. De dichtstbijzijnde praktijk ligt in het naburige Stationsbuurt-Noord, op ongeveer 19 minuten wandelen. Deze afstand is een aandachtspunt voor spoedgevallen — baasjes doen er goed aan om vooraf een dierenarts te selecteren die goed bereikbaar is met tram of fiets.",
    practices: [
      {
        icon: "fa-solid fa-house-medical",
        name: "Dr. Paul Panis",
        street: "Albertlaan",
        streetNumber: "25",
        municipality: "Gent",
        postalCode: "9000",
        distance: "19 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.037865,
          lon: 3.710691,
        },
      },
    ],
  },
  petStores: {
    intro:
      "In Watersportbaan - Ekkergem is Tom & Co beschikbaar op 12 minuten wandelen. Ideaal voor spontane aankopen of noodvoorraad — je loopt er even naartoe tijdens je wandeling en combineert het met een rondje door het Ekkergempark of De Blaarmeersen.",
    stores: [
      {
        icon: "fa-solid fa-store",
        name: "Tom & Co",
        street: "Martelaarslaan",
        streetNumber: "307",
        municipality: "Gent",
        postalCode: "9000",
        distance: "12 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.047322,
          lon: 3.708590,
        },
      },
    ],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in Watersportbaan - Ekkergem met een hond draait om groen, water en variatie. Je ochtend begint met De Blaarmeersen — letterlijk 23 meter van het wijkcentrum — waar je kiest tussen korte wandelingen langs het water of uitgebreide tochten door 86 hectare recreatiegebied. Tussendoor varieer je met het Ekkergempark, Groenevalleipark of een van de 17 andere parken binnen de wijk. Voor socialisatie loop je naar de hondenspeelweide (4 minuten), voor voeding naar Tom & Co (12 minuten), en 's avonds geniet je van het jonge wijkleven met dorpsgevoel.",
    benefits: [
      "Je ochtend begint met De Blaarmeersen — 86 hectare groen en water op 23 meter van het wijkcentrum. Dit is uniek: het grootste stedelijke recreatiegebied van Gent letterlijk om de hoek, met eindeloze wandelmogelijkheden langs water, door bossen en over sportvelden.",
      "De wijk voelt jong en levendig: 1 op 4 bewoners is twintigers, er is een actieve gemeenschap die zelf de handen uit de mouwen steekt, en het nieuwe buurthuis De Waterlelie (geopend april 2025) brengt mensen samen.",
      "Voor hondenspeelweide en dierenwinkel hoef je de wijk niet uit (respectievelijk 4 en 12 minuten wandelen). Voor dierenarts moet je naar Stationsbuurt-Noord (19 minuten), maar de goede tramverbindingen (lijn 1 en 4) maken dit haalbaar.",
      "De vier gezichten van de wijk — dorpsgevoel Ekkergem, sociale hoogbouw Neermeersen, historische Bijloke en groene Blaarmeersen — geven variatie en karakter. Je woont in een wijk die dorps aanvoelt, maar met alle voordelen van stadsnabijheid.",
      "De combinatie van 19 parken binnen de wijk, het grootste recreatiegebied van Gent om de hoek, en een jong, dynamisch wijkleven maakt deze wijk ideaal voor baasjes die groen, water en gemeenschap zoeken — en minder geschikt voor wie vooral hondenvoorzieningen op loopafstand wil.",
    ],
  },
  contributionCTA: {
    heading: "Baasjes helpen baasjes",
    intro:
      "Help andere baasjes en schets hoe hondvriendelijk deze buurt is. Vul de enquête hieronder in. Het neemt niet meer dan 5 minuten in beslag en je wordt hun grote held.",
    typeformId: "01KBDFG2BJG3DYTNX0X9GT2HDT",
  },
  statistics: {
    intro:
      "Hieronder vind je de belangrijkste cijfers over Watersportbaan - Ekkergem. Deze statistieken helpen je om de buurt beter te begrijken en te vergelijken met andere wijken.",
    medianPrice: 310000,
    inhabitants: 13500,
    availableHomes: 28,
    pricePerSqm: 3200,
  },
  houses: {
    intro:
      "Nu je weet dat Watersportbaan - Ekkergem bij jou en je hond past, is de volgende stap het bekijken van beschikbare woningen. We linken naar Immoweb waar je op postcode-niveau kunt zoeken — gefilterd op eigenschappen die belangrijk zijn voor hondeneigenaren.",
    hasOwnPostalCode: false,
  },
  neighboringNeighborhoods: [
    "gent-binnenstad",
    "gent-brugse-poort",
    "gent-drongen",
    "gent-elisabethbegijnhof",
    "gent-rabot",
    "gent-sint-denijs-westrem-afsnee",
    "gent-stationsbuurt-noord",
    "gent-stationsbuurt-zuid",
  ],
};
