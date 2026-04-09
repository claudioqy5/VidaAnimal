import axios from 'axios';

const api = axios.create({
  baseURL: 'https://vidaanimal.helifyferdigital.cloud/api',
  timeout: 10000,
});

export default api;
