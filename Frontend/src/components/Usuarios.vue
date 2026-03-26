<template>
  <div class="usuarios-module">
    <div class="module-header">
      <div>
        <h2 class="title">Gestión de Usuarios</h2>
        <p class="subtitle">Administra los cajeros y administradores del sistema</p>
      </div>
      <button class="primary-btn" @click="abrirModalNuevo">
        <svg class="icon-sm" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
        Nuevo Usuario
      </button>
    </div>

    <!-- Error global del modulo -->
    <div v-if="errorGlobal" class="error-banner">{{ errorGlobal }}</div>

    <!-- Tabla de Usuarios (Estado de carga) -->
    <div v-if="cargando" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando usuarios...</p>
    </div>

    <!-- Tabla de Usuarios -->
    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>DNI</th>
            <th>Nombre Completo</th>
            <th>Correo</th>
            <th>Rol</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in usuarios" :key="user.usuarioID" :class="{ 'inactive-row': !user.activo }">
            <td class="font-medium">{{ user.dni }}</td>
            <td>{{ user.nombreCompleto }}</td>
            <td class="text-muted">{{ user.correo }}</td>
            <td>
              <span class="badge" :class="user.rol === 'ADMINISTRADOR' ? 'badge-purple' : 'badge-blue'">
                {{ user.rol }}
              </span>
            </td>
            <td>
              <span class="status-dot" :class="user.activo ? 'dot-active' : 'dot-inactive'"></span>
              {{ user.activo ? 'Activo' : 'Inactivo' }}
            </td>
            <td class="actions-cell">
              <button class="action-btn edit-btn" @click="abrirModalEditar(user)" title="Editar">
                ✏️
              </button>
              <button class="action-btn toggle-btn" @click="toggleEstado(user.usuarioID)" :title="user.activo ? 'Desactivar' : 'Activar'">
                {{ user.activo ? '🚫' : '✅' }}
              </button>
            </td>
          </tr>
          <tr v-if="usuarios.length === 0">
            <td colspan="6" class="empty-state">No hay usuarios registrados.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Modal Formulario -->
    <div v-if="mostrarModal" class="modal-overlay" @click.self="cerrarModal">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ modoEdicion ? 'Editar Usuario' : 'Registrar Nuevo Usuario' }}</h3>
          <button class="close-btn" @click="cerrarModal">✕</button>
        </div>
        
        <form @submit.prevent="guardarUsuario" class="modal-form">
          <div v-if="errorFormulario" class="form-error">{{ errorFormulario }}</div>

          <div class="form-group">
            <label>DNI</label>
            <input type="text" v-model="formUser.dni" required maxlength="20" placeholder="Ej. 12345678" />
          </div>

          <div class="form-group">
            <label>Nombre Completo</label>
            <input type="text" v-model="formUser.nombreCompleto" required placeholder="Nombres y Apellidos" />
          </div>

          <div class="form-group">
            <label>Correo Electrónico</label>
            <input type="email" v-model="formUser.correo" required placeholder="correo@vidaanimal.com" />
          </div>

          <div class="form-group">
            <label>Rol en el Sistema</label>
            <select v-model="formUser.rol" required>
              <option value="CAJERO">Cajero (Ventas y Consultas)</option>
              <option value="ADMINISTRADOR">Administrador (Control Total)</option>
            </select>
          </div>

          <div class="form-group">
            <label>{{ modoEdicion ? 'Nueva Contraseña (Opcional)' : 'Contraseña' }}</label>
            <input type="password" v-model="formUser.password" :required="!modoEdicion" placeholder="••••••••" />
            <small v-if="modoEdicion" class="help-text">Deja vacío para no cambiarla</small>
          </div>

          <div class="modal-footer">
            <button type="button" class="cancel-btn" @click="cerrarModal">Cancelar</button>
            <button type="submit" class="primary-btn" :disabled="guardando">
              {{ guardando ? 'Guardando...' : (modoEdicion ? 'Actualizar' : 'Registrar') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const usuarios = ref([])
const cargando = ref(true)
const guardando = ref(false)
const errorGlobal = ref('')
const errorFormulario = ref('')

const mostrarModal = ref(false)
const modoEdicion = ref(false)
const formUser = ref({
  usuarioID: null,
  dni: '',
  nombreCompleto: '',
  correo: '',
  rol: 'CAJERO',
  password: ''
})

// Obtener Token para peticiones (si activas el Authorize en el controlador después)
const getToken = () => localStorage.getItem('jwt_token')

const cargarUsuarios = async () => {
  cargando.value = true
  errorGlobal.value = ''
  try {
    const res = await fetch('http://localhost:5044/api/Usuarios', {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    if (data.success) {
      usuarios.value = data.data
    } else {
      errorGlobal.value = data.mensaje
    }
  } catch (err) {
    errorGlobal.value = 'Error al conectar con el servidor.'
  } finally {
    cargando.value = false
  }
}

onMounted(() => {
  cargarUsuarios()
})

const abrirModalNuevo = () => {
  modoEdicion.value = false
  errorFormulario.value = ''
  formUser.value = { usuarioID: null, dni: '', nombreCompleto: '', correo: '', rol: 'CAJERO', password: '' }
  mostrarModal.value = true
}

const abrirModalEditar = (user) => {
  modoEdicion.value = true
  errorFormulario.value = ''
  formUser.value = {
    usuarioID: user.usuarioID,
    dni: user.dni,
    nombreCompleto: user.nombreCompleto,
    correo: user.correo,
    rol: user.rol,
    password: '' // Se manda vacío, si el admin quiere editar, escribe una nueva
  }
  mostrarModal.value = true
}

const cerrarModal = () => {
  mostrarModal.value = false
}

const guardarUsuario = async () => {
  guardando.value = true
  errorFormulario.value = ''

  try {
    let url = 'http://localhost:5044/api/Auth/registrar'
    let method = 'POST'
    let bodyData = { ...formUser.value }

    if (modoEdicion.value) {
      url = `http://localhost:5044/api/Usuarios/${formUser.value.usuarioID}`
      method = 'PUT'
      // El backend espera PasswordHash en caso de edición
      bodyData.passwordHash = formUser.value.password 
    }

    const res = await fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(bodyData)
    })

    const data = await res.json()

    if (data.success) {
      cerrarModal()
      await cargarUsuarios() // Refrescar tabla
    } else {
      errorFormulario.value = data.mensaje
    }
  } catch (err) {
    errorFormulario.value = 'Error de red al guardar.'
  } finally {
    guardando.value = false
  }
}

const toggleEstado = async (id) => {
  if(!confirm('¿Estás seguro de cambiar el estado de este usuario?')) return;
  
  try {
    const res = await fetch(`http://localhost:5044/api/Usuarios/${id}/toggle-activo`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    if (data.success) {
      await cargarUsuarios()
    } else {
      alert(data.mensaje)
    }
  } catch (err) {
    alert('Error al actualizar estado')
  }
}
</script>

<style scoped>
.usuarios-module {
  animation: fadeIn 0.4s ease;
}

.module-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 2rem;
}

.title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1A202C;
  margin: 0 0 0.25rem 0;
}

.subtitle {
  color: #718096;
  margin: 0;
  font-size: 0.95rem;
}

/* Base Buttons */
.primary-btn {
  background-color: #2D3748;
  color: white;
  border: none;
  padding: 0.75rem 1.25rem;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.2s;
  box-shadow: 0 4px 6px rgba(45, 55, 72, 0.1);
}
.primary-btn:hover:not(:disabled) {
  background-color: #1A202C;
  transform: translateY(-1px);
}
.primary-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
.cancel-btn {
  background-color: transparent;
  color: #4A5568;
  border: 1px solid #CBD5E0;
  padding: 0.75rem 1.25rem;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s;
}
.cancel-btn:hover { background-color: #F7FAFC; }

.icon-sm { width: 18px; height: 18px; }

/* Tabla Estilizada */
.table-container {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03);
  border: 1px solid #E2E8F0;
  overflow: hidden;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  background-color: #F8FAFC;
  padding: 1rem 1.25rem;
  text-align: left;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #4A5568;
  border-bottom: 1px solid #E2E8F0;
}

.data-table td {
  padding: 1rem 1.25rem;
  border-bottom: 1px solid #E2E8F0;
  color: #2D3748;
  font-size: 0.95rem;
  vertical-align: middle;
}

.text-right { text-align: right !important; }
.font-medium { font-weight: 600; }
.text-muted { color: #718096; }

.inactive-row td {
  opacity: 0.6;
  background-color: #FAFAFA;
}

/* Badges y Status */
.badge {
  padding: 0.25rem 0.75rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}
.badge-purple { background-color: #E9D8FD; color: #553C9A; }
.badge-blue { background-color: #BEE3F8; color: #2B6CB0; }

.status-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-right: 6px;
}
.dot-active { background-color: #48BB78; box-shadow: 0 0 0 3px rgba(72,187,120,0.2); }
.dot-inactive { background-color: #F56565; }

/* Botones de acción tabla */
.actions-cell {
  text-align: right;
  white-space: nowrap;
}
.action-btn {
  background: transparent;
  border: none;
  font-size: 1.1rem;
  padding: 0.4rem;
  cursor: pointer;
  border-radius: 6px;
  transition: background 0.2s;
  margin-left: 0.5rem;
  opacity: 0.7;
}
.action-btn:hover {
  background: #EDF2F7;
  opacity: 1;
}

.empty-state { text-align: center; color: #A0AEC0; padding: 3rem !important; }

/* Estados */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 4rem;
  color: #718096;
}
.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #E2E8F0;
  border-top-color: #C3B1E1;
  border-radius: 50%;
  animation: spin 1s infinite linear;
  margin-bottom: 1rem;
}
@keyframes spin { to { transform: rotate(360deg); } }

.error-banner {
  background-color: #FFF5F5;
  color: #C53030;
  padding: 1rem;
  border-radius: 10px;
  margin-bottom: 1.5rem;
  font-weight: 500;
}

/* Modal UI */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  animation: fadeIn 0.2s ease;
}

.modal-content {
  background: white;
  width: 100%;
  max-width: 480px;
  border-radius: 20px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  transform: translateY(0);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } }

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #E2E8F0;
}
.modal-header h3 { margin: 0; font-size: 1.25rem; color: #1A202C; font-weight: 700; }
.close-btn { background: none; border: none; font-size: 1.25rem; color: #A0AEC0; cursor: pointer; }
.close-btn:hover { color: #2D3748; }

.modal-form { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; }
.form-group label { font-size: 0.85rem; font-weight: 600; color: #4A5568; }
.form-group input, .form-group select {
  padding: 0.75rem 1rem;
  border: 1.5px solid #E2E8F0;
  border-radius: 10px;
  font-size: 0.95rem;
  color: #2D3748;
  outline: none;
  transition: border-color 0.2s;
}
.form-group input:focus, .form-group select:focus { border-color: #C3B1E1; box-shadow: 0 0 0 3px rgba(195,177,225,0.1); }
.help-text { font-size: 0.75rem; color: #718096; margin-top: 0.25rem; }

.form-error { background: #FFF5F5; color: #E53E3E; padding: 0.75rem; border-radius: 8px; font-size: 0.85rem; }

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1.5rem;
  border-top: 1px solid #E2E8F0;
}
</style>
