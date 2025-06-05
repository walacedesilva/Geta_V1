import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import DashboardView from '../views/DashboardView.vue';
import ChatView from '../views/ChatView.vue';
import ProfileView from '../views/ProfileView.vue';
import ProfileEdit from '../views/ProfileEdit.vue';
import PublicationListView from '../views/PublicationListView.vue';
import CreatePublicationView from '../views/CreatePublicationView.vue';
import PublicationDetailView from '../views/PublicationDetailView.vue';
import EditPublicationView from '../views/EditPublicationView.vue';
import SearchView from '../views/SearchView.vue'; // Importar SearchView

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
    meta: { requiresAuth: false }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/chat',
    name: 'Chat',
    component: ChatView,
    meta: { requiresAuth: true }
  },
  {
    path: '/profile/me',
    name: 'MyProfile',
    component: ProfileView,
    meta: { requiresAuth: true }
  },
  {
    path: '/profile/edit',
    name: 'ProfileEdit',
    component: ProfileEdit,
    meta: { requiresAuth: true }
  },
  {
    path: '/profile/:userId',
    name: 'ProfileView',
    component: ProfileView,
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/publications',
    name: 'PublicationList',
    component: PublicationListView,
    meta: { requiresAuth: true } 
  },
  {
    path: '/publications/new',
    name: 'CreatePublication',
    component: CreatePublicationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/publications/:publicationId',
    name: 'PublicationDetail',
    component: PublicationDetailView,
    props: true,
    meta: { requiresAuth: true } 
  },
  {
    path: '/publications/:publicationId/edit',
    name: 'EditPublication',
    component: EditPublicationView,
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/search', // Rota para a busca
    name: 'Search',
    component: SearchView,
    meta: { requiresAuth: true } // Ou false se a busca for pública
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

// Navegação guard para verificar autenticação
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);

  if (requiresAuth && !authStore.isAuthenticated) {
    next('/login');
  } else if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
    next('/dashboard');
  } else {
    next();
  }
});

export default router;
