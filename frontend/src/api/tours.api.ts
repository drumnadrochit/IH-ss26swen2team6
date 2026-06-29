import axiosClient from './axiosClient';
import type { Tour, CreateTourRequest, UpdateTourRequest, WeatherData } from '../types/tour.types';

export const getTours = () =>
  axiosClient.get<Tour[]>('/api/tours').then((r) => r.data);

export const searchTours = (query: string) =>
  axiosClient.get<Tour[]>('/api/tours/search', { params: { q: query } }).then((r) => r.data);

export const getTourById = (id: string) =>
  axiosClient.get<Tour>(`/api/tours/${id}`).then((r) => r.data);

export const createTour = (data: CreateTourRequest) =>
  axiosClient.post<Tour>('/api/tours', data).then((r) => r.data);

export const updateTour = (id: string, data: UpdateTourRequest) =>
  axiosClient.put<Tour>(`/api/tours/${id}`, data).then((r) => r.data);

export const deleteTour = (id: string) =>
  axiosClient.delete(`/api/tours/${id}`);

export const uploadTourImage = (id: string, file: File) => {
  const formData = new FormData();
  formData.append('file', file);
  return axiosClient
    .post<Tour>(`/api/tours/${id}/image`, formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    .then((r) => r.data);
};

export const getTourWeather = (id: string) =>
  axiosClient.get<WeatherData>(`/api/tours/${id}/weather`).then((r) => r.data);

export const exportTours = () =>
  axiosClient.get('/api/tours/export', { responseType: 'blob' }).then((r) => r.data as Blob);

export const importTours = (file: File) => {
  const formData = new FormData();
  formData.append('file', file);
  return axiosClient
    .post<{ imported: number }>('/api/tours/import', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    .then((r) => r.data);
};
