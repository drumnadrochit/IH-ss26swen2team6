import axiosClient from './axiosClient';
import type { TourLog, CreateTourLogRequest, UpdateTourLogRequest } from '../types/tourLog.types';

export const getTourLogs = (tourId: string) =>
  axiosClient.get<TourLog[]>(`/api/tours/${tourId}/logs`).then((r) => r.data);

export const createTourLog = (tourId: string, data: CreateTourLogRequest) =>
  axiosClient.post<TourLog>(`/api/tours/${tourId}/logs`, data).then((r) => r.data);

export const updateTourLog = (tourId: string, logId: string, data: UpdateTourLogRequest) =>
  axiosClient.put<TourLog>(`/api/tours/${tourId}/logs/${logId}`, data).then((r) => r.data);

export const deleteTourLog = (tourId: string, logId: string) =>
  axiosClient.delete(`/api/tours/${tourId}/logs/${logId}`);
