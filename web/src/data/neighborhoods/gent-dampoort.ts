import type { Neighborhood } from "../neighborhoods";

export const gentDampoort: Neighborhood = {
  id: "gent-dampoort",
  name: "Dampoort",
  subtitle:
    "Voor baasjes die houden van stadsleven, diversiteit én hondenvoorzieningen binnen handbereik",
  dateAdded: "2025-12-14",
  postalCode: "9000",
  inhabitants: 11200,
  labels: [
    { text: "Diverse gemeenschap", icon: "fa-regular fa-earth-europe" },
    { text: "Stationswijk", icon: "fa-regular fa-train" },
    { text: "Wijk in transformatie", icon: "fa-regular fa-seedling" },
  ],
  intro: `Dampoort is een levendige stationswijk waar 95 nationaliteiten samenwonen, elk met hun eigen verhalen en cultuur. Het is een buurt in beweging: mensen komen en gaan, het openbaar vervoer verbindt centrum en stadsrand, en de wijk transformeert van dichtbebouwde volkswijk naar een groener en hondvriendelijker geheel.

Wat Dampoort bijzonder maakt voor hondenbaasjes, is de recente vergroening. Waar de wijk historisch bekend stond als een stenen, dichtbebouwde buurt met weinig groen, telt ze nu maar liefst 9 parken — waarvan het Wolterspark letterlijk in het hart van de wijk ligt (9 meter van het wijkcentrum). Het Banierpark, Bijgaardepark en Speelterrein Wasstraat liggen allemaal binnen één minuut wandelen, terwijl grotere parken zoals Pastory Tuin en De groene banaan variatie brengen.

Maar het echte verhaal van Dampoort draait om toegankelijkheid. Een dierenarts op 2 minuten wandelen, Tom & Co op 6 minuten, en twee hondenspeelweiden binnen 5 minuten — alles in de wijk zelf. Voor baasjes die stadsleven willen combineren met praktische hondenvoorzieningen en een multiculturele omgeving, is Dampoort een verrassend goede match.`,
  coordinates: {
    lat: 51.0524,
    lon: 3.7453,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "3 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Twee hondenspeelweiden in de wijk zelf",
      detail: "Dichtstbijzijnde op 190 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "2 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Dierenarts in de wijk zelf",
      detail: "Slechts 155 meter",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "6 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Tom & Co in de wijk zelf",
      detail: "Ongeveer 510 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Wijk in vergroening met 9 parken",
      detail: "Wolterspark letterlijk in het hart (9 meter)",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "2 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Uitstekende verbindingen via Station Dampoort",
      detail: "Knooppunt van openbaar vervoer",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau onder het Gentse gemiddelde",
      detail: "€ 295.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "Dampoort scoort uitzonderlijk goed op hondenvoorzieningen: een dierenarts op 2 minuten wandelen, Tom & Co op 6 minuten, en twee hondenspeelweiden binnen 5 minuten. Gecombineerd met 9 parken — waarvan het Wolterspark letterlijk in het hart van de wijk ligt — is dit een verrassing voor wie Dampoort alleen kent als dichtbebouwde stationswijk.",
  },
  dogParks: {
    intro:
      "Dampoort heeft twee omheinde hondenspeelweiden binnen de wijk zelf. De dichtstbijzijnde ligt aan de Evarist De Buckstraat, op slechts 3 minuten wandelen. Hier kan je hond veilig los lopen en socialiseren. Voor dagelijkse uitlaatrondjes heb je de keuze uit 9 verschillende parken, elk met hun eigen karakter — van het centrale Wolterspark tot het rustige Pastory Tuin.",
    parks: [
      {
        name: "Dog Park near Evarist De Buckstraat",
        icon: "fa-solid fa-bench-tree",
        distance: "3 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.054479,
          lon: 3.746365,
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
        name: "Dog Park near Brugsevaart",
        icon: "fa-solid fa-bench-tree",
        distance: "5 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.054218,
          lon: 3.745926,
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
      "In Dampoort is een dierenartsenpraktijk beschikbaar op slechts 2 minuten wandelen — letterlijk om de hoek. Deze nabijheid is ideaal voor regelmatige controles en geeft enorme rust bij spoedgevallen. Je bent sneller bij de dierenarts dan bij de meeste huisartsen.",
    practices: [
      {
        icon: "fa-solid fa-house-medical",
        name: "Dierenartsenprkatijk Dr. Sofie Breugelmans",
        street: "Adolf Baeyensstraat",
        streetNumber: "147",
        municipality: "Sint-Amandsberg",
        postalCode: "9040",
        distance: "2 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.051934,
          lon: 3.747935,
        },
      },
    ],
  },
  petStores: {
    intro:
      "In Dampoort is een Tom & Co dierenwinkel beschikbaar op 6 minuten wandelen. Ideaal voor spontane aankopen, noodvoorraad of gewoon een nieuwe knuffelmuis als de vorige aan flarden ligt. Je loopt er even naartoe tijdens je wandeling en combineert het met een rondje door het Banierpark.",
    stores: [
      {
        icon: "fa-solid fa-store",
        name: "Tom & Co",
        street: "Dendermondsesteenweg",
        streetNumber: "134b",
        municipality: "Gent",
        postalCode: "9000",
        distance: "6 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.052916,
          lon: 3.742646,
        },
      },
    ],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in Dampoort met een hond draait om gemak, diversiteit en verrassing. Je ochtend begint met een wandeling naar het Wolterspark — letterlijk in het hart van de wijk — of het Banierpark om de hoek. Tussendoor loop je naar Tom & Co voor voeding, en 's avonds varieer je met de hondenspeelweide, Pastory Tuin of Bijgaardepark. Alles binnen 10 minuten wandelen, omringd door een multiculturele wijk vol leven.",
    benefits: [
      "Je ochtend begint met een keuze uit 9 parken, allemaal binnen 10 minuten wandelen. Het Wolterspark ligt letterlijk in het hart van de wijk (9 meter van het centrum), terwijl Banierpark en Bijgaardepark binnen 1 minuut bereikbaar zijn.",
      "De wijk voelt levendig en divers: 95 nationaliteiten wonen samen, er is altijd beweging rond het station, en je ontmoet baasjes uit alle hoeken van de wereld in de parken.",
      "Voor dierenarts, dierenwinkel of hondenspeelweide hoef je de wijk niet uit — alles ligt op loopafstand (2-6 minuten). Dit is uitzonderlijk voor een Gentse volkswijk.",
      "Station Dampoort maakt de wijk tot een knooppunt van openbaar vervoer, ideaal als je regelmatig naar het centrum of andere steden moet — en je hond kan mee.",
      "De recente transformatie van 'stenen volkswijk' naar 'groene, hondvriendelijke buurt' maakt Dampoort tot een verrassend goede keuze voor baasjes die stadsleven, diversiteit en praktische voorzieningen zoeken.",
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
      "Hieronder vind je de belangrijkste cijfers over Dampoort. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 295000,
    inhabitants: 11200,
    availableHomes: 22,
    pricePerSqm: 3250,
  },
  houses: {
    intro:
      "Nu je weet dat Dampoort bij jou en je hond past, is de volgende stap het bekijken van beschikbare woningen. We linken naar Immoweb waar je op postcode-niveau kunt zoeken — gefilterd op eigenschappen die belangrijk zijn voor hondeneigenaren.",
    hasOwnPostalCode: false,
  },
  neighboringNeighborhoods: [
    "gent-gentbrugge",
    "gent-kanaaldorpen",
    "gent-macharius-heirnis",
    "gent-muide",
    "gent-oud-gentbrugge",
    "gent-sint-amandsberg",
    "gent-sluizeken-tolhuis-ham",
  ],
};
