<template>
  <div class="flex flex-col h-full">
    <!-- Cabeçalho da conversa -->
    <div v-if="conversation" class="bg-blue-600 text-white p-4 shadow-md flex items-center">
      <div class="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-600 font-bold">
        {{ getInitials(participantName) }}
      </div>
      <h2 class="text-xl font-semibold ml-3">{{ participantName }}</h2>
    </div>
    <div v-else class="bg-blue-600 text-white p-4 shadow-md">
      <h2 class="text-xl font-semibold">Selecione uma conversa</h2>
    </div>
    
    <!-- Área de mensagens -->
    <div class="flex-grow overflow-y-auto p-4 bg-gray-50" ref="messagesContainer">
      <div v-if="!conversation" class="h-full flex items-center justify-center text-gray-500">
        <p>Selecione uma conversa para ver as mensagens</p>
      </div>
      
      <div v-else-if="loading" class="h-full flex items-center justify-center text-gray-500">
        <p>Carregando mensagens...</p>
      </div>
      
      <div v-else-if="messages.length === 0" class="h-full flex items-center justify-center text-gray-500">
        <p>Nenhuma mensagem. Comece a conversar!</p>
      </div>
      
      <div v-else class="space-y-3">
        <message-item 
          v-for="message in messages" 
          :key="message.id"
          :message="message"
          :is-sender="message.sender_id === currentUserId"
        />
      </div>
    </div>
    
    <!-- Área de input -->
    <div v-if="conversation" class="p-4 border-t bg-white">
      <div class="flex">
        <input 
          v-model="newMessage" 
          @keyup.enter="sendMessage"
          type="text" 
          class="flex-grow p-2 border rounded-l-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Digite sua mensagem..."
        />
        <button 
          @click="sendMessage" 
          class="px-4 py-2 bg-blue-600 text-white rounded-r-md hover:bg-blue-700 transition"
          :disabled="!newMessage.trim()"
        >
          Enviar
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { useChatStore } from '../stores/chatStore';
import { useAuthStore } from '../stores/authStore';
import { storeToRefs } from 'pinia';
import MessageItem from '../components/MessageItem.vue';

export default {
  name: 'ChatWindowView',
  components: {
    MessageItem
  },
  
  setup() {
    const chatStore = useChatStore();
    const authStore = useAuthStore();
    const { activeConversation, activeMessages, loading } = storeToRefs(chatStore);
    
    const newMessage = ref('');
    const messagesContainer = ref(null);
    
    // Buscar conversa ativa
    const conversation = computed(() => {
      if (!activeConversation.value) return null;
      return chatStore.conversations.find(c => c.id === activeConversation.value);
    });
    
    // Nome do participante (excluindo o usuário atual)
    const participantName = computed(() => {
      if (!conversation.value || !conversation.value.participants) return '';
      
      const filteredParticipants = conversation.value.participants.filter(
        p => p.id !== authStore.user.id
      );
      
      if (filteredParticipants.length === 0) return 'Você';
      
      return filteredParticipants.map(p => p.nome).join(', ');
    });
    
    // Obter iniciais do nome para avatar
    const getInitials = (name) => {
      if (!name) return '?';
      return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
    };
    
    // Enviar mensagem
    const sendMessage = () => {
      if (!newMessage.value.trim() || !activeConversation.value) return;
      
      chatStore.sendMessage(newMessage.value);
      newMessage.value = '';
    };
    
    // Rolar para o final quando novas mensagens chegarem
    watch(activeMessages, async () => {
      await nextTick();
      if (messagesContainer.value) {
        messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
      }
    }, { deep: true });
    
    return {
      conversation,
      messages: activeMessages,
      loading,
      newMessage,
      messagesContainer,
      currentUserId: authStore.user.id,
      participantName,
      getInitials,
      sendMessage
    };
  }
};
</script>
