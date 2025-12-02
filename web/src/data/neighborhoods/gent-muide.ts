import type { Neighborhood } from "../neighborhoods";

export const gentMuide: Neighborhood = {
  id: "gent-muide",
  name: "Gent-Muide",
  subtitle:
    "Voor baasjes die op zoek zijn naar een buurt die klapt voor hen én hun hond",
  postalCode: "9000",
  inhabitants: 12450,
  labels: ["Wandelvriendelijk", "Opkomende buurt", "Veel groen"],
  intro: `Gent-Muide is een rustige, groene buurt waar je hond volop ruimte vindt om te wandelen.

Wat deze buurt bijzonder maakt, zijn de korte afstanden naar groenstroken en parken — meestal binnen 5 minuten lopen. Er is één hondenspeelweide op ongeveer 18 minuten wandelen, en de buurt biedt verder vooral rust en ruimte voor langere wandelingen door groen. Dierenartsen en dierenwinkels zijn er niet in de directe omgeving, maar de rustige leefomgeving en de nabijheid van groen maken het een fijne plek voor wie een ontspannen buurt zoekt voor dagelijkse hondenrondes.

Deze buurt is vooral geschikt voor baasjes die waarde hechten aan ruimte en rust, en minder waarde hechten aan de nabijheid van gespecialiseerde hondenvoorzieningen.`,
  coordinates: {
    lat: 51.0759,
    lon: 3.7307,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-solid fa-dog",
      title: "Hondenparken",
      distance: "18 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Eén beschikbaar binnen wandelafstand",
      detail: "Meestal binnen 1 500 meter",
    },
    {
      icon: "fa-solid fa-shield-heart",
      title: "Dierenartsen",
      distance: "",
      distanceIcon: "",
      description: "Er zijn geen dierenartsen in deze buurt",
      detail: "",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "",
      distanceIcon: "fa-regular fa-bicycle",
      description: "Er zijn geen dierenwinkels in deze buurt",
      detail: "",
    },
    {
      icon: "fa-solid fa-trees",
      title: "Groene ruimtes",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Je vindt veel groen in deze buurt",
      detail: "Meestal binnen de 300 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "6 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Je raakt overal makkelijk zonder auto",
      detail: "Meestal binnen 500 meter",
    },
    {
      icon: "fa-solid fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau is lager dan in de buurtgemeenten",
      detail: "€ 325.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "In Gent-Muide zijn de hondenvoorzieningen beperkt: er is geen dierenarts of dierenwinkel in de buurt zelf, en slechts één hondenspeelweide op wandelafstand. Waar de buurt wél in uitblinkt, is groen: binnen 5 minuten loop je naar rustige groene plekken, ideaal voor dagelijkse uitlaatrondjes en langere wandelingen.",
  },
  dogParks: {
    intro:
      "Gent-Muide heeft één omheinde hondenspeelweide op ongeveer 18 minuten lopen: de Hondenspeelweide Muidepoort. Hier kan je hond veilig los lopen en spelen met andere honden. Voor wie meer behoefte heeft aan variatie in uitlaatplekken is de buurt iets beperkter, maar de groene omgeving biedt genoeg ruimte voor rustige wandelingen.",
    parks: [
      {
        name: "Hondenspeelweide Muidepoort",
        icon: "fa-solid fa-bench-tree",
        distance: "18 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.0734578,
          lon: 3.7315395,
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
    intro: "In Gent-Muide zijn geen dierenartsenpraktijken beschikbaar.",
    practices: [],
  },
  petStores: {
    intro:
      "In Gent-Muide zijn geen dierenwinkels beschikbaar.",
    stores: [
    ],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in Gent-Muide met een hond draait vooral om rust en ruimte. De nabijheid van groen (5 minuten lopen) maakt dagelijkse uitlaatrondjes eenvoudig en aangenaam. Voor gespecialiseerde voorzieningen zoals een dierenarts of dierenwinkel moet je de buurt uit, maar de rustige sfeer en groene omgeving compenseren dat voor veel baasjes.",
    benefits: [
      "Je dag begint rustig met een korte wandeling naar het dichtsbijzijnde groen, meestal binnen 5 minuten lopen.",
      "De buurt is overzichtelijk en rustig, wat zorgt voor ontspannen wandelrondes zonder drukke verkeerssituaties.",
      "Voor spoedgevallen of boodschappen voor je hond moet je de buurt uit, wat een aandachtspunt is als je behoefte hebt aan directe nabijheid van dierenartsen of winkels.",
      "De groene omgeving en rustige sfeer maken Gent-Muide vooral geschikt voor baasjes die prioriteit geven aan ruimte en rust boven voorzieningen.",
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
      "Hieronder vind je de belangrijkste cijfers over Gent-Muide. Deze statistieken helpen je om de buurt beter te begrijken en te vergelijken met andere wijken.",
    medianPrice: 485175,
    inhabitants: 12450,
    availableHomes: 29,
    pricePerSqm: 3650,
  },
};
