import type { WeatherData } from '../../types/tour.types';

interface Props {
  weather: WeatherData | null;
}

// Unique feature: current weather at the tour's starting location (Open-Meteo, no API key).
export default function WeatherWidget({ weather }: Props) {
  if (!weather) return null;

  return (
    <div className="weather-widget card">
      <span>{weather.isDay ? '☀️' : '🌙'}</span>
      <span>{weather.description}</span>
      <span>{Math.round(weather.temperatureCelsius)}°C</span>
      <span className="text-faint">Wind {Math.round(weather.windSpeedKmh)} km/h</span>
    </div>
  );
}
