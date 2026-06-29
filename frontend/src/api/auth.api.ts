import axiosClient from './axiosClient';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth.types';

export const registerUser = (data: RegisterRequest) =>
  axiosClient.post<AuthResponse>('/api/auth/register', data).then((r) => r.data);

export const loginUser = (data: LoginRequest) =>
  axiosClient.post<AuthResponse>('/api/auth/login', data).then((r) => r.data);
