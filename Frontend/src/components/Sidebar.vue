<template>
  <aside class="sidebar">
    <div class="logo">
      <div class="logo-icon glass">🐾</div>
      <h2>Vida Animal</h2>
    </div>
    <nav class="nav-menu">
      <button 
        v-for="item in filteredMenu" 
        :key="item.id"
        :class="['nav-item', { active: activeTab === item.id }]"
        @click="$emit('change-tab', item.id)"
      >
        <span class="icon">{{ item.icon }}</span>
        <span class="nav-label">{{ item.label }}</span>
      </button>
    </nav>
    <div class="user-profile" v-if="usuario" style="display: flex; flex-direction: column; gap: 0.5rem; align-items: stretch;">
      <div style="display: flex; align-items: center; gap: 0.75rem;">
        <div class="avatar">👨‍💼</div>
        <div class="info" style="flex: 1; overflow: hidden;">
          <p class="name" style="margin: 0; font-weight: 700; color: #1A202C; font-size: 0.85rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">{{ usuario.nombre }}</p>
          <p class="role" style="margin: 0; font-size: 0.7rem; color: #718096;">{{ usuario.rol }}</p>
        </div>
      </div>
      <button @click="$emit('logout')" style="padding: 0.4rem; border: none; border-radius: 6px; background: #FFF5F5; color: #C53030; font-weight: 600; cursor: pointer; display: flex; justify-content: center; align-items: center; gap: 0.4rem; font-size: 0.85rem; transition: background 0.2s; width: 100%;">
        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24" width="16" height="16">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
        </svg>
        Salir
      </button>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  activeTab: { type: String, required: true },
  usuario: { type: Object, required: false, default: () => ({}) }
})

defineEmits(['change-tab', 'logout'])

const menu = [
  { id: 'inicio',          label: 'Inicio',              icon: '🏠', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'dashboard',       label: 'Dashboard',            icon: '📊', roles: ['ADMINISTRADOR'] },

  { id: 'pos',             label: 'Punto de Venta',       icon: '🛒', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'ventas-cliente',  label: 'Historial de Ventas',  icon: '📅', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'productos',       label: 'Productos',            icon: '🐾', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'clasificacion',   label: 'Categorías y Especies', icon: '🏷️', roles: ['ADMINISTRADOR'] },

  { id: 'users',           label: 'Usuarios',             icon: '👥', roles: ['ADMINISTRADOR'] },
  { id: 'clientes',        label: 'Clientes',             icon: '👨‍👩‍👧', roles: ['ADMINISTRADOR', 'CAJERO'] },  
  { id: 'proveedores',     label: 'Proveedores',          icon: '🏢', roles: ['ADMINISTRADOR'] },

  { id: 'compras',         label: 'Compras (Reponer Stock)', icon: '📦', roles: ['ADMINISTRADOR'] },  
  { id: 'historial-compras', label: 'Historial de Compras',    icon: '🧾', roles: ['ADMINISTRADOR'] },
  { id: 'kardex',          label: 'Movimientos de Inventario',    icon: '📓', roles: ['ADMINISTRADOR'] }
  
]

const filteredMenu = computed(() => {
  if (!props.usuario || !props.usuario.rol) return []
  return menu.filter(m => m.roles.includes(props.usuario.rol))
})
</script>

<style scoped>
.sidebar {
  width: 5.5rem;
  height: 100vh;
  background-image: url('../assets/fondosidebar.jpg');  
  background-size: cover;
  background-position: center;
  border-right: none;
  display: flex;
  flex-direction: column;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 100;
  /* Overlay oscuro para bajar brillo y dar look minimalista */
  isolation: isolate;
  transition: width 0.4s cubic-bezier(0.25, 1, 0.5, 1);
  overflow-x: hidden;
  box-shadow: 4px 0 15px rgba(0,0,0,0.05);
}
.sidebar:hover {
  width: 16rem;
}
.sidebar::before {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(255, 255, 255, 0.93);
  z-index: 0;
}
.logo {
  padding: 1.5rem 1.25rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid rgba(255,255,255,0.15);
  position: relative;
  z-index: 1;
  min-width: 16rem;
}
.logo-icon {
  font-size: 1.5rem;
  width: 40px;
  height: 40px;
  min-width: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  background: linear-gradient(135deg, #FFD1DC 0%, #C3B1E1 100%);
  color: white;
  border: none;
}
.logo h2 {
  font-size: 1rem;
  font-weight: 700;
  color: #2D3748;
  font-family: 'Syncopate', sans-serif;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  opacity: 0;
  transition: opacity 0.3s ease;
  white-space: nowrap;
}
.sidebar:hover .logo h2 {
  opacity: 1;
  transition-delay: 0.1s;
}
.nav-menu {
  flex: 1;
  padding: 1.5rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  position: relative;
  z-index: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Estilizar el scrollbar para que sea delgado y estético */
.nav-menu::-webkit-scrollbar {
  width: 6px;
}
.nav-menu::-webkit-scrollbar-track {
  background: transparent;
}
.nav-menu::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.15);
  border-radius: 10px;
}
.nav-menu::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.25);
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1.15rem;
  border-radius: 12px;
  color: #4A5568;
  font-family: 'Poiret One', sans-serif;
  font-weight: 700;
  letter-spacing: 0.12em;
  background: transparent;
  width: 100%;
  text-align: left;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
  overflow: hidden;
}
.sidebar:hover .nav-item {
  min-width: 14rem;
  white-space: normal;
  overflow: visible;
}
.nav-item:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: #2D3748;
}
.nav-item.active {
  background-color: rgba(102, 126, 234, 0.1);
  color: #667eea;
  font-weight: 700;
  box-shadow: none;
}
.nav-item .icon {
  font-size: 1.25rem;
  min-width: 24px;
}
.nav-label {
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.3s ease, visibility 0.3s ease;
  line-height: 1.2;
}
.sidebar:hover .nav-label {
  opacity: 1;
  visibility: visible;
  transition-delay: 0.1s;
}
.user-profile {
  padding: 1.5rem 1.25rem;
  border-top: 1px solid rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
  position: relative;
  z-index: 1;
  min-width: 16rem;
}
.avatar {
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 50%;
  background-color: rgba(0,0,0,0.05);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  border: 1px solid rgba(0,0,0,0.1);
  color: #2D3748;
}
.info {
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.3s ease, visibility 0.3s ease;
  white-space: nowrap;
}
.sidebar:hover .info {
  opacity: 1;
  visibility: visible;
  transition-delay: 0.1s;
}
.info p { margin: 0; }
.info .name {
  font-weight: 600;
  font-size: 0.875rem;
  color: #2D3748;
  letter-spacing: 0.12em;
}
.info .role {
  color: #718096;
  font-size: 0.75rem;
  letter-spacing: 0.12em;
}
</style>
