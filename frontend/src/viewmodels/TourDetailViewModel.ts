import { deleteTour, getTourById, getTourWeather, updateTour, uploadTourImage } from '../api/tours.api';
import { createTourLog, deleteTourLog, getTourLogs, updateTourLog } from '../api/tourLogs.api';
import { getRoute } from '../api/route.api';
import type { RouteData, Tour, UpdateTourRequest, WeatherData } from '../types/tour.types';
import type { CreateTourLogRequest, TourLog } from '../types/tourLog.types';
import { ViewModel } from './ViewModel';

// Backs the Tour Detail view: the tour itself, its route/map data, current weather at
// the start location, and the list of tour logs with their own create/edit/delete flow.
export class TourDetailViewModel extends ViewModel {
  tour: Tour | null = null;
  logs: TourLog[] = [];
  routeData: RouteData | null = null;
  weather: WeatherData | null = null;
  loading = true;
  error = '';
  editingTour = false;
  showLogForm = false;
  editingLog: TourLog | null = null;
  pendingDeleteLogId: string | null = null;
  confirmingTourDelete = false;

  constructor(private readonly tourId: string) {
    super();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    this.notify();
    try {
      const [tour, logs] = await Promise.all([getTourById(this.tourId), getTourLogs(this.tourId)]);
      this.tour = tour;
      this.logs = logs;

      void getRoute(tour.from, tour.to, tour.transportType).then((route) => {
        this.routeData = route;
        this.notify();
      }).catch(() => undefined);

      void getTourWeather(this.tourId).then((weather) => {
        this.weather = weather;
        this.notify();
      }).catch(() => undefined);
    } catch {
      this.error = 'Tour not found.';
    } finally {
      this.loading = false;
      this.notify();
    }
  }

  startEditingTour(): void {
    this.editingTour = true;
    this.notify();
  }

  cancelEditingTour(): void {
    this.editingTour = false;
    this.notify();
  }

  async updateTour(data: UpdateTourRequest, image?: File): Promise<void> {
    this.tour = await updateTour(this.tourId, data);
    if (image) {
      this.tour = await uploadTourImage(this.tourId, image);
    }
    this.editingTour = false;
    this.notify();
  }

  async uploadImage(file: File): Promise<void> {
    this.tour = await uploadTourImage(this.tourId, file);
    this.notify();
  }

  requestDeleteTour(): void {
    this.confirmingTourDelete = true;
    this.notify();
  }

  cancelDeleteTour(): void {
    this.confirmingTourDelete = false;
    this.notify();
  }

  async confirmDeleteTour(): Promise<void> {
    await deleteTour(this.tourId);
  }

  openCreateLogForm(): void {
    this.showLogForm = true;
    this.editingLog = null;
    this.notify();
  }

  closeLogForm(): void {
    this.showLogForm = false;
    this.editingLog = null;
    this.notify();
  }

  startEditingLog(log: TourLog): void {
    this.editingLog = log;
    this.showLogForm = false;
    this.notify();
  }

  async createLog(data: CreateTourLogRequest): Promise<void> {
    const log = await createTourLog(this.tourId, data);
    this.logs = [log, ...this.logs];
    this.showLogForm = false;
    await this.refreshTourAttributes();
  }

  async updateLog(data: CreateTourLogRequest): Promise<void> {
    if (!this.editingLog) return;
    const updated = await updateTourLog(this.tourId, this.editingLog.id, data);
    this.logs = this.logs.map((l) => (l.id === updated.id ? updated : l));
    this.editingLog = null;
    await this.refreshTourAttributes();
  }

  requestDeleteLog(logId: string): void {
    this.pendingDeleteLogId = logId;
    this.notify();
  }

  cancelDeleteLog(): void {
    this.pendingDeleteLogId = null;
    this.notify();
  }

  async confirmDeleteLog(): Promise<void> {
    if (!this.pendingDeleteLogId) return;
    const id = this.pendingDeleteLogId;
    this.pendingDeleteLogId = null;
    this.logs = this.logs.filter((l) => l.id !== id);
    this.notify();
    await deleteTourLog(this.tourId, id);
    await this.refreshTourAttributes();
  }

  // Popularity and child-friendliness are derived from the logs, so they must be
  // re-fetched whenever the log collection changes.
  private async refreshTourAttributes(): Promise<void> {
    this.tour = await getTourById(this.tourId);
    this.notify();
  }
}
