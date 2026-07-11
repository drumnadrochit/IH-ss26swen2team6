import { useNavigate } from 'react-router-dom';
import type { Tour } from '../../types/tour.types';

interface Props {
  tour: Tour;
  onDelete: (id: string) => void;
}

export default function TourCard({ tour, onDelete }: Props) {
  const navigate = useNavigate();

  return (
    <div className="card flex-row justify-between items-start gap-3 tour-card">
      {tour.routeImagePath && (
        <img src={tour.routeImagePath} alt={tour.name} className="tour-card-thumb" />
      )}
      <div className="flex-1">
        <div className="flex-row items-center gap-2 mb-1">
          <h3 className="section-title" style={{ fontSize: 16 }}>{tour.name}</h3>
          <span className="badge">{tour.transportType}</span>
        </div>
        <p className="text-sm text-muted mb-1">
          {tour.from} &rarr; {tour.to}
        </p>
        <p className="text-sm text-body">
          {tour.distance > 0 ? `${tour.distance} km` : 'Distance: N/A'}
          {tour.estimatedTime > 0 && ` · ~${tour.estimatedTime} min`}
        </p>
        <p className="text-xs text-faint mt-2">
          Popularity: {tour.popularity} log{tour.popularity !== 1 ? 's' : ''} · {tour.childFriendliness}
        </p>
      </div>
      <div className="flex-row gap-2 flex-shrink-0">
        <button className="btn-secondary btn-sm" onClick={() => navigate(`/tours/${tour.id}`)}>
          View
        </button>
        <button className="btn-danger btn-sm" onClick={() => onDelete(tour.id)}>
          Delete
        </button>
      </div>
    </div>
  );
}
