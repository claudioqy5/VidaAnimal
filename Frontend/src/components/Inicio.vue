<template>
  <div class="inicio-container animate-fade-in">

    <!-- Header Premium -->
    <div class="page-header">
      <div>
        <h1 class="page-title">🌅 Resumen del Día</h1>
        <p class="page-subtitle">{{ fechaHoy }}</p>
      </div>
      <div class="header-actions">
        <div class="status-badge"><span class="dot-online"></span> Sistema Activo</div>
        <button class="refresh-btn" @click="cargarDatos" :disabled="loading">
          <span v-if="loading" class="spinner-mini"></span>
          <span v-else>🔄 Recargar</span>
        </button>
      </div>
    </div>

    <!-- KPIs Cuarteto de Éxito -->
    <div class="kpi-grid">
      <div class="kpi-card glass k1">
        <div class="kpi-icon-box">💰</div>
        <div class="kpi-body">
          <p class="kpi-label">Ventas Hoy</p>
          <p class="kpi-value">S/ {{ formatMoney(stats.ventasHoy) }}</p>
        </div>
      </div>
      <div class="kpi-card glass k2">
        <div class="kpi-icon-box">🧾</div>
        <div class="kpi-body">
          <p class="kpi-label">Transacciones</p>
          <p class="kpi-value">{{ stats.numTransacciones }}</p>
        </div>
      </div>
      <div class="kpi-card glass k3">
        <div class="kpi-icon-box">👥</div>
        <div class="kpi-body">
          <p class="kpi-label">Clientes</p>
          <p class="kpi-value">{{ stats.clientesAtendidos }}</p>
        </div>
      </div>
      <div class="kpi-card glass k4">
        <div class="kpi-icon-box">📈</div>
        <div class="kpi-body">
          <p class="kpi-label">Utilidad Real</p>
          <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
        </div>
      </div>
    </div>

    <div class="main-grid-inicio">
      
      <!-- LO MÁS VENDIDO -->
      <div class="card glass-card">
        <h2 class="card-title">🏆 Top Productos Hoy</h2>
        <div v-if="topProductos.length === 0" class="empty-state">No hay ventas registradas aún.</div>
        <div class="ranking-list" v-else>
          <div v-for="(p, i) in topProductos" :key="i" class="rank-row">
            <span class="rank-num">{{ i + 1 }}</span>
            <div class="rank-info">
              <p class="rank-name">{{ p.nombre }}</p>
              <div class="rank-track">
                <div class="rank-bar" :style="{ width: getWidth(p.totalMonto, maxMonto) + '%' }" :class="'color-'+i"></div>
              </div>
            </div>
            <div class="rank-totals">
              <span class="rank-money">S/ {{ formatMoney(p.totalMonto) }}</span>
              <span class="rank-qty">{{ p.totalUnidades }} uds</span>
            </div>
          </div>
        </div>
      </div>

      <!-- VENTAS POR HORA -->
      <div class="card glass-card">
        <h2 class="card-title">🕐 Flujo de Ventas (6am - 10pm)</h2>
        <div v-if="ventasPorHora.length === 0" class="empty-state">Esperando primera venta...</div>
        <div class="hourly-chart" v-else>
          <div v-for="h in horasCompletas" :key="h.hora" class="hour-col">
            <div class="hour-bar-wrap">
              <div class="hour-bar" :style="{ height: getWidth(h.total, maxHora) + '%' }" :title="'S/ ' + h.total"></div>
            </div>
            <span class="hour-label">{{ h.hora }}</span>
          </div>
        </div>
      </div>

    </div>

    <!-- STOCK CRÍTICO -->
    <div class="card alert-section" v-if="stockBajo.length > 0">
      <div class="alert-header">
        <h2 class="card-title">⚠️ Atención de Inventario</h2>
        <router-link to="/compras" class="link-compras">Ver todo el inventario →</router-link>
      </div>
      <div class="stock-scroll">
        <div v-for="p in stockBajo" :key="p.nombre" class="stock-card-mini">
          <p class="s-name">{{ p.nombre }}</p>
          <p class="s-val" :class="p.stockActual <= 0 ? 's-crit' : 's-warn'">{{ p.stockActual }} {{ p.unidadMedida }}</p>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const stats = ref({ ventasHoy: 0, gananciaHoy: 0, numTransacciones: 0, clientesAtendidos: 0 });
const topProductos = ref([]);
const ventasPorHora = ref([]);
const stockBajo = ref([]);
const loading = ref(true);

const fechaHoy = computed(() => {
  return new Date().toLocaleDateString('es-PE', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
});

const cargarDatos = async () => {
  loading.value = true;
  try {
    const res = await fetch('/api/reportes/inicio', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
    });
    const data = await res.json();
    if (data.success) {
      stats.value = {
        ventasHoy: data.data.totalHoy,
        gananciaHoy: data.data.gananciaHoy || 0,
        numTransacciones: data.data.numVentasHoy,
        clientesAtendidos: data.data.clientesHoy
      };
      topProductos.value = data.data.topProductosHoy;
      ventasPorHora.value = data.data.ventasPorHora;
      stockBajo.value = data.data.stockBajo;
    }
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const horasCompletas = computed(() => {
  const mapa = {};
  ventasPorHora.value.forEach(h => { mapa[h.hora] = h.total; });
  return Array.from({ length: 17 }, (_, i) => ({ hora: i + 6, total: mapa[i + 6] || 0 }));
});

const maxMonto = computed(() => Math.max(...topProductos.value.map(p => p.totalMonto), 1));
const maxHora = computed(() => Math.max(...horasCompletas.value.map(h => h.total), 1));

const getWidth = (val, max) => (val / max) * 100;
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

onMounted(cargarDatos);
</script>

<style scoped>
.inicio-container { padding: 2rem; max-width: 1400px; margin: 0 auto; color: #2D3748; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }
.page-title { font-size: 2.2rem; font-weight: 900; letter-spacing: -0.5px; }
.page-subtitle { color: #718096; font-size: 1.1rem; text-transform: capitalize; }

.header-actions { display: flex; align-items: center; gap: 1.5rem; }
.status-badge { background: #F0FFF4; color: #38A169; padding: 0.5rem 1rem; border-radius: 99px; font-weight: 700; font-size: 0.85rem; display: flex; align-items: center; gap: 0.5rem; }
.dot-online { width: 8px; height: 8px; background: #38A169; border-radius: 50%; display: inline-block; animation: pulse 1.5s infinite; }
@keyframes pulse { 0% { transform: scale(0.9); opacity: 1; } 70% { transform: scale(1.8); opacity: 0; } 100% { transform: scale(0.9); opacity: 0; } }

.refresh-btn { background: white; border: 1.5px solid #E2E8F0; padding: 0.6rem 1.2rem; border-radius: 14px; font-weight: 700; cursor: pointer; transition: all 0.3s; }
.refresh-btn:hover { background: #EDF2F7; transform: rotate(15deg); }

.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2.5rem; }
.kpi-card { border-radius: 24px; padding: 1.75rem; display: flex; align-items: center; gap: 1.25rem; color: white; box-shadow: 0 10px 25px rgba(0,0,0,0.08); transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
.kpi-card:hover { transform: translateY(-7px); }

.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.k2 { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }
.k3 { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); }
.k4 { background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%); }

.kpi-icon-box { font-size: 2.2rem; width: 60px; height: 60px; background: rgba(255,255,255,0.2); display: flex; align-items: center; justify-content: center; border-radius: 18px; }
.kpi-label { font-size: 0.85rem; font-weight: 700; opacity: 0.9; text-transform: uppercase; margin-bottom: 0px; }
.kpi-value { font-size: 1.8rem; font-weight: 900; }

.main-grid-inicio { display: grid; grid-template-columns: 1fr 1fr; gap: 2rem; margin-bottom: 2rem; }
.card { background: white; border-radius: 28px; padding: 2rem; box-shadow: 0 15px 40px rgba(0,0,0,0.04); border: 1px solid #F0F4F8; }
.card-title { font-size: 1.2rem; font-weight: 800; margin-bottom: 1.5rem; }

.ranking-list { display: flex; flex-direction: column; gap: 1.25rem; }
.rank-row { display: grid; grid-template-columns: 35px 1fr 120px; gap: 1rem; align-items: center; }
.rank-num { font-weight: 900; color: #CBD5E0; font-size: 1.1rem; }
.rank-name { font-weight: 700; font-size: 0.95rem; margin-bottom: 5px; }
.rank-track { background: #F7FAFC; height: 10px; border-radius: 99px; overflow: hidden; }
.rank-bar { height: 100%; border-radius: 99px; transition: width 1s linear; }
.color-0 { background: #667eea; } .color-1 { background: #f093fb; } .color-2 { background: #4facfe; }

.rank-totals { text-align: right; line-height: 1.2; }
.rank-money { display: block; font-weight: 800; color: #2D3748; font-size: 0.95rem; }
.rank-qty { font-size: 0.75rem; font-weight: 700; color: #A0AEC0; }

.hourly-chart { display: flex; align-items: flex-end; justify-content: space-between; height: 200px; padding: 1rem 0; gap: 6px; }
.hour-col { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; gap: 0.75rem; }
.hour-bar-wrap { flex: 1; width: 100%; display: flex; align-items: flex-end; justify-content: center; background: #F8FAFC; border-radius: 8px; }
.hour-bar { width: 100%; min-height: 4px; background: linear-gradient(to top, #667eea, #764ba2); border-radius: 8px 8px 4px 4px; transition: height 1s; }
.hour-label { font-size: 0.65rem; font-weight: 800; color: #A0AEC0; }

.alert-section { border-left: 6px solid #F6AD55; }
.alert-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.link-compras { font-size: 0.9rem; font-weight: 700; color: #553C9A; text-decoration: none; }

.stock-scroll { display: flex; gap: 1rem; overflow-x: auto; padding-bottom: 0.5rem; }
.stock-card-mini { min-width: 180px; background: #F7FAFC; padding: 1rem; border-radius: 18px; border: 1px solid #E2E8F0; }
.s-name { font-weight: 700; font-size: 0.85rem; margin-bottom: 5px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.s-val { font-weight: 800; font-size: 1rem; }
.s-warn { color: #DD6B20; }
.s-crit { color: #E53E3E; }

.empty-state { text-align: center; color: #CBD5E0; padding: 3rem 0; font-weight: 700; }

.animate-fade-in { animation: fadeIn 0.6s cubic-bezier(0.23, 1, 0.32, 1); }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1100px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .main-grid-inicio { grid-template-columns: 1fr; }
}
</style>
