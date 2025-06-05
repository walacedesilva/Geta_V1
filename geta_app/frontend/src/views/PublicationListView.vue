<template>
  <div class="container mx-auto p-6">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold">Publicações (Necessidades/Projetos)</h2>
      <router-link 
        :to="{ name: 'CreatePublication' }" 
        class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
      >
        Nova Publicação
      </router-link>
    </div>
    
    <div v-if="loading" class="text-center py-10">
      <p>Carregando publicações...</p>
    </div>
    
    <div v-else-if="error" class="text-center py-10 text-red-600">
      <p>Erro ao carregar publicações: {{ error }}</p>
    </div>
    
    <div v-else-if="publications.length === 0" class="text-center py-10 text-gray-500">
      <p>Nenhuma publicação encontrada.</p>
    </div>
    
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="pub in publications" 
        :key="pub.id" 
        class="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition cursor-pointer"
        @click="goToPublication(pub.id)"
      >
        <div class="p-6">
          <div class="flex justify-between items-start mb-2">
            <h3 class="text-xl font-semibold mb-2">{{ pub.titulo }}</h3>
            <span 
              class="text-xs font-medium px-2 py-0.5 rounded-full"
              :class="getStatusClass(pub.status)"
            >
              {{ pub.status }}
            </span>
          </div>
          <p class="text-sm text-gray-500 mb-1">Por: {{ pub.usuario_nome }}</p>
          <p class="text-sm text-gray-500 mb-3">Tipo: {{ pub.tipo }}</p>
          <!-- Adicionar um resumo da descrição -->
          <p class="text-gray-700 text-sm mb-4 line-clamp-3">
            {{ pub.descricao || 'Sem descrição detalhada.' }}
          </p>
          <div class="text-xs text-gray-400 text-right">
            Publicado em: {{ formatDate(pub.data_criacao) }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import publicationService from '../services/publicationService';

export default {
  name: 'PublicationListView',
  
  setup() {
    const router = useRouter();
    const publications = ref([]);
    const loading = ref(true);
    const error = ref(null);

    const fetchPublications = async () => {
      loading.value = true;
      error.value = null;
      try {
        publications.value = await publicationService.listPublications();
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro desconhecido';
      } finally {
        loading.value = false;
      }
    };

    const formatDate = (timestamp) => {
      if (!timestamp) return '';
      return new Date(timestamp).toLocaleDateString();
    };

    const getStatusClass = (status) => {
      switch (status) {
        case 'Aberto': return 'bg-green-100 text-green-700';
        case 'Em Andamento': return 'bg-yellow-100 text-yellow-700';
        case 'Concluído': return 'bg-blue-100 text-blue-700';
        case 'Cancelado': return 'bg-red-100 text-red-700';
        default: return 'bg-gray-100 text-gray-700';
      }
    };
    
    const goToPublication = (id) => {
      router.push({ name: 'PublicationDetail', params: { publicationId: id } });
    };

    onMounted(fetchPublications);

    return {
      publications,
      loading,
      error,
      formatDate,
      getStatusClass,
      goToPublication
    };
  }
};
</script>

<style scoped>
.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;  
  overflow: hidden;
}
</style>
