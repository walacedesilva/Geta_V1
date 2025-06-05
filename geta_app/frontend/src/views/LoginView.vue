<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-400 to-lime-600 p-4">
    <div class="bg-white p-8 rounded-lg shadow-xl w-full max-w-md">
      <div class="flex justify-center mb-6">
        <!-- Usar o logo Geta aqui quando disponível -->
        <img src="/path/to/geta-logo.png" alt="Geta Logo" class="h-12 w-auto" v-if="false"> 
        <div class="text-center" v-else>
          <span class="text-4xl font-bold text-green-700">Geta</span>
          <p class="text-sm text-gray-500">Global Economic and Trade Agreement</p>
        </div>
      </div>
      <h2 class="text-2xl font-semibold text-center text-gray-700 mb-6">Login</h2>
      <form @submit.prevent="handleLogin">
        <div class="mb-4">
          <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input 
            type="email" 
            id="email" 
            v-model="email" 
            required 
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
            placeholder="seu@email.com"
          >
        </div>
        <div class="mb-6">
          <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Senha</label>
          <input 
            type="password" 
            id="password" 
            v-model="password" 
            required 
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
            placeholder="********"
          >
        </div>
        
        <div v-if="authStore.error" class="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-md text-sm">
          {{ authStore.error }}
        </div>

        <button 
          type="submit" 
          :disabled="authStore.loading" 
          class="w-full bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-opacity-50 transition duration-150 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="authStore.loading">Entrando...</span>
          <span v-else>Entrar</span>
        </button>
        
        <div class="mt-6 text-center">
          <p class="text-sm text-gray-600">
            Não tem uma conta? 
            <router-link to="/register" class="font-medium text-green-600 hover:text-green-500">Cadastre-se</router-link>
          </p>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore";

const email = ref("");
const password = ref("");
const authStore = useAuthStore();
const router = useRouter();

const handleLogin = async () => {
  const success = await authStore.login({ 
    email: email.value, 
    senha: password.value // Campo esperado pelo backend é 'senha'
  });
  if (success) {
    // Redirecionar para o dashboard ou página principal após login
    router.push("/dashboard"); 
  } 
  // O erro já é tratado e exibido pela store
};
</script>

