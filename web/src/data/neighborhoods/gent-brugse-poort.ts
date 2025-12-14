import type { Neighborhood } from "../neighborhoods";

export const gentBrugsePoort: Neighborhood = {
  id: "gent-brugse-poort",
  name: "Brugse Poort - Rooigem",
  subtitle:
    "Voor baasjes die houden van diversiteit, groen rondom de hoek én twee hondenspeelweiden in de buurt",
  dateAdded: "2025-12-14",
  postalCode: "9000",
  inhabitants: 14200,
  labels: [
    { text: "Super divers", icon: "fa-regular fa-earth-europe" },
    { text: "Uitzonderlijk veel groen", icon: "fa-regular fa-trees" },
    { text: "Getransformeerd", icon: "fa-regular fa-seedling" },
  ],
  intro: `Brugse Poort - Rooigem is een wijk met een bijzonder verhaal: van 19e-eeuwse arbeiderswijk met textielfabrieken, via armoede en verval, naar een getransformeerde buurt vol groen en diversiteit. Tussen 2000 en 2015 bracht het stadsvernieuwingsproject 'Zuurstof voor de Brugse Poort' een radicale omslag: textielfabrieken werden groene parken, sociale woningen werden gerenoveerd, en de wijk kreeg maar liefst 24 parken — waarvan Weerstandsplein letterlijk in het hart ligt (10 meter van het wijkcentrum).

Wat deze buurt bijzonder maakt voor hondenbaasjes, is de uitzonderlijke hoeveelheid groen. Het Weerstandsplein, Fluweelpark, Wielewaalpark, Acaciapark en Kokerpark liggen allemaal binnen 50 meter van het wijkcentrum — je bent letterlijk omringd door parken. Het Groenevalleipark is het vlaggenschip: zes hectare groot, gebouwd op de plek waar ooit textielfabriek La Lys stond. En dan zijn er nog twee hondenspeelweiden binnen de wijk zelf (op 4 en 8 minuten wandelen), ideaal voor socialisatie en vrij lopen.

Maar de Brugse Poort is geen wijk zonder uitdagingen. Het is de jongste en meest diverse wijk van Gent (115+ nationaliteiten, 21% jonger dan 18 jaar), maar ook een werkende-klassenbuurt met overlast (44% ervaart buurtoverlast), zwerfvuil en verouderde huizen. Voor dierenarts en dierenwinkel moet je naar naburige wijken (respectievelijk 23 en 21 minuten wandelen), maar de combinatie van groen, twee hondenspeelweiden en goede openbaar vervoer maakt deze wijk uniek voor baasjes die houden van diversiteit en natuur dichtbij.`,
  coordinates: {
    lat: 51.0581,
    lon: 3.6953,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "4 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Twee hondenspeelweiden in de wijk zelf",
      detail: "Dichtstbijzijnde op 330 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "23 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Dierenarts in naburig Mariakerke",
      detail: "Ongeveer 1,9 kilometer",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "21 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Tom & Co in naburig Watersportbaan-Ekkergem",
      detail: "Ongeveer 1,7 kilometer",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Uitzonderlijk veel groen letterlijk om de hoek",
      detail: "24 parken, dichtstbijzijnde op 10 meter!",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "3 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Goed ontwikkeld openbaarvervoersnetwerk",
      detail: "Meestal binnen 250 meter",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau onder het Gentse gemiddelde",
      detail: "€ 285.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "Brugse Poort - Rooigem heeft twee hondenspeelweiden binnen de wijk (op 4 en 8 minuten wandelen) en maar liefst 24 parken — waarvan Weerstandsplein letterlijk in het hart ligt (10 meter). Het Groenevalleipark is het vlaggenschip: zes hectare groot, gebouwd op de plek van een oude textielfabriek. Voor dierenarts en dierenwinkel moet je naar naburige wijken (respectievelijk 23 en 21 minuten wandelen).",
  },
  dogParks: {
    intro:
      "Brugse Poort - Rooigem heeft twee omheinde hondenspeelweiden binnen de wijk zelf. De dichtstbijzijnde ligt bij de Papiermolenststraat, op slechts 4 minuten wandelen. De tweede ligt bij het Spinmolenplein, op 8 minuten. Hier kan je hond veilig los lopen en socialiseren met andere honden. Voor dagelijkse uitlaatrondjes heb je de keuze uit 24 verschillende parken — van het intieme Weerstandsplein (10 meter van het centrum) tot het ruime Groenevalleipark (zes hectare groot).",
    parks: [
      {
        name: "Dog Park near Papiermolenststraat",
        icon: "fa-solid fa-bench-tree",
        distance: "4 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.06129,
          lon: 3.688678,
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
      {
        name: "Dog Park near Spinmolenplein",
        icon: "fa-solid fa-bench-tree",
        distance: "8 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.054901,
          lon: 3.703387,
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
      "Brugse Poort - Rooigem heeft geen eigen dierenarts. De dichtstbijzijnde praktijk ligt in het naburige Mariakerke, op ongeveer 23 minuten wandelen. Deze afstand is een aandachtspunt voor spoedgevallen — baasjes doen er goed aan om vooraf een dierenarts te selecteren die goed bereikbaar is met openbaar vervoer of fiets.",
    practices: [],
  },
  petStores: {
    intro:
      "Brugse Poort - Rooigem heeft geen eigen dierenwinkel. De dichtstbijzijnde is Tom & Co in het naburige Watersportbaan-Ekkergem, op ongeveer 21 minuten wandelen. Voor regelmatige aankopen is online bestellen een praktische optie, of je combineert een bezoek aan de dierenwinkel met een langere wandeling — zo heeft je hond ook een uitgebreide uitlaatbeurt gehad.",
    stores: [],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in Brugse Poort - Rooigem met een hond draait om groen, diversiteit en transformatie. Je ochtend begint met een wandeling naar het Weerstandsplein of Acaciapark — letterlijk om de hoek (10 en 48 meter) — omringd door 115+ nationaliteiten en jonge gezinnen. Voor een langere wandeling kies je het Groenevalleipark (zes hectare) of een van de andere 24 parken. En voor socialisatie met andere honden heb je twee hondenspeelweiden binnen de wijk (4 en 8 minuten wandelen).",
    benefits: [
      "Je ochtend begint met een keuze uit 24 parken binnen de wijk, waarvan vijf binnen 50 meter van het centrum liggen. Weerstandsplein (10m), Fluweelpark (22m), Wielewaalpark (29m), Acaciapark (48m) en Kokerpark (49m) zijn letterlijk om de hoek.",
      "De wijk voelt divers en levendig: 115+ nationaliteiten, de jongste wijk van Gent (21% onder 18 jaar), maar ook uitdagingen zoals buurtoverlast (44%), zwerfvuil en verouderde huizen.",
      "Voor dierenarts en dierenwinkel moet je naar naburige wijken wandelen (respectievelijk 23 en 21 minuten), maar het goede openbaarvervoersnetwerk maakt dit haalbaar. Voor spoedgevallen is de afstand wel een aandachtspunt.",
      "De twee hondenspeelweiden binnen de wijk (op 4 en 8 minuten wandelen) zijn een groot voordeel — je hoeft de wijk niet uit voor socialisatie en vrij lopen.",
      "De combinatie van stadsvernieuwing ('Zuurstof voor de Brugse Poort' 2000-2015), uitzonderlijk veel groen (textielfabrieken werden parken) en diversiteit maakt deze wijk uniek — maar alleen geschikt voor baasjes die kunnen omgaan met buurtuitdagingen en houden van een multiculturele omgeving.",
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
      "Hieronder vind je de belangrijkste cijfers over Brugse Poort - Rooigem. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 285000,
    inhabitants: 14200,
    availableHomes: 32,
    pricePerSqm: 3150,
  },
};
