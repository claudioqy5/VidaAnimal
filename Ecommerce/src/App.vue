<script setup>
import Catalog from './components/Catalog.vue'
</script>

<template>
  <div class="app-wrapper">
    <!-- Navbar Glassmorphism -->
    <nav class="navbar glass">
      <div class="container nav-content">
        <div class="logo">
          <span class="logo-icon">🐾</span>
          <span class="logo-text">Vida<b>Animal</b></span>
        </div>
        
        <ul class="nav-links">
          <li><a href="#" class="active">Tienda</a></li>
          <li><a href="https://wa.me/51975418965" target="_blank" class="btn-cta">WhatsApp</a></li>
        </ul>
      </div>
    </nav>

    <!-- Hero Section -->
    <header class="hero">
      <div class="container hero-grid">
        <div class="hero-text fade-in">
          <span class="badge">Bienvenido a la nueva experiencia</span>
          <h1>Calidad premium para los que <span>más quieres.</span></h1>
          <p>Encuentra los mejores alimentos y accesorios seleccionados por expertos para el bienestar de tus mascotas.</p>
          <div class="hero-actions">
            <button class="btn-primary">Explorar Tienda</button>
            <button class="btn-outline">Ver Ofertas</button>
          </div>
        </div>
        <div class="hero-image fade-in" style="animation-delay: 0.2s">
          <div class="image-wrapper glass">
             <img src="https://images.unsplash.com/photo-1548191265-cc70d3d45ba1?q=80&w=1000&auto=format&fit=crop" alt="Mascotas felices">
          </div>
        </div>
      </div>
    </header>

    <!-- Aquí irá el catálogo dinámico -->
    <main class="container">
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
  padding: 1rem 0;
}

.nav-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.5rem;
  font-weight: 500;
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
  transition: color 0.3s;
}

.nav-links a:hover, .nav-links a.active {
  color: var(--primary);
}

.btn-cta {
  background: var(--primary);
  color: white !important;
  padding: 0.6rem 1.5rem;
  border-radius: 50px;
  font-size: 0.9rem;
}

/* Hero Section */
.hero {
  padding: 160px 0 80px;
  background: radial-gradient(circle at top right, #EADBC8 0%, transparent 40%);
}

.hero-grid {
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  align-items: center;
  gap: 4rem;
}

.badge {
  display: inline-block;
  background: #EADBC8;
  color: #8B5E3C;
  padding: 0.5rem 1rem;
  border-radius: 50px;
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.hero-text h1 {
  font-size: 3.5rem;
  line-height: 1.1;
  margin-bottom: 1.5rem;
  color: var(--text-dark);
}

.hero-text h1 span {
  color: var(--primary);
}

.hero-text p {
  font-size: 1.1rem;
  color: var(--text-light);
  margin-bottom: 2.5rem;
  max-width: 500px;
}

.hero-actions {
  display: flex;
  gap: 1rem;
}

.btn-primary {
  background: var(--primary);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.3s, box-shadow 0.3s;
}

.btn-outline {
  background: transparent;
  color: var(--primary);
  border: 2px solid var(--primary);
  padding: 1rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
}

.btn-primary:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 20px rgba(45, 90, 39, 0.2);
}

.image-wrapper {
  padding: 1rem;
  border-radius: 30px;
  overflow: hidden;
}

.image-wrapper img {
  width: 100%;
  border-radius: 20px;
  display: block;
}

@media (max-width: 968px) {
  .hero-grid {
    grid-template-columns: 1fr;
    text-align: center;
  }
  .hero-text p {
    margin: 0 auto 2.5rem;
  }
  .hero-actions {
    justify-content: center;
  }
  .hero-text h1 {
    font-size: 2.5rem;
  }
}
</style>
