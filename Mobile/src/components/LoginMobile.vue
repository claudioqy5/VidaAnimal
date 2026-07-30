<template>
  <div class="login-screen">
    <div class="login-card fade-in">
      <!-- Logo -->
      <div class="login-logo-wrap">🐾</div>

      <h1 class="login-title">Vida Animal</h1>
      <p class="login-subtitle">POS Móvil · Sistema de Ventas</p>

      <form class="login-form" @submit.prevent="handleLogin">
        <div class="form-group">
          <label class="label" for="login-email">Correo electrónico</label>
          <input
            id="login-email"
            v-model="correo"
            type="email"
            required
            autocomplete="email"
            class="input"
            placeholder="usuario@vidaanimal.com"
          />
        </div>

        <div class="form-group">
          <label class="label" for="login-password">Contraseña</label>
          <input
            id="login-password"
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            class="input"
            placeholder="••••••••"
          />
        </div>

        <div v-if="error" class="banner banner-error" style="border-radius: var(--radius-sm); margin: 0;">
          ⚠️ {{ error }}
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-full"
          :disabled="cargando"
          style="margin-top: 0.25rem; font-size: 1rem; min-height: 52px; border-radius: var(--radius);"
        >
          <span v-if="cargando" class="spinner"></span>
          {{ cargando ? 'Ingresando...' : 'Ingresar al Sistema' }}
        </button>
      </form>

      <p style="margin-top: 1.25rem; text-align: center; font-size: 0.75rem; color: var(--text-muted);">
        Vida Animal © {{ new Date().getFullYear() }}
      </p>
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
