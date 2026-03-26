<template>
  <!-- Si NO hay usuario, mostramos el Login -->
  <Login v-if="!usuarioLogueado" @login-success="iniciarSesion" />

  <!-- Si SÍ hay usuario, mostramos la App normal -->
  <div v-else class="app-layout">
    <Sidebar :activeTab="currentTab" :usuario="usuarioLogueado" @change-tab="tab => currentTab = tab" />
    
    <main class="main-content">
      <!-- Barra superior para saber quién está conectado -->
      <header class="top-header">
        <div class="user-info">
          <h2 class="welcome-text">Bienvenido, {{ usuarioLogueado.nombre }}</h2>
          <span class="role-badge" :class="usuarioLogueado.rol === 'ADMINISTRADOR' ? 'role-admin' : 'role-cashier'">
            {{ usuarioLogueado.rol }}
          </span>
        </div>
        
        <button @click="cerrarSesion" class="logout-btn">
          <svg class="logout-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
          </svg>
          Salir
        </button>
      </header>

      <Dashboard v-if="currentTab === 'dashboard'" />
      <POS v-else-if="currentTab === 'pos'" />
      <Usuarios v-else-if="currentTab === 'users'" />
      <Proveedores v-else-if="currentTab === 'proveedores'" />
      <Productos v-else-if="currentTab === 'productos'" />
      <Compras v-else-if="currentTab === 'compras'" />
      <div v-else class="placeholder-view fade-in">
        <h2 class="placeholder-title">Próximamente 🚧</h2>
        <p class="placeholder-text">El módulo de {{ currentTab }} estará disponible próximamente.</p>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import Login from './components/Login.vue'
import Sidebar from './components/Sidebar.vue'
import Dashboard from './components/Dashboard.vue'
import POS from './components/POS.vue'
import Usuarios from './components/Usuarios.vue'
import Proveedores from './components/Proveedores.vue'
import Productos from './components/Productos.vue'
import Compras from './components/Compras.vue'

// Estado global de la aplicación
const currentTab = ref('dashboard')
const usuarioLogueado = ref(null)

// Al cargar la app, revisamos si ya se había logueado antes
onMounted(() => {
  const guardado = localStorage.getItem('usuario')
  if (guardado) {
    usuarioLogueado.value = JSON.parse(guardado)
  }
})

// Función que se dispara desde el componente Login
const iniciarSesion = (usuario) => {
  usuarioLogueado.value = usuario
}

// Función para salir
const cerrarSesion = () => {
  localStorage.removeItem('jwt_token')
  localStorage.removeItem('usuario')
  usuarioLogueado.value = null
  currentTab.value = 'dashboard' // Reiniciar estado
}
</script>

<style scoped>
/* Variables estéticas pastel copiadas de tu index.css */
:root {
  --pastel-blue: #A7C7E7;
  --pastel-pink: #FFD1DC;
  --pastel-purple: #C3B1E1;
}

.app-layout {
  display: flex;
  min-height: 100vh;
  background-color: #FAFAFA;
  font-family: 'Inter', 'Segoe UI', sans-serif;
}

.main-content {
  flex: 1;
  margin-left: var(--sidebar-w, 16rem);
  padding: 2rem;
  max-width: 100%;
  overflow-x: hidden;
}

/* Header UI */
.top-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #E2E8F0;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.welcome-text {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2D3748;
  margin: 0;
}

.role-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.6rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  width: fit-content;
}

.role-admin {
  background-color: rgba(195, 177, 225, 0.2);
  color: #553C9A;
}

.role-cashier {
  background-color: rgba(167, 199, 231, 0.2);
  color: #2B6CB0;
}

/* Logout Button */
.logout-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: #E53E3E;
  background-color: #FFF5F5;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.logout-btn:hover {
  background-color: #FED7D7;
}

.logout-icon {
  width: 16px;
  height: 16px;
}

/* Placeholder Views */
.placeholder-view {
  min-height: calc(100vh - 10rem);
  background: white;
  border-radius: 16px;
  border: 1px dashed #CBD5E0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.placeholder-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2D3748;
  margin: 0 0 0.5rem 0;
}

.placeholder-text {
  color: #718096;
  margin: 0;
}

.fade-in { 
  animation: fadeIn 0.4s ease; 
}

@keyframes fadeIn { 
  from { opacity: 0; transform: translateY(10px); } 
  to { opacity: 1; transform: translateY(0); } 
}
</style>
