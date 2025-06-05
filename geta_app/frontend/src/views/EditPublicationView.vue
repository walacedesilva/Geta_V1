<template>
  <div class="container mx-auto p-6">
    <h2 class="text-2xl font-bold mb-6">Editar Publicação</h2>
    
    <div v-if="loading" class="text-center py-10">
      <p>Carregando dados da publicação...</p>
    </div>
    
    <div v-else-if="error" class="text-center py-10 text-red-600">
      <p>Erro ao carregar publicação: {{ error }}</p>
    </div>
    
    <div v-else-if="publication" class="bg-white rounded-lg shadow-md p-8 max-w-2xl mx-auto">
      <form @submit.prevent="submitUpdate">
        <!-- Título -->
        <div class="mb-4">
          <label for="titulo" class="block text-gray-700 font-medium mb-2">Título</label>
          <input 
            id="titulo" 
            v-model="publication.titulo" 
            type="text" 
            required
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        <!-- Descrição -->
        <div class="mb-4">
          <label for="descricao" class="block text-gray-700 font-medium mb-2">Descrição Detalhada</label>
          <textarea 
            id="descricao" 
            v-model="publication.descricao" 
            rows="5" 
            required
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          ></textarea>
        </div>
        
        <!-- Tipo -->
        <div class="mb-4">
          <label for="tipo" class="block text-gray-700 font-medium mb-2">Tipo</label>
          <select 
            id="tipo" 
            v-model="publication.tipo" 
            class="w-full p-2 border rounded-md bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="Necessidade">Necessidade</option>
            <option value="Projeto">Projeto</option>
          </select>
        </div>

        <!-- Status -->
        <div class="mb-4">
          <label for="status" class="block text-gray-700 font-medium mb-2">Status</label>
          <select 
            id="status" 
            v-model="publication.status" 
            class="w-full p-2 border rounded-md bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="Aberto">Aberto</option>
            <option value="Em Andamento">Em Andamento</option>
            <option value="Concluído">Concluído</option>
            <option value="Cancelado">Cancelado</option>
          </select>
        </div>
        
        <!-- Orçamento (Opcional) -->
        <div class="mb-4">
          <label for="orcamento" class="block text-gray-700 font-medium mb-2">Orçamento Estimado (Opcional)</label>
          <input 
            id="orcamento" 
            v-model.number="publication.orcamento" 
            type="number" 
            step="0.01"
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        <!-- Prazo (Opcional) -->
        <div class="mb-4">
          <label for="prazo" class="block text-gray-700 font-medium mb-2">Prazo Desejado (Opcional)</label>
          <input 
            id="prazo" 
            v-model="publication.prazo" 
            type="date" 
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        <!-- Tags (Opcional) -->
        <div class="mb-4">
          <label for="tags" class="block text-gray-700 font-medium mb-2">Tags (Separadas por vírgula)</label>
          <input 
            id="tags" 
            v-model="tagsInput" 
            type="text" 
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        <!-- Botões -->
        <div class="mt-8 flex justify-end space-x-4">
          <router-link 
            :to="{ name: 'PublicationDetail', params: { publicationId: publication.id } }" 
            class="px-6 py-2 border rounded-md hover:bg-gray-100 transition"
          >
            Cancelar
          </router-link>
          <button 
            type="submit" 
            class="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            :disabled="isSubmitting"
          >
            {{ isSubmitting ? 'Salvando...' : 'Salvar Alterações' }}
          </button>
        </div>
        
        <div v-if="submitError" class="mt-4 text-red-600 text-center">
          Erro ao salvar: {{ submitError }}
        </div>
        
      </form>
    </div>

     <div v-else class="text-center py-10 text-gray-500">
      <p>Publicação não encontrada ou você não tem permissão para editá-la.</p>
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import publicationService from '../services/publicationService';
import { useAuthStore } from '../stores/authStore';

export default {
  name: 'EditPublicationView',
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
    
    const publication = reactive({
      id: null,
      titulo: '',
      descricao: '',
      tipo: 'Necessidade',
      status: 'Aberto',
      orcamento: null,
      prazo: null,
      tags: []
    });
    const tagsInput = ref('');
    const loading = ref(true);
    const error = ref(null);
    const isSubmitting = ref(false);
    const submitError = ref(null);

    const fetchPublicationData = async () => {
      loading.value = true;
      error.value = null;
      try {
        const data = await publicationService.getPublicationDetails(props.publicationId);
        
        // Verificar se o usuário logado é o proprietário
        if (data.usuario_id !== authStore.user.id) {
          throw new Error('Você não tem permissão para editar esta publicação.');
        }
        
        Object.assign(publication, data);
        // Formatar data para o input type="date"
        if (publication.prazo) {
          publication.prazo = new Date(publication.prazo).toISOString().split('T')[0];
        }
        // Formatar tags para o input
        tagsInput.value = (publication.tags || []).join(', ');
        
      } catch (err) {
        error.value = err.response?.data?.message || err.message || 'Erro desconhecido';
        publication.id = null; // Indica que não foi possível carregar ou não tem permissão
      } finally {
        loading.value = false;
      }
    };

    const submitUpdate = async () => {
      isSubmitting.value = true;
      submitError.value = null;
      
      // Processar tags
      publication.tags = tagsInput.value
        .split(',')
        .map(tag => tag.trim())
        .filter(tag => tag !== '');
        
      // Remover campos nulos ou vazios que são opcionais
      const dataToSubmit = { ...publication };
      // Não enviar ID ou dados do usuário no corpo da requisição PUT
      delete dataToSubmit.id;
      delete dataToSubmit.usuario_id;
      delete dataToSubmit.usuario_nome;
      delete dataToSubmit.data_criacao;
      delete dataToSubmit.data_atualizacao;

      if (dataToSubmit.orcamento === null || dataToSubmit.orcamento === '') {
        dataToSubmit.orcamento = null; // Enviar null explicitamente se vazio
      }
      if (dataToSubmit.prazo === null || dataToSubmit.prazo === '') {
         dataToSubmit.prazo = null; // Enviar null explicitamente se vazio
      }
       if (dataToSubmit.tags.length === 0) {
        dataToSubmit.tags = []; // Enviar array vazio se não houver tags
      }

      try {
        await publicationService.updatePublication(props.publicationId, dataToSubmit);
        // Redirecionar para a página da publicação atualizada
        router.push({ name: 'PublicationDetail', params: { publicationId: props.publicationId } });
      } catch (err) {
        submitError.value = err.response?.data?.message || err.message || 'Erro desconhecido ao salvar';
      } finally {
        isSubmitting.value = false;
      }
    };

    // Observar mudanças no ID da rota (pouco provável nesta tela, mas por segurança)
    watch(() => props.publicationId, fetchPublicationData);

    onMounted(fetchPublicationData);

    return {
      publication,
      tagsInput,
      loading,
      error,
      isSubmitting,
      submitError,
      submitUpdate
    };
  }
};
</script>

