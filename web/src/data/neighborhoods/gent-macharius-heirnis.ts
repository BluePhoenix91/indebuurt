import type { Neighborhood } from "../neighborhoods";

export const gentMachariusHeirnis: Neighborhood = {
  id: "gent-macharius-heirnis",
  name: "Macharius - Heirnis",
  subtitle:
    "Voor baasjes die houden van groen rondom de hoek én alles binnen handbereik",
  dateAdded: "2025-12-14",
  postalCode: "9000",
  inhabitants: 6850,
  labels: [
    { text: "Centraal gelegen", icon: "fa-regular fa-location-dot" },
    { text: "Veel parken", icon: "fa-regular fa-trees" },
    { text: "Hechte gemeenschap", icon: "fa-regular fa-people-group" },
  ],
  intro: `Macharius-Heirnis is het beste van twee werelden: een dichtbevolkte stadsbuurt met een verrassende hoeveelheid groen en water, en dat op steenworp van het Gentse centrum. De wijk bestaat uit drie delen — Macharius, Heirnis en de Visserij — gescheiden door de Kasteellaan, maar verbonden door een hechte gemeenschap en maar liefst 10 parken binnen wandelafstand.

Wat deze buurt bijzonder maakt, is hoe groen en ruimte naadloos samenvloeien in een stedelijke omgeving. Het Koningin Astridpark en Rommelwaterpark liggen letterlijk om de hoek (respectievelijk 34 en 54 meter van het wijkcentrum), terwijl grotere parken zoals het Coyendanspark, Bijgaardepark en Visserijpark binnen enkele minuten lopen bereikbaar zijn. Voor baasjes betekent dit: elke ochtend een andere wandelroute, zonder ooit de wijk uit te moeten.

Hoewel de wijk zelf geen dierenarts, dierenwinkel of hondenspeelweide heeft, liggen deze voorzieningen uitzonderlijk dichtbij in de naburige wijken: Tom & Co in Dampoort op 5 minuten wandelen, een dierenarts in de Binnenstad op 7 minuten, en een hondenspeelweide in Dampoort op 9 minuten. De combinatie van nabijheid tot het centrum, voorzieningen op loopafstand en een overvloed aan groen maakt deze wijk ideaal voor baasjes die het stadsleven willen combineren met de ruimte en rust die hun hond nodig heeft.`,
  coordinates: {
    lat: 51.0505,
    lon: 3.7396,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "9 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Hondenspeelweide in naburig Dampoort",
      detail: "Ongeveer 680 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "7 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Dierenarts in naburige Binnenstad",
      detail: "Ongeveer 590 meter",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Tom & Co in naburig Dampoort",
      detail: "Ongeveer 380 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Uitzonderlijk veel parken letterlijk om de hoek",
      detail: "10 parken, dichtstbijzijnde op 34 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "4 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Uitstekende verbindingen met stad en regio",
      detail: "Meestal binnen 300 meter",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau vergelijkbaar met het Gentse gemiddelde",
      detail: "€ 340.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "Hoewel Macharius-Heirnis zelf geen dierenarts, dierenwinkel of hondenspeelweide heeft, liggen deze voorzieningen uitzonderlijk dichtbij in naburige wijken: een dierenwinkel (Tom & Co in Dampoort) op 5 minuten wandelen, een dierenarts (in de Binnenstad) op 7 minuten, en een hondenspeelweide (in Dampoort) op 9 minuten. Gecombineerd met 10 parken binnen de wijk zelf — waarvan de dichtstbijzijnde letterlijk om de hoek ligt — is dit een paradijs voor baasjes die stadsleven en hondvriendelijkheid zoeken.",
  },
  dogParks: {
    intro:
      "Macharius-Heirnis heeft een omheinde hondenspeelweide in het naburige Dampoort, op slechts 9 minuten wandelen. Hier kan je hond veilig los lopen en socialiseren met andere honden. Voor dagelijkse uitlaatrondjes heb je de keuze uit 10 verschillende parken, elk met hun eigen karakter — van het intieme Rommelwaterpark tot het ruime Visserijpark.",
    parks: [
      {
        name: "Dog Park near Evarist De Buckstraat",
        icon: "fa-solid fa-bench-tree",
        distance: "9 mins",
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
    ],
  },
  vets: {
    intro:
      "Hoewel Macharius-Heirnis zelf geen dierenarts heeft, ligt de dichtstbijzijnde praktijk in de naburige Binnenstad op slechts 7 minuten wandelen. Deze nabijheid is ideaal voor regelmatige controles en geeft rust bij spoedgevallen — je bent binnen 10 minuten ter plaatse, zelfs te voet.",
    practices: [
      {
        icon: "fa-solid fa-house-medical",
        name: "J. Van den Daele",
        street: "Graaf van Vlaanderenplein",
        streetNumber: "30",
        municipality: "Gent",
        postalCode: "9000",
        distance: "7 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.046863,
          lon: 3.732836,
        },
      },
    ],
  },
  petStores: {
    intro:
      "Hoewel Macharius-Heirnis zelf geen dierenwinkel heeft, ligt Tom & Co in het naburige Dampoort op slechts 5 minuten wandelen. Ideaal voor spontane aankopen of als je vergeten bent voeding te bestellen — je loopt er even naartoe tijdens je ochtendwandeling.",
    stores: [
      {
        icon: "fa-solid fa-store",
        name: "Tom & Co",
        street: "Dendermondsesteenweg",
        streetNumber: "134b",
        municipality: "Gent",
        postalCode: "9000",
        distance: "5 mins",
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
      "Het leven in Macharius-Heirnis met een hond draait om gemak en variatie. Je ochtend begint met een wandeling naar het dichtstbijzijnde park — vaak het Koningin Astridpark of Rommelwaterpark, letterlijk om de hoek. Tussendoor loop je naar Tom & Co in Dampoort voor een nieuwe zak brokken, en 's avonds varieer je met het Coyendanspark, Bijgaardepark of Visserijpark. De 10 parken liggen allemaal binnen de wijk zelf, de hondenvoorzieningen op enkele minuten wandelen in naburige wijken.",
    benefits: [
      "Je ochtend begint met een keuze uit 10 parken binnen de wijk zelf, allemaal binnen 10 minuten wandelen. Het Koningin Astridpark en Rommelwaterpark liggen zo dichtbij dat je ze gebruikt voor snelle uitlaatrondjes tussendoor.",
      "De wijk voelt hecht en levendig: baasjes groeten elkaar in de parken, kinderen spelen samen, en er is meer interactie tussen bewoners dan in andere Gentse wijken.",
      "Voor dierenarts, dierenwinkel of hondenspeelweide moet je naar naburige wijken (Dampoort en Binnenstad), maar alles ligt op korte loopafstand (5-9 minuten). Spoedgevallen, regelmatige controles of spontane aankopen zijn dus geen probleem.",
      "De combinatie van centrale ligging (dicht bij het Gentse centrum én toegangswegen), voorzieningen op loopafstand en overvloed aan groen binnen de wijk maakt deze buurt uniek voor hondenbaasjes die stadsleven en hondvriendelijkheid willen combineren.",
      "De historische Sint-Baafsabdij en recente transformatie van verouderde woonwijk naar populaire gezinsbuurt geven de wijk karakter en een gevoel van gemeenschap.",
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
      "Hieronder vind je de belangrijkste cijfers over Macharius-Heirnis. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 340000,
    inhabitants: 6850,
    availableHomes: 18,
    pricePerSqm: 3450,
  },
  houses: {
    intro:
      "Nu je weet dat Macharius-Heirnis bij jou en je hond past, is de volgende stap het bekijken van beschikbare woningen. We linken naar Immoweb waar je op postcode-niveau kunt zoeken — gefilterd op eigenschappen die belangrijk zijn voor hondeneigenaren.",
    hasOwnPostalCode: false,
  },
  neighboringNeighborhoods: [
    "gent-binnenstad",
    "gent-dampoort",
    "gent-muide",
    "gent-oud-gentbrugge",
    "gent-sluizeken-tolhuis-ham",
  ],
};
