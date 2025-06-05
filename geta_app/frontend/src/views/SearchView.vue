<template>
  <div class="container mx-auto p-6">
    <h2 class="text-2xl font-bold mb-6">Buscar</h2>

    <!-- Barra de Busca -->
    <div class="mb-8">
      <form @submit.prevent="executeSearch" class="flex items-center space-x-4">
        <input 
          v-model="searchTerm" 
          type="text" 
          placeholder="Buscar usuários ou publicações..." 
          class="flex-grow p-3 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <select 
          v-model="searchType" 
          class="p-3 border rounded-md bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="all">Tudo</option>
          <option value="users">Usuários</option>
          <option value="publications">Publicações</option>
        </select>
        <button 
          type="submit" 
          class="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
          :disabled="isLoading"
        >
          {{ isLoading ? 'Buscando...' : 'Buscar' }}
        </button>
      </form>
    </div>

    <!-- Resultados da Busca -->
    <div v-if="isLoading" class="text-center py-10">
      <p>Buscando...</p>
    </div>
    
    <div v-else-if="searchError" class="text-center py-10 text-red-600">
      <p>Erro na busca: {{ searchError }}</p>
    </div>
    
    <div v-else-if="!hasSearched" class="text-center py-10 text-gray-500">
      <p>Digite um termo e clique em buscar.</p>
    </div>

    <div v-else-if="results.users.length === 0 && results.publications.length === 0" class="text-center py-10 text-gray-500">
      <p>Nenhum resultado encontrado para "{{ lastSearchTerm }}".</p>
    </div>

    <div v-else>
      <!-- Resultados de Usuários -->
      <div v-if="results.users.length > 0" class="mb-10">
        <h3 class="text-xl font-semibold mb-4 border-b pb-2">Usuários Encontrados</h3>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <div 
            v-for="user in results.users" 
            :key="user.id" 
            class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition cursor-pointer"
            @click="goToProfile(user.id)"
          >
            <div class="flex items-center space-x-4">
              <img 
                :src="user.foto_url || defaultAvatar" 
                alt="Foto de perfil"
                class="w-16 h-16 rounded-full object-cover border-2 border-blue-100"
              />
              <div>
                <h4 class="font-semibold">{{ user.nome_completo }}</h4>
                <p class="text-sm text-gray-600">{{ user.tipo_perfil }}</p>
                <p v-if="user.localizacao" class="text-xs text-gray-500 mt-1">{{ user.localizacao }}</p>
              </div>
            </div>
             <p v-if="user.bio" class="text-sm text-gray-700 mt-3 pt-3 border-t line-clamp-2">
                {{ user.bio }}
             </p>
          </div>
        </div>
      </div>

      <!-- Resultados de Publicações -->
      <div v-if="results.publications.length > 0">
        <h3 class="text-xl font-semibold mb-4 border-b pb-2">Publicações Encontradas</h3>
        <div class="space-y-4">
          <div 
            v-for="pub in results.publications" 
            :key="pub.id" 
            class="bg-white rounded-lg shadow-md p-4 hover:shadow-lg transition cursor-pointer"
            @click="goToPublication(pub.id)"
          >
            <div class="flex justify-between items-start mb-1">
               <h4 class="font-semibold">{{ pub.titulo }}</h4>
               <span 
                class="text-xs font-medium px-2 py-0.5 rounded-full flex-shrink-0 ml-2"
                :class="getStatusClass(pub.status)"
               >
                 {{ pub.status }}
               </span>
            </div>
            <p class="text-sm text-gray-500 mb-2">Por: {{ pub.usuario_nome }} | Tipo: {{ pub.tipo }}</p>
            <p class="text-sm text-gray-700 line-clamp-2">{{ pub.descricao_curta }}...</p>
            <p class="text-xs text-gray-400 text-right mt-2">Publicado em: {{ formatDate(pub.data_criacao) }}</p>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import searchService from '../services/searchService';

export default {
  name: 'SearchView',
  setup() {
    const router = useRouter();
    const searchTerm = ref('');
    const searchType = ref('all'); // 'all', 'users', 'publications'
    const results = reactive({ users: [], publications: [] });
    const isLoading = ref(false);
    const searchError = ref(null);
    const hasSearched = ref(false);
    const lastSearchTerm = ref('');
    const defaultAvatar = '/src/assets/images/default-avatar.png'; // Certifique-se que este caminho existe

    const executeSearch = async () => {
      if (!searchTerm.value.trim()) return;
      
      isLoading.value = true;
      searchError.value = null;
      hasSearched.value = true;
      lastSearchTerm.value = searchTerm.value;
      results.users = [];
      results.publications = [];

      try {
        const searchResults = await searchService.performSearch(
          searchTerm.value,
          searchType.value === 'all' ? null : searchType.value
        );
        results.users = searchResults.users || [];
        results.publications = searchResults.publications || [];
      } catch (err) {
        searchError.value = err.response?.data?.message || err.message || 'Erro desconhecido';
      } finally {
        isLoading.value = false;
      }
    };

    const goToProfile = (userId) => {
      router.push({ name: 'ProfileView', params: { userId } });
    };

    const goToPublication = (publicationId) => {
      router.push({ name: 'PublicationDetail', params: { publicationId } });
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

    return {
      searchTerm,
      searchType,
      results,
      isLoading,
      searchError,
      hasSearched,
      lastSearchTerm,
      defaultAvatar,
      executeSearch,
      goToProfile,
      goToPublication,
      formatDate,
      getStatusClass,
    };
  },
};
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;  
  overflow: hidden;
}
</style>
