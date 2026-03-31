<template>
  <div class="ventas-container animate-fade-in">
    <div class="header-section">
      <div>
        <h1>Historial de Ventas</h1>
        <p class="subtitle">Consulta las ventas realizadas por cada cliente</p>
      </div>
    </div>

    <!-- Selector de Filtros y Orden -->
    <div class="filters-card glass">
      <div class="filter-group">
        <label>Filtrar por Cliente:</label>
        <select v-model="selectedClienteID" @change="fetchVentas">
          <option value="">Todos los clientes</option>
          <option v-for="c in clientes" :key="c.clienteID" :value="c.clienteID">
            {{ c.nombreCompleto }} — {{ c.documentoIdentidad }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label>Ordenar por:</label>
        <select v-model="orderBy">
          <option value="fecha_desc">📌 Fecha: Recientes primero</option>
          <option value="fecha_asc">📌 Fecha: Antiguas primero</option>
          <option value="monto_desc">💰 Monto: Mayor a Menor</option>
          <option value="monto_asc">💰 Monto: Menor a Mayor</option>
        </select>
      </div>

      <div class="filter-group">
        <label>Filtrar por Fecha:</label>
        <div class="date-filter-row">
          <input type="date" v-model="selectedFecha" @change="fetchVentas" class="date-input" />
          <button v-if="selectedFecha" class="btn-clear-filter" @click="verTodoElHistorial" title="Ver todo el historial">
            Ver Todo
          </button>
        </div>
      </div>

      <div class="stats-mini">
        <div class="stat-item">
          <span class="label">Monto Total:</span>
          <span class="value highlight">S/ {{ totalGeneral.toFixed(2) }}</span>
        </div>
      </div>
    </div>

    <!-- Lista de Ventas -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando ventas...</p>
    </div>

    <div v-else-if="ventas.length === 0" class="empty-state">
      <p>📋 No hay ventas para mostrar.</p>
    </div>

    <div v-else class="ventas-grid">
      <div 
        class="venta-card glass" 
        v-for="v in ventasOrdenadas" 
        :key="v.ventaID"
        :class="{ 'is-expanded': expandedVentas.has(v.ventaID) }"
        @click="toggleVenta(v.ventaID)"
      >
        <!-- CABECERA RESUMIDA -->
        <div class="card-summary">
          <div class="summary-main">
            <span class="comprobante-badge">{{ v.serieComprobante }}-{{ v.numeroComprobante }}</span>
              <div class="main-info">
                <span class="cliente-name">{{ v.cliente?.nombreCompleto || 'Consumidor Final' }}</span>
                <span class="fecha-text">{{ formatDate(v.fecha) }} • 👤 {{ v.cajero || 'Sistema' }}</span>
              </div>
          </div>
          <div class="summary-price">
            <span class="total-label">Pagado:</span>
            <span class="total-value">S/ {{ Number(v.total).toFixed(2) }}</span>
            <span class="expand-icon">{{ expandedVentas.has(v.ventaID) ? '▲' : '▼' }}</span>
          </div>
        </div>

        <!-- DETALLE EXPANDIBLE -->
        <div class="card-details-wrapper" v-if="expandedVentas.has(v.ventaID)">
          <div class="details-content animate-slide-down">
            <!-- PRODUCTOS PRIMERO -->
            <div class="items-list">
              <p class="section-title">📦 Detalle de Productos:</p>
              <table class="details-table">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Cant.</th>
                    <th>P. Unit</th>
                    <th style="text-align: right;">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="d in v.detalleVentas" :key="d.ventaDetalleID">
                    <td>{{ d.producto?.nombre || 'Producto' }}</td>
                    <td>{{ d.cantidad }}</td>
                    <td>S/ {{ Number(d.precioUnitario).toFixed(2) }}</td>
                    <td class="item-sub">S/ {{ (d.cantidad * d.precioUnitario).toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- RESUMEN FINANCIERO DESPUÉS -->
            <div class="financial-breakdown">
              <div class="breakdown-row">
                <span>Subtotal Bruto</span>
                <span>S/ {{ Number(v.subTotal || 0).toFixed(2) }}</span>
              </div>
              <div class="breakdown-row discount" v-if="v.descuento > 0">
                <span>Descuento aplicado</span>
                <span>- S/ {{ Number(v.descuento).toFixed(2) }}</span>
              </div>
              <div class="breakdown-row final">
                <span>Total Recaudado</span>
                <span>S/ {{ Number(v.total).toFixed(2) }}</span>
              </div>
            </div>

            <div class="extra-info" v-if="v.observaciones">
              <p><strong>Nota:</strong> {{ v.observaciones }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const getTodayInFormat = () => new Date().toLocaleDateString('en-CA');

const ventas = ref([]);
const clientes = ref([]);
const loading = ref(false);
const selectedClienteID = ref('');
const selectedFecha = ref(getTodayInFormat());
const orderBy = ref('fecha_desc');
const expandedVentas = ref(new Set());

const verTodoElHistorial = () => {
  selectedFecha.value = '';
  fetchVentas();
};

const toggleVenta = (id) => {
  if (expandedVentas.value.has(id)) {
    expandedVentas.value.delete(id);
  } else {
    expandedVentas.value.add(id);
  }
};

const apiFetch = async (endpoint) => {
  const res = await fetch(`${API_BASE}${endpoint}`, {
    headers: { 'Authorization': `Bearer ${getToken()}` }
  });
  return res.json();
};

const fetchClientes = async () => {
  try {
    const data = await apiFetch('/clientes');
    if (data.success) clientes.value = data.data;
  } catch (e) { console.error(e); }
};

const fetchVentas = async () => {
  loading.value = true;
  try {
    let url = `${API_BASE}/ventas?`;
    if (selectedClienteID.value) url += `clienteId=${selectedClienteID.value}&`;
    if (selectedFecha.value) url += `fecha=${selectedFecha.value}&`;
    
    const res = await fetch(url, {
        headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    const data = await res.json();
    if (data.success) ventas.value = data.data;
  } catch (e) {
    console.error('Error al cargar ventas', e);
  } finally {
    loading.value = false;
  }
};

const ventasOrdenadas = computed(() => {
  return [...ventas.value].sort((a, b) => {
    if (orderBy.value === 'fecha_desc') return new Date(b.fecha) - new Date(a.fecha);
    if (orderBy.value === 'fecha_asc') return new Date(a.fecha) - new Date(b.fecha);
    if (orderBy.value === 'monto_desc') return Number(b.total) - Number(a.total);
    if (orderBy.value === 'monto_asc') return Number(a.total) - Number(b.total);
    return 0;
  });
});

const totalGeneral = computed(() => 
  ventas.value.reduce((sum, v) => sum + Number(v.total || 0), 0)
);

const formatDate = (dateStr) => {
  if (!dateStr) return '---';
  return new Date(dateStr).toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
};

onMounted(async () => {
  await fetchClientes();
  await fetchVentas();
});
</script>

<style scoped>
.ventas-container { padding: 2rem; max-width: 1200px; margin: 0 auto; }
.header-section { margin-bottom: 2rem; }
.header-section h1 { font-size: 1.8rem; color: #2D3748; margin-bottom: 0.25rem; }
.subtitle { color: #718096; font-size: 0.9rem; }

.filters-card {
  padding: 1.25rem 1.5rem; margin-bottom: 2rem;
  display: flex; justify-content: space-between; align-items: center;
  border-radius: 16px; background: white; 
  border: 1px solid #E2E8F0; box-shadow: 0 2px 8px rgba(0,0,0,0.04);
  flex-wrap: wrap; gap: 1rem;
}
.filter-group { display: flex; flex-direction: column; gap: 0.4rem; }
.filter-group label { font-size: 0.75rem; font-weight: 700; color: #718096; text-transform: uppercase; letter-spacing: 0.05em;}
.filter-group select, .date-input {
  padding: 0.7rem 1rem; border-radius: 10px; min-width: 240px;
  border: 1px solid #E2E8F0; font-family: inherit; outline: none; background: #F8FAFC;
  transition: all 0.2s;
}

.date-filter-row { display: flex; gap: 0.5rem; align-items: center; }

.btn-clear-filter {
  padding: 0.7rem 1rem; border-radius: 10px; border: 1px solid #A7C7E7;
  background: #EBF8FF; color: #2C5282; font-weight: 700; font-size: 0.8rem;
  cursor: pointer; transition: 0.2s; white-space: nowrap;
}
.btn-clear-filter:hover { background: #BEE3F8; border-color: #63B3ED; }

.stats-mini { display: flex; align-items: flex-end; }
.stat-item { display: flex; flex-direction: column; align-items: flex-end;}
.stat-item .label { color: #718096; font-size: 0.8rem; font-weight: 600; }
.stat-item .value { font-weight: 800; color: #2D3748; font-size: 1.4rem; }
.stat-item .value.highlight { 
  background: linear-gradient(135deg, #276749, #48BB78);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.ventas-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.venta-card {
  background: white; border-radius: 12px; padding: 0.25rem;
  border: 1px solid #E2E8F0; box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  transition: all 0.2s; animation: slideUp 0.3s ease;
  cursor: pointer;
}
.venta-card:hover { border-color: #A7C7E7; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.venta-card.is-expanded { border-color: #A7C7E7; box-shadow: 0 8px 24px rgba(0,0,0,0.08); }

/* CONTENIDO RESUMIDO */
.card-summary {
  display: flex; justify-content: space-between; align-items: center;
  padding: 1rem 1.25rem;
}
.summary-main { display: flex; align-items: center; gap: 1.5rem; }
.comprobante-badge {
  background: #EDF2F7; color: #4A5568; padding: 0.35rem 0.75rem;
  border-radius: 8px; font-weight: 700; font-size: 0.9rem; font-family: 'Syncopate', sans-serif;
}
.main-info { display: flex; flex-direction: column; }
.cliente-name { font-weight: 600; color: #2D3748; font-size: 1rem; }
.fecha-text { font-size: 0.8rem; color: #A0AEC0; }

.summary-price { display: flex; align-items: center; gap: 1rem; }
.total-label { font-size: 0.8rem; color: #718096; }
.total-value { 
  font-size: 1.25rem; font-weight: 800; color: #2C5282;
  background: linear-gradient(135deg, #A7C7E7, #C3B1E1);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
}
.expand-icon { color: #CBD5E0; font-size: 0.8rem; }

/* CONTENIDO EXPANDIDO */
.card-details-wrapper {
  border-top: 1px solid #F0F4F8;
  padding: 1.5rem;
  background: #FAFBFC;
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
}

.financial-breakdown {
  display: flex; justify-content: space-around;
  margin-bottom: 2rem; padding: 1.25rem;
  background: white; border-radius: 12px; border: 1px solid #E2E8F0;
}
.breakdown-row { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; }
.breakdown-row span:first-child { font-size: 0.75rem; color: #A0AEC0; font-weight: 600; text-transform: uppercase; }
.breakdown-row span:last-child { font-size: 1.1rem; font-weight: 700; color: #4A5568; }
.breakdown-row.discount span:last-child { color: #E53E3E; }
.breakdown-row.final span:last-child { color: #2C5282; font-size: 1.3rem; }

.items-list { background: white; border-radius: 12px; padding: 1.25rem; border: 1px solid #E2E8F0; }
.section-title { font-weight: 700; color: #4A5568; font-size: 0.9rem; margin-bottom: 1rem; border-left: 4px solid #A7C7E7; padding-left: 0.75rem;}

.details-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.details-table th { text-align: left; padding: 0.75rem; color: #718096; font-weight: 600; border-bottom: 2px solid #F7FAFC; }
.details-table td { padding: 0.75rem; border-bottom: 1px solid #F7FAFC; color: #4A5568; }
.item-sub { font-weight: 700; color: #2D3748; text-align: right; }

.animate-slide-down { animation: slideDown 0.3s ease-out; }
@keyframes slideDown { from { transform: translateY(-10px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }

.loading-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; padding: 4rem; gap: 1rem; color: #718096;
}
.spinner {
  width: 40px; height: 40px; border: 4px solid #E2E8F0;
  border-top-color: #A7C7E7; border-radius: 50%;
  animation: spin 1s linear infinite;
}
.empty-state {
  text-align: center; padding: 4rem; color: #A0AEC0;
  background: white; border-radius: 16px; border: 1px dashed #CBD5E0;
}
@keyframes spin { to { transform: rotate(360deg); } }
@keyframes slideUp { from { transform: translateY(10px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.glass { backdrop-filter: blur(10px); }
</style>
