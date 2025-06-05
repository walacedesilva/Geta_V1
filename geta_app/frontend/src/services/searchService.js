import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

// Configurar instância do Axios com base URL
// A busca pode ser pública, então não adicionamos o token por padrão aqui,
// mas o interceptor pode ser adicionado se a rota for protegida no futuro.
const apiClient = axios.create({
  baseURL: `${API_URL}/api/search`,
});

// Interceptor opcional para adicionar o token JWT se a rota de busca for protegida
/*
apiClient.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore();
    const token = authStore.token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
*/

// Função para realizar a busca
const performSearch = async (query, type = null) => {
  try {
    const params = { q: query };
    if (type) {
      params.type = type; // 'users' ou 'publications'
    }
    const response = await apiClient.get('/', { params });
    return response.data; // Espera-se { users: [...], publications: [...] }
  } catch (error) {
    console.error('Erro ao realizar busca:', error.response?.data || error.message);
    throw error;
  }
};

export default {
  performSearch,
};
