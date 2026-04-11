<template>
  <section id="marcas-aliadas" class="brands-section fade-in">
    <div class="container marquee-header">
      <h2>Nuestras Marcas Aliadas</h2>
      <p class="section-description">Trabajamos con las mejores marcas del mercado para garantizar la nutrición y el cuidado de la más alta calidad que tus mascotas merecen.</p>
      <div class="header-line"></div>
    </div>

    <div class="marquee-wrapper">
      <!-- FILA 1 -->
      <div class="marquee-row">
        <div class="marquee-content">
          <div v-for="(logo, index) in row1" :key="'r1-'+index" class="brand-item">
            <img :src="logo" alt="Marca Aliada" class="brand-logo">
          </div>
          <!-- Duplicado para loop -->
          <div v-for="(logo, index) in row1" :key="'r1-dup-'+index" class="brand-item">
            <img :src="logo" alt="Marca Aliada" class="brand-logo">
          </div>
        </div>
      </div>

      <!-- FILA 2 -->
      <div class="marquee-row reverse">
        <div class="marquee-content">
          <div v-for="(logo, index) in row2" :key="'r2-'+index" class="brand-item">
            <img :src="logo" alt="Marca Aliada" class="brand-logo">
          </div>
          <!-- Duplicado para loop -->
          <div v-for="(logo, index) in row2" :key="'r2-dup-'+index" class="brand-item">
            <img :src="logo" alt="Marca Aliada" class="brand-logo">
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
// Importación de Logos
import logoChick from '../assets/marcas/chickmaster.jpg'
import logoCorimix from '../assets/marcas/corimix.JPG'
import logoCorina from '../assets/marcas/corina.jpg'
import logoCoripollo from '../assets/marcas/coripollo.png'
import logoGuau from '../assets/marcas/guauguau.png'
import logoOvet from '../assets/marcas/ovet.png'
import logoPeluchin from '../assets/marcas/peluchin.png'
import logoQuimivet from '../assets/marcas/quimivet.jpg'
import logoRicocan from '../assets/marcas/ricocan.jpg'
import logoRicocat from '../assets/marcas/ricocat.jpg'
import logoRicocrack from '../assets/marcas/ricocrack.jpg'
import logoRintisa from '../assets/marcas/rintisa.png'
import logoSupercat from '../assets/marcas/supercat.png'
import logoThor from '../assets/marcas/thor.png'
import logoDivetza from '../assets/marcas/michicat.jpg'

const row1 = [logoCorimix, logoCorina, logoCoripollo, logoChick, logoQuimivet, logoRintisa, logoOvet, logoDivetza]
const row2 = [logoRicocan, logoRicocat, logoRicocrack, logoThor, logoSupercat, logoGuau, logoPeluchin, logoCorimix]
</script>

<style scoped>
.brands-section {
  padding: 7rem 0;
  background: transparent;
  overflow: hidden;
}

.marquee-header {
  text-align: center;
  margin-bottom: 3rem;
}

.marquee-header h2 {
  font-size: 1.8rem;
  color: var(--primary);
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 2px;
}

.section-description {
  color: var(--text-light);
  font-size: 1.1rem;
  max-width: 700px;
  margin: 1rem auto 0;
  line-height: 1.6;
}

.header-line {
  width: 60px;
  height: 4px;
  background: var(--secondary);
  margin: 1.5rem auto;
  border-radius: 10px;
}

.marquee-wrapper {
  position: relative;
  width: 100%;
  /* Máscara de desvanecimiento lateral */
  mask-image: linear-gradient(
    to right, 
    transparent, 
    black 15%, 
    black 85%, 
    transparent
  );
  -webkit-mask-image: linear-gradient(
    to right, 
    transparent, 
    black 15%, 
    black 85%, 
    transparent
  );
}

.marquee-row {
  display: flex;
  margin-bottom: 2rem;
}

.marquee-content {
  display: flex;
  gap: 3rem;
  animation: scroll 40s linear infinite;
  white-space: nowrap;
}

.reverse .marquee-content {
  animation: scroll-reverse 40s linear infinite;
}

.brand-item {
  padding: 1.2rem 2.8rem;
  background: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 5px 25px rgba(61, 30, 30, 0.05);
  border: 1px solid rgba(0,0,0,0.03);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  cursor: default;
  min-width: 200px;
}

.brand-logo {
  height: 50px;
  max-width: 150px;
  object-fit: contain;
  transition: all 0.4s ease;
}

.brand-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 35px rgba(61, 30, 30, 0.1);
  border-color: var(--secondary);
}

.brand-item:hover .brand-logo {
  transform: scale(1.1);
}

@keyframes scroll {
  from { transform: translateX(0); }
  to { transform: translateX(-50%); }
}

@keyframes scroll-reverse {
  from { transform: translateX(-50%); }
  to { transform: translateX(0); }
}

/* Modero la velocidad en móviles */
@media (max-width: 768px) {
  .marquee-content {
    animation-duration: 25s;
    gap: 1.5rem;
  }
  .brand-item {
    padding: 0.8rem 1.5rem;
    font-size: 0.9rem;
  }
}
</style>
