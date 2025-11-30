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
  features: {
    text: string;
    icon: string;
  }[];
}

export interface Neighborhood {
  id: string;
  name: string;
  subtitle: string;
  postalCode: string;
  inhabitants: number;
  labels: string[];
  intro: string;
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
          features: [
            { text: "Volledig omheind terrein", icon: "fa-regular fa-shield-check" },
            { text: "Los lopen toegestaan", icon: "fa-regular fa-dog-leashed" },
            { text: "Open van zonsopgang tot zonsondergang", icon: "fa-regular fa-clock" },
            { text: "Watervoorziening aanwezig", icon: "fa-regular fa-droplet" },
          ],
        },
        {
          name: "Hondenspeelweide Aziëstraat",
          icon: "fa-solid fa-bench-tree",
          distance: "12 mins",
          distanceIcon: "fa-regular fa-person-walking",
          features: [
            { text: "Volledig omheind terrein", icon: "fa-regular fa-shield-check" },
            { text: "Los lopen toegestaan", icon: "fa-regular fa-dog-leashed" },
            { text: "Open van 07:00 tot 21:00", icon: "fa-regular fa-clock" },
            { text: "Geschikt voor alle hondenrassen", icon: "fa-regular fa-paw" },
          ],
        },
        {
          name: "Hondenspeelweide Sudermanstraat",
          icon: "fa-solid fa-bench-tree",
          distance: "15 mins",
          distanceIcon: "fa-regular fa-person-walking",
          features: [
            { text: "Volledig omheind terrein", icon: "fa-regular fa-shield-check" },
            { text: "Los lopen toegestaan", icon: "fa-regular fa-dog-leashed" },
            { text: "Open van zonsopgang tot zonsondergang", icon: "fa-regular fa-clock" },
            { text: "Schaduwplekken aanwezig", icon: "fa-regular fa-tree" },
          ],
        },
        {
          name: "Park Den Brandt",
          icon: "fa-solid fa-trees",
          distance: "18 mins",
          distanceIcon: "fa-regular fa-person-walking",
          features: [
            { text: "Groot groen domein", icon: "fa-regular fa-tree-large" },
            { text: "Losloopzone in aangewezen zones", icon: "fa-regular fa-dog-leashed" },
            { text: "Open van 06:00 tot 22:00", icon: "fa-regular fa-clock" },
            { text: "Wandelpaden en open velden", icon: "fa-regular fa-signs-post" },
          ],
        },
      ],
      mapImage: {
        src: "/images/maps/antwerpen-zuid-dogparks.svg",
        alt: "Locaties van hondenspeelweides in Antwerpen-Zuid",
      },
    },
  },
};
