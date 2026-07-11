import type { TourLog } from '../../types/tourLog.types';
import TourLogCard from './TourLogCard';

interface Props {
  logs: TourLog[];
  onEdit: (log: TourLog) => void;
  onDelete: (id: string) => void;
}

export default function TourLogList({ logs, onEdit, onDelete }: Props) {
  if (logs.length === 0) {
    return <p className="empty-state">No logs yet. Record your first trip!</p>;
  }
  return (
    <div className="flex flex-col gap-2">
      {logs.map((log) => (
        <TourLogCard key={log.id} log={log} onEdit={onEdit} onDelete={onDelete} />
      ))}
    </div>
  );
}
