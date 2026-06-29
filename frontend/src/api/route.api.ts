import axiosClient from './axiosClient';
import type { RouteData, TransportType } from '../types/tour.types';

export const getRoute = (from: string, to: string, type: TransportType) =>
  axiosClient
    .get<RouteData>('/api/route', { params: { from, to, type } })
    .then((r) => r.data);
