import { useEffect, useRef } from 'react';
import { TourListViewModel } from '../viewmodels/TourListViewModel';
import { useViewModel } from '../viewmodels/ViewModel';
import TourList from '../components/tours/TourList';
import TourForm from '../components/tours/TourForm';
import TourSearch from '../components/tours/TourSearch';
import LoadingSpinner from '../components/common/LoadingSpinner';
import ConfirmDialog from '../components/common/ConfirmDialog';

export default function ToursPage() {
  const vm = useViewModel(() => new TourListViewModel());
  const importInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => { void vm.load(); }, [vm]);

  return (
    <div className="page">
      <div className="flex-row justify-between items-center mb-4">
        <h1 className="page-title">My Tours</h1>
        <div className="flex-row gap-2">
          <button className="btn-secondary" onClick={() => void vm.export()}>Export</button>
          <button className="btn-secondary" onClick={() => importInputRef.current?.click()}>Import</button>
          <input
            ref={importInputRef}
            type="file"
            accept="application/json"
            hidden
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) void vm.import(file);
              e.target.value = '';
            }}
          />
          <button className="btn-primary" onClick={() => vm.toggleCreateForm()}>
            {vm.showCreateForm ? 'Cancel' : '+ New Tour'}
          </button>
        </div>
      </div>

      <TourSearch onSearch={(q) => void vm.search(q)} onClear={() => void vm.clearSearch()} />

      {vm.importMessage && <p className="text-sm text-muted mb-4">{vm.importMessage}</p>}

      {vm.showCreateForm && (
        <TourForm onSubmit={(data, image) => vm.createTour(data, image)} onCancel={() => vm.toggleCreateForm()} />
      )}

      {vm.error && <p className="error mb-4">{vm.error}</p>}
      {vm.loading ? (
        <LoadingSpinner />
      ) : (
        <TourList tours={vm.tours} onDelete={(id) => vm.requestDelete(id)} />
      )}

      {vm.pendingDeleteId && (
        <ConfirmDialog
          message="Delete this tour and all its logs?"
          onConfirm={() => void vm.confirmDelete()}
          onCancel={() => vm.cancelDelete()}
        />
      )}
    </div>
  );
}
