import { useState } from 'react';

interface Props {
  onSearch: (q: string) => void;
  onClear: () => void;
}

export default function TourSearch({ onSearch, onClear }: Props) {
  const [query, setQuery] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) onSearch(query.trim());
  };

  const handleClear = () => {
    setQuery('');
    onClear();
  };

  return (
    <form onSubmit={handleSubmit} className="flex-row gap-2 mb-4">
      <input
        className="flex-1"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search tours, logs, popularity, child-friendliness..."
      />
      <button type="submit" className="btn-primary btn-sm">Search</button>
      {query && <button type="button" className="btn-secondary btn-sm" onClick={handleClear}>Clear</button>}
    </form>
  );
}
