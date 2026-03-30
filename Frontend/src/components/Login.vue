<template>
  <div class="login-wrapper">
    <!-- Fondo animado con scroll horizontal -->
    <div class="animated-bg"></div>
    <!-- Overlay oscuro -->
    <div class="bg-overlay"></div>

    <div class="login-card">
      <div class="login-header">
        <div class="logo-box">
          <img src="../assets/logoVidaAnimal.png" alt="Vida Animal" class="logo-img" />
        </div>
        <h1 class="login-title">Vida Animal</h1>
        <p class="login-subtitle">Sistema de Gestión y Punto de Venta</p>
      </div>

      <form class="login-form" @submit.prevent="handleLogin">
        <div class="form-group">
          <label class="form-label">Correo Electrónico</label>
          <input 
            v-model="correo" 
            type="email" 
            required
            class="form-input"
            placeholder="admin@vidaanimal.com"
          />
        </div>

        <div class="form-group">
          <label class="form-label">Contraseña</label>
          <input 
            v-model="password" 
            type="password" 
            required
            class="form-input"
            placeholder="••••••••"
          />
        </div>

        <div v-if="error" class="error-box animate-shake">
          <svg class="error-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          {{ error }}
        </div>

        <button type="submit" class="submit-btn" :disabled="cargando">
          <svg v-if="cargando" class="spinner-icon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="spinner-track" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="spinner-slice" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ cargando ? 'Iniciando sesión...' : 'Ingresar al sistema' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const emit = defineEmits(['login-success'])

const API_URL = import.meta.env.VITE_API_URL;
const correo = ref('')
const password = ref('')
const error = ref('')
const cargando = ref(false)

const API_BASE = import.meta.env.VITE_API_URL;
const getToken = () => localStorage.getItem('jwt_token');

const handleLogin = async () => {
  error.value = ''
  cargando.value = true

  try {
    const response = await fetch(`${API_URL}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        correo: correo.value,
        password: password.value
      })
    })

    const data = await response.json()

    if (data.success) {
      localStorage.setItem('jwt_token', data.token)
      localStorage.setItem('usuario', JSON.stringify(data.usuario))
      emit('login-success', data.usuario)
    } else {
      error.value = data.mensaje || 'Credenciales incorrectas.'
      password.value = ''
    }
  } catch (err) {
    console.error(err)
    error.value = 'No se pudo conectar con el servidor.'
  } finally {
    cargando.value = false
  }
}
</script>

<style scoped>
/* Reset base */
* {
  box-sizing: border-box;
  font-family: 'Poiret One', 'Segoe UI', sans-serif;
}


.login-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background-color: #1a1a2e;
  overflow: hidden;
}

/* Fondo animado con scroll horizontal infinito */
.animated-bg {
  position: absolute;
  top: 0;
  left: 0;
  width: 200%;
  height: 100%;
  background-image: url('../assets/fondologin.png');
  background-repeat: repeat-x;
  background-size: auto 100%;
  animation: moveBackground 100s linear infinite;
  z-index: 1;
}

@keyframes moveBackground {
  0%   { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

/* Overlay oscuro semitransparente */
.bg-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  z-index: 2;
}


/* Glassmorphism Card */
.login-card {
  position: relative;
  z-index: 10;
  width: 100%;
  max-width: 420px;
  padding: 2.5rem 2rem;  
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.08);  
  margin: 1rem;
  letter-spacing: 0.155em;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-box {
  width: 90px;
  height: 90px;
  margin: 0 auto 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  filter: drop-shadow(0 4px 12px rgba(0,0,0,0.35));
}

.logo-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.logo-icon {
  width: 32px;
  height: 32px;
  color: white;
}

.login-title {
  font-size: 2.3rem;
  font-weight: 700;
  color: #FFFFFF;
  margin: 0 0 0.5rem 0;  
  letter-spacing: 0em;
  font-family: 'Syncopate', sans-serif;
  text-shadow: 0 2px 12px rgba(0,0,0,0.5);
}

.login-subtitle {
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.85);
  margin: 0;
  
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
  text-shadow: 0 1px 4px rgba(0,0,0,0.3);
}

.form-input {
  width: 100%;
  padding: 0.8rem 1rem;
  font-size: 1rem;
  background-color: rgba(255, 255, 255, 0.18);
  border: 1.5px solid rgba(255, 255, 255, 0.4);
  border-radius: 12px;
  transition: all 0.2s ease;
  outline: none;
  color: #FFFFFF;
}

.form-input::placeholder {
  color: rgba(255, 255, 255, 0.55);
}

.form-input:focus {
  border-color: rgba(255, 255, 255, 0.8);
  box-shadow: 0 0 0 4px rgba(255, 255, 255, 0.15);
  background-color: rgba(255, 255, 255, 0.25);
}

.submit-btn {
  width: 100%;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 700;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 18px rgba(102, 126, 234, 0.5);  
  letter-spacing: 0.18em;
}

.submit-btn:hover:not(:disabled) {
  background-color: #1A202C;
  transform: translateY(-1px);
}

.submit-btn:active:not(:disabled) {
  transform: translateY(1px);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.error-box {
  background-color: #FFF5F5;
  color: #E53E3E;
  padding: 0.8rem;
  border-radius: 10px;
  font-size: 0.85rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.error-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

/* Spinner animado */
.spinner-icon {
  width: 20px;
  height: 20px;
  margin-right: 10px;
  animation: spin 1s linear infinite;
}
.spinner-track { opacity: 0.25; }
.spinner-slice { opacity: 0.75; }
@keyframes spin { 100% { transform: rotate(360deg); } }

/* Animación de error */
.animate-shake {
  animation: shake 0.3s ease-in-out;
}
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

/* Blobs decorativos de fondo */
.blob {
  position: absolute;
  width: 350px;
  height: 350px;
  border-radius: 50%;
  filter: blur(60px);
  opacity: 0.4;
  animation: float 8s infinite alternate ease-in-out;
}

.blob-blue {
  background: var(--pastel-blue, #A7C7E7);
  top: -10%;
  left: -5%;
}

.blob-pink {
  background: var(--pastel-pink, #FFD1DC);
  top: 10%;
  right: -5%;
  animation-delay: 2s;
}

.blob-purple {
  background: var(--pastel-purple, #C3B1E1);
  bottom: -15%;
  left: 30%;
  animation-delay: 4s;
}

@keyframes float {
  0% { transform: translate(0, 0) scale(1); }
  100% { transform: translate(30px, 40px) scale(1.1); }
}
</style>
