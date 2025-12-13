import type { Neighborhood } from "../neighborhoods";

export const gentRabot: Neighborhood = {
  id: "gent-rabot",
  name: "Gent-Rabot",
  subtitle: "Een dichtbevolkte buurt met veel leven en een centrale ligging",
  dateAdded: "2025-12-10",
  postalCode: "9000",
  inhabitants: 9668,
  labels: [
    { text: "Stadse buurt", icon: "fa-regular fa-city" },
    { text: "Multicultureel", icon: "fa-regular fa-people-group" },
    { text: "Dicht bij centrum", icon: "fa-regular fa-location-dot" },
  ],
  intro: `Het Rabot is een buurt die ademt: superdiversiteit, dichtbevolkt en altijd in beweging. Je wandelt er met je hond door smalle straten met arbeidershuisjes uit de textielindustrie, tussen multiculturele winkeltjes en buurtbewoners die elkaar kennen. Het Rabotpark is de groene uitlaatklep — een compact park met speelterreinen, wandelpaden en banken waar baasjes uit de buurt samenkomen.

Wat het Rabot bijzonder maakt, is de mix van authenticiteit en vernieuwing. De buurt ligt op een steenworp van het centrum, waardoor je snel overal bent, maar behoudt zijn volkse, stedelijke karakter. Het is een 19e-eeuwse gordel die volop in transitie is — denk aan het stadsvernieuwingsproject "Bruggen naar Rabot" — en waar ruimte schaars maar gezelligheid groot is. Voor honden is het vooral een buurt voor korte, stadse rondes in plaats van lange groene wandelingen.

Deze buurt is vooral geschikt voor baasjes die houden van stedelijke levendigheid, diversiteit en een buurt waar altijd iets gebeurt — en minder voor wie op zoek is naar veel groen en ruimte.`,
  coordinates: {
    lat: 51.0675,
    lon: 3.7219,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "20 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Er is twee parkjes beschikbaar binnen wandelafstand",
      detail: "Meestal binnen 2 500 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "",
      distanceIcon: "",
      description: "Er is geen dierarts gevestigd op het Rabot",
      detail: "",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "",
      distanceIcon: "",
      description: "Er zijn geen dierenwinkels in deze buurt",
      detail: "",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "10 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Je moet er even voor stappen",
      detail: "Meestal binnen de 800 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Je raakt overal makkelijk zonder auto",
      detail: "Meestal binnen 500 meter",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau is lager dan in de buurtwijken",
      detail: "€ 315.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "In het Rabot zijn de hondenvoorzieningen schaars: er is geen dierenarts of dierenwinkel in de buurt zelf, en de dichtstbijzijnde hondenspeelweide ligt op ongeveer 20 minuten lopen. Waar de buurt in uitblinkt, is de centrale ligging: binnen 5 minuten ben je bij openbaar vervoer, waardoor voorzieningen in andere wijken makkelijk bereikbaar zijn. Het Rabotpark ligt op 10 minuten lopen en biedt ruimte voor korte, stadse uitlaatrondjes.",
  },
  dogParks: {
    intro:
      "Het Rabot heeft twee omheinde hondenspeelweide op ongeveer 20 minuten lopen: de Hondenspeelweide Neuseplein. Voor wie meer variatie zoekt, ligt dat net iets verder dan ideaal, maar de centrale ligging van de buurt betekent dat je snel in andere wijken bent met meer uitlaatopties. Het Rabot is vooral een buurt voor korte, stadse wandelrondes — het Rabotpark en de smalle straatjes zijn perfect voor snelle ochtenduitlaatbeurten.",
    parks: [
      {
        name: "Hondenspeelweide Neuseplein",
        icon: "fa-solid fa-bench-tree",
        distance: "20 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.0670556,
          lon: 3.7265774,
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
        name: "Hondenspeelweide Rabotpark",
        icon: "fa-solid fa-bench-tree",
        distance: "16 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.0636779,
          lon: 3.7177188,
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
      "In het Rabot zijn geen dierenartsenpraktijken gevestigd. Door de centrale ligging en het goede openbaar vervoer (5 minuten lopen) zijn dierenartsen in aangrenzende wijken of het stadscentrum wel makkelijk bereikbaar.",
    practices: [],
  },
  petStores: {
    intro:
      "In het Rabot zijn geen dierenwinkels beschikbaar. De nabijheid van het stadscentrum en het goede openbaar vervoer maken dat je snel bij dierenwinkels in andere wijken bent.",
    stores: [],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in het Rabot met een hond draait om stedelijke compactheid en buurtgevoel. Het Rabotpark ligt centraal en is dé plek voor snelle ochtendrondes en ontmoetingen met andere baasjes. Voor langere wandelingen ben je binnen enkele minuten in aangrenzende wijken of richting het centrum, en de centrale ligging maakt dat je makkelijk bij voorzieningen komt.",
    benefits: [
      "Je ochtend begint compact: een korte wandeling naar het Rabotpark is meestal voldoende voor een snelle uitlaatbeurt, en de buurt is overzichtelijk genoeg om snel een rondje te maken.",
      "De buurt voelt levendig en authentiek: een multiculturele mix van gezinnen, jongeren en hondenbaasjes die elkaar kennen en groeten. Er is altijd wel iets te zien op straat.",
      "De centrale ligging betekent dat je binnen enkele minuten in het stadscentrum bent, wat handig is voor boodschappen, dierenarts of een uitstapje naar grotere parken buiten de wijk.",
      "Het Rabot is geen buurt voor lange, groene wandelingen — het is compact en stedelijk. Maar juist die drukte en diversiteit maken het interessant voor baasjes die houden van een levendige, authentieke buurt waar altijd wat gebeurt.",
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
      "Hieronder vind je de belangrijkste cijfers over het Rabot. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 0, // TODO: Add actual median price
    inhabitants: 9668,
    availableHomes: 0, // TODO: Add actual available homes
    pricePerSqm: 0, // TODO: Add actual price per sqm
  },
};
