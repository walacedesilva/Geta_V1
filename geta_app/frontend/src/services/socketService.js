import { io } from 'socket.io-client';
import { useAuthStore } from '../stores/authStore';

// Classe para gerenciar a conexão Socket.IO
class SocketService {
  constructor() {
    this.socket = null;
    this.connected = false;
  }

  // Inicializar conexão com o servidor Socket.IO
  init() {
    if (this.socket) {
      return;
    }

    const authStore = useAuthStore();
    const token = authStore.token;

    if (!token) {
      console.error('Tentativa de conexão socket sem token de autenticação');
      return;
    }

    // Conectar ao servidor com o token JWT para autenticação
    this.socket = io(import.meta.env.VITE_API_URL || 'http://localhost:3000', {
      auth: {
        token
      }
    });

    // Configurar handlers de eventos básicos
    this.socket.on('connect', () => {
      console.log('Conectado ao servidor Socket.IO');
      this.connected = true;
    });

    this.socket.on('disconnect', () => {
      console.log('Desconectado do servidor Socket.IO');
      this.connected = false;
    });

    this.socket.on('error', (error) => {
      console.error('Erro no Socket.IO:', error);
    });

    return this.socket;
  }

  // Desconectar do servidor
  disconnect() {
    if (this.socket) {
      this.socket.disconnect();
      this.socket = null;
      this.connected = false;
    }
  }

  // Verificar se está conectado
  isConnected() {
    return this.connected && this.socket && this.socket.connected;
  }

  // Emitir evento para o servidor
  emit(event, data) {
    if (!this.isConnected()) {
      console.error('Tentativa de emitir evento sem conexão ativa');
      return;
    }
    this.socket.emit(event, data);
  }

  // Registrar handler para evento
  on(event, callback) {
    if (!this.socket) {
      console.error('Tentativa de registrar evento sem socket inicializado');
      return;
    }
    this.socket.on(event, callback);
  }

  // Remover handler de evento
  off(event, callback) {
    if (!this.socket) {
      return;
    }
    this.socket.off(event, callback);
  }
}

// Exportar instância única do serviço
export default new SocketService();
