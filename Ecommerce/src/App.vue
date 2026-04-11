<script setup>
import { ref, onMounted, watch } from 'vue'
import Catalog from './components/Catalog.vue'
import AnimatedHero from './components/AnimatedHero.vue'

const isScrolled = ref(false)
const selectedSpecies = ref('')
const selectedCategory = ref('')
const globalSearch = ref('')
const activeView = ref('home') 

// Al buscar en el header, forzamos la vista de catálogo
watch(globalSearch, (newVal) => {
  if (newVal.length > 0 && activeView.value === 'home') {
    activeView.value = 'catalog'
  }
})

onMounted(() => {
  window.addEventListener('scroll', () => {
    isScrolled.value = window.scrollY > 50
  })
})

const handleFilter = (species, category) => {
  selectedSpecies.value = species
  selectedCategory.value = category
  globalSearch.value = '' // Limpiamos búsqueda al usar filtros de menú
  activeView.value = 'catalog' 
  window.scrollTo(0, 0) 
}

const goHome = () => {
  activeView.value = 'home'
  selectedSpecies.value = ''
  selectedCategory.value = ''
  globalSearch.value = ''
  window.scrollTo(0, 0)
}
</script>

<template>
  <div class="app-wrapper">
    <!-- Navbar Glassmorphism con cambio de estado al scroll -->
    <nav class="navbar" :class="{ 
      'glass scrolled': isScrolled, 
      'transparent': !isScrolled && activeView === 'home',
      'nav-dark': activeView === 'catalog'
    }">
      <div class="container nav-content">
        <div class="logo" @click="goHome" style="cursor: pointer">
          <span class="logo-icon">🐾</span>
          <span class="logo-text">Vida<b>Animal</b></span>
        </div>
        
        <nav class="nav-links">
          <!-- DROPDOWN PERROS -->
          <div class="nav-item-dropdown">
            <a href="#" class="nav-link" @click.prevent="handleFilter('Perro', '')">PERROS</a>
            <div class="dropdown-menu">
              <a href="#" @click.prevent="handleFilter('Perro', 'Medicina')">Medicina</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Ropa')">Ropa</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Camas')">Camas</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Alimento')">Alimentos</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Bebedero')">Bebedero</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Juguetes')">Juguetes</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Comederos')">Comederos / Platos</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Collares')">Collares</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Limpieza')">Limpieza</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Arnes')">Arnes / Correas</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Bozales')">Bozales</a>
              <a href="#" @click.prevent="handleFilter('Perro', 'Accesorios')">Accesorios</a>
            </div>
          </div>

          <!-- DROPDOWN GATOS -->
          <div class="nav-item-dropdown">
            <a href="#" class="nav-link" @click.prevent="handleFilter('Gato', '')">GATOS</a>
            <div class="dropdown-menu">
              <a href="#" @click.prevent="handleFilter('Gato', 'Medicina')">Medicina</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Ropa')">Ropa</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Camas')">Camas</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Alimento')">Alimentos</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Bebedero')">Bebedero</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Juguetes')">Juguetes</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Comederos')">Comederos / Platos</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Collares')">Collares</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Limpieza')">Limpieza</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Arnes')">Arnes / Correas</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Bozales')">Bozales</a>
              <a href="#" @click.prevent="handleFilter('Gato', 'Accesorios')">Accesorios</a>
            </div>
          </div>

          <!-- DROPDOWN POLLOS -->
          <div class="nav-item-dropdown">
            <a href="#" class="nav-link" @click.prevent="handleFilter('Pollo', '')">POLLO</a>
            <div class="dropdown-menu">
              <a href="#" @click.prevent="handleFilter('Pollo', 'Medicina')">Medicina</a>
              <a href="#" @click.prevent="handleFilter('Pollo', 'Alimento')">Alimentos</a>
              <a href="#" @click.prevent="handleFilter('Pollo', 'Bebedero')">Bebedero</a>
              <a href="#" @click.prevent="handleFilter('Pollo', 'Comederos')">Comederos / Platos</a>
              <a href="#" @click.prevent="handleFilter('Pollo', 'Limpieza')">Limpieza</a>
              <a href="#" @click.prevent="handleFilter('Pollo', 'Accesorios')">Accesorios</a>
            </div>
          </div>

          <a href="#">OTRAS MASCOTAS</a>
          <a href="#">MARCAS</a>
          <a href="#">PELUQUERÍA</a>
          <a href="#">UBÍCANOS</a>
          <a href="#">NOSOTROS</a>
        </nav>

        <div class="nav-actions">
          <div class="header-search glass">
            <span class="search-icon">🔍</span>
            <input type="text" v-model="globalSearch" placeholder="¿Qué buscas hoy?">
          </div>
          <div class="cart-trigger">
            <span class="cart-icon">🛒</span>
            <span class="cart-count">0</span>
          </div>
          <a href="https://wa.me/51975418965" target="_blank" class="btn-cta">WhatsApp</a>
        </div>
      </div>
    </nav>

    <!-- Composición Animada - SOLO EN HOME -->
    <AnimatedHero @explore="handleFilter('', '')" v-if="activeView === 'home'" />

    <!-- Catálogo de productos -->
    <main class="container main-content" :class="{ 'catalog-only': activeView === 'catalog' }">
       <Catalog 
          :species-filter="selectedSpecies" 
          :category-filter="selectedCategory" 
          :search-query="globalSearch"
       />
    </main>

    <footer class="footer">
      <div class="container footer-content">
         <p>&copy; 2026 Vida Animal. Todos los derechos reservados.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.navbar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  z-index: 1000;
  padding: 1.5rem 0;
  transition: all 0.4s ease;
}

.navbar.transparent {
  background: transparent;
  box-shadow: none;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.navbar.scrolled {
  padding: 1rem 0;
  background: var(--glass);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0,0,0,0.05);
}

.nav-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.header-search {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 50px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  width: 180px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.header-search:focus-within {
  width: 240px;
  background: white;
  border-color: var(--primary);
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}

.header-search input {
  background: transparent;
  border: none;
  outline: none;
  width: 100%;
  font-family: inherit;
  font-size: 0.85rem;
  color: white;
}

.header-search .search-icon {
  font-size: 0.9rem;
  opacity: 0.8;
}

.scrolled .header-search, .nav-dark .header-search {
  background: rgba(0,0,0,0.05);
  border-color: rgba(0,0,0,0.1);
}

.scrolled .header-search input, .nav-dark .header-search input {
  color: var(--text-dark);
}

.scrolled .header-search:focus-within, .nav-dark .header-search:focus-within {
  background: white;
}

.cart-trigger {
  position: relative;
  cursor: pointer;
  font-size: 1.4rem;
  transition: transform 0.2s;
}

.cart-trigger:hover {
  transform: scale(1.1);
}

.cart-count {
  position: absolute;
  top: -5px;
  right: -8px;
  background: var(--secondary);
  color: white;
  font-size: 0.7rem;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  border: 2px solid white;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.5rem;
  font-weight: 500;
  color: white; /* Color inicial en hero oscuro */
  transition: color 0.4s;
}

.navbar.nav-dark {
  background: #111;
  box-shadow: 0 4px 20px rgba(0,0,0,0.1);
}

.scrolled .logo, .nav-dark .logo {
  color: #fff;
}

.nav-dark .logo b {
  color: var(--primary);
}

.logo b {
  font-weight: 700;
}

.nav-links {
  display: flex;
  gap: 2.5rem;
  align-items: center;
  margin-left: 2rem;
  margin-right: 2rem;
}

.nav-links a {
  text-decoration: none;
  color: white;
  font-weight: 700;
  font-size: 0.85rem;
  letter-spacing: 1.5px;
  transition: all 0.3s;
  text-transform: uppercase;
}

/* Estilos Dropdown */
.nav-item-dropdown {
  position: relative;
  display: flex;
  align-items: center;
}

.arrow {
  font-size: 0.7rem;
  margin-left: 4px;
  transition: transform 0.3s;
}

.nav-item-dropdown:hover .arrow {
  transform: rotate(180deg);
  color: var(--secondary);
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%) translateY(20px);
  background: rgba(18, 18, 18, 0.9);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 1.5rem 0;
  border-radius: 12px;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 40px rgba(0,0,0,0.5);
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
  z-index: 1000;
}

.nav-item-dropdown:hover .dropdown-menu {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(10px);
}

.dropdown-menu a {
  padding: 0.8rem 2rem;
  width: 100%;
  text-align: left;
  border-left: 3px solid transparent;
}

.dropdown-menu a:hover {
  background: rgba(255, 255, 255, 0.05);
  color: var(--secondary) !important;
  border-left: 3px solid var(--secondary);
}

.nav-links a:hover {
  color: var(--secondary);
}

.nav-links a:hover, .nav-links a.active {
  color: var(--secondary) !important;
}

.btn-cta {
  background: var(--whatsapp);
  color: white !important;
  padding: 0.6rem 1.5rem;
  border-radius: 50px;
  font-size: 0.9rem;
  font-weight: 600;
  transition: all 0.3s;
}

.btn-cta:hover {
  background: #1e3d1a;
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(37, 211, 102, 0.2);
}

.main-content {
  padding: 4rem 1.5rem;
  transition: padding 0.5s ease;
}

.main-content.catalog-only {
  padding-top: 120px; /* Espacio para el header cuando no hay hero */
}
</style>
