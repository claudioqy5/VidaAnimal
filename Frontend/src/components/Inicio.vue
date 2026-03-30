<template>
  <div class="inicio-container animate-fade-in">

    <!-- Header -->
    <div class="page-header">
      <div>
        <h1 class="page-title">🌅 Resumen del Día</h1>
        <p class="page-subtitle">{{ fechaHoy }}</p>
      </div>
      <button class="refresh-btn" @click="cargarDatos" :class="{ spinning: loading }">🔄</button>
    </div>

    <!-- KPIs principales -->
    <div class="kpi-grid">
      <div class="kpi-card kpi-ventas">
        <div class="kpi-icon">💰</div>
        <div class="kpi-info">
          <p class="kpi-label">Ventas Hoy</p>
          <p class="kpi-value">S/ {{ formatMoney(datos.totalHoy) }}</p>
        </div>
      </div>
      <div class="kpi-card kpi-num">
        <div class="kpi-icon">🧾</div>
        <div class="kpi-info">
          <p class="kpi-label">Nº Transacciones</p>
          <p class="kpi-value">{{ datos.numVentasHoy }}</p>
        </div>
      </div>
      <div class="kpi-card kpi-clientes">
        <div class="kpi-icon">👤</div>
        <div class="kpi-info">
          <p class="kpi-label">Clientes Atendidos</p>
          <p class="kpi-value">{{ datos.clientesHoy }}</p>
        </div>
      </div>
    </div>

    <div class="grid-two">

      <!-- Productos más vendidos (gráfico de barras) -->
      <div class="card">
        <h2 class="card-title">🏆 Lo mas vendido</h2>
        <div v-if="datos.topProductosHoy?.length === 0" class="empty-msg">
          Aún no hay ventas hoy.
        </div>
        <div class="bar-chart" v-else>
          <div
            v-for="(p, i) in datos.topProductosHoy"
            :key="i"
            class="bar-row"
          >
            <div class="bar-label">{{ p.nombre }}</div>
            <div class="bar-track">
              <div
                class="bar-fill"
                :style="{ width: barWidth(p.totalUnidades, maxUnidades) + '%' }"
                :class="['bar-color-' + i]"
              >
                <span class="bar-val">{{ p.totalUnidades }} uds</span>
              </div>
            </div>
            <div class="bar-money">S/ {{ formatMoney(p.totalMonto) }}</div>
          </div>
        </div>
      </div>

      <!-- Ventas por hora -->
      <div class="card">
        <h2 class="card-title">🕐 Ventas por Hora</h2>
        <div v-if="datos.ventasPorHora?.length === 0" class="empty-msg">
          Aún no hay ventas hoy.
        </div>
        <div class="hora-chart" v-else>
          <div
            v-for="h in horasCompletas"
            :key="h.hora"
            class="hora-col"
          >
            <div class="hora-bar-wrap">
              <div
                class="hora-bar"
                :style="{ height: barWidth(h.total, maxHoraTotal) + '%' }"
                :title="'S/ ' + formatMoney(h.total)"
              ></div>
            </div>
            <div class="hora-label">{{ h.hora }}h</div>
          </div>
        </div>
      </div>

    </div>

    <!-- Stock Bajo (alerta) -->
    <div class="card alert-card" v-if="datos.stockBajo?.length > 0">
      <h2 class="card-title">⚠️ Productos con Stock Bajo</h2>
      <div class="stock-grid">
        <div v-for="p in datos.stockBajo" :key="p.productoID" class="stock-item">
          <span class="stock-name">{{ p.nombre }}</span>
          <div class="stock-bar-wrap">
            <div
              class="stock-bar"
              :class="p.stockActual === 0 ? 'stock-empty' : 'stock-low'"
              :style="{ width: Math.min((p.stockActual / p.stockMinimo) * 100, 100) + '%' }"
            ></div>
          </div>
          <span class="stock-nums">{{ p.stockActual }} / {{ p.stockMinimo }}</span>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const loading = ref(false);
const datos = ref({
  totalHoy: 0,
  numVentasHoy: 0,
  clientesHoy: 0,
  ventasPorHora: [],
  topProductosHoy: [],
  stockBajo: []
});

const fechaHoy = computed(() => new Date().toLocaleDateString('es-PE', {
  weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
}));

const maxUnidades = computed(() =>
  Math.max(...(datos.value.topProductosHoy?.map(p => p.totalUnidades) || [1]))
);

const maxHoraTotal = computed(() =>
  Math.max(...(horasCompletas.value.map(h => h.total) || [1]))
);

const horasCompletas = computed(() => {
  const mapa = {};
  (datos.value.ventasPorHora || []).forEach(h => { mapa[h.hora] = h.total; });
  // Solo mostrar de 8am a 10pm
  return Array.from({ length: 15 }, (_, i) => ({
    hora: i + 8,
    total: mapa[i + 8] || 0
  }));
});

const barWidth = (val, max) => max > 0 ? Math.max((val / max) * 100, 4) : 4;
const formatMoney = (n) => Number(n || 0).toFixed(2);

const cargarDatos = async () => {
  loading.value = true;
  try {
    const res = await fetch(`${API_BASE}/reportes/inicio`, {
      headers: { Authorization: `Bearer ${getToken()}` }
    });
    const json = await res.json();
    if (json.success) datos.value = json.data;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

onMounted(cargarDatos);
</script>

<style scoped>
.inicio-container { padding: 2rem; max-width: 1300px; margin: 0 auto; }

.page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2rem; }
.page-title { font-size: 1.9rem; font-weight: 800; color: #2D3748; margin: 0 0 0.25rem 0; }
.page-subtitle { color: #718096; font-size: 0.95rem; text-transform: capitalize; }
.refresh-btn {
  background: white; border: 1px solid #E2E8F0; border-radius: 10px;
  padding: 0.6rem 0.9rem; cursor: pointer; font-size: 1.2rem;
  transition: all 0.3s; box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}
.refresh-btn:hover { transform: rotate(30deg); }
.spinning { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* KPIs */
.kpi-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1.25rem; margin-bottom: 1.5rem; }
.kpi-card {
  border-radius: 16px; padding: 1.5rem;
  display: flex; align-items: center; gap: 1.25rem;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
  transition: transform 0.2s;
}
.kpi-card:hover { transform: translateY(-3px); }
.kpi-ventas { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; }
.kpi-num { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; }
.kpi-clientes { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; }
.kpi-ticket { background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%); color: white; }
.kpi-icon { font-size: 2rem; }
.kpi-label { font-size: 0.8rem; opacity: 0.85; margin: 0; font-weight: 500; }
.kpi-value { font-size: 1.7rem; font-weight: 800; margin: 0; }

/* Grid dos columnas */
.grid-two { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1.5rem; }
.card {
  background: white; border-radius: 16px; padding: 1.5rem;
  border: 1px solid #E2E8F0; box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}
.card-title { font-size: 1rem; font-weight: 700; color: #2D3748; margin: 0 0 1.25rem 0; }
.empty-msg { color: #A0AEC0; text-align: center; padding: 2rem; font-size: 0.9rem; }

/* Barras de productos */
.bar-chart { display: flex; flex-direction: column; gap: 0.75rem; }
.bar-row { display: grid; grid-template-columns: 130px 1fr 80px; gap: 0.5rem; align-items: center; }
.bar-label { font-size: 0.8rem; color: #4A5568; font-weight: 600; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.bar-track { background: #F7FAFC; border-radius: 999px; height: 28px; overflow: hidden; }
.bar-fill { height: 100%; border-radius: 999px; display: flex; align-items: center; padding-left: 0.5rem; transition: width 0.8s cubic-bezier(.25,.8,.25,1); }
.bar-val { font-size: 0.75rem; font-weight: 700; color: white; white-space: nowrap; }
.bar-color-0 { background: linear-gradient(90deg, #667eea, #764ba2); }
.bar-color-1 { background: linear-gradient(90deg, #f093fb, #f5576c); }
.bar-color-2 { background: linear-gradient(90deg, #4facfe, #00f2fe); }
.bar-color-3 { background: linear-gradient(90deg, #43e97b, #38f9d7); }
.bar-color-4 { background: linear-gradient(90deg, #fa709a, #fee140); }
.bar-money { font-size: 0.8rem; font-weight: 700; color: #553C9A; text-align: right; }

/* Gráfico por hora */
.hora-chart {
  display: flex; align-items: flex-end; gap: 4px; height: 140px;
  padding-bottom: 24px; position: relative;
  border-bottom: 2px solid #EDF2F7;
}
.hora-col { display: flex; flex-direction: column; align-items: center; flex: 1; height: 100%; }
.hora-bar-wrap { flex: 1; display: flex; align-items: flex-end; width: 100%; }
.hora-bar {
  width: 100%; min-height: 4px; border-radius: 4px 4px 0 0;
  background: linear-gradient(180deg, #667eea, #764ba2);
  transition: height 0.6s cubic-bezier(.25,.8,.25,1);
}
.hora-label { font-size: 0.65rem; color: #A0AEC0; margin-top: 4px; }

/* Stock bajo */
.alert-card { border-left: 4px solid #F6AD55; }
.stock-grid { display: flex; flex-direction: column; gap: 0.75rem; }
.stock-item { display: grid; grid-template-columns: 200px 1fr 80px; gap: 0.75rem; align-items: center; }
.stock-name { font-weight: 600; color: #4A5568; font-size: 0.875rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.stock-bar-wrap { background: #EDF2F7; border-radius: 999px; height: 12px; overflow: hidden; }
.stock-bar { height: 100%; border-radius: 999px; transition: width 0.6s; }
.stock-low { background: linear-gradient(90deg, #F6AD55, #ED8936); }
.stock-empty { background: #FC8181; }
.stock-nums { font-size: 0.8rem; color: #718096; text-align: right; }

.animate-fade-in { animation: fadeIn 0.4s ease; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .grid-two { grid-template-columns: 1fr; }
}
</style>
