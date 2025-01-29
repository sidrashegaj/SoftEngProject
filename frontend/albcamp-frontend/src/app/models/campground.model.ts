export interface Campground {
  campgroundId: number;
  name: string;
  location: string;
  description: string;
  price: number;
  geometry: {
    type: string;
    coordinates: [number, number];
  };
  images: Array<{
    imageId: number;
    url: string;
    filename: string;
  }>;
  userId: number;
  author?: {
    userId: number;
    username: string;
    email: string;
  };
  reviews?: any[];
}
