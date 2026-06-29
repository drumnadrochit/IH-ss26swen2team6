import type { Tour } from '../../types/tour.types';
import TourCard from './TourCard';

interface Props {
  tours: Tour[];
  onDelete: (id: string) => void;
}

export default function TourList({ tours, onDelete }: Props) {
  if (tours.length === 0) {
    return <p className="empty-state">No tours found. Create your first tour!</p>;
  }
  return (
    <div className="flex flex-col gap-3">
      {tours.map((tour) => (
        <TourCard key={tour.id} tour={tour} onDelete={onDelete} />
      ))}
    </div>
  );
}
