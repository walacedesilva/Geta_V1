<template>
  <div class="flex flex-col h-full">
    <!-- Cabeçalho da lista de conversas -->
    <div class="bg-blue-600 text-white p-4 shadow-md">
      <h2 class="text-xl font-semibold">Conversas</h2>
    </div>
    
    <!-- Lista de conversas -->
    <div class="flex-grow overflow-y-auto bg-white">
      <div v-if="loading" class="p-4 text-center text-gray-500">
        <p>Carregando conversas...</p>
      </div>
      
      <div v-else-if="conversations.length === 0" class="p-4 text-center text-gray-500">
        <p>Nenhuma conversa encontrada</p>
        <button 
          @click="showNewConversationModal = true" 
          class="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
        >
          Iniciar nova conversa
        </button>
      </div>
      
      <div v-else>
        <div 
          v-for="conversation in conversations" 
          :key="conversation.id"
          @click="selectConversation(conversation.id)"
          class="p-4 border-b hover:bg-gray-100 cursor-pointer transition"
          :class="{'bg-blue-50': activeConversationId === conversation.id}"
        >
          <!-- Participantes (excluindo o usuário atual) -->
          <div class="flex items-center">
            <div class="w-10 h-10 rounded-full bg-gray-300 flex items-center justify-center text-gray-600 font-bold">
              {{ getInitials(conversation.participants[0]?.nome) }}
            </div>
            <div class="ml-3 flex-grow">
              <h3 class="font-medium">
                {{ getParticipantNames(conversation.participants) }}
              </h3>
              <p class="text-sm text-gray-500 truncate">
                {{ conversation.last_message || 'Nenhuma mensagem' }}
              </p>
            </div>
            <div v-if="conversation.last_message_time" class="text-xs text-gray-500">
              {{ formatTime(conversation.last_message_time) }}
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Botão para nova conversa -->
    <div class="p-4 border-t bg-white">
      <button 
        @click="showNewConversationModal = true" 
        class="w-full py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
      >
        Nova conversa
      </button>
    </div>
    
    <!-- Modal para nova conversa -->
    <div v-if="showNewConversationModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 class="text-xl font-semibold mb-4">Nova conversa</h3>
        
        <!-- Implementação simplificada - em uma versão completa, teria busca de usuários -->
        <div class="mb-4">
          <label class="block text-gray-700 mb-2">ID do usuário para conversar:</label>
          <input 
            v-model="newConversationUserId" 
            type="number" 
            class="w-full p-2 border rounded-md"
            placeholder="Digite o ID do usuário"
          />
          <p class="text-xs text-gray-500 mt-1">
            Em uma versão completa, aqui teria uma busca de usuários por nome/email.
          </p>
        </div>
        
        <div class="flex justify-end space-x-3">
          <button 
            @click="showNewConversationModal = false" 
            class="px-4 py-2 border rounded-md hover:bg-gray-100 transition"
          >
            Cancelar
          </button>
          <button 
            @click="createNewConversation" 
            class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            :disabled="!newConversationUserId"
          >
            Iniciar conversa
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useChatStore } from '../stores/chatStore';
import { useAuthStore } from '../stores/authStore';
import { storeToRefs } from 'pinia';

export default {
  name: 'ConversationListView',
  
  setup() {
    const chatStore = useChatStore();
    const authStore = useAuthStore();
    const { conversations, loading } = storeToRefs(chatStore);
    const { sortedConversations, activeConversation } = storeToRefs(chatStore);
    
    const showNewConversationModal = ref(false);
    const newConversationUserId = ref('');
    
    // Inicializar e buscar conversas ao montar o componente
    onMounted(() => {
      chatStore.fetchConversations();
    });
    
    // Selecionar uma conversa
    const selectConversation = (conversationId) => {
      chatStore.setActiveConversation(conversationId);
      chatStore.fetchMessages(conversationId);
    };
    
    // Criar nova conversa
    const createNewConversation = () => {
      if (!newConversationUserId.value) return;
      
      // Incluir o usuário atual e o destinatário
      const participantIds = [
        authStore.user.id,
        parseInt(newConversationUserId.value)
      ];
      
      chatStore.createConversation(participantIds);
      showNewConversationModal.value = false;
      newConversationUserId.value = '';
    };
    
    // Obter iniciais do nome para avatar
    const getInitials = (name) => {
      if (!name) return '?';
      return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
    };
    
    // Formatar nomes dos participantes (excluindo o usuário atual)
    const getParticipantNames = (participants) => {
      if (!participants || participants.length === 0) return 'Sem participantes';
      
      const filteredParticipants = participants.filter(p => p.id !== authStore.user.id);
      
      if (filteredParticipants.length === 0) return 'Você';
      
      return filteredParticipants.map(p => p.nome).join(', ');
    };
    
    // Formatar hora da última mensagem
    const formatTime = (timestamp) => {
      const date = new Date(timestamp);
      const now = new Date();
      
      // Se for hoje, mostrar apenas a hora
      if (date.toDateString() === now.toDateString()) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      }
      
      // Se for esta semana, mostrar o dia da semana
      const diffDays = Math.floor((now - date) / (1000 * 60 * 60 * 24));
      if (diffDays < 7) {
        return date.toLocaleDateString([], { weekday: 'short' });
      }
      
      // Caso contrário, mostrar a data
      return date.toLocaleDateString();
    };
    
    return {
      conversations: sortedConversations,
      loading,
      activeConversationId: activeConversation,
      showNewConversationModal,
      newConversationUserId,
      selectConversation,
      createNewConversation,
      getInitials,
      getParticipantNames,
      formatTime
    };
  }
};
</script>
