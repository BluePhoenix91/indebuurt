import type { Neighborhood } from "../neighborhoods";

export const gentWondelgem: Neighborhood = {
  id: "gent-wondelgem",
  name: "Wondelgem",
  subtitle:
    "Voor baasjes die willen verdwalen tussen eindeloos veel parken en groene plekken",
  postalCode: "9032",
  inhabitants: 18212,
  labels: [
    { text: "Zeer veel groen", icon: "fa-regular fa-trees" },
    { text: "Gezinsvriendelijk", icon: "fa-regular fa-people-roof" },
    { text: "Sterke gemeenschap", icon: "fa-regular fa-handshake" }
  ],
  intro: `Wondelgem is een groeiende gezinsbuurt waar groen domineert. Met drieëntwintig parken is dit een van de groenste buurten van Gent — van het intieme Vroonstalledries tot Het Houtjen, Ter Durmenpark en het uitgebreide Driemasterpark dat uitgroeit tot een volwaardig wijkpark. De buurt groeide de laatste 25 jaar spectaculair: van 13.442 inwoners in 2000 naar 18.212 vandaag (+36%), vooral door de komst van jonge gezinnen. Bijna 30% van de inwoners zijn gezinnen met minderjarige kinderen, wat je direct merkt in de parken waar baasjes elkaar tegenkomen tijdens het uitlaten.

Wat Wondelgem bijzonder maakt, is de sterke gemeenschapszin: met meer dan 60 verenigingen, sportclubs en buurtinitiatieven is er een rijk verenigingsleven dat zorgt voor sociale cohesie. Je hebt niet één, maar twee hondenspeelweiden op korte loopafstand — bij de Boeierstraat en aan de Industrieweg. Tom & Co ligt midden in de buurt op acht minuten lopen, waardoor je makkelijk voeding en accessoires kunt halen zonder de wijk uit te moeten. Het enige wat verder weg ligt, is de dierenarts: de dichtstbijzijnde praktijk zit in Mariakerke, wat een rit met de auto of fiets vergt.

Deze buurt is vooral geschikt voor baasjes die houden van een groene, gezinsvriendelijke omgeving met sterke sociale cohesie en graag hun hond regelmatig naar een speelweide brengen — en minder waarde hechten aan een dierenarts om de hoek.`,
  coordinates: {
    lat: 51.0887,
    lon: 3.7118,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "4 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Twee beschikbaar binnen wandelafstand",
      detail: "Meestal binnen 300 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "9 mins",
      distanceIcon: "fa-regular fa-car",
      description: "Dichtstbijzijnde praktijk in Mariakerke",
      detail: "Meestal binnen 3 100 meter",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "8 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Tom & Co midden in de buurt",
      detail: "Meestal binnen 550 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "1 min",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Eén van de groenste buurten van Gent",
      detail: "Meestal binnen 50 meter",
    },
    {
      icon: "fa-solid fa-bus",
      title: "Openbaar vervoer",
      distance: "6 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Goede busverbindingen naar centrum",
      detail: "Meestal binnen 500 meter",
    },
    {
      icon: "fa-regular fa-house",
      title: "Gemiddelde woningprijs",
      distance: "",
      distanceIcon: "",
      description: "Rustig wonen in groene omgeving",
      detail: "€ 295.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "In Wondelgem zijn de hondenvoorzieningen goed geregeld: twee hondenspeelweiden op loopafstand en Tom & Co midden in de buurt voor al je hondenbenodigdheden. Alleen de dierenarts ligt verder weg, in Mariakerke, wat betekent dat je voor een bezoek de auto of fiets zult pakken. Waar de buurt écht in uitblinkt, is groen: met drieëntwintig parken heb je eindeloos veel wandelroutes en uitlaatplekken binnen handbereik.",
  },
  dogParks: {
    intro:
      "Wondelgem heeft twee omheinde hondenspeelweiden: één bij de Boeierstraat op vier minuten lopen, en één aan de Industrieweg op veertien minuten lopen. Beide speelweiden bieden voldoende ruimte voor je hond om veilig los te lopen en te spelen met andere honden. Voor wie meer variatie zoekt, biedt de buurt ook drieëntwintig parken — van het intieme Vroonstalledries tot het grotere Merenpark en Het Houtjen — die allemaal perfect zijn voor rustige wandelingen.",
    parks: [
      {
        name: "Hondenspeelweide Boeierstraat",
        icon: "fa-solid fa-bench-tree",
        distance: "4 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.087269,
          lon: 3.720681,
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
        name: "Hondenspeelweide Industrieweg",
        icon: "fa-solid fa-bench-tree",
        distance: "14 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.092409,
          lon: 3.699085,
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
      "In Wondelgem zijn geen dierenartsenpraktijken binnen de buurt zelf. De dichtstbijzijnde praktijk is Dierenartsencentrum De Brug in Mariakerke, op ongeveer drie kilometer afstand — wat in de praktijk betekent dat je de auto of fiets zult nemen voor een bezoek aan de dierenarts.",
    practices: [
      {
        name: "Dierenartsencentrum De Brug",
        icon: "fa-solid fa-hospital",
        street: "Brugsesteenweg",
        streetNumber: "588",
        municipality: "Mariakerke",
        postalCode: "9030",
        distance: "9 mins",
        distanceIcon: "fa-regular fa-car",
        coordinates: {
          lat: 51.074023,
          lon: 3.680784,
        },
      },
    ],
  },
  petStores: {
    intro:
      "In Wondelgem vind je Tom & Co op de Botestraat 14, midden in de buurt. Op acht minuten lopen kun je er terecht voor voeding, snacks, speelgoed en alle andere benodigdheden voor je hond — een groot voordeel ten opzichte van buurten waar je de wijk uit moet voor een dierenwinkel.",
    stores: [
      {
        name: "Tom & Co",
        icon: "fa-solid fa-store",
        street: "Botestraat",
        streetNumber: "14",
        municipality: "Gent",
        postalCode: "9032",
        distance: "8 mins",
        distanceIcon: "fa-regular fa-person-walking",
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
      "Het leven in Wondelgem met een hond draait om keuze, groen en gemeenschap. Met drieëntwintig parken binnen de wijk heb je eindeloos veel wandelroutes: van korte ochtendrondes in het Vroonstalledries tot langere avondwandelingen door Het Houtjen, Ter Durmenpark of het uitgebreide Driemasterpark. De buurt voelt gezinsvriendelijk en groen, met veel jonge gezinnen en baasjes die elkaar tegenkomen tijdens het uitlaten — de sterke sociale cohesie merk je direct in de parken.",
    benefits: [
      "Je ochtend begint met een korte wandeling naar één van de vele nabijgelegen parken — het Vroonstalledries ligt op drie minuten, andere parken letterlijk om de hoek. Door de vele jonge gezinnen in de buurt (bijna 30% heeft kinderen) kom je vaak andere baasjes tegen die ook met het gezin wandelen.",
      "Met twee hondenspeelweiden heb je de luxe om te variëren: de ene keer ga je naar de Boeierstraat (vier minuten), de andere keer naar de Industrieweg voor een langere wandeling (veertien minuten). Door de sterke gemeenschapszin (60+ verenigingen) leer je snel andere hondenbaasjes kennen.",
      "Tom & Co ligt midden in de buurt, waardoor je makkelijk voeding en accessoires kunt halen zonder de wijk uit te moeten — een groot voordeel voor baasjes die vaak iets nodig hebben.",
      "Voor dierenarts moet je naar Mariakerke, wat lastig kan zijn bij spoedgevallen — een aandachtspunt als je veel waarde hecht aan nabijheid van veterinaire zorg. Maar de overvloed aan groen, sterke gemeenschap en gezinsvriendelijke sfeer maken Wondelgem tot een fantastische buurt voor honden en hun baasjes.",
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
      "Hieronder vind je de belangrijkste cijfers over Gent Wondelgem. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 295000,
    inhabitants: 18212,
    availableHomes: 22,
    pricePerSqm: 3150,
  },
};
