<template>
  <div class="kardex-container animate-fade-in">
    <div class="header-section">
      <div>
        <h1>Kardex de Inventario</h1>
        <p class="subtitle">Historial completo de movimientos de stock</p>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filters-card glass">
      <div class="filter-row">
        <div class="filter-group">
          <label>Fecha:</label>
          <input type="date" v-model="filtroFecha" class="date-input" />
        </div>
        <div class="filter-group">
          <label>Tipo:</label>
          <select v-model="filterTipo" class="select-input">
            <option value="">Todos los tipos</option>
            <option value="ENTRADA">📥 Entradas</option>
            <option value="SALIDA">📤 Salidas</option>
            <option value="AJUSTE">🔧 Ajustes</option>
          </select>
        </div>
        <div class="filter-group">
          <label>Responsable:</label>
          <select v-model="filtroResponsable" class="select-input">
            <option value="">Todos</option>
            <option v-for="resp in responsablesUnicos" :key="resp" :value="resp">{{ resp }}</option>
          </select>
        </div>
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input type="text" v-model="searchQuery" placeholder="Producto u observación..." />
        </div>
        <button v-if="filtroFecha || filterTipo || filtroResponsable || searchQuery" @click="limpiarFiltros" class="btn-clear">Limpiar</button>
      </div>
      
      <div class="stats-row">
        <span class="stat-badge">Mostrando: {{ filteredMovimientos.length }} registros</span>
      </div>
    </div>

    <!-- Tabla -->
    <div class="table-card glass">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Cargando movimientos...</p>
      </div>
      <table class="custom-table" v-else>
        <thead>
          <tr>
            <th>#</th>
            <th>Fecha</th>
            <th>Tipo</th>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Stock Anterior</th>
            <th>Stock Nuevo</th>
            <th>Responsable</th>
            <th>N° BOLETA</th>
            <th>Observaciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in filteredMovimientos" :key="m.movimientoID">
            <td class="id-cell">{{ m.movimientoID }}</td>
            <td>{{ formatDate(m.fecha) }}</td>
            <td>
              <span :class="['tipo-badge', m.tipo.toLowerCase()]">
                {{ m.tipo === 'ENTRADA' ? '📥' : m.tipo === 'SALIDA' ? '📤' : '🔧' }} {{ m.tipo }}
              </span>
            </td>
            <td class="product-cell">
              <span class="prod-name">{{ m.productoNombre }}</span>
            </td>
            <td class="qty-cell">
              <span :class="m.tipo === 'SALIDA' ? 'qty-out' : 'qty-in'">
                {{ m.tipo === 'SALIDA' ? '-' : '+' }}{{ m.cantidad }}
              </span>
            </td>
            <td>{{ m.stockAnterior ?? '---' }}</td>
            <td>{{ m.stockNuevo ?? '---' }}</td>
            <td class="font-medium">{{ m.usuarioNombre || 'Sistema' }}</td>
            <td>
              <span v-if="m.referenciaID" class="ref-badge">
                #{{ m.referenciaID }}
              </span>
              <span v-else class="text-muted">---</span>
            </td>
            <td class="obs-cell">{{ m.observaciones || '---' }}</td>
          </tr>
          <tr v-if="filteredMovimientos.length === 0">
            <td colspan="10" class="no-results">No se encontraron movimientos.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const movimientos = ref([]);
const loading = ref(true);
const searchQuery = ref('');
const filterTipo = ref('');
const filtroFecha = ref(new Date().toISOString().split('T')[0]); // Por defecto hoy
const filtroResponsable = ref('');

const limpiarFiltros = () => {
  searchQuery.value = '';
  filterTipo.value = '';
  filtroFecha.value = '';
  filtroResponsable.value = '';
};

const fetchKardex = async () => {
  loading.value = true;
  try {
    const res = await fetch(`${API_BASE}/kardex`, {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    const data = await res.json();
    if (data.success) movimientos.value = data.data;
  } catch (e) {
    console.error('Error al cargar kardex', e);
  } finally {
    loading.value = false;
  }
};

const responsablesUnicos = computed(() => {
  const resp = movimientos.value.map(m => m.usuarioNombre || 'Sistema').filter(Boolean);
  return [...new Set(resp)].sort();
});

const filteredMovimientos = computed(() => {
  let list = [...movimientos.value];
  
  if (filtroFecha.value) {
    list = list.filter(m => m.fecha && m.fecha.startsWith(filtroFecha.value));
  }
  
  if (filterTipo.value) list = list.filter(m => m.tipo === filterTipo.value);
  
  if (filtroResponsable.value) {
    list = list.filter(m => (m.usuarioNombre || 'Sistema') === filtroResponsable.value);
  }
  
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    list = list.filter(m => 
      (m.observaciones || '').toLowerCase().includes(q) ||
      (m.productoNombre || '').toLowerCase().includes(q) ||
      (m.usuarioNombre || '').toLowerCase().includes(q) ||
      String(m.referenciaID || '').includes(q)
    );
  }
  return list;
});

const formatDate = (dateStr) => {
  if (!dateStr) return '---';
  return new Date(dateStr).toLocaleString('es-PE', { 
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
};

onMounted(fetchKardex);
</script>

<style scoped>
.kardex-container { padding: 2rem; margin: 0 auto; }
.header-section { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.header-section h1 { font-size: 1.8rem; color: #2D3748; margin-bottom: 0.25rem; }
.subtitle { color: #718096; font-size: 0.9rem; }

.filters-card { background: white; padding: 1.25rem; border-radius: 16px; margin-bottom: 1.5rem; border: 1px solid #E2E8F0; display: flex; flex-direction: column; gap: 1rem; box-shadow: 0 2px 8px rgba(0,0,0,0.04); }
.filter-row { display: flex; gap: 1rem; flex-wrap: wrap; align-items: flex-end; }
.filter-group { display: flex; flex-direction: column; gap: 0.25rem; }
.filter-group label { font-size: 0.8rem; font-weight: 600; color: #718096; }
.date-input, .select-input { padding: 0.6rem 1rem; border-radius: 10px; border: 1px solid #E2E8F0; outline: none; color: #2D3748; background: #F8FAFC; }
.search-box { display: flex; align-items: center; background: #F7FAFC; border: 1px solid #E2E8F0; border-radius: 10px; padding: 0.1rem 1rem; flex: 1; min-width: 200px; height: 38px;}
.search-box input { border: none; background: transparent; padding: 0.5rem; width: 100%; outline: none; color: #2D3748; }
.btn-clear { padding: 0.6rem 1rem; background: #FED7D7; color: #9B2335; border: none; border-radius: 10px; font-weight: 600; cursor: pointer; }
.stats-row { font-size: 0.85rem; color: #A0AEC0; }
.stat-badge { background: #EBF8FF; color: #2B6CB0; padding: 0.3rem 0.8rem; border-radius: 20px; font-weight: 600; }


.table-card {
  border-radius: 16px; overflow: hidden;
  border: 1px solid #E2E8F0; background: white;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}
.custom-table { width: 100%; border-collapse: collapse; }
.custom-table th {
  background: #F8FAFC; padding: 0.875rem 1rem;
  text-align: left; font-size: 0.8rem;
  text-transform: uppercase; color: #718096; letter-spacing: 0.05em;
}
.custom-table td {
  padding: 0.875rem 1rem; border-bottom: 1px solid #EDF2F7;
  color: #4A5568; font-size: 0.9rem;
}
.custom-table tr:last-child td { border-bottom: none; }
.custom-table tr:hover td { background: #F8FAFC; }

.id-cell { color: #A0AEC0; font-size: 0.8rem; }
.product-cell .prod-name {
  font-weight: 600;
  color: #2D3748;
}
.obs-cell { max-width: 250px; font-size: 0.85rem; color: #718096; }

.tipo-badge {
  padding: 0.3rem 0.75rem; border-radius: 20px;
  font-size: 0.8rem; font-weight: 600; white-space: nowrap;
}
.tipo-badge.entrada { background: #C6F6D5; color: #276749; }
.tipo-badge.salida { background: #FED7D7; color: #9B2335; }
.tipo-badge.ajuste { background: #FEEBC8; color: #92400E; }

.qty-in { color: #276749; font-weight: 700; }
.qty-out { color: #9B2335; font-weight: 700; }

.ref-badge {
  padding: 0.2rem 0.6rem; border-radius: 8px;
  background: #EBF8FF; color: #2B6CB0;
  font-size: 0.8rem; font-weight: 600;
}
.text-muted { color: #A0AEC0; }

.no-results { text-align: center; padding: 3rem; color: #A0AEC0; }

.loading-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; padding: 4rem; gap: 1rem; color: #718096;
}
.spinner {
  width: 40px; height: 40px; border: 4px solid #E2E8F0;
  border-top-color: #A7C7E7; border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.glass { backdrop-filter: blur(10px); }
</style>
