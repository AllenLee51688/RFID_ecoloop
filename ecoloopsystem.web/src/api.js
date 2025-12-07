import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7173/api', // Match the backend port
});

export default api;
