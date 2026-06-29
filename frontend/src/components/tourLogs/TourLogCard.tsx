import type { TourLog } from '../../types/tourLog.types';

interface Props {
  log: TourLog;
  onEdit: (log: TourLog) => void;
  onDelete: (id: string) => void;
}

const STAR = '★';
const EMPTY_STAR = '☆';

function Stars({ n, max = 5 }: { n: number; max?: number }) {
  return (
    <span className="star-rating">
      {Array.from({ length: max }, (_, i) => i < n ? STAR : EMPTY_STAR).join('')}
    </span>
  );
}

export default function TourLogCard({ log, onEdit, onDelete }: Props) {
  const date = new Date(log.dateTime).toLocaleDateString();
  return (
    <div className="card flex-row justify-between items-start">
      <div className="flex-1">
        <div className="flex-row gap-3 items-center mb-1">
          <span style={{ fontWeight: 600, fontSize: 14 }}>{date}</span>
          <Stars n={log.rating} />
          <span className="text-xs text-muted">Difficulty: {log.difficulty}/5</span>
        </div>
        <p className="text-sm text-body mb-1">{log.comment || <em className="text-faint">No comment</em>}</p>
        <p className="text-xs text-faint">
          {log.totalDistance} km · {log.totalTime} min
        </p>
      </div>
      <div className="flex-row gap-2 flex-shrink-0">
        <button className="btn-secondary btn-sm" onClick={() => onEdit(log)}>Edit</button>
        <button className="btn-danger btn-sm" onClick={() => onDelete(log.id)}>Delete</button>
      </div>
    </div>
  );
}
