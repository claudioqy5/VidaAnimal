<template>
  <div class="clientes-container animate-fade-in">
    <div class="header-section">
      <div>
        <h1>Gestión de Clientes</h1>
        <p class="subtitle">Administra la base de datos de dueños y pacientes</p>
      </div>
      <button class="btn-primary" @click="showModal = true; isEditing = false; resetForm()">
        <span class="icon">+</span> Nuevo Cliente
      </button>
    </div>

    <!-- Filtros y Búsqueda -->
    <div class="filters-card glass">
      <div class="search-box">
        <i class="fas fa-search"></i>
        <input type="text" v-model="searchQuery" placeholder="Buscar por nombre o DNI/RUC..." />
      </div>
      <div class="stats-mini">
        <div class="stat-item">
          <span class="label">Total Clientes:</span>
          <span class="value">{{ clientes.length }}</span>
        </div>
        <div class="stat-item highlight-birthday" v-if="clientesCumpleano.length > 0">
          <span class="label">🎂 Cumpleaños Hoy:</span>
          <span class="value">{{ clientesCumpleano.length }}</span>
        </div>
      </div>
    </div>

    <!-- Tabla de Clientes -->
    <div class="table-card glass">
      <table class="custom-table">
        <thead>
          <tr>
            <th>Cliente</th>
            <th>Documento</th>
            <th>Contacto</th>
            <th>Cumpleaños</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in filteredClientes" :key="c.clienteID" :class="{ 'birthday-row': isBirthdayToday(c.fechaNacimiento) }">
            <td>
              <div class="client-info">
                <div class="avatar-small" :class="{ 'avatar-birthday': isBirthdayToday(c.fechaNacimiento) }">
                  {{ isBirthdayToday(c.fechaNacimiento) ? '🎂' : getInitials(c.nombreCompleto) }}
                </div>
                <div>
                  <p class="name">{{ c.nombreCompleto }}</p>
                  <p class="meta" v-if="isBirthdayToday(c.fechaNacimiento)">🎉 ¡Hoy es su cumpleaños!</p>
                </div>
              </div>
            </td>
            <td>{{ c.documentoIdentidad }}</td>
            <td>
              <div class="contact-meta">
                <p><i class="fas fa-phone"></i> {{ c.telefono || '---' }}</p>
                <p><i class="fas fa-envelope"></i> {{ c.correo || '---' }}</p>
              </div>
            </td>
            <td>
              <span :class="['bday-tag', { active: isBirthdayToday(c.fechaNacimiento) }]">
                {{ formatBirthdate(c.fechaNacimiento) }}
              </span>
            </td>
            <td>
              <span :class="['status-badge', c.activo ? 'active' : 'inactive']">
                {{ c.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td class="actions">
              <button class="btn-icon edit" @click="editClient(c)" title="Editar">✏️</button>
              <button class="btn-icon delete" @click="confirmDelete(c)" title="Eliminar">🗑️</button>
          </td>
          </tr>
          <tr v-if="filteredClientes.length === 0">
            <td colspan="6" class="no-results">No se encontraron clientes que coincidan con la búsqueda.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal de Registro/Edición -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-card animate-slide-up">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Editar Cliente' : 'Nuevo Cliente' }}</h2>
          <button class="btn-close" @click="showModal = false">×</button>
        </div>
        <form @submit.prevent="saveCliente" class="modal-body">
          <div class="form-grid">
            <div class="form-group full">
              <label>Nombre Completo *</label>
              <input type="text" v-model="form.nombreCompleto" required placeholder="Ej. Juan Perez" />
            </div>
            <div class="form-group">
              <label>DNI / RUC *</label>
              <input type="text" v-model="form.documentoIdentidad" required placeholder="8 o 11 dígitos" />
            </div>
            <div class="form-group">
              <label>Fecha de Nacimiento</label>
              <input type="date" v-model="form.fechaNacimiento" />
            </div>
            <div class="form-group">
              <label>Teléfono</label>
              <input type="text" v-model="form.telefono" placeholder="Ej. 987654321" />
            </div>
            <div class="form-group">
              <label>Correo Electrónico</label>
              <input type="email" v-model="form.correo" placeholder="ejemplo@correo.com" />
            </div>
            <div class="form-group full">
              <label>Dirección</label>
              <input type="text" v-model="form.direccion" placeholder="Av. Siempre Viva 123" />
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn-secondary" @click="showModal = false">Cancelar</button>
            <button type="submit" class="btn-primary" :disabled="loading">
              {{ loading ? 'Guardando...' : (isEditing ? 'Actualizar Cliente' : 'Registrar Cliente') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Banner de Éxito -->
    <div v-if="showSuccessBanner" class="success-banner animate-slide-up">
      <i class="fas fa-check-circle"></i>
      {{ successMessage }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const apiFetch = async (endpoint, options = {}) => {
  const url = `${API_BASE}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${getToken()}`,
    ...options.headers
  };
  
  const response = await fetch(url, { ...options, headers });
  const data = await response.json();
  if (!response.ok) throw new Error(data.mensaje || 'Error en la petición');
  return data;
};

// State
const clientes = ref([]);
const searchQuery = ref('');
const showModal = ref(false);
const isEditing = ref(false);
const loading = ref(false);
const showSuccessBanner = ref(false);
const successMessage = ref('');

const form = ref({
  clienteID: 0,
  nombreCompleto: '',
  documentoIdentidad: '',
  telefono: '',
  correo: '',
  direccion: '',
  fechaNacimiento: '',
  activo: true
});

// Logic
const fetchClientes = async () => {
  try {
    const res = await apiFetch('/clientes');
    clientes.value = res.data;
  } catch (e) {
    console.error("Error al cargar clientes", e);
  }
};

const isBirthdayToday = (dateStr) => {
  if (!dateStr) return false;
  const bday = new Date(dateStr);
  const today = new Date();
  return bday.getDate() === (today.getDate() + 1) && bday.getMonth() === today.getMonth(); // +1 adjust for UTC/ISO weirdness usually
};

// Birthday logic is tricky with timezones, let's simplify for the client
const isActualBirthday = (dateStr) => {
  if (!dateStr) return false;
  const bday = new Date(dateStr);
  const today = new Date();
  // We compare day and month only
  const bdayDate = bday.getUTCDate();
  const bdayMonth = bday.getUTCMonth();
  const todayDate = today.getDate();
  const todayMonth = today.getMonth();
  return bdayDate === todayDate && bdayMonth === todayMonth;
};

const formatBirthdate = (dateStr) => {
  if (!dateStr) return 'No registrado';
  const options = { day: 'numeric', month: 'long' };
  return new Date(dateStr).toLocaleDateString('es-ES', options);
};

const filteredClientes = computed(() => {
  let list = [...clientes.value];
  
  // 1. Search filter
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    list = list.filter(c => 
      c.nombreCompleto.toLowerCase().includes(q) || 
      c.documentoIdentidad.includes(q)
    );
  }

  // 2. Sort by birthday first, then by registration
  return list.sort((a, b) => {
    const aIsBday = isActualBirthday(a.fechaNacimiento);
    const bIsBday = isActualBirthday(b.fechaNacimiento);
    if (aIsBday && !bIsBday) return -1;
    if (!aIsBday && bIsBday) return 1;
    return 0;
  });
});

const clientesCumpleano = computed(() => {
  return clientes.value.filter(c => isActualBirthday(c.fechaNacimiento));
});

const getInitials = (name) => {
  return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
};

const resetForm = () => {
  form.value = {
    clienteID: 0,
    nombreCompleto: '',
    documentoIdentidad: '',
    telefono: '',
    correo: '',
    direccion: '',
    fechaNacimiento: '',
    activo: true
  };
};

const editClient = (c) => {
  isEditing.value = true;
  form.value = { ...c };
  if (c.fechaNacimiento) {
    form.value.fechaNacimiento = new Date(c.fechaNacimiento).toISOString().split('T')[0];
  }
  showModal.value = true;
};

const saveCliente = async () => {
  loading.value = true;
  try {
    if (isEditing.value) {
      await apiFetch(`/clientes/${form.value.clienteID}`, {
        method: 'PUT',
        body: JSON.stringify(form.value)
      });
      triggerSuccess("Cliente actualizado correctamente.");
    } else {
      await apiFetch('/clientes', {
        method: 'POST',
        body: JSON.stringify(form.value)
      });
      triggerSuccess("Nuevo cliente registrado.");
    }
    showModal.value = false;
    fetchClientes();
  } catch (e) {
    alert("Error al guardar: " + e.message);
  } finally {
    loading.value = false;
  }
};

const triggerSuccess = (msg) => {
  successMessage.value = msg;
  showSuccessBanner.value = true;
  setTimeout(() => showSuccessBanner.value = false, 3000);
};

const confirmDelete = async (c) => {
  if (confirm(`¿Estás seguro de eliminar permanentemente a ${c.nombreCompleto}?`)) {
    try {
      await apiFetch(`/clientes/${c.clienteID}`, { method: 'DELETE' });
      triggerSuccess("Cliente eliminado.");
      fetchClientes();
    } catch (e) {
      alert("Error al eliminar.");
    }
  }
};

onMounted(fetchClientes);
</script>

<style scoped>
.clientes-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.header-section h1 {
  font-size: 1.8rem;
  color: #2D3748;
  margin-bottom: 0.25rem;
}

.subtitle {
  color: #718096;
  font-size: 0.9rem;
}

.filters-card {
  padding: 1.25rem;
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 16px;
}

.search-box {
  position: relative;
  width: 350px;
}

.search-box i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #A0AEC0;
}

.search-box input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.8rem;
  border-radius: 12px;
  border: 1px solid #E2E8F0;
  background: white;
  transition: all 0.2s;
}

.search-box input:focus {
  border-color: #C3B1E1;
  box-shadow: 0 0 0 3px rgba(195, 177, 225, 0.2);
  outline: none;
}

.stats-mini {
  display: flex;
  gap: 1.5rem;
}

.stat-item {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.stat-item .label { color: #718096; font-size: 0.9rem; }
.stat-item .value { font-weight: 700; color: #2D3748; }

.highlight-birthday {
  background: #FFF5F7;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  border: 1px solid #FED7E2;
}

.highlight-birthday .value { color: #D53F8C; }

/* Table Styles */
.table-card {
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid #E2E8F0;
}

.custom-table {
  width: 100%;
  border-collapse: collapse;
}

.custom-table th {
  background: #F8FAFC;
  padding: 1rem;
  text-align: left;
  font-size: 0.85rem;
  text-transform: uppercase;
  color: #718096;
  letter-spacing: 0.05em;
}

.custom-table td {
  padding: 1rem;
  border-bottom: 1px solid #EDF2F7;
  color: #4A5568;
}

.birthday-row {
  background: linear-gradient(135deg, #FFF0F6 0%, #FFF5D6 100%) !important;
  border-left: 4px solid #ED64A6;
}

.birthday-row td {
  color: #702459;
  font-weight: 500;
}

.client-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.avatar-small {
  width: 38px;
  height: 38px;
  min-width: 38px;
  background: #C3B1E1;
  color: white;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
}

.avatar-birthday {
  background: linear-gradient(135deg, #F6AD55, #ED64A6) !important;
  font-size: 1.2rem;
  animation: pulse-bday 1.5s infinite;
}

@keyframes pulse-bday {
  0% { box-shadow: 0 0 0 0 rgba(237, 100, 166, 0.4); }
  70% { box-shadow: 0 0 0 8px rgba(237, 100, 166, 0); }
  100% { box-shadow: 0 0 0 0 rgba(237, 100, 166, 0); }
}

.client-info .name {
  font-weight: 600;
  color: #2D3748;
  margin: 0;
}

.client-info .meta {
  font-size: 0.75rem;
  color: #D53F8C;
  font-weight: 600;
  margin: 0;
}

.contact-meta p {
  margin: 0;
  font-size: 0.85rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.contact-meta i {
  color: #A0AEC0;
  width: 14px;
}

.bday-tag {
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  font-size: 0.85rem;
  background: #F1F5F9;
  color: #64748B;
}

.bday-tag.active {
  background: #FED7E2;
  color: #D53F8C;
  font-weight: 600;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-badge.active { background: #C6F6D5; color: #2F855A; }
.status-badge.inactive { background: #EDF2F7; color: #718096; }

.actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.btn-icon {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  border: 1px solid #E2E8F0;
  background: white;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}

.btn-icon.edit:hover { background: #EBF8FF; border-color: #BEE3F8; transform: scale(1.1); }
.btn-icon.delete:hover { background: #FFF5F5; border-color: #FED7E2; transform: scale(1.1); }

/* Modals */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal-card {
  background: white;
  width: 100%;
  max-width: 600px;
  border-radius: 24px;
  padding: 2rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
}

.form-group.full { grid-column: span 2; }

label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #4A5568;
  margin-bottom: 0.5rem;
}

input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid #E2E8F0;
}

input:focus {
  outline: none;
  border-color: #C3B1E1;
}

.modal-footer {
  margin-top: 2rem;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.btn-primary {
  background: linear-gradient(135deg, #C3B1E1 0%, #FFD1DC 100%);
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  border: none;
  font-weight: 600;
  cursor: pointer;
}

.btn-secondary {
  background: #F7FAFC;
  color: #4A5568;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  border: 1px solid #E2E8F0;
  cursor: pointer;
}

.success-banner {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  background: #2D3748;
  color: white;
  padding: 1rem 2rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 1rem;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.animate-fade-in { animation: fadeIn 0.4s ease-out; }
.animate-slide-up { animation: slideUp 0.3s ease-out; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
</style>
