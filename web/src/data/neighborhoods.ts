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
  },
};
