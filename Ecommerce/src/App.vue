<script setup>
import { ref, onMounted } from 'vue'
import Catalog from './components/Catalog.vue'
import AnimatedHero from './components/AnimatedHero.vue'

const isScrolled = ref(false)
const catalogSection = ref(null)

onMounted(() => {
  window.addEventListener('scroll', () => {
    isScrolled.value = window.scrollY > 50
  })
})

const scrollToCatalog = () => {
  window.scrollTo({
    top: window.innerHeight - 80,
    behavior: 'smooth'
  })
}
</script>

<template>
  <div class="app-wrapper">
    <!-- Navbar Glassmorphism con cambio de estado al scroll -->
    <nav class="navbar" :class="{ 'glass scrolled': isScrolled, 'transparent': !isScrolled }">
      <div class="container nav-content">
        <div class="logo">
          <span class="logo-icon">🐾</span>
          <span class="logo-text">Vida<b>Animal</b></span>
        </div>
        
        <ul class="nav-links">
          <li><a href="#" @click.prevent="scrollToCatalog">Tienda</a></li>
          <li><a href="#">Marcas</a></li>
          <li><a href="#">Nosotros</a></li>
        </ul>

        <div class="nav-actions">
          <div class="cart-trigger">
            <span class="cart-icon">🛒</span>
            <span class="cart-count">0</span>
          </div>
          <a href="https://wa.me/51975418965" target="_blank" class="btn-cta">WhatsApp</a>
        </div>
      </div>
    </nav>

    <!-- Nueva Composición Animada -->
    <AnimatedHero @explore="scrollToCatalog" />

    <!-- Catálogo de productos -->
    <main class="container main-content" ref="catalogSection">
       <Catalog />
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
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1.5rem;
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

.scrolled .logo {
  color: var(--primary);
}

.logo b {
  font-weight: 700;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 2rem;
}

.nav-links a {
  font-weight: 500;
  color: rgba(255, 255, 255, 0.9);
  transition: all 0.3s;
}

.scrolled .nav-links a {
  color: var(--text-dark);
}

.nav-links a:hover, .nav-links a.active {
  color: var(--secondary) !important;
}

.btn-cta {
  background: var(--primary);
  color: white !important;
  padding: 0.6rem 1.5rem;
  border-radius: 50px;
  font-size: 0.9rem;
}

.main-content {
  padding: 4rem 1.5rem;
}
</style>
