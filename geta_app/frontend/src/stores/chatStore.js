import { defineStore } from 'pinia';
import socketService from '../services/socketService';

export const useChatStore = defineStore('chat', {
  state: () => ({
    conversations: [],
    activeConversation: null,
    messages: {},
    loading: {
      conversations: false,
      messages: false
    },
    error: null
  }),
  
  getters: {
    // Retorna as conversas ordenadas por data da última mensagem
    sortedConversations: (state) => {
      return [...state.conversations].sort((a, b) => {
        const dateA = a.last_message_time ? new Date(a.last_message_time) : new Date(a.created_at);
        const dateB = b.last_message_time ? new Date(b.last_message_time) : new Date(b.created_at);
        return dateB - dateA;
      });
    },
    
    // Retorna as mensagens da conversa ativa ordenadas por data
    activeMessages: (state) => {
      if (!state.activeConversation) return [];
      
      const conversationMessages = state.messages[state.activeConversation] || [];
      return [...conversationMessages].sort((a, b) => {
        return new Date(a.created_at) - new Date(b.created_at);
      });
    },
    
    // Verifica se há uma conversa ativa
    hasActiveConversation: (state) => !!state.activeConversation
  },
  
  actions: {
    // Inicializa o serviço de socket e registra os handlers
    initSocketConnection() {
      socketService.init();
      
      // Registrar handlers para eventos do socket
      socketService.on('conversations_list', this.handleConversationsList);
      socketService.on('messages_list', this.handleMessagesList);
      socketService.on('new_message', this.handleNewMessage);
      socketService.on('conversation_created', this.handleConversationCreated);
      socketService.on('new_conversation', this.handleNewConversation);
      socketService.on('error', this.handleError);
    },
    
    // Limpa a conexão e os handlers
    clearSocketConnection() {
      socketService.off('conversations_list', this.handleConversationsList);
      socketService.off('messages_list', this.handleMessagesList);
      socketService.off('new_message', this.handleNewMessage);
      socketService.off('conversation_created', this.handleConversationCreated);
      socketService.off('new_conversation', this.handleNewConversation);
      socketService.off('error', this.handleError);
      
      socketService.disconnect();
    },
    
    // Busca a lista de conversas do usuário
    fetchConversations() {
      this.loading.conversations = true;
      socketService.emit('get_conversations');
    },
    
    // Busca as mensagens de uma conversa específica
    fetchMessages(conversationId, limit = 50, offset = 0) {
      this.loading.messages = true;
      socketService.emit('get_messages', { conversationId, limit, offset });
      
      // Entrar na sala da conversa
      this.setActiveConversation(conversationId);
    },
    
    // Define a conversa ativa
    setActiveConversation(conversationId) {
      // Se já existe uma conversa ativa, sair dela primeiro
      if (this.activeConversation && this.activeConversation !== conversationId) {
        socketService.emit('leave_conversation', { conversationId: this.activeConversation });
      }
      
      this.activeConversation = conversationId;
    },
    
    // Envia uma nova mensagem
    sendMessage(content) {
      if (!this.activeConversation || !content.trim()) {
        return;
      }
      
      socketService.emit('send_message', {
        conversationId: this.activeConversation,
        content: content.trim()
      });
    },
    
    // Cria uma nova conversa
    createConversation(participantIds) {
      socketService.emit('create_conversation', { participantIds });
    },
    
    // Handler para receber lista de conversas
    handleConversationsList(conversations) {
      this.conversations = conversations;
      this.loading.conversations = false;
    },
    
    // Handler para receber lista de mensagens
    handleMessagesList({ conversationId, messages }) {
      this.messages[conversationId] = messages;
      this.loading.messages = false;
    },
    
    // Handler para receber nova mensagem
    handleNewMessage(message) {
      const conversationId = message.conversation_id;
      
      // Inicializar array de mensagens se não existir
      if (!this.messages[conversationId]) {
        this.messages[conversationId] = [];
      }
      
      // Adicionar mensagem ao array
      this.messages[conversationId].push(message);
      
      // Atualizar última mensagem na lista de conversas
      const conversationIndex = this.conversations.findIndex(c => c.id === conversationId);
      if (conversationIndex !== -1) {
        this.conversations[conversationIndex].last_message = message.content;
        this.conversations[conversationIndex].last_message_time = message.created_at;
      }
    },
    
    // Handler para conversa criada
    handleConversationCreated(conversation) {
      // Adicionar à lista de conversas se não existir
      const exists = this.conversations.some(c => c.id === conversation.id);
      if (!exists) {
        this.conversations.push(conversation);
      }
      
      // Definir como conversa ativa
      this.setActiveConversation(conversation.id);
      
      // Buscar mensagens (que provavelmente estarão vazias para uma nova conversa)
      this.fetchMessages(conversation.id);
    },
    
    // Handler para nova conversa (quando outro usuário inicia)
    handleNewConversation(conversation) {
      // Adicionar à lista de conversas se não existir
      const exists = this.conversations.some(c => c.id === conversation.id);
      if (!exists) {
        this.conversations.push(conversation);
      }
    },
    
    // Handler para erros
    handleError(error) {
      this.error = error.message;
      this.loading.conversations = false;
      this.loading.messages = false;
    }
  }
});
