import { createApp } from "vue";
import App from "./App.vue";
import router from "./router"; // Importar o router
import pinia from "./stores"; // Importar a instância Pinia
import "./assets/css/tailwind.css"; // Importar o CSS do Tailwind

const app = createApp(App);

app.use(pinia); // Usar Pinia
app.use(router); // Usar Vue Router

app.mount("#app");
