import { useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { TourDetailViewModel } from '../viewmodels/TourDetailViewModel';
import { useViewModel } from '../viewmodels/ViewModel';
import TourForm from '../components/tours/TourForm';
import WeatherWidget from '../components/tours/WeatherWidget';
import TourLogList from '../components/tourLogs/TourLogList';
import TourLogForm from '../components/tourLogs/TourLogForm';
import RouteMap from '../components/map/RouteMap';
import LoadingSpinner from '../components/common/LoadingSpinner';
import ConfirmDialog from '../components/common/ConfirmDialog';

export default function TourDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const vm = useViewModel(() => new TourDetailViewModel(id!));
  const imageInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => { void vm.load(); }, [vm]);

  if (vm.loading) return <LoadingSpinner />;
  if (vm.error || !vm.tour) return (
    <div className="page">
      <p className="error">{vm.error || 'Tour not found.'}</p>
      <button className="btn-secondary mt-4" onClick={() => navigate('/')}>Back</button>
    </div>
  );

  const tour = vm.tour;

  return (
    <div className="page-wide">
      <button className="btn-secondary btn-sm mb-4" onClick={() => navigate('/')}>&larr; Back</button>

      {vm.editingTour ? (
        <TourForm initial={tour} onSubmit={(data, image) => vm.updateTour(data, image)} onCancel={() => vm.cancelEditingTour()} />
      ) : (
        <div className="card mb-4">
          <div className="flex-row justify-between items-start">
            <div>
              <div className="flex-row items-center gap-2 mb-2">
                <h1 className="detail-title">{tour.name}</h1>
                <span className="badge">{tour.transportType}</span>
              </div>
              <p className="text-muted mb-1">{tour.from} &rarr; {tour.to}</p>
              {tour.description && <p className="text-sm text-body mb-2">{tour.description}</p>}
              {tour.routeImagePath && (
                <img src={tour.routeImagePath} alt={tour.name} style={{ maxWidth: 320, borderRadius: 8, marginBottom: 8 }} />
              )}
              <div className="flex-row gap-4 text-sm text-muted">
                <span>{tour.distance > 0 ? `${tour.distance} km` : 'Distance: N/A'}</span>
                {tour.estimatedTime > 0 && <span>~{tour.estimatedTime} min</span>}
                <span>Popularity: {tour.popularity}</span>
                <span>{tour.childFriendliness}</span>
              </div>
            </div>
            <div className="flex-row gap-2">
              <button className="btn-secondary btn-sm" onClick={() => imageInputRef.current?.click()}>Upload Image</button>
              <input
                ref={imageInputRef}
                type="file"
                accept="image/jpeg,image/png,image/webp"
                hidden
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) void vm.uploadImage(file);
                  e.target.value = '';
                }}
              />
              <button className="btn-secondary btn-sm" onClick={() => vm.startEditingTour()}>Edit</button>
              <button className="btn-danger btn-sm" onClick={() => vm.requestDeleteTour()}>Delete</button>
            </div>
          </div>
        </div>
      )}

      <div className="mb-4">
        <WeatherWidget weather={vm.weather} />
      </div>

      <div className="mb-4">
        <RouteMap
          coordinates={vm.routeData?.coordinates as [number, number][] | null}
          fromName={tour.from}
          toName={tour.to}
        />
      </div>

      <div className="flex-row justify-between items-center mb-4">
        <h2 className="section-title">Tour Logs ({vm.logs.length})</h2>
        {!vm.showLogForm && !vm.editingLog && (
          <button className="btn-primary btn-sm" onClick={() => vm.openCreateLogForm()}>+ Add Log</button>
        )}
      </div>

      {vm.showLogForm && (
        <TourLogForm onSubmit={(data) => vm.createLog(data)} onCancel={() => vm.closeLogForm()} />
      )}
      {vm.editingLog && (
        <TourLogForm initial={vm.editingLog} onSubmit={(data) => vm.updateLog(data)} onCancel={() => vm.closeLogForm()} />
      )}

      <TourLogList
        logs={vm.logs}
        onEdit={(log) => vm.startEditingLog(log)}
        onDelete={(logId) => vm.requestDeleteLog(logId)}
      />

      {vm.pendingDeleteLogId && (
        <ConfirmDialog
          message="Delete this log?"
          onConfirm={() => void vm.confirmDeleteLog()}
          onCancel={() => vm.cancelDeleteLog()}
        />
      )}

      {vm.confirmingTourDelete && (
        <ConfirmDialog
          message="Delete this tour and all its logs?"
          onConfirm={() => void vm.confirmDeleteTour().then(() => navigate('/'))}
          onCancel={() => vm.cancelDeleteTour()}
        />
      )}
    </div>
  );
}
