<template>
  <div class="flex h-screen bg-gray-100">
    <!-- Sidebar com lista de conversas (1/3 da largura) -->
    <div class="w-1/3 border-r">
      <conversation-list-view />
    </div>
    
    <!-- Área principal com janela de chat (2/3 da largura) -->
    <div class="w-2/3">
      <chat-window-view />
    </div>
  </div>
</template>

<script>
import { onMounted, onUnmounted } from 'vue';
import { useChatStore } from '../stores/chatStore';
import ConversationListView from './ConversationListView.vue';
import ChatWindowView from './ChatWindowView.vue';

export default {
  name: 'ChatView',
  components: {
    ConversationListView,
    ChatWindowView
  },
  
  setup() {
    const chatStore = useChatStore();
    
    // Inicializar conexão socket e handlers ao montar o componente
    onMounted(() => {
      chatStore.initSocketConnection();
    });
    
    // Limpar conexão ao desmontar o componente
    onUnmounted(() => {
      chatStore.clearSocketConnection();
    });
    
    return {};
  }
};
</script>
