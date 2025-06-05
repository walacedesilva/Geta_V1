import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

// Configurar instância do Axios com base URL e interceptor para token
const apiClient = axios.create({
  baseURL: `${API_URL}/api/publications`,
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

// Função para criar uma nova publicação
const createPublication = async (publicationData) => {
  try {
    const response = await apiClient.post('/', publicationData);
    return response.data;
  } catch (error) {
    console.error('Erro ao criar publicação:', error.response?.data || error.message);
    throw error;
  }
};

// Função para listar todas as publicações
const listPublications = async (/* filters = {} */) => {
  try {
    // Adicionar params para filtros no futuro: { params: filters }
    const response = await apiClient.get('/');
    return response.data;
  } catch (error) {
    console.error('Erro ao listar publicações:', error.response?.data || error.message);
    throw error;
  }
};

// Função para obter detalhes de uma publicação
const getPublicationDetails = async (publicationId) => {
  try {
    const response = await apiClient.get(`/${publicationId}`);
    return response.data;
  } catch (error) {
    console.error(`Erro ao buscar detalhes da publicação ${publicationId}:`, error.response?.data || error.message);
    throw error;
  }
};

// Função para atualizar uma publicação
const updatePublication = async (publicationId, publicationData) => {
  try {
    const response = await apiClient.put(`/${publicationId}`, publicationData);
    return response.data;
  } catch (error) {
    console.error(`Erro ao atualizar publicação ${publicationId}:`, error.response?.data || error.message);
    throw error;
  }
};

// Função para deletar uma publicação
const deletePublication = async (publicationId) => {
  try {
    await apiClient.delete(`/${publicationId}`);
  } catch (error) {
    console.error(`Erro ao deletar publicação ${publicationId}:`, error.response?.data || error.message);
    throw error;
  }
};

export default {
  createPublication,
  listPublications,
  getPublicationDetails,
  updatePublication,
  deletePublication,
};
