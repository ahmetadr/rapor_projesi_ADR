import { setAuthToken, removeAuthToken } from './auth';

export const handleApiError = (error) => {
  if (error.response) {
    if (error.response.status === 401) {
      removeAuthToken();
      window.location.href = '/login';
    }
    return {
      message: error.response.data?.message || 'Bir hata oluştu',
      status: error.response.status
    };
  }
  return { message: 'Network hatası', status: 500 };
};

export const prepareHeaders = (headers = {}) => {
  const token = getAuthToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  headers['Content-Type'] = 'application/json';
  return headers;
};