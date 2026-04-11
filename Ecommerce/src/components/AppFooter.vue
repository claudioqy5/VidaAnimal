<template>
  <footer class="app-footer fade-in">
    <div class="container footer-grid">
      
      <!-- Columna 1: Brand Info -->
      <div class="footer-col brand-info">
        <h2 class="footer-logo">Vida<span>Animal</span></h2>
        <p>Tu aliado de confianza en Aucayacu para la nutrición y el cuidado de los que más amas. Calidad premium para mascotas extraordinarias.</p>
        <div class="social-links">
          <a href="#" class="social-icon">Fb</a>
          <a href="#" class="social-icon">Ig</a>
          <a href="#" class="social-icon">Tk</a>
        </div>
      </div>

      <!-- Columna 2: Enlaces Rápidos -->
      <div class="footer-col">
        <h3>Explorar</h3>
        <ul class="footer-links">
          <li><a href="#" @click.prevent="$emit('go-home')">Inicio</a></li>
          <li><a href="#" @click.prevent="$emit('go-brands')">Marcas Aliadas</a></li>
          <li><a href="#" @click.prevent="$emit('go-map')">Ubicación</a></li>
          <li><a href="#" @click.prevent="$emit('go-about')">Nosotros</a></li>
        </ul>
      </div>

      <!-- Columna 3: Categorías -->
      <div class="footer-col">
        <h3>Mascotas</h3>
        <ul class="footer-links">
          <li><a href="#" @click.prevent="$emit('filter', 'Perro')">Perros</a></li>
          <li><a href="#" @click.prevent="$emit('filter', 'Gato')">Gatos</a></li>
          <li><a href="#" @click.prevent="$emit('filter', 'Pollo')">Pollos</a></li>
          <li><a href="#" @click.prevent="$emit('filter', 'others')">Otros</a></li>
        </ul>
      </div>

      <!-- Columna 4: Contacto -->
      <div class="footer-col">
        <h3>Contacto</h3>
        <div class="contact-info">
          <p>📍 Jiron Atahualpa N° 291</p>
          <p>Aucayacu, Perú</p>
          <p>📞 +51 975 418 965</p>
          <a href="https://wa.me/51975418965" target="_blank" class="footer-whatsapp">
            Chatear por WhatsApp
          </a>
        </div>
      </div>

    </div>

    <!-- Bottom Bar -->
    <div class="footer-bottom">
      <div class="container bottom-content">
        <p>&copy; 2026 Vida Animal. Todos los derechos reservados.</p>
        <div class="bottom-links">
          <a href="#">Términos y Condiciones</a>
          <a href="#">Privacidad</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup>
defineEmits(['go-home', 'go-brands', 'go-map', 'go-about', 'filter'])
</script>

<style scoped>
.app-footer {
  background: #261313;
  color: white;
  padding: 8rem 0 0;  
}

.footer-grid {
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr 1.2fr;
  gap: 4rem;
  padding-bottom: 6rem;
}

.footer-logo {
  font-size: 2.2rem;
  font-weight: 900;
  margin-bottom: 1.5rem;
}

.footer-logo span {
  color: var(--secondary);
}

.brand-info p {
  color: rgba(255,255,255,0.7);
  line-height: 1.8;
  margin-bottom: 2rem;
  font-size: 1rem;
}

.social-links {
  display: flex;
  gap: 1.2rem;
}

.social-icon {
  width: 40px;
  height: 40px;
  border: 1px solid rgba(255,255,255,0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  text-decoration: none;
  font-weight: 700;
  font-size: 0.8rem;
  transition: all 0.3s;
}

.social-icon:hover {
  background: var(--secondary);
  border-color: var(--secondary);
  transform: translateY(-5px);
}

h3 {
  font-size: 1.3rem;
  margin-bottom: 2.5rem;
  position: relative;
  font-weight: 800;
}

h3::after {
  content: '';
  position: absolute;
  bottom: -10px;
  left: 0;
  width: 30px;
  height: 2px;
  background: var(--secondary);
}

.footer-links {
  list-style: none;
  padding: 0;
}

.footer-links li {
  margin-bottom: 1rem;
}

.footer-links a {
  color: rgba(255,255,255,0.7);
  text-decoration: none;
  transition: all 0.3s;
  font-size: 1.05rem;
}

.footer-links a:hover {
  color: var(--secondary);
  padding-left: 10px;
}

.contact-info p {
  margin-bottom: 1rem;
  color: rgba(255,255,255,0.8);
}

.footer-whatsapp {
  display: inline-block;
  margin-top: 1rem;
  color: var(--secondary);
  text-decoration: none;
  font-weight: 800;
  border-bottom: 2px solid var(--secondary);
  padding-bottom: 5px;
}

.footer-bottom {
  border-top: 1px solid rgba(255,255,255,0.05);
  padding: 2.5rem 0;
  background: rgba(0,0,0,0.2);
}

.bottom-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: rgba(255,255,255,0.4);
  font-size: 0.9rem;
}

.bottom-links {
  display: flex;
  gap: 2rem;
}

.bottom-links a {
  color: inherit;
  text-decoration: none;
}

@media (max-width: 992px) {
  .footer-grid {
    grid-template-columns: 1fr 1fr;
    gap: 3rem;
  }
}

@media (max-width: 600px) {
  .footer-grid {
    grid-template-columns: 1fr;
  }
  .bottom-content {
    flex-direction: column;
    gap: 1.5rem;
    text-align: center;
  }
}
</style>
