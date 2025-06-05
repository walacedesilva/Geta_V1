import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

// Configurar instância do Axios com base URL e interceptor para token
const apiClient = axios.create({
  baseURL: `${API_URL}/api/profiles`,
});

// Interceptor para adicionar o token JWT a cada requisição
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

// Função para buscar o perfil do usuário logado
const getMyProfile = async () => {
  try {
    const response = await apiClient.get('/me');
    return response.data;
  } catch (error) {
    console.error('Erro ao buscar meu perfil:', error.response?.data || error.message);
    throw error;
  }
};

// Função para buscar o perfil de um usuário por ID
const getUserProfile = async (userId) => {
  try {
    const response = await apiClient.get(`/${userId}`);
    return response.data;
  } catch (error) {
    console.error(`Erro ao buscar perfil do usuário ${userId}:`, error.response?.data || error.message);
    throw error;
  }
};

// Função para atualizar o perfil do usuário logado
const updateMyProfile = async (profileData) => {
  try {
    const response = await apiClient.put('/me', profileData);
    return response.data;
  } catch (error) {
    console.error('Erro ao atualizar meu perfil:', error.response?.data || error.message);
    throw error;
  }
};

export default {
  getMyProfile,
  getUserProfile,
  updateMyProfile,
};
