<template>
  <div class="login-screen">
    <div class="login-glow login-glow-1"></div>
    <div class="login-glow login-glow-2"></div>

    <div class="login-card fade-in">
      <!-- Logo -->
      <div class="login-logo-fallback">🐾</div>

      <h1 class="login-title">Vida Animal</h1>
      <p class="login-subtitle">POS Móvil · Sistema de Ventas</p>

      <form class="login-form" @submit.prevent="handleLogin">
        <div class="form-group">
          <label class="label">Correo Electrónico</label>
          <input
            v-model="correo"
            type="email"
            required
            autocomplete="email"
            class="input"
            placeholder="admin@vidaanimal.com"
          />
        </div>

        <div class="form-group">
          <label class="label">Contraseña</label>
          <input
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            class="input"
            placeholder="••••••••"
          />
        </div>

        <div v-if="error" class="banner banner-error" style="border-radius: var(--radius-sm); margin: 0;">
          {{ error }}
        </div>

        <button type="submit" class="btn btn-primary btn-full" :disabled="cargando" style="margin-top: 0.5rem; font-size: 1rem; min-height: 52px; border-radius: var(--radius);">
          <span v-if="cargando" class="spinner" style="width: 20px; height: 20px; border-width: 2px; flex-shrink: 0;"></span>
          {{ cargando ? 'Ingresando...' : 'Ingresar al Sistema' }}
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
    const response = await fetch('/api/Auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      // Normalizamos el correo a minúsculas para evitar problemas de mayúsculas
      body: JSON.stringify({ correo: correo.value.trim().toLowerCase(), password: password.value })
    })

    let data = {}
    try { data = await response.json() } catch { /* body vacío */ }

    if (response.ok && data.success) {
      localStorage.setItem('jwt_token', data.token)
      localStorage.setItem('usuario', JSON.stringify(data.usuario))
      emit('login-success', data.usuario)
    } else {
      error.value = data.mensaje || 'Correo o contraseña incorrectos.'
      password.value = ''
    }
  } catch {
    error.value = 'No se pudo conectar con el servidor.'
  } finally {
    cargando.value = false
  }
}
</script>
