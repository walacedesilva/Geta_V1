<template>
  <div class="container mx-auto p-6">
    <h2 class="text-2xl font-bold mb-6">Nova Publicação</h2>
    
    <div class="bg-white rounded-lg shadow-md p-8 max-w-2xl mx-auto">
      <form @submit.prevent="submitPublication">
        <!-- Título -->
        <div class="mb-4">
          <label for="titulo" class="block text-gray-700 font-medium mb-2">Título</label>
          <input 
            id="titulo" 
            v-model="publication.titulo" 
            type="text" 
            required
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Ex: Preciso de um desenvolvedor Vue.js"
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
            placeholder="Descreva a necessidade ou o projeto em detalhes..."
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
        
        <!-- Orçamento (Opcional) -->
        <div class="mb-4">
          <label for="orcamento" class="block text-gray-700 font-medium mb-2">Orçamento Estimado (Opcional)</label>
          <input 
            id="orcamento" 
            v-model.number="publication.orcamento" 
            type="number" 
            step="0.01"
            class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Ex: 5000.00"
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
            placeholder="Ex: vuejs, frontend, desenvolvimento web"
          />
        </div>
        
        <!-- Botões -->
        <div class="mt-8 flex justify-end space-x-4">
          <router-link 
            :to="{ name: 'PublicationList' }" 
            class="px-6 py-2 border rounded-md hover:bg-gray-100 transition"
          >
            Cancelar
          </router-link>
          <button 
            type="submit" 
            class="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            :disabled="isSubmitting"
          >
            {{ isSubmitting ? 'Publicando...' : 'Publicar' }}
          </button>
        </div>
        
        <div v-if="submitError" class="mt-4 text-red-600 text-center">
          Erro ao publicar: {{ submitError }}
        </div>
        
      </form>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import publicationService from '../services/publicationService';

export default {
  name: 'CreatePublicationView',
  
  setup() {
    const router = useRouter();
    const publication = reactive({
      titulo: '',
      descricao: '',
      tipo: 'Necessidade', // Valor padrão
      orcamento: null,
      prazo: null,
      tags: []
    });
    const tagsInput = ref('');
    const isSubmitting = ref(false);
    const submitError = ref(null);

    const submitPublication = async () => {
      isSubmitting.value = true;
      submitError.value = null;
      
      // Processar tags
      publication.tags = tagsInput.value
        .split(',')
        .map(tag => tag.trim())
        .filter(tag => tag !== '');
        
      // Remover campos nulos ou vazios que são opcionais
      const dataToSubmit = { ...publication };
      if (dataToSubmit.orcamento === null || dataToSubmit.orcamento === '') {
        delete dataToSubmit.orcamento;
      }
      if (dataToSubmit.prazo === null || dataToSubmit.prazo === '') {
        delete dataToSubmit.prazo;
      }
      if (dataToSubmit.tags.length === 0) {
        delete dataToSubmit.tags;
      }

      try {
        const newPublication = await publicationService.createPublication(dataToSubmit);
        // Redirecionar para a página da publicação criada ou para a lista
        router.push({ name: 'PublicationDetail', params: { publicationId: newPublication.id } });
      } catch (err) {
        submitError.value = err.response?.data?.message || err.message || 'Erro desconhecido ao publicar';
      } finally {
        isSubmitting.value = false;
      }
    };

    return {
      publication,
      tagsInput,
      isSubmitting,
      submitError,
      submitPublication
    };
  }
};
</script>

