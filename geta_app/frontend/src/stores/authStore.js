import { defineStore } from "pinia";
import axios from "axios"; // Ou seu serviço de API configurado

// Idealmente, a URL base da API viria de uma variável de ambiente
const API_URL = "http://localhost:3000/api/auth";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: localStorage.getItem("token") || null,
    user: JSON.parse(localStorage.getItem("user")) || null,
    error: null,
    loading: false,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    getUser: (state) => state.user,
    getLoading: (state) => state.loading,
    getError: (state) => state.error,
  },
  actions: {
    setToken(token) {
      this.token = token;
      if (token) {
        localStorage.setItem("token", token);
        // Opcional: Decodificar token para obter dados do usuário se não vierem na resposta
        // axios.defaults.headers.common["Authorization"] = `Bearer ${token}`; // Configurar header global
      } else {
        localStorage.removeItem("token");
        // delete axios.defaults.headers.common["Authorization"];
      }
    },
    setUser(user) {
      this.user = user;
      if (user) {
        localStorage.setItem("user", JSON.stringify(user));
      } else {
        localStorage.removeItem("user");
      }
    },
    setError(error) {
      this.error = error;
    },
    setLoading(loading) {
      this.loading = loading;
    },

    async login(credentials) {
      this.setLoading(true);
      this.setError(null);
      try {
        const response = await axios.post(`${API_URL}/login`, credentials);
        const { token /*, user */ } = response.data; // Assumindo que o backend retorna o token
        this.setToken(token);
        // Se o backend retornar dados do usuário no login, use this.setUser(user)
        // Caso contrário, pode ser necessário fazer outra chamada ou decodificar o token
        // this.setUser(userDataFromTokenOrResponse);
        return true; // Indicar sucesso
      } catch (err) {
        const errorMessage = err.response?.data?.message || "Erro ao fazer login.";
        this.setError(errorMessage);
        this.setToken(null);
        this.setUser(null);
        console.error("Erro de login:", err);
        return false; // Indicar falha
      } finally {
        this.setLoading(false);
      }
    },

    async register(userData) {
      this.setLoading(true);
      this.setError(null);
      try {
        const response = await axios.post(`${API_URL}/register`, userData);
        const { token /*, user */ } = response.data;
        this.setToken(token);
        // Similar ao login, tratar dados do usuário se retornados
        // this.setUser(userDataFromTokenOrResponse);
        return true; // Indicar sucesso
      } catch (err) {
        const errorMessage = err.response?.data?.message || "Erro ao registrar.";
        this.setError(errorMessage);
        console.error("Erro de registro:", err);
        return false; // Indicar falha
      } finally {
        this.setLoading(false);
      }
    },

    logout() {
      this.setToken(null);
      this.setUser(null);
      // Opcional: Redirecionar para a página de login
      // router.push("/login");
    },
  },
});
