<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import Catalog from './components/Catalog.vue'
import AnimatedHero from './components/AnimatedHero.vue'
import Features from './components/Features.vue'
import BrandsMarquee from './components/BrandsMarquee.vue'
import MapSection from './components/MapSection.vue'
import AboutSection from './components/AboutSection.vue'
import AppFooter from './components/AppFooter.vue'
import WhatsAppFloat from './components/WhatsAppFloat.vue'
import AppCart from './components/AppCart.vue'

// --- LÓGICA DEL CARRITO ---
const cart = ref(JSON.parse(localStorage.getItem('vida_animal_cart') || '[]'))

// Guardar en localStorage cada vez que el carrito cambie
watch(cart, (newCart) => {
  localStorage.setItem('vida_animal_cart', JSON.stringify(newCart))
}, { deep: true })

const addToCart = (product, quantity) => {
  const existing = cart.value.find(item => item.productoId === product.productoID || item.productoId === product.productoId)
  if (existing) {
    existing.quantity += quantity
  } else {
    cart.value.push({
      productoId: product.productoID || product.productoId,
      nombre: product.nombre,
      precio: product.precioVenta,
      imagen: product.imagenURL,
      quantity: quantity
    })
  }
}

const removeFromCart = (productId) => {
  cart.value = cart.value.filter(item => item.productoId !== productId)
}

const updateCartQty = (productId, newQty) => {
  if (newQty <= 0) {
    removeFromCart(productId)
    return
  }
  const item = cart.value.find(i => i.productoId === productId)
  if (item) {
    item.quantity = newQty
  }
}

const isCartOpen = ref(false)
const cartCount = computed(() => cart.value.reduce((acc, item) => acc + item.quantity, 0))

// --- NAVEGACIÓN Y FILTROS ---
const isScrolled = ref(false)
const selectedSpecies = ref('')
const selectedCategory = ref('')
const globalSearch = ref('')
const activeView = ref('home') 
const showSoonModal = ref(false) // Control para la ventana de "Pronto"

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

const openSoonModal = () => {
  showSoonModal.value = true
}

const scrollToBrands = async () => {
  if (activeView.value !== 'home') {
    activeView.value = 'home'
    // Limpiamos estados para volver al Home limpio
    selectedSpecies.value = ''
    selectedCategory.value = ''
    globalSearch.value = ''
  }
  
  await nextTick()
  const el = document.getElementById('marcas-aliadas')
  if (el) {
    el.scrollIntoView({ behavior: 'smooth' })
  }
}

const scrollToMap = async () => {
  if (activeView.value !== 'home') {
    activeView.value = 'home'
    selectedSpecies.value = ''
    selectedCategory.value = ''
    globalSearch.value = ''
  }
  
  await nextTick()
  const el = document.getElementById('ubicacion-central')
  if (el) {
    el.scrollIntoView({ behavior: 'smooth' })
  }
}

const scrollToAbout = async () => {
  if (activeView.value !== 'home') {
    activeView.value = 'home'
    selectedSpecies.value = ''
    selectedCategory.value = ''
    globalSearch.value = ''
  }
  
  await nextTick()
  const el = document.getElementById('sobre-nosotros')
  if (el) {
    el.scrollIntoView({ behavior: 'smooth' })
  }
}

const handleBrandFilter = (brand) => {
  activeView.value = 'catalog'
  globalSearch.value = brand.toLowerCase()
  selectedSpecies.value = ''
  selectedCategory.value = ''
  window.scrollTo({ top: 0, behavior: 'smooth' })
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

          <!-- DROPDOWN OTRAS MASCOTAS (ANIDADO) -->
          <div class="nav-item-dropdown">
            <a href="#" class="nav-link" @click.prevent="handleFilter('others', '')">OTRAS MASCOTAS</a>
            <div class="dropdown-menu">
              
              <!-- CUY -->
              <div class="nav-item-dropdown nested">
                <a href="#" @click.prevent="handleFilter('Cuy', '')" class="nested-link">CUY <span class="arrow-right">→</span></a>
                <div class="dropdown-menu flyout">
                  <a href="#" @click.prevent="handleFilter('Cuy', 'Medicina')">Medicina</a>
                  <a href="#" @click.prevent="handleFilter('Cuy', 'Alimento')">Alimentos</a>
                  <a href="#" @click.prevent="handleFilter('Cuy', 'Bebedero')">Bebedero</a>
                  <a href="#" @click.prevent="handleFilter('Cuy', 'Comederos')">Comederos / Platos</a>
                  <a href="#" @click.prevent="handleFilter('Cuy', 'Limpieza')">Limpieza</a>
                </div>
              </div>

              <!-- CHANCHO -->
              <div class="nav-item-dropdown nested">
                <a href="#" @click.prevent="handleFilter('Chancho', '')" class="nested-link">CHANCHO <span class="arrow-right">→</span></a>
                <div class="dropdown-menu flyout">
                  <a href="#" @click.prevent="handleFilter('Chancho', 'Medicina')">Medicina</a>
                  <a href="#" @click.prevent="handleFilter('Chancho', 'Alimento')">Alimentos</a>
                  <a href="#" @click.prevent="handleFilter('Chancho', 'Bebedero')">Bebedero</a>
                  <a href="#" @click.prevent="handleFilter('Chancho', 'Limpieza')">Limpieza</a>
                </div>
              </div>

              <!-- CONEJO -->
              <div class="nav-item-dropdown nested">
                <a href="#" @click.prevent="handleFilter('Conejo', '')" class="nested-link">CONEJO <span class="arrow-right">→</span></a>
                <div class="dropdown-menu flyout">
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Medicina')">Medicina</a>
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Ropa')">Ropa</a>
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Camas')">Camas</a>
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Alimento')">Alimentos</a>
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Bebedero')">Bebedero</a>
                  <a href="#" @click.prevent="handleFilter('Conejo', 'Juguetes')">Juguetes</a>
                </div>
              </div>

            </div>
          </div>

          <div class="nav-item-dropdown">
            <a href="#" @click.prevent="scrollToBrands">MARCAS</a>
            <div class="dropdown-menu brands-dropdown">
              <div class="brands-grid">
                <a 
                  v-for="brand in ['CORIMIX', 'CORINA', 'CORIPOLLO', 'CHICK MASTER', 'QUIMIVET', 'RINTISA', 'DIVETZA', 'OVET DEL PERÚ', 'RICOCAN', 'RICOCAT', 'RICOCRACK', 'MICHICAT', 'THOR', 'SUPERCAT', 'GUAU GUAU', 'PELUCHIN']" 
                  :key="brand" 
                  href="#" 
                  @click.prevent="scrollToBrands"
                >
                  {{ brand }}
                </a>
              </div>
            </div>
          </div>
          <a href="#" @click.prevent="openSoonModal">PELUQUERÍA</a>
          <a href="#" @click.prevent="scrollToMap">UBÍCANOS</a>
          <a href="#" @click.prevent="scrollToAbout">NOSOTROS</a>
        </nav>

        <div class="nav-actions">
          <div class="header-search glass">
            <span class="search-icon">🔍</span>
            <input type="text" v-model="globalSearch" placeholder="¿Qué buscas hoy?">
          </div>
          <div class="cart-trigger" @click="isCartOpen = true">
            <span class="cart-icon">🛒</span>
            <span class="cart-count">{{ cartCount }}</span>
          </div>
          <a href="https://wa.me/51975418965" target="_blank" class="btn-cta">Contáctanos</a>
        </div>
      </div>
    </nav>

    <!-- Composición Animada - SOLO EN HOME -->
    <AnimatedHero @explore="handleFilter('', '')" v-if="activeView === 'home'" />
    <Features v-if="activeView === 'home'" />
    <BrandsMarquee v-if="activeView === 'home'" />
    
    <!-- Mapa de Ubicación -->
    <MapSection v-if="activeView === 'home'" />

    <!-- Sobre Nosotros / Misión -->
    <AboutSection v-if="activeView === 'home'" />

    <!-- Catálogo de productos (Solo visible si no estamos en Home) -->
    <main v-if="activeView === 'catalog'" class="container main-content catalog-only">
       <Catalog 
          :species-filter="selectedSpecies" 
          :category-filter="selectedCategory" 
          :search-query="globalSearch"
          @add-to-cart="addToCart"
       />
    </main>

    <!-- Modal de "Pronto" -->
    <Teleport to="body">
      <div v-if="showSoonModal" class="modal-overlay" @click="showSoonModal = false">
        <div class="modal-content glass" @click.stop>
          <div class="modal-header">
            <span class="soon-icon">✂️</span>
            <button class="close-btn" @click="showSoonModal = false">×</button>
          </div>
          <h2>¡Peluquería Vida Animal!</h2>
          <p>Estamos preparando las mejores tijeras y el shampoo más espumoso para consentir a tus mascotas.</p>
          <div class="soon-badge">PRÓXIMAMENTE</div>
          <p class="stay-tuned">Mantente atento a nuestras redes sociales.</p>
          <button class="btn-cta" @click="showSoonModal = false">¡Entendido!</button>
        </div>
      </div>
    </Teleport>

    <AppFooter 
      @go-home="goHome"
      @go-brands="scrollToBrands"
      @go-map="scrollToMap"
      @go-about="scrollToAbout"
      @filter="(spec) => handleFilter(spec, '')"
    />

    <!-- Botón de WhatsApp Flotante (Solo visible si el carrito está cerrado) -->
    <WhatsAppFloat v-if="!isCartOpen" />

    <!-- Panel del Carrito -->
    <AppCart 
      :is-open="isCartOpen"
      :items="cart"
      @close="isCartOpen = false"
      @remove="removeFromCart"
      @update-qty="updateCartQty"
    />
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
background: rgba(38, 19, 19, 0.6);
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
  color: white; /* Letras blancas al escribir */
}

.header-search input::placeholder {
  color: rgba(255, 255, 255, 0.8) !important; /* Ahora mucho más claro */
  opacity: 1;
}

.header-search:focus-within input {
  color: #333; /* Color oscuro cuando el fondo es blanco */
}

.header-search:focus-within input::placeholder {
  color: #999;
}

.header-search .search-icon {
  font-size: 0.9rem;
  opacity: 1;
}

.scrolled .header-search, .nav-dark .header-search {
  background: rgba(0,0,0,0.05);
  border-color: rgba(0,0,0,0.1);
}

.scrolled .header-search input, .nav-dark .header-search input {
  /* color: var(--text-dark); REMOVIDO para mantenerlo blanco */
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
  background: rgba(38, 19, 19, 0.6);
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

.nav-item-dropdown:hover > .dropdown-menu {
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

/* Mega Menú Anidado (Flyout) */
.nav-item-dropdown.nested {
  width: 100%;
}

.nested-link {
  display: flex !important;
  justify-content: space-between;
  align-items: center;
}

.arrow-right {
  font-size: 0.8rem;
  opacity: 0.5;
  transition: transform 0.3s;
}

.nav-item-dropdown.nested:hover .arrow-right {
  transform: translateX(5px);
  color: var(--secondary);
}

.dropdown-menu.flyout {
  top: -1px;
  left: 100%;
  transform: translateX(15px) translateY(0) !important;
  margin-left: 2px;
  opacity: 0; /* Asegurar que empiece oculto */
  visibility: hidden;
}

/* SOLO mostrar el flyout cuando se hace hover específico en su contenedor .nested */
.nav-item-dropdown.nested:hover > .dropdown-menu.flyout {
  opacity: 1;
  visibility: visible;
  transform: translateX(0) translateY(0) !important;
}

/* Estilos Específicos para el Dropdown de Marcas */
.brands-dropdown {
  min-width: 450px;
  padding: 2rem !important;
}

.brands-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px;
}

.brands-grid a {
  padding: 0.5rem 1rem !important;
  font-size: 0.8rem !important;
  border-left: none !important;
  color: rgba(255, 255, 255, 0.7) !important;
}

.brands-grid a:hover {
  color: var(--secondary) !important;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
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
/* Estilos del Modal de Peluquería */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.6);
  backdrop-filter: blur(8px);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-content {
  background: white;
  padding: 3rem;
  border-radius: 24px;
  max-width: 500px;
  width: 90%;
  text-align: center;
  position: relative;
  box-shadow: 0 40px 100px rgba(0,0,0,0.2);
  animation: slideUp 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}

@keyframes slideUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.soon-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
  display: block;
}

.close-btn {
  position: absolute;
  top: 1.5rem;
  right: 2rem;
  background: none;
  border: none;
  font-size: 2.5rem;
  color: #ccc;
  cursor: pointer;
  transition: color 0.3s;
}

.close-btn:hover {
  color: var(--primary);
}

.modal-content h2 {
  font-size: 2.2rem;
  color: var(--primary);
  margin-bottom: 1.2rem;
  font-weight: 800;
}

.modal-content p {
  color: var(--text-light);
  line-height: 1.6;
  margin-bottom: 2rem;
}

.soon-badge {
  display: inline-block;
  background: var(--secondary);
  color: white;
  padding: 0.5rem 1.5rem;
  border-radius: 50px;
  font-weight: 900;
  font-size: 0.8rem;
  letter-spacing: 2px;
  margin-bottom: 2rem;
}

.stay-tuned {
  font-weight: 600;
  color: var(--text-dark) !important;
  font-size: 0.9rem !important;
}

.modal-content .btn-cta {
  width: 100%;
  padding: 1rem;
  cursor: pointer;
  border: none;
}
</style>
