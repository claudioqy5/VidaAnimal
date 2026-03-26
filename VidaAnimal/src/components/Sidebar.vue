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
        {{ item.label }}
      </button>
    </nav>
    <div class="user-profile" v-if="usuario">
      <div class="avatar">👨‍💼</div>
      <div class="info">
        <p class="name">{{ usuario.nombre }}</p>
        <p class="role">{{ usuario.rol }}</p>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  activeTab: { type: String, required: true },
  usuario: { type: Object, required: false, default: () => ({}) }
})

defineEmits(['change-tab'])

const menu = [
  { id: 'dashboard', label: 'Dashboard', icon: '📊', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'users', label: 'Usuarios', icon: '👥', roles: ['ADMINISTRADOR'] },
  { id: 'productos', label: 'Productos', icon: '🐾', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'compras', label: 'Compras (Reponer Stock)', icon: '📦', roles: ['ADMINISTRADOR'] },
  { id: 'pos', label: 'Punto de Venta', icon: '🛒', roles: ['ADMINISTRADOR', 'CAJERO'] },
  { id: 'proveedores', label: 'Proveedores', icon: '🏢', roles: ['ADMINISTRADOR'] }
]

const filteredMenu = computed(() => {
  if (!props.usuario || !props.usuario.rol) return []
  return menu.filter(m => m.roles.includes(props.usuario.rol))
})
</script>

<style scoped>
.sidebar {
  width: var(--sidebar-w, 16rem);
  height: 100vh;
  background-color: white; /* Clean background */
  border-right: 1px solid #E2E8F0;
  display: flex;
  flex-direction: column;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 10;
}
.logo {
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid #E2E8F0;
}
.logo-icon {
  font-size: 1.5rem;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  background: linear-gradient(135deg, #FFD1DC 0%, #C3B1E1 100%);
  color: white;
  border: none;
}
.logo h2 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #2D3748;
}
.nav-menu {
  flex: 1;
  padding: 1.5rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border-radius: 12px;
  color: #718096;
  font-weight: 500;
  background: transparent;
  width: 100%;
  text-align: left;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
}
.nav-item:hover {
  background-color: #F7FAFC;
  color: #2D3748;
}
.nav-item.active {
  background-color: rgba(195, 177, 225, 0.15); /* Pastel purple tint */
  color: #553C9A;
  font-weight: 600;
}
.nav-item .icon {
  font-size: 1.25rem;
}
.user-profile {
  padding: 1.5rem;
  border-top: 1px solid #E2E8F0;
  display: flex;
  align-items: center;
  gap: 1rem;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: #EDF2F7;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  border: 1px solid #E2E8F0;
}
.info p { margin: 0; }
.info .name {
  font-weight: 600;
  font-size: 0.875rem;
  color: #2D3748;
}
.info .role {
  color: #718096;
  font-size: 0.75rem;
}
</style>
