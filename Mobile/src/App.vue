<template>
  <!-- LOGIN -->
  <LoginMobile v-if="!usuarioLogueado" @login-success="iniciarSesion" />

  <!-- APP PRINCIPAL -->
  <div v-else style="height: 100%; display: flex; flex-direction: column; background: var(--bg);">

    <!-- HEADER FIJO -->
    <header class="mobile-header">
      <div class="login-logo-wrap" style="width:30px;height:30px;font-size:1rem;border-radius:8px;margin:0;box-shadow:none;min-width:30px;">🐾</div>
      <span class="header-title">Vida Animal</span>

      <!-- Badge conexión (solo en POS) -->
      <div v-if="currentTab === 'pos'" class="header-badge" :class="conexionOk ? 'badge-online' : 'badge-offline'">
        <span class="pulse-dot"></span>
        {{ conexionOk ? 'En línea' : 'Verificando...' }}
      </div>

      <!-- Rol badge -->
      <div class="header-badge badge-role">
        {{ usuarioLogueado.rol === 'ADMINISTRADOR' ? '👑 Admin' : '💼 Cajero' }}
      </div>

      <!-- Botón salir -->
      <button @click="cerrarSesion" class="btn-icon" title="Cerrar sesión">
        <svg xmlns="http://www.w3.org/2000/svg" width="17" height="17" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
        </svg>
      </button>
    </header>

    <!-- CONTENIDO SCROLLABLE -->
    <main class="screen">
      <POSMobile
        v-if="currentTab === 'pos'"
        :usuario-rol="usuarioLogueado.rol"
        @conexion-ok="conexionOk = $event"
      />
      <InventarioMobile v-else-if="currentTab === 'inventario'" />
    </main>

    <!-- BARRA DE NAVEGACIÓN INFERIOR -->
    <nav class="bottom-nav">
      <button class="nav-tab" :class="{ active: currentTab === 'pos' }" @click="currentTab = 'pos'">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 11h.01M12 11h.01M15 11h.01M4 19V6a2 2 0 012-2h12a2 2 0 012 2v13M4 19h16M4 19l-2 2m18-2l2 2" />
        </svg>
        POS
      </button>
      <button class="nav-tab" :class="{ active: currentTab === 'inventario' }" @click="currentTab = 'inventario'">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
        Inventario
      </button>
    </nav>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import LoginMobile from './components/LoginMobile.vue'
import POSMobile from './components/POSMobile.vue'
import InventarioMobile from './components/InventarioMobile.vue'

const usuarioLogueado = ref(null)
const currentTab = ref('pos')
const conexionOk = ref(false)

onMounted(() => {
  const userStored = localStorage.getItem('usuario')
  const tokenStored = localStorage.getItem('jwt_token')
  if (userStored && tokenStored) {
    try { usuarioLogueado.value = JSON.parse(userStored) }
    catch { usuarioLogueado.value = null }
  }
})

const iniciarSesion = (usuario) => {
  usuarioLogueado.value = usuario
  currentTab.value = 'pos'
}

const cerrarSesion = () => {
  localStorage.removeItem('jwt_token')
  localStorage.removeItem('usuario')
  usuarioLogueado.value = null
  currentTab.value = 'pos'
}
</script>
