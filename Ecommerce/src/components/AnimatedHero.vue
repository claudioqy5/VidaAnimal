<script setup>
import { onMounted, ref } from 'vue'
import publicidadPata from '../assets/publicidadpata.png'
import publicidadRopita from '../assets/publicidadropita.png'
import publicidadStitch from '../assets/publicidadstitch.jpg'
import publicidadSchnauzer from '../assets/publicidadschnauzer.png'
import publicidadZapatos from '../assets/publicidadzapatos.png'
import publicidadBebedero from '../assets/publicidadbebedero.png'
import publicidadSofaDog from '../assets/publicidadsofa.png'
import publicidadalimentos from '../assets/publicidadalimentos.png'
import publicidadalimentosotros from '../assets/publicidadalimentosotros.png'
import publicidadjuguetes from '../assets/publicidadjuguetes.png' 

const isLoaded = ref(false)
const isDimmed = ref(false)
const displayText = ref('')

// Configuración Carousel
const adImages = [
  publicidadPata,
  publicidadRopita,  
  publicidadStitch,  
  publicidadZapatos,
  publicidadBebedero,
  publicidadSofaDog,
  publicidadalimentos,
  publicidadjuguetes,
  publicidadSchnauzer,
  publicidadalimentosotros
]
const currentAdIndex = ref(0)
const isPaused = ref(false)

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
    typeSpeed = 2000 
  } else if (isDeleting && charIndex === 0) {
    isDeleting = false
    phraseIndex = (phraseIndex + 1) % phrases.length
    typeSpeed = 500
  }

  setTimeout(typeEffect, typeSpeed)
}

const nextAd = () => {
  currentAdIndex.value = (currentAdIndex.value + 1) % adImages.length
}

const prevAd = () => {
  currentAdIndex.value = (currentAdIndex.value - 1 + adImages.length) % adImages.length
}

onMounted(() => {
  setTimeout(() => {
    isLoaded.value = true
    typeEffect()
  }, 100)

  // Rotación del Carousel cada 2 segundos (solo si no está pausado)
  setInterval(() => {
    if (!isPaused.value) {
      currentAdIndex.value = (currentAdIndex.value + 1) % adImages.length
    }
  }, 2000)

  // Oscurecemos el fondo a los 3 segundos
  setTimeout(() => {
    isDimmed.value = true
  }, 3000)
})
</script>

<template>
  <section class="animated-hero" :class="{ 'start-animation': isLoaded, 'is-dimmed': isDimmed }">
    <!-- Capas de arte -->
    <div class="layer layer-bg">
      <img src="../assets/fondototal.png" alt="Fondo" class="full-img">
    </div>
    <div class="layer layer-left">
      <img src="../assets/ladoizquierdo.png" alt="Decoración Izquierda">
    </div>
    <div class="layer layer-right">
      <img src="../assets/ladoderecho.png" alt="Decoración Derecha">
    </div>
    <div class="layer layer-center">
      <img src="../assets/centro.png" alt="Centro" class="lift-img">
    </div>

    <!-- Minicarousel Publicitario (Derecha) - Solo aparece cuando se oscurece el fondo -->
    <Transition name="fade-ad-entrance">
      <div 
        class="ad-carousel-container" 
        v-if="isDimmed"
        @mouseenter="isPaused = true"
        @mouseleave="isPaused = false"
      >
        <!-- Botones de control -->
        <button class="ad-nav-btn prev" @click="prevAd">‹</button>
        <button class="ad-nav-btn next" @click="nextAd">›</button>

        <Transition name="fade-ad" mode="out-in">
          <img :key="currentAdIndex" :src="adImages[currentAdIndex]" class="ad-image" alt="Publicidad Destacada">
        </Transition>
      </div>
    </Transition>

    <!-- Overlay de textos -->
    <div class="hero-overlay fade-in" v-if="isLoaded">
      <div class="container welcome-container">
        <div class="massive-text">
          <h1><span class="vida">Vida</span><br><span class="animal">ANIMAL</span></h1>
        </div>
        <div class="hero-actions-box">          
          <p class="typewriter-text">{{ displayText }}<span class="cursor">|</span></p>          
          
          <div class="hero-pickup-badge fade-in-delayed">
            <span class="icon">🛍️</span>
            <span>Arma tu pedido online, recógelo en tienda (Lunes a Domingo) y reclama tu <b>regalo</b></span>
          </div>
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
  background-color: #000;
}

/* Carousel Estilos */
.ad-carousel-container {
  position: absolute;
  right: 8%; /* Posicionado a la derecha */
  top: 50%;
  transform: translateY(-50%);
  /*width: 30%;  Tamaño proporcional */
  height:75%;
  z-index: 40; /* Subido para estar por encima del overlay y recibir clics */
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
  border-radius: 100%; /* Esquinas redondeadas premium */
  overflow: visible; /* Permitir que los botones sobresalgan un poco si se desea */
  /* YA NO SE OSCURECE: Mantiene su color natural al 100% */
  filter: brightness(1) !important;
  pointer-events: auto; /* IMPORTANTE: Ahora sí recibe clics para los botones */
}

.ad-nav-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  font-size: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 35;
  transition: all 0.3s;
  line-height: 0;
  padding-bottom: 5px;
}

.ad-nav-btn:hover {
  background: rgba(255, 255, 255, 0.4);
  transform: translateY(-50%) scale(1.1);
}

.ad-nav-btn.prev { left: -20px; }
.ad-nav-btn.next { right: -20px; }

.ad-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  border-radius: 24px; /* Aplicar redondeado a la imagen también */
  box-shadow: 0 20px 40px rgba(0,0,0,0.3);
}

.fade-ad-entrance-enter-active {
  transition: opacity 2s ease, transform 2s ease;
}
.fade-ad-entrance-enter-from {
  opacity: 0;
  transform: translateY(-40%) scale(0.9);
}

/* Animación de fundido para el anuncio */
.fade-ad-enter-active,
.fade-ad-leave-active {
  transition: opacity 0.7s ease;
}
.fade-ad-enter-from,
.fade-ad-leave-to {
  opacity: 0;
}

/* Capas y Filtros */
.layer {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.full-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: filter 2s ease;
}

.is-dimmed .full-img {
  filter: brightness(0.35);
}

.layer-left {
  transform: translateX(-100%);
  transition: transform 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  justify-content: flex-start;
  z-index: 10;
  max-height: 100%;
}

.layer-left img {
  height: 100%;
  width: auto;
  transition: filter 2s ease;
}

.is-dimmed .layer-left img {
  filter: brightness(0.25);
}

.start-animation .layer-left {
  transform: translateX(0);
}

.layer-right {
  transform: translateX(100%);
  transition: transform 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  justify-content: flex-end;
  z-index: 10;
}

.layer-right img {
  height: 100%;
  width: auto;
  transition: filter 2s ease;
}

.is-dimmed .layer-right img {
  filter: brightness(0.25);
}

.start-animation .layer-right {
  transform: translateX(0);
}

.layer-center {
  transform: translateY(100vh);
  opacity: 0;
  transition: all 2.5s cubic-bezier(0.16, 1, 0.3, 1);
  transition-delay: 1s;
  z-index: 20;
}

.layer-center img {
  max-height: 110%;
  width: auto;
  object-fit: contain;
  transition: filter 2s ease;
}

.is-dimmed .layer-center img {
  filter: brightness(0.25);
}

.start-animation .layer-center {
  transform: translateY(0) scale(1.05);
  opacity: 1;
}

/* Overlay y Contenedores */
.hero-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  z-index: 30;
  background: linear-gradient(to right, rgba(0,0,0,0.5) 0%, transparent 90%);
}

.welcome-container {
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  align-items: flex-start;
  height: 100%;
  padding-bottom: 2rem;
  margin-left: 5rem;
}

.massive-text {
  opacity: 0;
  animation: fadeInText 1.5s forwards 3s;
}

.vida {
  color: rgb(228, 180, 107);
  font-family: 'Pacifico', cursive; /* Fuente elegante y manuscrita */
  letter-spacing: 5px;
  text-transform: none; /* Las cursivas se ven mejor en minúsculas/mixtas */
  text-shadow: 0 5px 15px rgba(228, 180, 107, 0.3);
}

.animal {
  color: white;
  font-family: 'Bungee', sans-serif; /* Fuente sólida y bloque */
  letter-spacing: -2px;
  text-transform: uppercase;
}

.massive-text h1 {
  font-size: clamp(5rem, 15vw, 15rem);
  color: rgb(255, 255, 255);
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
  margin-bottom: 4rem;
}

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
  animation: pulse-glow 2s infinite 5s;
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
  font-size: 1.5rem;
  font-weight: 500;
  letter-spacing: 2px;
  text-transform: uppercase;
  opacity: 0.9;
  border-left: 3px solid var(--secondary);
  padding-left: 1rem;
  min-height: 1.5em;
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

.hero-pickup-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.8rem;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: 0.8rem 1.5rem;
  border-radius: 50px;
  color: white;
  font-size: 1.05rem;
  font-weight: 500;
  letter-spacing: 0.5px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  width: fit-content;
}

.hero-pickup-badge b {
  color: var(--secondary);
  font-weight: 800;
}

.fade-in-delayed {
  opacity: 0;
  animation: fadeInText 1s forwards 4.5s;
}

@media (max-width: 1024px) {
  .welcome-text h1 { font-size: 2.5rem; }
  .layer-left, .layer-right { opacity: 0.5; }
  .ad-carousel-container { display: none; } /* Ocultar en móvil para limpiar el diseño */
}
</style>
