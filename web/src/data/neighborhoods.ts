export interface ValueCard {
  icon: string;
  title: string;
  distance: string;
  distanceIcon?: string;
  description: string;
  detail: string;
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
}

export const neighborhoods: Record<string, Neighborhood> = {
  'antwerpen-zuid': {
    id: 'antwerpen-zuid',
    name: 'Antwerpen-Zuid',
    subtitle: 'Voor baasjes die op zoek zijn naar een buurt die klapt voor hen én hun hond',
    postalCode: '2000',
    inhabitants: 12450,
    labels: ['Hondvriendelijk', 'Populair buurt', 'Veel groen'],
    intro: `Antwerpen-Zuid is een van de meest hondvriendelijke buurten van Antwerpen.

Met korte wandelafstanden, 3 hondenspeelweides en groene rustige binnen wandelafstand vind elke buurt meteen ering en voelbare aan voor jou en je hond. Bovendien woon je vooraf hoe makkelijk het dagelijkse leven hier aanvoelt: boodschappen op de hoek, veilige verkeersituaties en alle wat overzichtelijke richten makkelijk te voet of met de fiets.

Deze buurt is ideaal voor mensen die hun hond zien als volwaarding gezinslid — en graag wonen op een plek waar dat vanzelfsprekend is.`,
    mapImage: {
      src: '/images/maps/antwerpen-zuid-hero.svg',
      alt: 'Kaart van Antwerpen-Zuid',
    },
    valueCards: [
      {
        icon: 'fa-solid fa-dog',
        title: 'Hondenparken',
        distance: '10 mins',
        distanceIcon: 'fa-regular fa-person-walking',
        description: 'Altijd dichtbij - perfect voor snelle wandelroutes',
        detail: 'Meestal binnen 800 meter',
      },
      {
        icon: 'fa-solid fa-shield-heart',
        title: 'Dierenartsen',
        distance: '10 mins',
        distanceIcon: 'fa-regular fa-car',
        description: 'Altijd snel hulp bij nood',
        detail: 'Meestal binnen 1 000 meter',
      },
      {
        icon: 'fa-solid fa-bag-shopping',
        title: 'Dierenwinkels',
        distance: '20 mins',
        distanceIcon: 'fa-regular fa-bicycle',
        description: 'Voor een extra speeltje ben je nooit ver weg',
        detail: 'Meestal binnen 600 meter',
      },
      {
        icon: 'fa-solid fa-trees',
        title: 'Groene ruimtes',
        distance: '10 mins',
        distanceIcon: 'fa-regular fa-person-walking',
        description: 'Je moet er even voor stappen',
        detail: 'Meestal binnen de 2 000 meter',
      },
      {
        icon: 'fa-solid fa-bus',
        title: 'Openbaar vervoer',
        distance: '10 mins',
        distanceIcon: 'fa-regular fa-person-walking',
        description: 'Je raakt overal makkelijk zonder auto',
        detail: 'Meestal binnen 800 meter',
      },
      {
        icon: 'fa-solid fa-house',
        title: 'Gemiddelde woningprijs',
        distance: '10 mins',
        distanceIcon: 'fa-regular fa-person-walking',
        description: 'Prijsniveau in lijn met overige buurten',
        detail: '€ 385.000 mediaanprijs',
      },
    ],
  },
};
