<template>
  <!-- Si NO hay usuario, mostramos el Login -->
  <Login v-if="!usuarioLogueado" @login-success="iniciarSesion" />

  <!-- Si SÍ hay usuario, mostramos la App normal -->
  <div v-else class="app-layout">
    <Sidebar :activeTab="currentTab" :usuario="usuarioLogueado" @change-tab="tab => currentTab = tab" @logout="cerrarSesion" />
    
    <main class="main-content">
      <!-- Header removido para maximizar espacio vertical -->

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
      <!-- Corazones Flotantes (En el fondo) -->
      <div class="floating-hearts">
        <span class="heart h1">❤️</span>
        <span class="heart h2">💖</span>
        <span class="heart h3">🐾</span>
        <span class="heart h4">✨</span>
        <span class="heart h5">💕</span>
        <span class="heart h6">💘</span>
      </div>

      <div class="love-content">
        <div class="love-layout">
          <!-- Lado Izquierdo: Titulo + Mensaje Editorial -->
          <div class="love-left-panel">
            <h1 class="love-title">
              ¡FELIZ<br>MESARIOOO,<br>{{ primerNombre }}!
            </h1>
            
            <div class="love-divider"></div>

            <div class="love-letter">
              <p class="love-text-main">
                ¡Feliz mesario amorcitooooo, hoy celebramos un mesecito más! 👩‍❤️‍👨
              </p>                        
              <p class="love-text-sub">              
                No queria que este día tan especial siga pasando desapercibido, sabes que te amo mucho perita, se que no estamos juntos fisicamente pero te quiero mandar en enorme beso y abrazo con todo mi corazón.
                <br>  
                <br>
                Y tambien quiero reconocer el esfuerzo que haces todos los dias para sacar adelante el proyecto en el que estamos encaminados mascota              
                enserio, al parecer todo está yendo por buen camino, estamos haciendo las cosas de corazón y cada esfuerzo, vale la pena.
              </p>
              <p class="love-text-highlight">
                Somos el mejor equipo mi gordi, en el amor y en la vida. ¡Te amo infinitamente! 🍐❤️
              </p>
              
              <button class="love-btn" @click="cerrarAniversario">
                <span>Continuar con <span style="color: #eb6856ff;">VIDA ANIMAL</span></span>
                <svg class="btn-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M14 5l7 7m0 0l-7 7m7-7H3" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Lado Derecho: Galería Editorial Minimalista (Desde arriba) -->
          <div class="love-gallery-panel">
            <div v-if="fotosAniversario.length > 0" class="gallery-grid">
              <div v-for="(foto, idx) in fotosAniversario" :key="idx" class="gallery-item">
                <img :src="foto" alt="Nuestro recuerdo hermoso" />
              </div>
            </div>
          </div>
        </div>
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
  if (hoy.getDate() === 22) {
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
  margin-left: 5.5rem;
  padding: 1rem 1.5rem;
  max-width: 100%;
  overflow-x: hidden;
  transition: margin-left 0.4s cubic-bezier(0.25, 1, 0.5, 1);
}

.app-layout:has(.sidebar:hover) .main-content {
  margin-left: 16rem;
}

/* Se eliminó el CSS del top-header porque se movió al Sidebar */

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

/* Pantalla Completa Aniversario Amor (Editorial Minimalist Design) */
.fullscreen-love-modal {
  position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
  background-color: #77a362;
  display: flex; align-items: flex-start; justify-content: center; z-index: 10000;
  overflow-y: auto;
  padding: 3rem 2rem;
}

/* Custom scrollbar matching the green aesthetic */
.fullscreen-love-modal::-webkit-scrollbar {
  width: 6px;
}
.fullscreen-love-modal::-webkit-scrollbar-track {
  background: #77a362;
}
.fullscreen-love-modal::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.35);
  border-radius: 3px;
}
.fullscreen-love-modal::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.6);
}

.love-content {
  width: 100%;
  max-width: 1350px;
  margin: auto 0;
  display: flex;
  flex-direction: column;
  text-align: left;
}

.love-left-panel {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  text-align: left;
}

.love-title {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  font-size: clamp(3rem, 7.5vw, 14vh);
  font-weight: 500;
  text-transform: uppercase;
  color: #931b1b; /* Vibrant rich red */
  line-height: 0.85;
  letter-spacing: -0.05em;
  margin: 0 0 1.5rem 0;
  text-shadow: none;
}

.love-divider {
  height: 1px;
  background-color: #931b1b;
  opacity: 0.25;
  width: 100%;
  margin-bottom: 2rem;
}

.love-layout {
  display: grid;
  grid-template-columns: 1.15fr 1fr;
  gap: 4rem;
  align-items: start;
}

@media (max-width: 900px) {
  .love-layout {
    grid-template-columns: 1fr;
    gap: 3rem;
  }
}

.love-letter {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  text-align: left;
}

.love-text-main {
  font-family: 'Inter', sans-serif;
  font-size: clamp(1.25rem, 2vw, 1.75rem);
  font-weight: 700;
  line-height: 1.35;
  color: #1A1A1A;
  margin: 0;
}

.love-text-sub {
  font-family: 'Inter', sans-serif;
  font-size: 1.05rem;
  font-weight: 400;
  line-height: 1.65;
  color: #4A4A4A;
  margin: 0;
}

.love-text-highlight {
  font-family: 'Inter', sans-serif;
  font-size: 1.15rem;
  font-weight: 700;
  color: #931b1b;  
  margin: 0;
}

.love-btn {
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  background: #1A1A1A;
  color: #F4F3EF;
  border: none;
  padding: 1.25rem 2rem;
  border-radius: 0; /* Minimalist sharp corners */
  font-weight: 700;
  font-size: 0.95rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  width: fit-content;
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.love-btn:hover {
  background: #C53030;
  color: #ffffff;
  transform: translateY(-2px);
}

.btn-arrow {
  width: 18px;
  height: 18px;
  stroke: currentColor;
  transition: transform 0.3s ease;
}

.love-btn:hover .btn-arrow {
  transform: translateX(6px);
}

.love-gallery-panel {
  width: 100%;
}

.gallery-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.gallery-item {
  aspect-ratio: 1 / 1;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.08);
  background-color: #E2E2DF;
  transition: transform 0.5s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.5s ease;
  position: relative;
}

.gallery-item:first-child {
  grid-column: span 2;
  aspect-ratio: 2 / 1; /* Beautiful double-width hero image for standard 5-photo layouts */
}

.gallery-item:hover {
  transform: scale(1.02);
  z-index: 2;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

.gallery-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: filter 0.5s ease, transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  filter: grayscale(15%) contrast(102%);
}

.gallery-item:hover img {
  filter: grayscale(0%) contrast(100%);
  transform: scale(1.03);
}

/* Corazones Flotantes en el Fondo (Con opacidad reducida para no competir con el diseño minimalista) */
.floating-hearts {
  position: absolute;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  pointer-events: none;
  z-index: 0;
  overflow: hidden;
}

.heart {
  position: absolute;
  opacity: 0;
  animation: floatUpUp 8s linear infinite;
  filter: drop-shadow(0 0 10px rgba(0, 0, 0, 0.03));
}

.h1 { left: 10%; animation-delay: 0s; font-size: 3.5rem; animation-duration: 9s;}
.h2 { left: 30%; animation-delay: 2s; font-size: 2.2rem; animation-duration: 7s;}
.h3 { left: 75%; animation-delay: 4s; font-size: 4.5rem; animation-duration: 11s;}
.h4 { left: 85%; animation-delay: 1s; font-size: 2.8rem; animation-duration: 8s;}
.h5 { left: 50%; animation-delay: 5s; font-size: 1.8rem; animation-duration: 6s;}
.h6 { left: 20%; animation-delay: 6s; font-size: 3rem; animation-duration: 10s;}

@keyframes floatUpUp { 
  0% { transform: translateY(105vh) rotate(0deg) scale(0.6); opacity: 0; } 
  15% { opacity: 0.35; } 
  85% { opacity: 0.25; } 
  100% { transform: translateY(-20vh) rotate(360deg) scale(1.1); opacity: 0; } 
}
</style>
