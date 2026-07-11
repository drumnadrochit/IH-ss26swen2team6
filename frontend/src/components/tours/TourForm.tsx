import { useState } from 'react';
import type { CreateTourRequest, Tour, TransportType } from '../../types/tour.types';
import { getApiErrorMessage } from '../../utils/apiError';

interface Props {
  initial?: Tour;
  onSubmit: (data: CreateTourRequest, image?: File) => Promise<void>;
  onCancel: () => void;
}

const TRANSPORT_TYPES: TransportType[] = ['Bike', 'Hike', 'Running', 'Vacation'];

export default function TourForm({ initial, onSubmit, onCancel }: Props) {
  const [name, setName] = useState(initial?.name ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [from, setFrom] = useState(initial?.from ?? '');
  const [to, setTo] = useState(initial?.to ?? '');
  const [transportType, setTransportType] = useState<TransportType>(initial?.transportType ?? 'Bike');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!name.trim() || !from.trim() || !to.trim()) {
      setError('Name, From, and To are required.');
      return;
    }
    setLoading(true);
    try {
      await onSubmit({ name, description, from, to, transportType }, imageFile ?? undefined);
    } catch (err: unknown) {
      setError(getApiErrorMessage(err, 'Failed to save tour.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="card flex flex-col gap-3 mb-4">
      <h3 className="section-title" style={{ fontSize: 16 }}>
        {initial ? 'Edit Tour' : 'Create New Tour'}
      </h3>
      <div>
        <label className="label">Name *</label>
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Tour name" />
      </div>
      <div>
        <label className="label">Description</label>
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Describe the tour..." rows={3} style={{ resize: 'vertical' }} />
      </div>
      <div className="grid-2col">
        <div>
          <label className="label">From *</label>
          <input value={from} onChange={(e) => setFrom(e.target.value)} placeholder="e.g. Vienna" />
        </div>
        <div>
          <label className="label">To *</label>
          <input value={to} onChange={(e) => setTo(e.target.value)} placeholder="e.g. Salzburg" />
        </div>
      </div>
      <div>
        <label className="label">Transport Type</label>
        <select value={transportType} onChange={(e) => setTransportType(e.target.value as TransportType)}>
          {TRANSPORT_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>
      <div>
        <label className="label">Tour Image</label>
        <input
          type="file"
          accept="image/jpeg,image/png,image/webp"
          onChange={(e) => setImageFile(e.target.files?.[0] ?? null)}
        />
      </div>
      {error && <p className="error">{error}</p>}
      <div className="flex-row gap-2">
        <button type="submit" className="btn-primary" disabled={loading}>
          {loading ? 'Saving...' : (initial ? 'Update Tour' : 'Create Tour')}
        </button>
        <button type="button" className="btn-secondary" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  );
}
