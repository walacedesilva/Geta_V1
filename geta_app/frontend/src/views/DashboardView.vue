<template>
  <div class="min-h-screen bg-gray-100">
    <nav class="bg-blue-600 text-white p-4">
      <div class="container mx-auto flex justify-between items-center">
        <h1 class="text-2xl font-bold">Geta</h1>
        <div class="flex space-x-4">
          <router-link to="/dashboard" class="hover:text-blue-200 transition">Dashboard</router-link>
          <router-link to="/chat" class="hover:text-blue-200 transition">Chat</router-link>
          <button @click="logout" class="hover:text-blue-200 transition">Sair</button>
        </div>
      </div>
    </nav>
    
    <div class="container mx-auto p-6">
      <h2 class="text-2xl font-semibold mb-6">Dashboard</h2>
      
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <h3 class="text-xl font-semibold mb-4">Bem-vindo, {{ user.nome }}!</h3>
        <p class="mb-4">Este é o seu painel de controle no aplicativo Geta.</p>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
          <div class="bg-blue-50 p-6 rounded-lg border border-blue-200">
            <h4 class="font-semibold text-lg mb-2">Mensagens</h4>
            <p class="text-gray-600 mb-4">Acesse o sistema de chat para se comunicar com outros usuários.</p>
            <router-link to="/chat" class="inline-block px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition">
              Ir para Chat
            </router-link>
          </div>
          
          <div class="bg-gray-50 p-6 rounded-lg border border-gray-200">
            <h4 class="font-semibold text-lg mb-2">Perfil</h4>
            <p class="text-gray-600 mb-4">Visualize e edite suas informações de perfil.</p>
            <button class="inline-block px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 transition" disabled>
              Em breve
            </button>
          </div>
          
          <div class="bg-gray-50 p-6 rounded-lg border border-gray-200">
            <h4 class="font-semibold text-lg mb-2">Projetos</h4>
            <p class="text-gray-600 mb-4">Gerencie seus projetos e necessidades.</p>
            <button class="inline-block px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 transition" disabled>
              Em breve
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';

export default {
  name: 'DashboardView',
  
  setup() {
    const router = useRouter();
    const authStore = useAuthStore();
    
    const user = computed(() => authStore.user);
    
    const logout = () => {
      authStore.logout();
      router.push('/login');
    };
    
    return {
      user,
      logout
    };
  }
};
</script>
