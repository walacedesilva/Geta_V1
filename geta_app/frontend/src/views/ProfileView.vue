<template>
  <div class="container mx-auto p-6">
    <div v-if="loading" class="text-center py-10">
      <p>Carregando perfil...</p>
    </div>
    
    <div v-else-if="error" class="text-center py-10 text-red-600">
      <p>Erro ao carregar perfil: {{ error }}</p>
    </div>
    
    <div v-else-if="profile" class="bg-white rounded-lg shadow-md p-8">
      <div class="flex flex-col md:flex-row items-center md:items-start">
        <!-- Foto e Nome -->
        <div class="flex-shrink-0 mb-6 md:mb-0 md:mr-8 text-center">
          <img 
            :src="profile.foto_url || defaultAvatar" 
            alt="Foto de perfil"
            class="w-32 h-32 rounded-full object-cover mx-auto border-4 border-blue-200 shadow-sm"
          />
          <h2 class="text-2xl font-bold mt-4">{{ profile.nome_completo }}</h2>
          <span class="text-sm bg-blue-100 text-blue-700 px-3 py-1 rounded-full mt-2 inline-block">
            {{ profile.tipo_perfil }}
          </span>
        </div>
        
        <!-- Informações do Perfil -->
        <div class="flex-grow">
          <div class="mb-6">
            <h3 class="text-lg font-semibold border-b pb-2 mb-3">Sobre</h3>
            <p class="text-gray-700 whitespace-pre-wrap">{{ profile.bio || 'Nenhuma biografia informada.' }}</p>
          </div>
          
          <div class="mb-6">
            <h3 class="text-lg font-semibold border-b pb-2 mb-3">Localização</h3>
            <p class="text-gray-700">{{ profile.localizacao || 'Não informada.' }}</p>
          </div>
          
          <!-- Detalhes Específicos do Perfil -->
          <div v-if="profile.detalhes && Object.keys(profile.detalhes).length > 0" class="mb-6">
            <h3 class="text-lg font-semibold border-b pb-2 mb-3">Detalhes Adicionais</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div v-for="(value, key) in profile.detalhes" :key="key" class="bg-gray-50 p-3 rounded">
                <span class="font-medium capitalize">{{ formatDetailKey(key) }}:</span> 
                <span class="text-gray-700">{{ value }}</span>
              </div>
            </div>
          </div>
          
          <!-- Botão de Edição (se for o perfil do usuário logado) -->
          <div v-if="isOwnProfile" class="mt-8 text-right">
            <router-link 
              :to="{ name: 'ProfileEdit' }" 
              class="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            >
              Editar Perfil
            </router-link>
          </div>
        </div>
      </div>
    </div>
    
    <div v-else class="text-center py-10 text-gray-500">
      <p>Perfil não encontrado.</p>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import profileService from '../services/profileService';

default {
  name: 'ProfileView',
  
  setup() {
    const route = useRoute();
    const authStore = useAuthStore();
    
    const profile = ref(null);
    const loading = ref(true);
    const error = ref(null);
    const defaultAvatar = '/src/assets/images/default-avatar.png'; // Certifique-se que este caminho existe

    const userIdFromRoute = computed(() => route.params.userId ? parseInt(route.params.userId, 10) : null);
    const loggedInUserId = computed(() => authStore.user?.id);
    
    const isOwnProfile = computed(() => {
      // Se não há ID na rota, assume que é o próprio perfil (ex: /profile/me)
      // Ou se o ID na rota é igual ao ID do usuário logado
      return !userIdFromRoute.value || userIdFromRoute.value === loggedInUserId.value;
    });

    const fetchProfile = async () => {
      loading.value = true;
      error.value = null;
      profile.value = null;
      
      try {
        if (isOwnProfile.value) {
          // Se for o próprio perfil, busca /me
          profile.value = await profileService.getMyProfile();
          // Se o perfil estiver incompleto, redirecionar para edição?
          if (profile.value.profile_incomplete) {
            // Opcional: Redirecionar para a página de edição
            // router.push({ name: 'ProfileEdit' });
            // Ou apenas mostrar uma mensagem indicando para completar
            console.log("Perfil incompleto, sugerir edição.");
          }
        } else {
          // Se for o perfil de outro usuário, busca pelo ID
          profile.value = await profileService.getUserProfile(userIdFromRoute.value);
        }
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro desconhecido';
      } finally {
        loading.value = false;
      }
    };

    // Formatar chaves dos detalhes (ex: 'area_atuacao' -> 'Área de Atuação')
    const formatDetailKey = (key) => {
      return key.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
    };

    // Observar mudanças na rota (ex: navegar de um perfil para outro)
    watch(() => route.params.userId, fetchProfile, { immediate: true });

    // Buscar perfil ao montar (caso não tenha sido pego pelo watch immediate)
    // onMounted(fetchProfile);

    return {
      profile,
      loading,
      error,
      isOwnProfile,
      defaultAvatar,
      formatDetailKey
    };
  }
};
</script>

<style scoped>
/* Estilos adicionais, se necessário */
</style>
