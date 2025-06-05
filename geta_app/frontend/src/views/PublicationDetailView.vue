<template>
  <div class="container mx-auto p-6">
    <div v-if="loading" class="text-center py-10">
      <p>Carregando publicação...</p>
    </div>
    
    <div v-else-if="error" class="text-center py-10 text-red-600">
      <p>Erro ao carregar publicação: {{ error }}</p>
    </div>
    
    <div v-else-if="publication" class="bg-white rounded-lg shadow-md p-8">
      <!-- Cabeçalho com Título e Status -->
      <div class="flex justify-between items-start mb-4 border-b pb-4">
        <h2 class="text-3xl font-bold">{{ publication.titulo }}</h2>
        <span 
          class="text-sm font-medium px-3 py-1 rounded-full ml-4 flex-shrink-0"
          :class="getStatusClass(publication.status)"
        >
          {{ publication.status }}
        </span>
      </div>
      
      <!-- Informações Gerais -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4 mb-6">
        <div>
          <span class="font-semibold text-gray-600">Publicado por:</span> 
          <router-link 
            :to="{ name: 'ProfileView', params: { userId: publication.usuario_id } }"
            class="text-blue-600 hover:underline"
          >
            {{ publication.usuario_nome }}
          </router-link>
        </div>
        <div>
          <span class="font-semibold text-gray-600">Tipo:</span> 
          <span class="text-gray-800">{{ publication.tipo }}</span>
        </div>
        <div>
          <span class="font-semibold text-gray-600">Data de Publicação:</span> 
          <span class="text-gray-800">{{ formatDate(publication.data_criacao) }}</span>
        </div>
        <div>
          <span class="font-semibold text-gray-600">Última Atualização:</span> 
          <span class="text-gray-800">{{ formatDate(publication.data_atualizacao) }}</span>
        </div>
        <div v-if="publication.orcamento">
          <span class="font-semibold text-gray-600">Orçamento Estimado:</span> 
          <span class="text-gray-800">R$ {{ formatCurrency(publication.orcamento) }}</span>
        </div>
        <div v-if="publication.prazo">
          <span class="font-semibold text-gray-600">Prazo Desejado:</span> 
          <span class="text-gray-800">{{ formatDate(publication.prazo) }}</span>
        </div>
      </div>
      
      <!-- Descrição -->
      <div class="mb-6">
        <h3 class="text-xl font-semibold mb-2">Descrição</h3>
        <p class="text-gray-700 whitespace-pre-wrap">{{ publication.descricao }}</p>
      </div>
      
      <!-- Tags -->
      <div v-if="publication.tags && publication.tags.length > 0" class="mb-6">
        <h3 class="text-xl font-semibold mb-2">Tags</h3>
        <div class="flex flex-wrap gap-2">
          <span 
            v-for="tag in publication.tags" 
            :key="tag"
            class="bg-gray-200 text-gray-700 px-3 py-1 rounded-full text-sm"
          >
            {{ tag }}
          </span>
        </div>
      </div>
      
      <!-- Ações (Editar/Deletar - se for o proprietário) -->
      <div v-if="isOwner" class="mt-8 pt-6 border-t flex justify-end space-x-4">
        <router-link 
          :to="{ name: 'EditPublication', params: { publicationId: publication.id } }" 
          class="px-4 py-2 border border-blue-600 text-blue-600 rounded-md hover:bg-blue-50 transition"
        >
          Editar
        </router-link>
        <button 
          @click="confirmDelete" 
          class="px-4 py-2 border border-red-600 text-red-600 rounded-md hover:bg-red-50 transition"
          :disabled="isDeleting"
        >
          {{ isDeleting ? 'Deletando...' : 'Deletar' }}
        </button>
      </div>
      
      <!-- Botão de Interesse/Candidatura (para outros usuários) -->
      <div v-else class="mt-8 pt-6 border-t text-right">
         <button 
            class="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition"
            @click="showInterest"
          >
            Demonstrar Interesse
          </button>
      </div>
      
      <div v-if="deleteError" class="mt-4 text-red-600 text-center">
        Erro ao deletar: {{ deleteError }}
      </div>

    </div>
    
    <div v-else class="text-center py-10 text-gray-500">
      <p>Publicação não encontrada.</p>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import publicationService from '../services/publicationService';
import { useAuthStore } from '../stores/authStore';

export default {
  name: 'PublicationDetailView',
  props: {
    publicationId: {
      type: [String, Number],
      required: true
    }
  },
  
  setup(props) {
    const route = useRoute();
    const router = useRouter();
    const authStore = useAuthStore();
    
    const publication = ref(null);
    const loading = ref(true);
    const error = ref(null);
    const isDeleting = ref(false);
    const deleteError = ref(null);

    const fetchPublicationDetails = async () => {
      loading.value = true;
      error.value = null;
      try {
        publication.value = await publicationService.getPublicationDetails(props.publicationId);
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro desconhecido';
      } finally {
        loading.value = false;
      }
    };

    const isOwner = computed(() => {
      return publication.value && authStore.user && publication.value.usuario_id === authStore.user.id;
    });

    const formatDate = (timestamp) => {
      if (!timestamp) return '';
      return new Date(timestamp).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };
    
    const formatCurrency = (value) => {
        if (value === null || value === undefined) return '';
        return parseFloat(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
    };

    const getStatusClass = (status) => {
      // Reutilizar a mesma lógica da lista
      switch (status) {
        case 'Aberto': return 'bg-green-100 text-green-700';
        case 'Em Andamento': return 'bg-yellow-100 text-yellow-700';
        case 'Concluído': return 'bg-blue-100 text-blue-700';
        case 'Cancelado': return 'bg-red-100 text-red-700';
        default: return 'bg-gray-100 text-gray-700';
      }
    };

    const confirmDelete = () => {
      if (window.confirm('Tem certeza que deseja deletar esta publicação? Esta ação não pode ser desfeita.')) {
        deletePublication();
      }
    };

    const deletePublication = async () => {
      isDeleting.value = true;
      deleteError.value = null;
      try {
        await publicationService.deletePublication(props.publicationId);
        router.push({ name: 'PublicationList' }); // Redirecionar para a lista após deletar
      } catch (err) {
        deleteError.value = err.response?.data?.message || err.message || 'Erro ao deletar';
      } finally {
        isDeleting.value = false;
      }
    };
    
    const showInterest = () => {
        // Lógica para demonstrar interesse (pode ser um chat, um registro no banco, etc.)
        // Por enquanto, apenas um alerta
        alert(`Interesse demonstrado na publicação '${publication.value.titulo}'! Em uma versão futura, isso iniciaria um chat ou registraria seu interesse.`);
        // Exemplo: Iniciar chat com o autor
        // const chatStore = useChatStore();
        // chatStore.createConversation([authStore.user.id, publication.value.usuario_id]);
        // router.push('/chat');
    };

    onMounted(fetchPublicationDetails);

    return {
      publication,
      loading,
      error,
      isOwner,
      isDeleting,
      deleteError,
      formatDate,
      formatCurrency,
      getStatusClass,
      confirmDelete,
      showInterest
    };
  }
};
</script>

