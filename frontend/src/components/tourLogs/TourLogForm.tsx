import { useState } from 'react';
import type { TourLog, CreateTourLogRequest } from '../../types/tourLog.types';
import { getApiErrorMessage } from '../../utils/apiError';

interface Props {
  initial?: TourLog;
  onSubmit: (data: CreateTourLogRequest) => Promise<void>;
  onCancel: () => void;
}

export default function TourLogForm({ initial, onSubmit, onCancel }: Props) {
  const toLocalInput = (iso?: string) =>
    iso ? new Date(iso).toISOString().slice(0, 16) : new Date().toISOString().slice(0, 16);

  const [dateTime, setDateTime] = useState(toLocalInput(initial?.dateTime));
  const [comment, setComment] = useState(initial?.comment ?? '');
  const [difficulty, setDifficulty] = useState(String(initial?.difficulty ?? 3));
  const [totalDistance, setTotalDistance] = useState(String(initial?.totalDistance ?? ''));
  const [totalTime, setTotalTime] = useState(String(initial?.totalTime ?? ''));
  const [rating, setRating] = useState(String(initial?.rating ?? 3));
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const diffNum = parseInt(difficulty);
    const distNum = parseFloat(totalDistance);
    const timeNum = parseInt(totalTime);
    const ratingNum = parseInt(rating);

    if (!dateTime) { setError('Date is required.'); return; }
    if (diffNum < 1 || diffNum > 5) { setError('Difficulty must be 1-5.'); return; }
    if (ratingNum < 1 || ratingNum > 5) { setError('Rating must be 1-5.'); return; }
    if (isNaN(distNum) || distNum <= 0) { setError('Distance must be a positive number.'); return; }
    if (isNaN(timeNum) || timeNum <= 0) { setError('Time must be a positive number.'); return; }

    setLoading(true);
    try {
      await onSubmit({
        dateTime: new Date(dateTime).toISOString(),
        comment,
        difficulty: diffNum,
        totalDistance: distNum,
        totalTime: timeNum,
        rating: ratingNum,
      });
    } catch (err: unknown) {
      setError(getApiErrorMessage(err, 'Failed to save log.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="card flex flex-col gap-3 mb-4">
      <h4 style={{ fontWeight: 600 }}>{initial ? 'Edit Log' : 'Add Log'}</h4>
      <div className="grid-2col">
        <div>
          <label className="label">Date & Time *</label>
          <input type="datetime-local" value={dateTime} onChange={(e) => setDateTime(e.target.value)} />
        </div>
        <div>
          <label className="label">Difficulty (1-5) *</label>
          <input type="number" min={1} max={5} value={difficulty} onChange={(e) => setDifficulty(e.target.value)} />
        </div>
        <div>
          <label className="label">Distance (km) *</label>
          <input type="number" min={0.1} step={0.1} value={totalDistance} onChange={(e) => setTotalDistance(e.target.value)} placeholder="e.g. 12.5" />
        </div>
        <div>
          <label className="label">Time (minutes) *</label>
          <input type="number" min={1} value={totalTime} onChange={(e) => setTotalTime(e.target.value)} placeholder="e.g. 90" />
        </div>
        <div>
          <label className="label">Rating (1-5) *</label>
          <input type="number" min={1} max={5} value={rating} onChange={(e) => setRating(e.target.value)} />
        </div>
      </div>
      <div>
        <label className="label">Comment</label>
        <textarea value={comment} onChange={(e) => setComment(e.target.value)} rows={2} placeholder="How was it?" style={{ resize: 'vertical' }} />
      </div>
      {error && <p className="error">{error}</p>}
      <div className="flex-row gap-2">
        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Saving...' : (initial ? 'Update' : 'Add Log')}
        </button>
        <button type="button" className="btn-secondary" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  );
}
