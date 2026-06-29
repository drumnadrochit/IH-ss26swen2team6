import { createTour, deleteTour, exportTours, getTours, importTours, searchTours, uploadTourImage } from '../api/tours.api';
import type { CreateTourRequest, Tour } from '../types/tour.types';
import { getApiErrorMessage } from '../utils/apiError';
import { ViewModel } from './ViewModel';

// Backs the Tours list view: loading/searching/creating/deleting tours, plus
// import/export of the user's tour data.
export class TourListViewModel extends ViewModel {
  tours: Tour[] = [];
  loading = true;
  error = '';
  searchQuery = '';
  showCreateForm = false;
  pendingDeleteId: string | null = null;
  importMessage = '';

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    this.notify();
    try {
      this.tours = this.searchQuery ? await searchTours(this.searchQuery) : await getTours();
    } catch {
      this.error = 'Failed to load tours.';
    } finally {
      this.loading = false;
      this.notify();
    }
  }

  async search(query: string): Promise<void> {
    this.searchQuery = query;
    await this.load();
  }

  async clearSearch(): Promise<void> {
    this.searchQuery = '';
    await this.load();
  }

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
    this.notify();
  }

  async createTour(data: CreateTourRequest, image?: File): Promise<void> {
    const tour = await createTour(data);
    if (image) {
      await uploadTourImage(tour.id, image);
    }
    this.showCreateForm = false;
    await this.load();
  }

  requestDelete(id: string): void {
    this.pendingDeleteId = id;
    this.notify();
  }

  cancelDelete(): void {
    this.pendingDeleteId = null;
    this.notify();
  }

  async confirmDelete(): Promise<void> {
    if (!this.pendingDeleteId) return;
    const id = this.pendingDeleteId;
    this.pendingDeleteId = null;
    this.tours = this.tours.filter((t) => t.id !== id);
    this.notify();
    await deleteTour(id);
  }

  async export(): Promise<void> {
    const blob = await exportTours();
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `tours-export-${new Date().toISOString().slice(0, 10)}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }

  async import(file: File): Promise<void> {
    this.importMessage = '';
    this.notify();
    try {
      const result = await importTours(file);
      this.importMessage = `Imported ${result.imported} tour(s).`;
      await this.load();
    } catch (err) {
      this.importMessage = getApiErrorMessage(err, 'Import failed.');
      this.notify();
    }
  }
}
