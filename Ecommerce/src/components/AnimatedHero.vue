<script setup>
import { onMounted, ref } from 'vue'

const isLoaded = ref(false)

onMounted(() => {
  // Pequeño delay para asegurar que el navegador esté listo para animar
  setTimeout(() => {
    isLoaded.value = true
  }, 100)
})
</script>

<template>
  <section class="animated-hero" :class="{ 'start-animation': isLoaded }">
    <!-- Fondo Total -->
    <div class="layer layer-bg">
      <img src="../assets/fondototal.png" alt="Fondo" class="full-img">
    </div>

    <!-- Imagen Izquierda -->
    <div class="layer layer-left">
      <img src="../assets/ladoizquierdo.png" alt="Decoración Izquierda">
    </div>

    <!-- Imagen Derecha -->
    <div class="layer layer-right">
      <img src="../assets/ladoderecho.png" alt="Decoración Derecha">
    </div>

    <!-- Imagen Centro -->
    <div class="layer layer-center">
      <img src="../assets/centro.png" alt="Centro" class="lift-img">
    </div>

    <div class="hero-overlay fade-in" v-if="isLoaded">
      <div class="container welcome-container">        
        <div class="hero-actions-box">
          <p>Nutrición y cuidado de alta gama.</p>
          <button class="btn-primary" @click="$emit('explore')">Comenzar a Comprar</button>
        </div>
        <div class="massive-text">
          <h1>VIDA<br>ANIMAL</h1>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.animated-hero {
  position: relative;
  width: 100%;
  height: 100vh;
  overflow: hidden;
  background-color: #000; /* Fondo de seguridad */
}

.layer {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none; /* Las imágenes no bloquean clicks */
}

.full-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: brightness(0.65); /* Oscurece el fondo para que resalten las letras */
}

/* Animación: Lado Izquierdo */
.layer-left {
  transform: translateX(-100%);
  transition: transform 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  justify-content: flex-start;
  z-index: 10;
  max-height: 100%; /* Más grande como en la referencia */
}

.layer-left img {
  height: 100%;
  width: auto;
  filter: brightness(0.65); /* Oscurece la imagen izquierda */
}

.start-animation .layer-left {
  transform: translateX(0);
}

/* Animación: Lado Derecho */
.layer-right {
  transform: translateX(100%);
  transition: transform 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  justify-content: flex-end;
  z-index: 10;
}

.layer-right img {
  height: 100%;
  width: auto;
  filter: brightness(0.65); /* Oscurece la imagen derecha */
}

.start-animation .layer-right {
  transform: translateX(0);
}

/* Animación: Centro (Levantamiento) */
.layer-center {
  transform: translateY(100vh); /* Empieza completamente abajo (suelo) */
  opacity: 0;
  transition: all 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  transition-delay: 1s; /* Espera 2.5 segundos para aparecer */
  z-index: 20;
}

.layer-center img {
  max-height: 110%; /* Más grande como en la referencia */
  width: auto;
  object-fit: contain;
  filter: brightness(0.65); /* Oscurece también la imagen central */
}

.start-animation .layer-center {
  transform: translateY(0) scale(1.05); /* Ligeramente más grande al final */
  opacity: 1;
}

/* Texto sobre el diseño */
.hero-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  z-index: 30;
  background: linear-gradient(to right, rgba(0,0,0,0.5) 0%, transparent 70%);
}

.welcome-container {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: flex-start;
  height: 100%;
}

.massive-text {
  opacity: 0;
  animation: fadeInText 1.5s forwards 3s;
}

.massive-text h1 {
  font-size: clamp(5rem, 15vw, 15rem); /* Gigante y responsivo */
  color: white;
  line-height: 0.85;
  font-weight: 900;
  letter-spacing: -0.04em;
  text-transform: uppercase;
  margin-bottom: 2rem;
  text-shadow: 0 10px 30px rgba(0,0,0,0.5);
}

.hero-actions-box {
  opacity: 0;
  animation: fadeInText 1s forwards 4s;
}

.hero-actions-box p {
  color: #f0f0f0;
  font-size: 1.5rem;
  margin-bottom: 2rem;
  font-weight: 500;
}

@keyframes fadeInText {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 1024px) {
  .welcome-text h1 { font-size: 2.5rem; }
  .layer-left, .layer-right { opacity: 0.5; } /* Menos distracción en móvil */
}
</style>
