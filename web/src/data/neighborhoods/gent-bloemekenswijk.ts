import type { Neighborhood } from "../neighborhoods";

export const gentBloemekenswijk: Neighborhood = {
  id: "gent-bloemekenswijk",
  name: "Bloemekenswijk",
  subtitle:
    "Voor baasjes die houden van een jonge, levendige buurt met parken op letterlijk elke hoek",
  postalCode: "9030",
  inhabitants: 12500,
  labels: [
    { text: "Zeer jong", icon: "fa-regular fa-baby" },
    { text: "Levendig", icon: "fa-regular fa-sparkles" },
    { text: "Groene parken", icon: "fa-regular fa-tree-city" }
  ],
  intro: `Bloemekenswijk is Gents jongste buurt — bijna 1 op 4 inwoners is tussen 0 en 17 jaar oud. Gelegen tussen Rabot en Wondelgem, combineert de wijk arbeiderswoningen uit de late 19de en vroege 20ste eeuw met een levendige, diverse gemeenschap. Wat opvalt: hoewel de buurt dichtbebouwd aanvoelt vanaf de straat, oogt ze verrassend groen vanuit de lucht. Met twaalf parken binnen de wijk — waaronder het centrale Bloemekenspark op letterlijk 50 meter van het centrum — heb je als baasje eindeloos veel keuze voor korte en lange wandelingen.

Wat Bloemekenswijk bijzonder maakt, is de combinatie van levendigheid en groen. Van Beverenplein staat bekend in heel Gent: met zijn drukke zondagsmarkt, gezellige buurtcafés en diverse horeca voelt het plein als het kloppend hart van de wijk. De vele winkels en buurtinitiatieven zorgen voor een levendige sfeer, terwijl je twee hondenspeelweiden hebt op korte loopafstand — bij het Sumakpad (4 minuten) en de Hakkeneistraat (2 minuten). Tom & Co in Wondelgem ligt op zes minuten fietsen, en de dichtstbijzijnde dierenarts zit in Mariakerke.

Deze buurt is vooral geschikt voor baasjes die houden van een jonge, diverse en levendige woonomgeving met veel parken en een sterk gemeenschapsgevoel — en minder waarde hechten aan een dierenarts om de hoek.`,
  coordinates: {
    lat: 51.0705,
    lon: 3.7085,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "2 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Twee beschikbaar op zeer korte afstand",
      detail: "Meestal binnen 150 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "8 mins",
      distanceIcon: "fa-regular fa-car",
      description: "Dichtstbijzijnde praktijk in Mariakerke",
      detail: "Meestal binnen 1 500 meter",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "6 mins",
      distanceIcon: "fa-regular fa-bicycle",
      description: "Tom & Co bereikbaar per fiets",
      detail: "Meestal binnen 2 000 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Verrassend veel groen voor dichte bebouwing",
      detail: "Meestal binnen 60 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Goed bereikbaar met tram en bus",
      detail: "Meestal binnen 400 meter",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Betaalbaar wonen in populaire buurt",
      detail: "€ 275.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "In Bloemekenswijk zijn de hondenvoorzieningen goed geregeld: twee hondenspeelweiden op zeer korte loopafstand en Tom & Co in Wondelgem op zes minuten fietsen. Alleen de dierenarts ligt verder weg, in Mariakerke, wat betekent dat je voor een bezoek de auto of fiets zult pakken. Waar de buurt écht in uitblinkt, is groen: met twaalf parken heb je verrassend veel groene plekken binnen handbereik, ondanks de dichte bebouwing.",
  },
  dogParks: {
    intro:
      "Bloemekenswijk heeft twee omheinde hondenspeelweiden: één bij het Sumakpad op vier minuten lopen, en één aan de Hakkeneistraat op slechts twee minuten lopen — perfect voor een snelle ochtendbezoek. Beide speelweiden bieden voldoende ruimte voor je hond om veilig los te lopen en te spelen met andere honden. Voor wie meer variatie zoekt, biedt de buurt ook twaalf parken — van het centrale Bloemekenspark tot het Lampistenpark en Machinistenpark — die allemaal perfect zijn voor rustige wandelingen.",
    parks: [
      {
        name: "Hondenspeelweide Hakkeneistraat",
        icon: "fa-solid fa-bench-tree",
        distance: "2 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.076273,
          lon: 3.715706,
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
        name: "Hondenspeelweide Sumakpad",
        icon: "fa-solid fa-bench-tree",
        distance: "4 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.069234,
          lon: 3.711616,
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
      "In Bloemekenswijk zijn geen dierenartsenpraktijken binnen de buurt zelf. De dichtstbijzijnde praktijk is Jan-Pieter Vandenbussche in Mariakerke, op ongeveer 1,5 kilometer afstand — wat in de praktijk betekent dat je de auto of fiets zult nemen voor een bezoek aan de dierenarts.",
    practices: [
      {
        name: "Jan-Pieter Vandenbussche",
        icon: "fa-solid fa-hospital",
        street: "Brugsesteenweg",
        streetNumber: "330-334",
        municipality: "Mariakerke",
        postalCode: "9030",
        distance: "8 mins",
        distanceIcon: "fa-regular fa-car",
        coordinates: {
          lat: 51.068055,
          lon: 3.686312,
        },
      },
    ],
  },
  petStores: {
    intro:
      "In Bloemekenswijk zijn geen dierenwinkels binnen de buurt zelf. De dichtstbijzijnde dierenwinkel is Tom & Co in Wondelgem op de Botestraat, op ongeveer zes minuten fietsen — een korte rit voor voeding, snacks en accessoires voor je hond.",
    stores: [
      {
        name: "Tom & Co",
        icon: "fa-solid fa-store",
        street: "Botestraat",
        streetNumber: "14",
        municipality: "Wondelgem",
        postalCode: "9032",
        distance: "6 mins",
        distanceIcon: "fa-regular fa-bicycle",
        coordinates: {
          lat: 51.089123,
          lon: 3.707950,
        },
      },
    ],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in Bloemekenswijk met een hond draait om levendigheid, jeugd en verassend veel groen. Als Gents jongste buurt (bijna 25% kinderen en jongeren) kom je constant jonge gezinnen en baasjes tegen in de parken en op straat. Met twaalf parken binnen de wijk heb je eindeloos veel wandelroutes: van korte ochtendrondes in het Bloemekenspark (letterlijk om de hoek) tot langere avondwandelingen door het Lampistenpark of Machinistenpark. Van Beverenplein met zijn zondagsmarkt en gezellige cafés is het kloppend hart van de buurt.",
    benefits: [
      "Je ochtend begint met een korte wandeling naar het Bloemekenspark (50 meter!) of een andere nabijgelegen groene plek — door de vele jonge gezinnen in de buurt kom je vaak andere baasjes tegen die ook met kinderen en hond wandelen.",
      "Met twee hondenspeelweiden op zeer korte afstand heb je de luxe om dagelijks te variëren: de ene keer ga je naar de Hakkeneistraat (twee minuten), de andere keer naar het Sumakpad (vier minuten). Door de vele jonge inwoners leer je snel andere hondenbaasjes kennen.",
      "Van Beverenplein biedt een levendige mix van zondagsmarkt, buurtcafés en diverse horeca — perfect voor een koffie na de ochtendwandeling. De vele winkels en buurtinitiatieven zorgen voor een sterke gemeenschapszin.",
      "Voor dierenarts moet je naar Mariakerke, wat lastig kan zijn bij spoedgevallen — een aandachtspunt als je veel waarde hecht aan nabijheid van veterinaire zorg. Maar de overvloed aan groen, jonge energie en levendige sfeer maken Bloemekenswijk tot een fantastische buurt voor honden en hun baasjes.",
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
      "Hieronder vind je de belangrijkste cijfers over Gent Bloemekenswijk. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 275000,
    inhabitants: 12500,
    availableHomes: 16,
    pricePerSqm: 3100,
  },
};
