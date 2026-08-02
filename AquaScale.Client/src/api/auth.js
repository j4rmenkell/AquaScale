import { apiRequest } from './client';

// Mirrors AquaScale.Api LoginResponse
export function login(email, password) {
  return apiRequest('/auth/login', {
    method: 'POST',
    body: { email, password },
  });
}

export function logout() {
  return apiRequest('/auth/logout', { method: 'POST' });
}

export function getCurrentUser() {
  return apiRequest('/auth/me');
}