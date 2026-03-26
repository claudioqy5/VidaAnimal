<template>
  <div class="proveedores-module">
    <div class="module-header">
      <div>
        <h2 class="title">Gestión de Proveedores</h2>
        <p class="subtitle">Directorio de contacto con empresas y distribuidores</p>
      </div>
    </div>

    <div v-if="errorGlobal" class="error-banner">{{ errorGlobal }}</div>

    <div v-if="cargando" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando proveedores...</p>
    </div>

    <!-- Tabla -->
    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>Nombre Empresa</th>
            <th>Teléfono</th>
            <th>Correo Electrónico</th>
            <th>Dirección</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="prov in proveedores" :key="prov.proveedorID" :class="{ 'inactive-row': !prov.activo }">
            <td class="font-medium">
              <div class="company-name">
                <span class="company-icon">🏢</span>
                {{ prov.nombre }}
              </div>
            </td>
            <td>{{ prov.telefono || '-' }}</td>
            <td class="text-muted">{{ prov.email || '-' }}</td>
            <td class="text-muted"><span class="truncate-text" :title="prov.direccion">{{ prov.direccion || '-' }}</span></td>
            <td>
              <span class="status-dot" :class="prov.activo ? 'dot-active' : 'dot-inactive'"></span>
              {{ prov.activo ? 'Activo' : 'Inactivo' }}
            </td>
            <td class="actions-cell">
              <button class="action-btn edit-btn" @click="abrirModalEditar(prov)" title="Editar">✏️</button>
              <button class="action-btn toggle-btn" @click="toggleEstado(prov.proveedorID)" :title="prov.activo ? 'Desactivar' : 'Activar'">
                {{ prov.activo ? '🚫' : '✅' }}
              </button>
            </td>
          </tr>
          <tr v-if="proveedores.length === 0">
            <td colspan="6" class="empty-state">No hay proveedores registrados aún.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Formulario -->
    <div v-if="mostrarModal" class="modal-overlay" @click.self="cerrarModal">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ modoEdicion ? 'Editar Proveedor' : 'Registrar Proveedor' }}</h3>
          <button class="close-btn" @click="cerrarModal">✕</button>
        </div>
        
        <form @submit.prevent="guardarProveedor" class="modal-form">
          <div v-if="errorFormulario" class="form-error">{{ errorFormulario }}</div>

          <div class="form-group">
            <label>Nombre de la Empresa *</label>
            <input type="text" v-model="formProv.nombre" required placeholder="Ej. Purina / Pedigree" />
          </div>

          <div class="form-row">
            <div class="form-group half-width">
              <label>Teléfono</label>
              <input type="tel" v-model="formProv.telefono" placeholder="Ej. 123456789" />
            </div>
            <div class="form-group half-width">
              <label>Correo Electrónico</label>
              <input type="email" v-model="formProv.email" placeholder="contacto@empresa.com" />
            </div>
          </div>

          <div class="form-group">
            <label>Dirección</label>
            <input type="text" v-model="formProv.direccion" placeholder="Calle, Ciudad..." />
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

const proveedores = ref([])
const cargando = ref(true)
const guardando = ref(false)
const errorGlobal = ref('')
const errorFormulario = ref('')

const mostrarModal = ref(false)
const modoEdicion = ref(false)
const formProv = ref({
  proveedorID: 0,
  nombre: '',
  telefono: '',
  email: '',
  direccion: ''
})

const getToken = () => localStorage.getItem('jwt_token')

const cargarProveedores = async () => {
  cargando.value = true
  errorGlobal.value = ''
  try {
    const res = await fetch('http://localhost:5044/api/Proveedores', {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    if (data.success) {
      proveedores.value = data.data
    } else {
      errorGlobal.value = data.mensaje
    }
  } catch (err) {
    errorGlobal.value = 'Error al conectar con la API de Proveedores.'
  } finally {
    cargando.value = false
  }
}

onMounted(() => cargarProveedores())

const abrirModalNuevo = () => {
  modoEdicion.value = false
  errorFormulario.value = ''
  formProv.value = { proveedorID: 0, nombre: '', telefono: '', email: '', direccion: '' }
  mostrarModal.value = true
}

const abrirModalEditar = (prov) => {
  modoEdicion.value = true
  errorFormulario.value = ''
  formProv.value = { ...prov }
  mostrarModal.value = true
}

const cerrarModal = () => mostrarModal.value = false

const guardarProveedor = async () => {
  guardando.value = true
  errorFormulario.value = ''

  let url = 'http://localhost:5044/api/Proveedores'
  let method = 'POST'

  if (modoEdicion.value) {
    url = `http://localhost:5044/api/Proveedores/${formProv.value.proveedorID}`
    method = 'PUT'
  }

  try {
    const res = await fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(formProv.value)
    })
    const data = await res.json()
    if (data.success) {
      cerrarModal()
      cargarProveedores()
    } else {
      errorFormulario.value = data.mensaje
    }
  } catch (err) {
    errorFormulario.value = 'Fallo en la conexión de red.'
  } finally {
    guardando.value = false
  }
}

const toggleEstado = async (id) => {
  if(!confirm('¿Seguro que deseas cambiar el estado de este proveedor? Los productos relacionados a él podrían no mostrarse si es inactivo.')) return;
  
  try {
    const res = await fetch(`http://localhost:5044/api/Proveedores/${id}/toggle-activo`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    if (data.success) {
      cargarProveedores()
    } else {
      alert(data.mensaje)
    }
  } catch (err) {
    alert('Error al desactivar proveedor')
  }
}
</script>

<style scoped>
/* Aprovechamos la misma estética pastel y minimal de Usuarios.vue */
.proveedores-module {
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

.subtitle { color: #718096; margin: 0; font-size: 0.95rem; }

/* Buttons */
.primary-btn {
  background-color: #A7C7E7; /* Pastel blue */
  color: #1A365D; /* Dark blue text */
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
  box-shadow: 0 4px 10px rgba(167, 199, 231, 0.4);
}
.primary-btn:hover:not(:disabled) {
  background-color: #8BA9C7;
  transform: translateY(-1px);
}
.primary-btn:disabled { opacity: 0.7; cursor: not-allowed; }

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

/* Tabla */
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
  color: #4A5568;
  border-bottom: 1px solid #E2E8F0;
}

.data-table td {
  padding: 1rem 1.25rem;
  border-bottom: 1px solid #E2E8F0;
  color: #2D3748;
  font-size: 0.95rem;
}

.text-right { text-align: right !important; }
.font-medium { font-weight: 600; }
.text-muted { color: #718096; }

.inactive-row td { opacity: 0.6; background-color: #FAFAFA; }

.company-name { display: flex; align-items: center; gap: 0.5rem; }
.company-icon { font-size: 1.1rem; opacity: 0.8;}

.truncate-text {
  display: inline-block;
  max-width: 150px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  vertical-align: middle;
}

/* Dots and Action btn*/
.status-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-right: 6px;
}
.dot-active { background-color: #48BB78; box-shadow: 0 0 0 3px rgba(72,187,120,0.2); }
.dot-inactive { background-color: #F56565; }

.actions-cell { text-align: right; white-space: nowrap; }
.action-btn { background: transparent; border: none; font-size: 1.1rem; padding: 0.4rem; cursor: pointer; border-radius: 6px; opacity: 0.7; transition: all 0.2s;}
.action-btn:hover { background: #EDF2F7; opacity: 1; }

.empty-state { text-align: center; color: #A0AEC0; padding: 3rem !important; }

/* Status loading */
.loading-state { display: flex; flex-direction: column; align-items: center; padding: 4rem; color: #718096; }
.spinner { width: 40px; height: 40px; border: 4px solid #E2E8F0; border-top-color: #A7C7E7; border-radius: 50%; animation: spin 1s infinite linear; margin-bottom: 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Error banner */
.error-banner { background-color: #FFF5F5; color: #C53030; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; }

/* Modal */
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center;
  z-index: 50; animation: fadeIn 0.2s ease;
}

.modal-content {
  background: white; width: 100%; max-width: 500px;
  border-radius: 20px; box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } }

.modal-header { display: flex; justify-content: space-between; align-items: center; padding: 1.5rem; border-bottom: 1px solid #E2E8F0; }
.modal-header h3 { margin: 0; font-size: 1.25rem; font-weight: 700; }
.close-btn { background: none; border: none; font-size: 1.25rem; color: #A0AEC0; cursor: pointer; }

.modal-form { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; }
.form-row { display: flex; gap: 1rem; }
.half-width { flex: 1; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; }
.form-group label { font-size: 0.85rem; font-weight: 600; color: #4A5568; }
.form-group input { padding: 0.75rem 1rem; border: 1.5px solid #E2E8F0; border-radius: 10px; color: #2D3748; outline: none; transition: border-color 0.2s; }
.form-group input:focus { border-color: #A7C7E7; box-shadow: 0 0 0 3px rgba(167, 199, 231, 0.2); }

.form-error { background: #FFF5F5; color: #E53E3E; padding: 0.75rem; border-radius: 8px; font-size: 0.85rem; }

.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; padding-top: 1.5rem; border-top: 1px solid #E2E8F0; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; }}
</style>
