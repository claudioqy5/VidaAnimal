<script setup>
import { onMounted, ref } from 'vue'

const isLoaded = ref(false)
const displayText = ref('')
const phrases = [
  "en un solo lugar.",
  "para vivir cómodos y felices.",
  "al mejor precio.",
  "con calidad y cariño."
]

const baseText = "Todo lo que ellos necesitan, "
let phraseIndex = 0
let charIndex = 0
let isDeleting = false
let typeSpeed = 100

const typeEffect = () => {
  const currentPhrase = phrases[phraseIndex]
  
  if (isDeleting) {
    displayText.value = baseText + currentPhrase.substring(0, charIndex - 1)
    charIndex--
    typeSpeed = 50
  } else {
    displayText.value = baseText + currentPhrase.substring(0, charIndex + 1)
    charIndex++
    typeSpeed = 100
  }

  if (!isDeleting && charIndex === currentPhrase.length) {
    isDeleting = true
    typeSpeed = 2000 // Tiempo que se queda la frase completa
  } else if (isDeleting && charIndex === 0) {
    isDeleting = false
    phraseIndex = (phraseIndex + 1) % phrases.length
    typeSpeed = 500
  }

  setTimeout(typeEffect, typeSpeed)
}

onMounted(() => {
  setTimeout(() => {
    isLoaded.value = true
    typeEffect()
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
          <p class="typewriter-text">{{ displayText }}<span class="cursor">|</span></p>          
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
  filter: brightness(0.45); /* Más oscuro para máximo contraste */
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
  filter: brightness(0.45); /* Oscurece la imagen izquierda */
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
  filter: brightness(0.45); /* Oscurece la imagen derecha */
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
  filter: brightness(0.45); /* Oscurece también la imagen central */
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
  justify-content: flex-end; /* Lo manda al fondo */
  align-items: flex-start;  /* Lo mantiene a la izquierda */
  height: 100%;
  padding-bottom: 2rem;     /* Más cerca del suelo como en la captura */
  margin-left: 5rem;
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
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin-bottom: 4rem; /* Separación de las letras gigantes */
}

/* El botón con 'Vida' */
.btn-primary {
  background: linear-gradient(135deg, var(--primary) 0%, #3e7a36 100%);
  color: white;
  border: none;
  padding: 1.2rem 2.5rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  width: fit-content;
  box-shadow: 0 10px 25px rgba(45, 90, 39, 0.4);
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  animation: pulse-glow 2s infinite 5s; /* Empieza a pulsar después de salir */
  text-transform: uppercase;
  letter-spacing: 1px;
}

.btn-primary:hover {
  transform: translateY(-5px) scale(1.05);
  box-shadow: 0 15px 30px rgba(45, 90, 39, 0.6);
  filter: brightness(1.1);
}

.hero-actions-box p {
  color: #fff;
  font-size: 1.1rem;
  font-weight: 500;
  letter-spacing: 2px;
  text-transform: uppercase;
  opacity: 0.9;
  border-left: 3px solid var(--secondary);
  padding-left: 1rem;
  min-height: 1.5em; /* Evita que el layout salte */
}

.cursor {
  color: var(--secondary);
  animation: blink 0.8s infinite;
  margin-left: 5px;
  font-weight: 900;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0; }
}

@keyframes pulse-glow {
  0% { box-shadow: 0 0 0 0 rgba(45, 90, 39, 0.7); }
  70% { box-shadow: 0 0 0 15px rgba(45, 90, 39, 0); }
  100% { box-shadow: 0 0 0 0 rgba(45, 90, 39, 0); }
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
