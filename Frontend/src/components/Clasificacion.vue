<template>
  <div class="clasificacion-container fade-in">
    <div class="header-section">
      <h1 class="page-title">Gestión de Categorías y Especies</h1>
      <p class="page-subtitle">Administra los atributos utilizados para clasificar los productos.</p>
    </div>

    <div class="grid-layout">
      <!-- PANEL DE CATEGORIAS -->
      <div class="card">
        <div class="card-header">
          <h2>🏷️ Categorías</h2>
          <button @click="abrirModal('categoria')" class="btn-primary">+ Nueva Categoría</button>
        </div>
        
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="cat in categorias" :key="cat.categoriaID">
                <td>{{ cat.categoriaID }}</td>
                <td>{{ cat.nombre }}</td>
                <td>
                  <span :class="['status-badge', cat.activo ? 'status-active' : 'status-inactive']">
                    {{ cat.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td class="actions">
                  <button @click="editarItem(cat, 'categoria')" class="btn-icon edit" title="Editar">✏️</button>
                  <button v-if="cat.activo" @click="eliminarItem(cat.categoriaID, 'categoria')" class="btn-icon delete" title="Inactivar">❌</button>
                </td>
              </tr>
              <tr v-if="categorias.length === 0">
                <td colspan="4" class="empty-state">No hay categorías registradas.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- PANEL DE ESPECIES -->
      <div class="card">
        <div class="card-header">
          <h2>🐾 Especies (Mascotas)</h2>
          <button @click="abrirModal('especie')" class="btn-primary">+ Nueva Especie</button>
        </div>
        
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="esp in especies" :key="esp.especieID">
                <td>{{ esp.especieID }}</td>
                <td>{{ esp.nombre }}</td>
                <td>
                  <span :class="['status-badge', esp.activo ? 'status-active' : 'status-inactive']">
                    {{ esp.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td class="actions">
                  <button @click="editarItem(esp, 'especie')" class="btn-icon edit" title="Editar">✏️</button>
                  <button v-if="esp.activo" @click="eliminarItem(esp.especieID, 'especie')" class="btn-icon delete" title="Inactivar">❌</button>
                </td>
              </tr>
              <tr v-if="especies.length === 0">
                <td colspan="4" class="empty-state">No hay especies registradas.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- MODAL -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-content fade-in">
        <h3 class="modal-title">{{ editando ? 'Editar' : 'Nueva' }} {{ tipoActual === 'categoria' ? 'Categoría' : 'Especie' }}</h3>
        
        <form @submit.prevent="guardarItem">
          <div class="form-group">
            <label>Nombre</label>
            <input type="text" v-model="formData.nombre" required placeholder="Ej: Alimentos, Perros..." class="form-control" />
          </div>

          <div class="form-group checkbox-group" v-if="editando">
            <label>
              <input type="checkbox" v-model="formData.activo" />
              Estado Activo
            </label>
          </div>

          <div class="modal-actions">
            <button type="button" @click="cerrarModal" class="btn-secondary">Cancelar</button>
            <button type="submit" class="btn-primary" :disabled="guardando">
              {{ guardando ? 'Guardando...' : 'Guardar' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';

const categorias = ref([]);
const especies = ref([]);

const showModal = ref(false);
const tipoActual = ref('categoria'); // 'categoria' o 'especie'
const editando = ref(false);
const guardando = ref(false);

const formData = ref({ id: null, nombre: '', activo: true });

const getToken = () => localStorage.getItem('jwt_token');

const cargarDatos = async () => {
  try {
    const headers = { 'Authorization': `Bearer ${getToken()}` };
    
    const [resCat, resEsp] = await Promise.all([
      fetch('/api/Clasificacion/categorias', { headers }),
      fetch('/api/Clasificacion/especies', { headers })
    ]);

    const dataCat = await resCat.json();
    const dataEsp = await resEsp.json();

    if (dataCat.success) categorias.value = dataCat.data;
    if (dataEsp.success) especies.value = dataEsp.data;
  } catch (error) {
    console.error("Error al cargar datos:", error);
    alert("Error al cargar clasificaciones");
  }
};

const abrirModal = (tipo) => {
  tipoActual.value = tipo;
  editando.value = false;
  formData.value = { id: null, nombre: '', activo: true };
  showModal.value = true;
};

const cerrarModal = () => {
  showModal.value = false;
};

const editarItem = (item, tipo) => {
  tipoActual.value = tipo;
  editando.value = true;
  formData.value = { 
    id: tipo === 'categoria' ? item.categoriaID : item.especieID,
    nombre: item.nombre, 
    activo: item.activo 
  };
  showModal.value = true;
};

const guardarItem = async () => {
  guardando.value = true;
  try {
    const endpoint = tipoActual.value === 'categoria' ? '/api/Clasificacion/categorias' : '/api/Clasificacion/especies';
    const method = editando.value ? 'PUT' : 'POST';
    const finalUrl = editando.value ? `${endpoint}/${formData.value.id}` : endpoint;

    const payload = tipoActual.value === 'categoria' 
      ? { CategoriaID: formData.value.id || 0, Nombre: formData.value.nombre, Activo: formData.value.activo }
      : { EspecieID: formData.value.id || 0, Nombre: formData.value.nombre, Activo: formData.value.activo };

    const res = await fetch(finalUrl, {
      method,
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}` 
      },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (data.success) {
      await cargarDatos();
      cerrarModal();
    } else {
      alert("Error al guardar: " + data.message);
    }
  } catch (error) {
    console.error("Error al guardar:", error);
    alert("Ocurrió un error");
  } finally {
    guardando.value = false;
  }
};

const eliminarItem = async (id, tipo) => {
  if (!confirm(`¿Estás seguro de eliminar esta ${tipo}?`)) return;

  try {
    const endpoint = tipo === 'categoria' ? '/api/Clasificacion/categorias' : '/api/Clasificacion/especies';
    const res = await fetch(`${endpoint}/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${getToken()}` }
    });

    const data = await res.json();
    if (data.success) {
      await cargarDatos();
    } else {
      alert("Error al eliminar: " + data.message);
    }
  } catch (error) {
    console.error("Error al eliminar:", error);
  }
};

onMounted(() => {
  cargarDatos();
});
</script>

<style scoped>
.clasificacion-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.header-section {
  background: white;
  padding: 1.5rem 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #2d3748;
  margin: 0 0 0.5rem 0;
}

.page-subtitle {
  color: #718096;
  margin: 0;
}

.grid-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 1024px) {
  .grid-layout {
    grid-template-columns: 1fr;
  }
}

.card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.card-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #edf2f7;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2d3748;
  margin: 0;
}

.table-container {
  overflow-x: auto;
  padding: 1rem;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th, .data-table td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid #edf2f7;
}

.data-table th {
  background-color: #f7fafc;
  font-weight: 600;
  color: #4a5568;
  font-size: 0.875rem;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-active { background-color: #c6f6d5; color: #22543d; }
.status-inactive { background-color: #fed7d7; color: #822727; }

.actions {
  display: flex;
  gap: 0.5rem;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.25rem;
  padding: 0.25rem;
  transition: transform 0.2s;
}

.btn-icon:hover { transform: scale(1.1); }

.empty-state {
  text-align: center;
  color: #a0aec0;
  padding: 2rem !important;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #2d3748;
  margin: 0 0 1.5rem 0;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #4a5568;
  margin-bottom: 0.5rem;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.form-control:focus {
  border-color: #667eea;
}

.checkbox-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}

/* Buttons */
.btn-primary {
  background: #667eea;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-primary:hover:not(:disabled) { background: #5a67d8; }
.btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

.btn-secondary {
  background: #edf2f7;
  color: #4a5568;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-secondary:hover { background: #e2e8f0; }

.fade-in { animation: fadeIn 0.3s ease; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
