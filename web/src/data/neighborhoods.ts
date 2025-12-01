export interface ValueCard {
  icon: string;
  title: string;
  distance: string;
  distanceIcon?: string;
  description: string;
  detail: string;
}

export interface DogPark {
  icon: string;
  name: string;
  distance: string;
  distanceIcon?: string;
  coordinates: {
    lat: number;
    lon: number;
  };
  features: {
    text: string;
    icon: string;
  }[];
}

export interface Vet {
  icon: string;
  name: string;
  street: string;
  streetNumber: string;
  bus?: string;
  municipality: string;
  postalCode: string;
  distance: string;
  distanceIcon?: string;
  coordinates: {
    lat: number;
    lon: number;
  };
}

export interface PetStore {
  icon: string;
  name: string;
  street: string;
  streetNumber: string;
  bus?: string;
  municipality: string;
  postalCode: string;
  distance: string;
  distanceIcon?: string;
  coordinates: {
    lat: number;
    lon: number;
  };
}

export interface Neighborhood {
  id: string;
  name: string;
  subtitle: string;
  postalCode: string;
  inhabitants: number;
  labels: string[];
  intro: string;
  coordinates: {
    lat: number;
    lon: number;
    zoom: number;
  };
  mapImage: {
    src: string;
    alt: string;
  };
  valueCards: ValueCard[];
  facilities: {
    intro: string;
  };
  dogParks: {
    intro: string;
    parks: DogPark[];
    mapImage: {
      src: string;
      alt: string;
    };
  };
  vets: {
    intro: string;
    practices: Vet[];
    mapImage: {
      src: string;
      alt: string;
    };
  };
  petStores: {
    intro: string;
    stores: PetStore[];
    mapImage: {
      src: string;
      alt: string;
    };
  };
  dailyLife: {
    title: string;
    intro: string;
    benefits: string[];
  };
  contributionCTA: {
    heading: string;
    intro: string;
    typeformId: string;
  };
  statistics: {
    intro: string;
    medianPrice: number;
    inhabitants: number;
    availableHomes: number;
    pricePerSqm: number;
  };
}

export const neighborhoods: Record<string, Neighborhood> = {
  "antwerpen-zuid": {
    id: "antwerpen-zuid",
    name: "Antwerpen-Zuid",
    subtitle:
      "Voor baasjes die op zoek zijn naar een buurt die klapt voor hen én hun hond",
    postalCode: "2000",
    inhabitants: 12450,
    labels: ["Hondvriendelijk", "Populair buurt", "Veel groen"],
    intro: `Antwerpen-Zuid is een van de meest hondvriendelijke buurten van Antwerpen.

Met korte wandelafstanden, 3 hondenspeelweides en groene rustige binnen wandelafstand vind elke buurt meteen ering en voelbare aan voor jou en je hond. Bovendien woon je vooraf hoe makkelijk het dagelijkse leven hier aanvoelt: boodschappen op de hoek, veilige verkeersituaties en alle wat overzichtelijke richten makkelijk te voet of met de fiets.

Deze buurt is ideaal voor mensen die hun hond zien als volwaarding gezinslid — en graag wonen op een plek waar dat vanzelfsprekend is.`,
    coordinates: {
      lat: 51.2000,
      lon: 4.4000,
      zoom: 14,
    },
    mapImage: {
      src: "/images/maps/antwerpen-zuid-hero.svg",
      alt: "Kaart van Antwerpen-Zuid",
    },
    valueCards: [
      {
        icon: "fa-solid fa-dog",
        title: "Hondenparken",
        distance: "10 mins",
        distanceIcon: "fa-regular fa-person-walking",
        description: "Altijd dichtbij - perfect voor snelle wandelroutes",
        detail: "Meestal binnen 800 meter",
      },
      {
        icon: "fa-solid fa-shield-heart",
        title: "Dierenartsen",
        distance: "10 mins",
        distanceIcon: "fa-regular fa-car",
        description: "Altijd snel hulp bij nood",
        detail: "Meestal binnen 1 000 meter",
      },
      {
        icon: "fa-solid fa-bag-shopping",
        title: "Dierenwinkels",
        distance: "20 mins",
        distanceIcon: "fa-regular fa-bicycle",
        description: "Voor een extra speeltje ben je nooit ver weg",
        detail: "Meestal binnen 600 meter",
      },
      {
        icon: "fa-solid fa-trees",
        title: "Groene ruimtes",
        distance: "10 mins",
        distanceIcon: "fa-regular fa-person-walking",
        description: "Je moet er even voor stappen",
        detail: "Meestal binnen de 2 000 meter",
      },
      {
        icon: "fa-solid fa-bus",
        title: "Openbaar vervoer",
        distance: "10 mins",
        distanceIcon: "fa-regular fa-person-walking",
        description: "Je raakt overal makkelijk zonder auto",
        detail: "Meestal binnen 800 meter",
      },
      {
        icon: "fa-solid fa-house",
        title: "Gemiddelde woningprijs",
        distance: "10 mins",
        distanceIcon: "fa-regular fa-person-walking",
        description: "Prijsniveau in lijn met overige buurten",
        detail: "€ 385.000 mediaanprijs",
      },
    ],
    facilities: {
      intro:
        "In Antwerpen-Zuid vind je vrijwel alles wat je als hondenbaasje nodig hebt binnen korte wandelafstand. Dat maakt het leven onverwacht makkelijk: je hoeft nooit ver te gaan voor een veilige uitlaatplek, een dierenwinkel of gewoon een rustige avondronde.",
    },
    dogParks: {
      intro:
        "Antwerpen-Zuid heeft meerdere volledig omheinde hondenspeelweides en groene losloopzones waar je hond veilig kan ravotten en spelen met andere honden. Alle locaties liggen binnen wandelafstand en bieden ruimte, vrijheid en sociale interactie voor je viervoeter.",
      parks: [
        {
          name: "Hondenspeelweide Morekstraat",
          icon: "fa-solid fa-bench-tree",
          distance: "8 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.1980,
            lon: 4.3980,
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
            {
              text: "Watervoorziening aanwezig",
              icon: "fa-regular fa-droplet",
            },
          ],
        },
        {
          name: "Hondenspeelweide Aziëstraat",
          icon: "fa-solid fa-bench-tree",
          distance: "12 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.2020,
            lon: 4.4020,
          },
          features: [
            {
              text: "Volledig omheind terrein",
              icon: "fa-regular fa-shield-check",
            },
            { text: "Los lopen toegestaan", icon: "fa-regular fa-dog-leashed" },
            { text: "Open van 07:00 tot 21:00", icon: "fa-regular fa-clock" },
            {
              text: "Geschikt voor alle hondenrassen",
              icon: "fa-regular fa-paw",
            },
          ],
        },
        {
          name: "Hondenspeelweide Sudermanstraat",
          icon: "fa-solid fa-bench-tree",
          distance: "15 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.1960,
            lon: 4.3950,
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
            { text: "Schaduwplekken aanwezig", icon: "fa-regular fa-tree" },
          ],
        },
        {
          name: "Park Den Brandt",
          icon: "fa-solid fa-trees",
          distance: "18 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.2040,
            lon: 4.4050,
          },
          features: [
            { text: "Groot groen domein", icon: "fa-regular fa-tree-large" },
            {
              text: "Losloopzone in aangewezen zones",
              icon: "fa-regular fa-dog-leashed",
            },
            { text: "Open van 06:00 tot 22:00", icon: "fa-regular fa-clock" },
            {
              text: "Wandelpaden en open velden",
              icon: "fa-regular fa-signs-post",
            },
          ],
        },
      ],
      mapImage: {
        src: "/images/maps/antwerpen-zuid-dogparks.svg",
        alt: "Locaties van hondenspeelweides in Antwerpen-Zuid",
      },
    },
    vets: {
      intro:
        "In Antwerpen-Zuid zijn meerdere dierenartsenpraktijken binnen korte afstand beschikbaar. Of het nu voor routinecontroles, vaccinaties of noodgevallen is, je hebt altijd snel toegang tot professionele veterinaire zorg.",
      practices: [
        {
          name: "Dierenkliniek Antwerpen-Zuid",
          icon: "fa-solid fa-shield-heart",
          street: "Mechelsesteenweg",
          streetNumber: "245",
          municipality: "Antwerpen",
          postalCode: "2018",
          distance: "8 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.1990,
            lon: 4.3990,
          },
        },
        {
          name: "Dierenarts Van den Berghe",
          icon: "fa-solid fa-shield-heart",
          street: "Borsbeeksebrug",
          streetNumber: "12",
          municipality: "Antwerpen",
          postalCode: "2018",
          distance: "12 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.2010,
            lon: 4.4010,
          },
        },
        {
          name: "Dierenkliniek Deurne",
          icon: "fa-solid fa-shield-heart",
          street: "Turnhoutsebaan",
          streetNumber: "456",
          municipality: "Antwerpen",
          postalCode: "2140",
          distance: "15 mins",
          distanceIcon: "fa-regular fa-car",
          coordinates: {
            lat: 51.2030,
            lon: 4.4030,
          },
        },
      ],
      mapImage: {
        src: "/images/maps/antwerpen-zuid-vets.svg",
        alt: "Locaties van dierenartsenpraktijken in Antwerpen-Zuid",
      },
    },
    petStores: {
      intro:
        "Voor hondenvoer, speeltjes, accessoires en andere benodigdheden zijn er verschillende dierenwinkels in de buurt. Alle winkels liggen op wandel- of fietsafstand, waardoor je makkelijk even langs kunt gaan voor een nieuwe voorraad.",
      stores: [
        {
          name: "Dierenwinkel Antwerpen-Zuid",
          icon: "fa-solid fa-bag-shopping",
          street: "Mechelsesteenweg",
          streetNumber: "189",
          municipality: "Antwerpen",
          postalCode: "2018",
          distance: "6 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.1970,
            lon: 4.3970,
          },
        },
        {
          name: "Pet Shop Deurne",
          icon: "fa-solid fa-bag-shopping",
          street: "Turnhoutsebaan",
          streetNumber: "312",
          municipality: "Antwerpen",
          postalCode: "2140",
          distance: "12 mins",
          distanceIcon: "fa-regular fa-bicycle",
          coordinates: {
            lat: 51.2005,
            lon: 4.4005,
          },
        },
        {
          name: "Dierenwinkel Het Huisdier",
          icon: "fa-solid fa-bag-shopping",
          street: "Borsbeeksebrug",
          streetNumber: "45",
          municipality: "Antwerpen",
          postalCode: "2018",
          distance: "10 mins",
          distanceIcon: "fa-regular fa-person-walking",
          coordinates: {
            lat: 51.1985,
            lon: 4.3985,
          },
        },
      ],
      mapImage: {
        src: "/images/maps/antwerpen-zuid-pet-stores.svg",
        alt: "Locaties van dierenwinkels in Antwerpen-Zuid",
      },
    },
    dailyLife: {
      title: "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
      intro:
        "Het leven in Antwerpen-Zuid met een hond voelt onverwacht makkelijk. De combinatie van korte wandelafstanden, veilige uitlaatplekken en alle benodigde voorzieningen binnen handbereik maakt dat je dagelijkse routine soepel verloopt zonder dat je er veel over na hoeft te denken.",
      benefits: [
        "Je dag begint rustiger wanneer je hond zijn energie kwijt kan in het park in de Morekstraat, nog voor jij aan je koffie toe bent.",
        "De buurt voelt sociaal en vertrouwd: baasjes herkennen elkaar, groeten elkaar, en delen tips over routes en hondvriendelijke plekjes.",
        "En als er ooit iets gebeurt, is er altijd een dierenarts of dierenwinkel dichtbij.",
        'De combinatie van korte wandelrondes en langere groene routes maakt je leven "makkelijker zonder dat je het doorhebt".',
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
        "Hieronder vind je de belangrijkste cijfers over Antwerpen-Zuid. Deze statistieken helpen je om de buurt beter te begrijpen en te vergelijken met andere wijken.",
      medianPrice: 485175,
      inhabitants: 12450,
      availableHomes: 29,
      pricePerSqm: 3650,
    },
  },
};
