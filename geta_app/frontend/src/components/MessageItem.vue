<template>
  <div 
    class="max-w-3/4 rounded-lg p-3 mb-2"
    :class="[
      isSender 
        ? 'bg-blue-600 text-white ml-auto' 
        : 'bg-gray-200 text-gray-800 mr-auto'
    ]"
  >
    <!-- Informações do remetente (apenas para mensagens recebidas) -->
    <div v-if="!isSender" class="text-xs text-gray-600 mb-1">
      {{ message.sender.nome }}
    </div>
    
    <!-- Conteúdo da mensagem -->
    <div>
      {{ message.content }}
    </div>
    
    <!-- Horário da mensagem -->
    <div 
      class="text-xs mt-1 text-right"
      :class="[isSender ? 'text-blue-200' : 'text-gray-500']"
    >
      {{ formatTime(message.created_at) }}
    </div>
  </div>
</template>

<script>
export default {
  name: 'MessageItem',
  props: {
    message: {
      type: Object,
      required: true
    },
    isSender: {
      type: Boolean,
      default: false
    }
  },
  
  setup(props) {
    // Formatar horário da mensagem
    const formatTime = (timestamp) => {
      const date = new Date(timestamp);
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };
    
    return {
      formatTime
    };
  }
};
</script>
