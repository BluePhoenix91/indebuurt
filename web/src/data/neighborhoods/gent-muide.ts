import type { Neighborhood } from "../neighborhoods";

export const gentMuide: Neighborhood = {
  id: "gent-muide",
  name: "Muide-Meulestede-Afrikalaan",
  subtitle:
    "Voor baasjes die op zoek zijn naar een buurt die klapt voor hen én hun hond",
  dateAdded: "2024-12-10",
  postalCode: "9000",
  inhabitants: 12450,
  labels: [
    { text: "Wandelvriendelijk", icon: "fa-regular fa-person-walking" },
    { text: "Opkomende buurt", icon: "fa-regular fa-arrow-trend-up" },
    { text: "Veel groen", icon: "fa-regular fa-trees" },
  ],
  intro: `De Muide is een buurt met karakter: oude havenkranen, water en veel creatieve energie. Je wandelt er met je hond langs brede kades, tussen voormalige pakhuizen en nieuwe gezinsbuurten. Het Kapitein Zeppospark aan het Houtdok is de groene long van de wijk — 3 hectare park waar je binnen 5 minuten loopt en waar baasjes elkaar kruisen tijdens de ochtend- en avondrondes.

Wat de Muide bijzonder maakt, is hoe snel de buurt verandert van industrieel naar rustig woonerf. De 4 kilometer lange maritieme promenade langs de oude dokken biedt ruimte voor lange wandelingen met je hond, terwijl groene plekken als het Kapitein Zeppospark perfect zijn voor een korte uitlaatbeurt. Een hondenspeelweide ligt op 18 minuten wandelen, maar dierenartsen en dierenwinkels zijn er niet in de buurt zelf.

Deze buurt is vooral geschikt voor baasjes die zoeken naar ruimte, water en een wijk die nog in beweging is — en minder waarde hechten aan nabijheid van gespecialiseerde hondenvoorzieningen.`,
  coordinates: {
    lat: 51.0759,
    lon: 3.7307,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "18 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Eén beschikbaar binnen wandelafstand",
      detail: "Meestal binnen 1 500 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
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
      icon: "fa-regular fa-trees",
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
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Prijsniveau is lager dan in de buurtwijken",
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
    intro: "In Gent-Muide zijn geen dierenwinkels beschikbaar.",
    stores: [],
  },
  dailyLife: {
    title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    intro:
      "Het leven in de Muide met een hond draait om water, ruimte en variatie. De 4 kilometer lange maritieme promenade langs de oude dokken geeft je eindeloos veel wandelroutes, van korte ochtendrondes langs de kades tot langere avondwandelingen met uitzicht over het water. Het Kapitein Zeppospark ligt op enkele minuten en is dé plek waar baasjes uit de buurt elkaar tegenkomen.",
    benefits: [
      "Je ochtend begint met een snelle wandeling naar het Kapitein Zeppospark, of — als je tijd hebt — een langere route langs de Schipperskaai of Kleindokkaai met uitzicht op de oude havenkranen.",
      "De buurt voelt levendig maar overzichtelijk: een mix van gezinnen, creatieve types en hondenbaasjes die elkaar groeten en tips delen over routes en uitlaatplekken.",
      "Voor dierenarts of dierenwinkels moet je de wijk uit, wat lastig kan zijn bij spoedgevallen — een aandachtspunt als je veel waarde hecht aan nabijheid van deze voorzieningen.",
      "De combinatie van industrieel erfgoed, water en groen maakt elke wandeling net iets anders — en dat maakt het dagelijkse leven met je hond in de Muide minder voorspelbaar en juist daarom leuker.",
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
    medianPrice: 325000,
    inhabitants: 12450,
    availableHomes: 29,
    pricePerSqm: 3650,
  },
};
