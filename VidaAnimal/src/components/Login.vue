<template>
  <div class="login-wrapper">
    <!-- Círculos decorativos de fondo -->
    <div class="blob blob-blue"></div>
    <div class="blob blob-pink"></div>
    <div class="blob blob-purple"></div>

    <div class="login-card">
      <div class="login-header">
        <div class="logo-box">
          <svg class="logo-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 10l-2 1m0 0l-2-1m2 1v2.5M20 7l-2 1m2-1l-2-1m2 1v2.5M14 4l-2-1-2 1M4 7l2-1M4 7l2 1M4 7v2.5M12 21l-2-1m2 1l2-1m-2 1v-2.5M6 18l-2-1v-2.5M18 18l2-1v-2.5"></path>
          </svg>
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

const correo = ref('')
const password = ref('')
const error = ref('')
const cargando = ref(false)

const handleLogin = async () => {
  error.value = ''
  cargando.value = true

  try {
    const response = await fetch('http://localhost:5044/api/Auth/login', {
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
  font-family: 'Inter', 'Segoe UI', sans-serif;
}

.login-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background-color: #FAFAFA;
  overflow: hidden;
}

/* Glassmorphism Card */
.login-card {
  position: relative;
  z-index: 10;
  width: 100%;
  max-width: 420px;
  padding: 2.5rem 2rem;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.5);
  margin: 1rem;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-box {
  width: 64px;
  height: 64px;
  margin: 0 auto 1rem;
  background: linear-gradient(135deg, #FFD1DC 0%, #C3B1E1 100%);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  transform: rotate(4deg);
  box-shadow: 0 10px 20px rgba(195, 177, 225, 0.4);
}

.logo-icon {
  width: 32px;
  height: 32px;
  color: white;
}

.login-title {
  font-size: 1.8rem;
  font-weight: 700;
  color: #2D3748;
  margin: 0 0 0.5rem 0;
  letter-spacing: -0.5px;
}

.login-subtitle {
  font-size: 0.9rem;
  color: #718096;
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
  color: #4A5568;
}

.form-input {
  width: 100%;
  padding: 0.8rem 1rem;
  font-size: 1rem;
  background-color: #FFFFFF;
  border: 1.5px solid #E2E8F0;
  border-radius: 12px;
  transition: all 0.2s ease;
  outline: none;
  color: #2D3748;
}

.form-input::placeholder {
  color: #A0AEC0;
}

.form-input:focus {
  border-color: #C3B1E1;
  box-shadow: 0 0 0 4px rgba(195, 177, 225, 0.2);
}

.submit-btn {
  width: 100%;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 600;
  color: white;
  background-color: #2D3748;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(45, 55, 72, 0.2);
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
