import type { Neighborhood } from "../neighborhoods";

export const gentSluizekenTolhuisHam: Neighborhood = {
  id: "gent-sluizeken-tolhuis-ham",
  name: "Sluizeken-Tolhuis-Ham",
  subtitle:
    "Voor baasjes die een groene, rustige buurt zoeken met parken om de hoek",
  postalCode: "9000",
  inhabitants: 8200,
  labels: [
    { text: "Veel parken", icon: "fa-regular fa-trees" },
    { text: "Waterrijk", icon: "fa-regular fa-water" },
    { text: "Divers", icon: "fa-regular fa-people-group" }
  ],
  intro: `Sluizeken-Tolhuis-Ham is een 19de-eeuwse gordelwijk die aan drie kanten door water wordt omringd: het Tolhuisdok en de Blaisantvest in het noorden, het Handelsdok in het oosten, en de Leie in het zuiden. Dit maakt de buurt ideaal voor baasjes die van waterkanten houden — je wandelt langs rustige kades en historische havenstructuren. Met vijftien parken binnen de wijk heb je als baasje eindeloos veel keuze voor korte en lange wandelingen. Het Tolhuispark, Vogelenzangpark en Van Crombrugghepark liggen allemaal op een paar minuten lopen.

Wat Sluizeken-Tolhuis-Ham bijzonder maakt, is het superdiverse karakter: de Sleepstraat staat bekend als dé Turkse straat van Gent, met talrijke Turkse restaurants, groenten- en fruitwinkels en stoffenzaken. De buurt verandert snel door nieuwe woonprojecten en culturele initiatieven, wat zorgt voor een levendige mix van oud en nieuw. Je loopt binnen twee minuten naar je dichtstbijzijnde park, en de hondenspeelweide aan het Neuseplein ligt op zeven minuten wandelen. Wat verder weg ligt: dierenartsen en dierenwinkels zijn er niet in de buurt zelf, waardoor je daarvoor een stuk moet fietsen of de auto moet pakken.

Deze buurt is vooral geschikt voor baasjes die houden van een diverse, levendige woonomgeving met veel groen en water — en minder waarde hechten aan nabijheid van gespecialiseerde hondenvoorzieningen zoals dierenartsen of dierenwinkels.`,
  coordinates: {
    lat: 51.062,
    lon: 3.729,
    zoom: 14,
  },
  valueCards: [
    {
      icon: "fa-regular fa-dog",
      title: "Hondenparken",
      distance: "7 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Eén beschikbaar op korte wandelafstand",
      detail: "Meestal binnen 450 meter",
    },
    {
      icon: "fa-solid fa-user-doctor",
      title: "Dierenartsen",
      distance: "24 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Dichtstbijzijnde praktijk ligt op wandelafstand",
      detail: "Meestal binnen 1 600 meter",
    },
    {
      icon: "fa-solid fa-bag-shopping",
      title: "Dierenwinkels",
      distance: "5 mins",
      distanceIcon: "fa-regular fa-bicycle",
      description: "Tom & Co bereikbaar per fiets",
      detail: "Meestal binnen 1 500 meter",
    },
    {
      icon: "fa-regular fa-trees",
      title: "Groene ruimtes",
      distance: "2 mins",
      distanceIcon: "fa-regular fa-person-walking",
      description: "Overvloed aan parken in deze buurt",
      detail: "Meestal binnen de 200 meter",
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
      description: "Betaalbaar wonen in het centrum van Gent",
      detail: "€ 285.000 mediaanprijs",
    },
  ],
  facilities: {
    intro:
      "In Sluizeken-Tolhuis-Ham zijn de gespecialiseerde hondenvoorzieningen beperkt: dierenartsen en dierenwinkels liggen buiten de buurt en vergen een fietstocht of rit met de auto. Waar de buurt wél in uitblinkt, is groen: binnen twee minuten loop je naar rustige groene plekken, en de hondenspeelweide ligt op zeven minuten wandelen — ideaal voor dagelijkse uitlaatrondjes en sociale momenten met andere baasjes.",
  },
  dogParks: {
    intro:
      "Sluizeken-Tolhuis-Ham heeft één omheinde hondenspeelweide op zeven minuten lopen: de Hondenspeelweide bij het Neuseplein. Hier kan je hond veilig los lopen en spelen met andere honden. Voor wie meer variatie zoekt, biedt de buurt ook vijftien parken — waaronder het Tolhuispark, Vogelenzangpark en Van Crombrugghepark — die allemaal perfect zijn voor rustige wandelingen en kortere uitlaatbeurten.",
    parks: [
      {
        name: "Hondenspeelweide Neuseplein",
        icon: "fa-solid fa-bench-tree",
        distance: "7 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.066983,
          lon: 3.726609,
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
    intro: "In Sluizeken-Tolhuis-Ham zijn geen dierenartsenpraktijken binnen de buurt zelf. De dichtstbijzijnde praktijk is Heughebaert Anne op de Recollettenlei, op ongeveer 24 minuten wandelen — wat in de praktijk betekent dat je de fiets of auto zult nemen voor een bezoek aan de dierenarts.",
    practices: [
      {
        name: "Heughebaert Anne",
        icon: "fa-solid fa-hospital",
        street: "Recollettenlei",
        streetNumber: "18",
        municipality: "Gent",
        postalCode: "9000",
        distance: "24 mins",
        distanceIcon: "fa-regular fa-person-walking",
        coordinates: {
          lat: 51.049784,
          lon: 3.719543,
        },
      },
    ],
  },
  petStores: {
    intro: "In Sluizeken-Tolhuis-Ham zijn geen dierenwinkels binnen de buurt zelf. De dichtstbijzijnde dierenwinkel is Tom & Co op de Dendermondsesteenweg in Dampoort, op ongeveer vijf minuten fietsen — een korte rit voor voeding, snacks en accessoires voor je hond.",
    stores: [
      {
        name: "Tom & Co",
        icon: "fa-solid fa-store",
        street: "Dendermondsesteenweg",
        streetNumber: "134b",
        municipality: "Gent",
        postalCode: "9000",
        distance: "5 mins",
        distanceIcon: "fa-regular fa-bicycle",
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
      "Het leven in Sluizeken-Tolhuis-Ham met een hond draait om parken, water en diversiteit. Als 19de-eeuwse gordelwijk omringd door het Tolhuisdok, Handelsdok en de Leie heb je eindeloos veel wandelroutes langs rustige kades en historische havenstructuren. Met vijftien parken binnen de wijk kun je variëren tussen korte ochtendrondes in het Tolhuispark en langere avondwandelingen langs het water. De buurt voelt divers en levendig — van de Turkse winkels in de Sleepstraat tot nieuwe woonprojecten en culturele initiatieven.",
    benefits: [
      "Je ochtend begint met een korte wandeling naar één van de vele nabijgelegen parken — het Tolhuispark ligt op drie minuten, het Vogelenzangpark op vijf minuten lopen. Of je kiest voor een route langs de kades van het Tolhuisdok met uitzicht op het water.",
      "De buurt voelt superdivers en levendig: je wandelt tussen Turkse restaurants en groentewinkels in de Sleepstraat, passeert nieuwe woonprojecten en oude industriële gebouwen — elke wandeling voelt anders door de mix van culturen en architectuur.",
      "Voor dierenarts of dierenwinkel moet je de wijk uit, wat lastig kan zijn bij spoedgevallen — een aandachtspunt als je veel waarde hecht aan nabijheid van deze voorzieningen.",
      "De combinatie van water, parken en diversiteit maakt elke wandeling bijzonder: je kunt variëren tussen rustige parkjes en levendige straten, tussen waterkanten en groene plekken, wat het dagelijkse leven met je hond in Sluizeken-Tolhuis-Ham veelzijdig en verrassend maakt.",
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
      "Hieronder vind je de belangrijkste cijfers over Gent Sluizeken-Tolhuis-Ham. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
    medianPrice: 285000,
    inhabitants: 8200,
    availableHomes: 18,
    pricePerSqm: 3250,
  },
};
