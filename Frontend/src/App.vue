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
      <Inicio v-else-if="currentTab === 'inicio'" />
      <POS v-else-if="currentTab === 'pos'" />
      <Usuarios v-else-if="currentTab === 'users'" />
      <Proveedores v-else-if="currentTab === 'proveedores'" />
      <Productos v-else-if="currentTab === 'productos'" />
      <Clasificacion v-else-if="currentTab === 'clasificacion'" />
      <Compras v-else-if="currentTab === 'compras'" />
      <HistorialCompras v-else-if="currentTab === 'historial-compras'" />
      <Clientes v-else-if="currentTab === 'clientes'" />
      <Kardex v-else-if="currentTab === 'kardex'" />
      <VentasCliente v-else-if="currentTab === 'ventas-cliente'" />
      <div v-else class="placeholder-view fade-in">
        <h2 class="placeholder-title">Próximamente 🚧</h2>
        <p class="placeholder-text">El módulo de {{ currentTab }} estará disponible próximamente.</p>
      </div>
    </main>

    <!-- Chat Bot Global -->
    <ChatIA />

    <!-- Pantalla Completa de Aniversario de Amor (Aparece los 22 de cada mes) -->
    <div v-if="mostrarAniversario" class="fullscreen-love-modal">
      <div class="floating-hearts">
        <span class="heart h1">❤️</span>
        <span class="heart h2">💖</span>
        <span class="heart h3">🐾</span>
        <span class="heart h4">✨</span>
        <span class="heart h5">💕</span>
        <span class="heart h6">💘</span>
      </div>
      
      <div class="love-content animate-float">
        <h1 class="love-title">¡Feliz Aniversario,<br>{{ primerNombre }}!</h1>
        
        <div class="love-card">
          <p class="love-text main-message">
            ¡Hoy celebramos un mes más de nuestra hermosa relación! 👩‍❤️‍👨
          </p>
          <p class="love-text sub-message">
            Quiero recordarte lo increíblemente orgulloso/a que estoy de nosotros y de todo lo que estamos logrando con <b>Vida Animal</b>. 
            El negocio está yendo por buen camino, estamos haciendo las cosas de corazón y cada esfuerzo, cada madrugada, vale la pena.
          </p>
          <p class="love-text highlight-message">
            Nunca te rindas, porque juntos somos el mejor equipo, en el amor y en la vida. ¡Te amo infinitamente! 🚀❤️
          </p>
        </div>

        <!-- Galería de Recuerdos -->
        <div v-if="fotosAniversario.length > 0" class="gallery-container">
          <div v-for="(foto, idx) in fotosAniversario" :key="idx" class="polaroid-wrapper" :style="{ animationDelay: (idx * 0.4) + 's' }">
            <div class="polaroid">
              <img :src="foto" alt="Nuestro recuerdo hermoso" />
            </div>
          </div>
        </div>
        
        <button class="love-btn" @click="cerrarAniversario">Continuar a nuestro negocio ✨</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import Login from './components/Login.vue'
import Sidebar from './components/Sidebar.vue'
import Dashboard from './components/Dashboard.vue'
import POS from './components/POS.vue'
import Usuarios from './components/Usuarios.vue'
import Proveedores from './components/Proveedores.vue'
import Productos from './components/Productos.vue'
import Clasificacion from './components/Clasificacion.vue'
import Compras from './components/Compras.vue'
import Clientes from './components/Clientes.vue'
import Kardex from './components/Kardex.vue'
import VentasCliente from './components/VentasCliente.vue'
import HistorialCompras from './components/HistorialCompras.vue'
import Inicio from './components/Inicio.vue'
import ChatIA from './components/ChatIA.vue'

// Estado global de la aplicación
const currentTab = ref('inicio')
const usuarioLogueado = ref(null)
const mostrarAniversario = ref(false)

// Cargar dinámicamente todas las fotos de la carpeta fotosmesarios
const fotosModules = import.meta.glob('./assets/fotosmesarios/*.{png,jpg,jpeg,JPG,webp}', { eager: true });
const fotosAniversario = ref(Object.values(fotosModules).map(m => m.default));

const verificarAniversario = () => {
  const hoy = new Date();
  if (hoy.getDate() === 17) {
    const fechaStr = hoy.toISOString().split('T')[0];
    const flag = localStorage.getItem('aniversario_mostrado');
    if (flag !== fechaStr) {
      mostrarAniversario.value = true;
    }
  }
}

const cerrarAniversario = () => {
  mostrarAniversario.value = false;
  const hoy = new Date();
  const fechaStr = hoy.toISOString().split('T')[0];
  localStorage.setItem('aniversario_mostrado', fechaStr);
}

const primerNombre = computed(() => {
  if (!usuarioLogueado.value) return 'Mi Amor';
  // Extrae el primer nombre sea del campo 'nombreCompleto' o 'nombre'
  const nombreF = usuarioLogueado.value.nombreCompleto || usuarioLogueado.value.nombre || 'Mi Amor';
  return nombreF.split(' ')[0];
});

// Al cargar la app, verificamos si hay sesión previa
onMounted(() => {
  const userStored = localStorage.getItem('usuario');
  const tokenStored = localStorage.getItem('jwt_token');
  
  if (userStored && tokenStored) {
    try {
      usuarioLogueado.value = JSON.parse(userStored);
      verificarAniversario();
    } catch (e) {
      usuarioLogueado.value = null;
    }
  } else {
    usuarioLogueado.value = null;
  }
})

// Función que se dispara desde el componente Login
const iniciarSesion = (usuario) => {
  usuarioLogueado.value = usuario
  verificarAniversario()
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

/* Pantalla Completa Aniversario Amor */
.fullscreen-love-modal {
  position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
  background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 99%, #fecfef 100%);
  display: flex; align-items: center; justify-content: center; z-index: 10000;
  overflow: hidden;
}

.love-content {
  text-align: center; max-width: 850px; padding: 2rem;
  z-index: 2; position: relative; width: 90%;
}

.love-title {
  font-family: 'Syncopate', sans-serif;
  font-size: clamp(2.5rem, 6vw, 4.5rem); font-weight: 900;
  color: white; text-shadow: 0px 4px 20px rgba(213, 63, 140, 0.5);
  margin-bottom: 2rem; line-height: 1.1; letter-spacing: -2px;
}

.love-card {
  background: rgba(255, 255, 255, 0.4);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 2px solid rgba(255, 255, 255, 0.6);
  padding: 3.5rem 2rem; border-radius: 40px; margin-bottom: 3rem;
  box-shadow: 0 30px 60px rgba(213, 63, 140, 0.2);
}

.love-text {
  font-family: 'Inter', sans-serif;
  color: #702a46;
}

.main-message { font-size: clamp(1.2rem, 3vw, 2rem); font-weight: 900; margin-bottom: 1.5rem; letter-spacing: 0.02em; }
.sub-message { font-size: clamp(1rem, 2vw, 1.25rem); font-weight: 600; margin-bottom: 2rem; line-height: 1.7; color: #5B2138;}
.highlight-message { font-size: clamp(1.1rem, 2.5vw, 1.4rem); font-weight: 900; color: #D53F8C; font-style: italic; }

.love-btn {
  background: white; color: #D53F8C; border: none; padding: 1.25rem 3.5rem;
  border-radius: 50px; font-weight: 900; font-size: 1.2rem; cursor: pointer;
  transition: all 0.3s; text-transform: uppercase; letter-spacing: 0.1em;
  box-shadow: 0 15px 30px rgba(213, 63, 140, 0.3);
}
.love-btn:hover { transform: translateY(-5px) scale(1.05); box-shadow: 0 20px 40px rgba(213, 63, 140, 0.5); color: #B83280;}

.animate-float { animation: float 6s ease-in-out infinite; }
@keyframes float { 0% { transform: translateY(0px); } 50% { transform: translateY(-15px); } 100% { transform: translateY(0px); } }

/* Galería Polaroids */
.gallery-container {
  display: flex; gap: 1.5rem; justify-content: center; flex-wrap: wrap; margin-bottom: 2.5rem;
}

.polaroid-wrapper {
  animation: floatPolaroid 4s ease-in-out infinite alternate;
}

.polaroid {
  background: white; padding: 10px 10px 25px 10px; border-radius: 4px;
  box-shadow: 0 10px 20px rgba(213, 63, 140, 0.2);
  transform: rotate(-3deg); transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  cursor: pointer; position: relative;
}

.polaroid-wrapper:nth-child(even) .polaroid { transform: rotate(4deg); }
.polaroid-wrapper:nth-child(3n) .polaroid { transform: rotate(-5deg); }

.polaroid:hover {
  transform: scale(1.25) rotate(0deg) !important;
  box-shadow: 0 20px 40px rgba(213, 63, 140, 0.5); z-index: 10;
}

.polaroid img {
  width: 130px; height: 130px; object-fit: cover; border-radius: 2px;
}

@keyframes floatPolaroid {
  0% { transform: translateY(0); }
  100% { transform: translateY(-12px); }
}

/* Corazones Flotantes */
.floating-hearts { position: absolute; width: 100%; height: 100%; top: 0; left: 0; pointer-events: none; z-index: 0; overflow: hidden;}

.heart { position: absolute; opacity: 0; animation: floatUp 8s linear infinite; filter: drop-shadow(0 0 10px rgba(255,255,255,0.5));}
.h1 { left: 10%; animation-delay: 0s; font-size: 4rem; animation-duration: 9s;}
.h2 { left: 30%; animation-delay: 2s; font-size: 2.5rem; animation-duration: 7s;}
.h3 { left: 75%; animation-delay: 4s; font-size: 5rem; animation-duration: 11s;}
.h4 { left: 85%; animation-delay: 1s; font-size: 3rem; animation-duration: 8s;}
.h5 { left: 50%; animation-delay: 5s; font-size: 2rem; animation-duration: 6s;}
.h6 { left: 20%; animation-delay: 6s; font-size: 3.5rem; animation-duration: 10s;}

@keyframes floatUp { 
  0% { transform: translateY(100vh) rotate(0deg) scale(0.5); opacity: 0; } 
  20% { opacity: 0.8; } 
  80% { opacity: 0.6; } 
  100% { transform: translateY(-20vh) rotate(360deg) scale(1.2); opacity: 0; } 
}
</style>
