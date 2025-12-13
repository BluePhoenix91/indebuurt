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

export interface Label {
  text: string;
  icon: string;
}

export interface Neighborhood {
  id: string;
  name: string;
  subtitle: string;
  postalCode: string;
  inhabitants: number;
  labels: Label[];
  intro: string;
  coordinates: {
    lat: number;
    lon: number;
    zoom: number;
  };
  valueCards: ValueCard[];
  facilities: {
    intro: string;
  };
  dogParks: {
    intro: string;
    parks: DogPark[];
  };
  vets: {
    intro: string;
    practices: Vet[];
  };
  petStores: {
    intro: string;
    stores: PetStore[];
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

// Re-export neighborhoods from the neighborhoods folder
// This maintains backward compatibility with existing imports
export { neighborhoods } from "./neighborhoods/index";
