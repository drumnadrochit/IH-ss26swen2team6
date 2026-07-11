export type TransportType = 'Bike' | 'Hike' | 'Running' | 'Vacation';

export interface Tour {
  id: string;
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
  distance: number;
  estimatedTime: number;
  routeImagePath: string | null;
  popularity: number;
  childFriendliness: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTourRequest {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

export interface UpdateTourRequest {
  name: string;
  description: string;
  from: string;
  to: string;
  transportType: TransportType;
}

export interface RouteData {
  distance: number;
  duration: number;
  coordinates: [number, number][] | null;
}

export interface WeatherData {
  temperatureCelsius: number;
  windSpeedKmh: number;
  weatherCode: number;
  description: string;
  isDay: boolean;
}
