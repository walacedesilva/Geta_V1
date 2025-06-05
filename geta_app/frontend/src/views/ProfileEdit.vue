<template>
  <div class="container mx-auto p-6">
    <h2 class="text-2xl font-bold mb-6">Editar Perfil</h2>
    
    <div v-if="loading" class="text-center py-10">
      <p>Carregando dados do perfil...</p>
    </div>
    
    <div v-else-if="error" class="text-center py-10 text-red-600">
      <p>Erro ao carregar perfil: {{ error }}</p>
    </div>
    
    <div v-else class="bg-white rounded-lg shadow-md p-8 max-w-2xl mx-auto">
      <form @submit.prevent="saveProfile">
        <!-- Campos Comuns -->
        <div class="mb-6">
          <label for="foto_url" class="block text-gray-700 font-medium mb-2">URL da Foto de Perfil</label>
          <input 
            id="foto_url" 
            v-model="profileData.foto_url" 
            type="text" 
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="https://exemplo.com/sua-foto.jpg"
          />
        </div>
        
        <div class="mb-6">
          <label for="bio" class="block text-gray-700 font-medium mb-2">Biografia</label>
          <textarea 
            id="bio" 
            v-model="profileData.bio" 
            rows="4" 
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Fale um pouco sobre você..."
          ></textarea>
        </div>
        
        <div class="mb-6">
          <label for="localizacao" class="block text-gray-700 font-medium mb-2">Localização</label>
          <input 
            id="localizacao" 
            v-model="profileData.localizacao" 
            type="text" 
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Cidade, Estado"
          />
        </div>
        
        <!-- Campos Específicos por Tipo de Perfil -->
        <div class="mb-6">
          <h3 class="text-lg font-semibold border-b pb-2 mb-3">Detalhes Específicos ({{ profileData.tipo_perfil }})</h3>
          
          <!-- Exemplo para Empreendedor -->
          <div v-if="profileData.tipo_perfil === 'Empreendedor'">
            <div class="mb-4">
              <label for="nome_empresa" class="block text-gray-700 mb-1">Nome da Empresa</label>
              <input id="nome_empresa" v-model="profileData.detalhes.nome_empresa" type="text" class="w-full p-2 border rounded-md"/>
            </div>
            <div class="mb-4">
              <label for="segmento" class="block text-gray-700 mb-1">Segmento</label>
              <input id="segmento" v-model="profileData.detalhes.segmento" type="text" class="w-full p-2 border rounded-md"/>
            </div>
          </div>
          
          <!-- Exemplo para Fornecedor -->
          <div v-else-if="profileData.tipo_perfil === 'Fornecedor'">
            <div class="mb-4">
              <label for="area_atuacao" class="block text-gray-700 mb-1">Área de Atuação</label>
              <input id="area_atuacao" v-model="profileData.detalhes.area_atuacao" type="text" class="w-full p-2 border rounded-md"/>
            </div>
            <div class="mb-4">
              <label for="experiencia" class="block text-gray-700 mb-1">Anos de Experiência</label>
              <input id="experiencia" v-model="profileData.detalhes.experiencia" type="number" class="w-full p-2 border rounded-md"/>
            </div>
          </div>
          
          <!-- Exemplo para Colaborador -->
          <div v-else-if="profileData.tipo_perfil === 'Colaborador'">
            <div class="mb-4">
              <label for="habilidades" class="block text-gray-700 mb-1">Principais Habilidades</label>
              <input id="habilidades" v-model="profileData.detalhes.habilidades" type="text" class="w-full p-2 border rounded-md" placeholder="Separadas por vírgula"/>
            </div>
            <div class="mb-4">
              <label for="disponibilidade" class="block text-gray-700 mb-1">Disponibilidade</label>
              <input id="disponibilidade" v-model="profileData.detalhes.disponibilidade" type="text" class="w-full p-2 border rounded-md" placeholder="Ex: Integral, Parcial, Freelance"/>
            </div>
          </div>
          
          <p class="text-xs text-gray-500 mt-2">
            Estes são apenas exemplos. Você pode adicionar mais campos conforme necessário editando o JSON diretamente ou adaptando o formulário.
          </p>
          <div class="mt-4">
             <label for="detalhes_json" class="block text-gray-700 font-medium mb-2">Detalhes (JSON)</label>
             <textarea 
                id="detalhes_json" 
                :value="JSON.stringify(profileData.detalhes, null, 2)"
                @input="updateDetalhesJson"
                rows="5" 
                class="w-full p-2 border rounded-md font-mono text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Edite os detalhes em formato JSON..."
             ></textarea>
             <p v-if="jsonError" class="text-red-500 text-xs mt-1">JSON inválido: {{ jsonError }}</p>
          </div>
        </div>
        
        <!-- Botões -->
        <div class="mt-8 flex justify-end space-x-4">
          <router-link 
            :to="{ name: 'ProfileView', params: { userId: profileData.user_id } }" 
            class="px-6 py-2 border rounded-md hover:bg-gray-100 transition"
          >
            Cancelar
          </router-link>
          <button 
            type="submit" 
            class="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            :disabled="isSaving || !!jsonError"
          >
            {{ isSaving ? 'Salvando...' : 'Salvar Alterações' }}
          </button>
        </div>
        
        <div v-if="saveError" class="mt-4 text-red-600 text-center">
          Erro ao salvar: {{ saveError }}
        </div>
        <div v-if="saveSuccess" class="mt-4 text-green-600 text-center">
          Perfil atualizado com sucesso!
        </div>
      </form>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, reactive } from 'vue';
import { useRouter } from 'vue-router';
import profileService from '../services/profileService';
import { useAuthStore } from '../stores/authStore';

export default {
  name: 'ProfileEdit',
  
  setup() {
    const router = useRouter();
    const authStore = useAuthStore();
    
    const profileData = reactive({
      user_id: null,
      nome_completo: '',
      email: '',
      tipo_perfil: '',
      profile_id: null,
      foto_url: '',
      bio: '',
      localizacao: '',
      detalhes: {}
    });
    
    const loading = ref(true);
    const error = ref(null);
    const isSaving = ref(false);
    const saveError = ref(null);
    const saveSuccess = ref(false);
    const jsonError = ref(null);

    const fetchProfile = async () => {
      loading.value = true;
      error.value = null;
      try {
        const data = await profileService.getMyProfile();
        Object.assign(profileData, data);
        // Garantir que detalhes seja um objeto
        if (!profileData.detalhes || typeof profileData.detalhes !== 'object') {
          profileData.detalhes = {};
        }
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro desconhecido';
      } finally {
        loading.value = false;
      }
    };

    const saveProfile = async () => {
      isSaving.value = true;
      saveError.value = null;
      saveSuccess.value = false;
      jsonError.value = null; // Limpa erro JSON antes de salvar
      
      try {
        // Validar JSON antes de enviar
        if (typeof profileData.detalhes !== 'object') {
           throw new Error("Formato de detalhes inválido.");
        }
        
        const dataToSave = {
          foto_url: profileData.foto_url,
          bio: profileData.bio,
          localizacao: profileData.localizacao,
          detalhes: profileData.detalhes
        };
        
        const updatedProfile = await profileService.updateMyProfile(dataToSave);
        Object.assign(profileData, updatedProfile);
        saveSuccess.value = true;
        // Opcional: redirecionar após salvar
        // router.push({ name: 'ProfileView', params: { userId: profileData.user_id } });
      } catch (err) {
        saveError.value = err.response?.data?.message || err.message || 'Erro ao salvar perfil';
      } finally {
        isSaving.value = false;
      }
    };
    
    // Atualizar o objeto 'detalhes' quando o textarea JSON for modificado
    const updateDetalhesJson = (event) => {
      try {
        const parsedJson = JSON.parse(event.target.value);
        profileData.detalhes = parsedJson;
        jsonError.value = null; // Limpa erro se JSON for válido
      } catch (e) {
        jsonError.value = e.message; // Define mensagem de erro se JSON for inválido
      }
    };

    onMounted(fetchProfile);

    return {
      profileData,
      loading,
      error,
      isSaving,
      saveError,
      saveSuccess,
      jsonError,
      saveProfile,
      updateDetalhesJson
    };
  }
};
</script>
