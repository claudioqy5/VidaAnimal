<template>
  <div class="dash-container animate-fade-in">

    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Panel de Control</h1>
        <p class="page-subtitle">Monitoreo de ingresos y rentabilidad neta</p>
      </div>
      <div class="header-actions">
        <span class="live-indicator"><span class="dot"></span> En Vivo</span>
        <button class="refresh-btn" @click="cargar" :disabled="loading">
          <span v-if="loading" class="spinner-small"></span>
          <span v-else>🔄 Actualizar</span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading-full">
      <div class="spinner-big"></div>
      <p>Sincronizando finanzas...</p>
    </div>

    <template v-else>
      <!-- KPIs PRINCIPALES (CON UTILIDAD) -->
      <div class="kpi-grid">
        <div class="kpi-card glass k1">
          <div class="kpi-icon-wrap">💵</div>
          <div class="kpi-body">
            <p class="kpi-label">Ventas de Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k2">
          <div class="kpi-icon-wrap">📈</div>
          <div class="kpi-body">
            <p class="kpi-label">Utilidad de Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k3">
          <div class="kpi-icon-wrap">🏦</div>
          <div class="kpi-body">
            <p class="kpi-label">Ventas del Mes</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasMes) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k4">
          <div class="kpi-icon-wrap">💎</div>
          <div class="kpi-body">
            <p class="kpi-label">Utilidad del Mes</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaMes) }}</p>
          </div>
        </div>
      </div>

      <div class="main-grid">
        <!-- GRÁFICO SEMANAL (LUNES A DOMINGO) -->
        <div class="card chart-card">
          <div class="card-header">
            <h2 class="card-title">📅 Rendimiento Semanal (L-D)</h2>
            <div class="chart-legend">
              <span class="leg-item"><i class="dot-v"></i> Ventas</span>
              <span class="leg-item"><i class="dot-g"></i> Utilidad</span>
            </div>
          </div>
          <div class="weekly-bars-container">
            <div v-for="dia in graficoSemanal" :key="dia.dia" class="bar-column">
              <div class="bar-stack">
                <!-- Barra de Ventas -->
                <div class="bar-v" :style="{ height: getBarHeight(dia.totalVentas, maxVenta) + '%' }" :title="'Ventas: S/ ' + dia.totalVentas"></div>
                <!-- Barra de Ganancia -->
                <div class="bar-g" :style="{ height: getBarHeight(dia.totalGanancia, maxVenta) + '%' }" :title="'Ganancia: S/ ' + dia.totalGanancia"></div>
              </div>
              <div class="bar-info">
                <span class="dia-name">{{ dia.dia }}</span>
                <span class="dia-date">{{ dia.fecha }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- REPOSICIÓN DE INVENTARIO -->
        <div class="card stock-card">
          <h2 class="card-title">⚠️ Reposición de Inventario</h2>
          <div v-if="stockBajo.length === 0" class="empty-state">
            <span class="check-icon">✅</span>
            <p>Todo el stock está en niveles óptimos.</p>
          </div>
          <div class="stock-list" v-else>
            <div v-for="item in stockBajo" :key="item.nombre" class="stock-item">
              <div class="stock-info">
                <p class="stock-name">{{ item.nombre }}</p>
                <div class="stock-badge" :class="item.stockActual < 1 ? 'critical' : 'warning'">
                  {{ item.stockActual }} {{ item.unidadMedida }}
                </div>
              </div>
              <router-link to="/compras" class="restock-link">Pedir</router-link>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const stats = ref({ ventasHoy: 0, gananciaHoy: 0, ventasMes: 0, gananciaMes: 0 });
const graficoSemanal = ref([]);
const stockBajo = ref([]);
const loading = ref(true);

const getToken = () => localStorage.getItem('jwt_token');

const cargar = async () => {
  loading.value = true;
  try {
    const res = await fetch('/api/Dashboard/resumen', {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    const data = await res.json();
    if (data.success) {
      stats.value = data.stats;
      graficoSemanal.value = data.graficoSemanal;
      stockBajo.value = data.stockBajo;
    }
  } catch (e) {
    console.error("Error cargando dashboard", e);
  } finally {
    loading.value = false;
  }
};

const maxVenta = computed(() => {
  const vals = graficoSemanal.value.map(d => d.totalVentas);
  return Math.max(...vals, 1);
});

const getBarHeight = (val, max) => (val / max) * 100;
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

onMounted(cargar);
</script>

<style scoped>
.dash-container { padding: 2rem; max-width: 1400px; margin: 0 auto; color: #2D3748; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }
.page-title { font-size: 2.2rem; font-weight: 900; letter-spacing: -0.5px; }
.page-subtitle { color: #718096; font-size: 1.1rem; }

/* Header Actions */
.header-actions { display: flex; align-items: center; gap: 1.5rem; }
.live-indicator { display: flex; align-items: center; gap: 0.5rem; font-size: 0.85rem; font-weight: 700; color: #48BB78; background: #F0FFF4; padding: 0.5rem 1rem; border-radius: 99px; }
.dot { width: 8px; height: 8px; background: #48BB78; border-radius: 50%; display: inline-block; animation: pulse 1.5s infinite; }
@keyframes pulse { 0% { transform: scale(0.9); opacity: 1; } 70% { transform: scale(1.8); opacity: 0; } 100% { transform: scale(0.9); opacity: 0; } }

.refresh-btn { background: #EDF2F7; border: none; padding: 0.6rem 1.2rem; border-radius: 12px; font-weight: 700; cursor: pointer; transition: all 0.2s; color: #4A5568; }
.refresh-btn:hover { background: #E2E8F0; }

/* KPI Cards Premium */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2.5rem; }
.kpi-card { border-radius: 24px; padding: 1.75rem; display: flex; align-items: center; gap: 1.5rem; color: white; transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); box-shadow: 0 10px 30px rgba(0,0,0,0.1); overflow: hidden; position: relative; }
.kpi-card:hover { transform: translateY(-8px); }
.glass { backdrop-filter: blur(10px); background: rgba(255, 255, 255, 0.9); border: 1px solid rgba(255, 255, 255, 0.3); }

.k1 { background: linear-gradient(135deg, #FF6B6B 0%, #FF8E8E 100%); }
.k2 { background: linear-gradient(135deg, #48BB78 0%, #68D391 100%); }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #63B3ED 100%); }
.k4 { background: linear-gradient(135deg, #F6AD55 0%, #FBD38D 100%); }

.kpi-icon-wrap { font-size: 2.2rem; background: rgba(255,255,255,0.25); width: 60px; height: 60px; display: flex; align-items: center; justify-content: center; border-radius: 18px; }
.kpi-label { font-size: 0.9rem; font-weight: 700; margin-bottom: 2px; text-transform: uppercase; letter-spacing: 0.5px; opacity: 0.9; }
.kpi-value { font-size: 1.8rem; font-weight: 900; }

/* Grid Layout */
.main-grid { display: grid; grid-template-columns: 1.8fr 1fr; gap: 2rem; }
.card { background: white; border-radius: 28px; padding: 2rem; box-shadow: 0 15px 45px rgba(0,0,0,0.05); border: 1px solid #F0F4F8; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.card-title { font-size: 1.25rem; font-weight: 800; }

/* Chart Styling */
.chart-legend { display: flex; gap: 1.5rem; }
.leg-item { display: flex; align-items: center; gap: 0.5rem; font-size: 0.85rem; font-weight: 700; color: #718096; }
.dot-v { width: 10px; height: 10px; background: #EDF2F7; border-radius: 50%; }
.dot-g { width: 10px; height: 10px; background: #68D391; border-radius: 50%; }

.weekly-bars-container { display: flex; justify-content: space-between; align-items: flex-end; height: 320px; padding: 1rem 0; }
.bar-column { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; gap: 1rem; }
.bar-stack { flex: 1; display: flex; align-items: flex-end; justify-content: center; width: 100%; position: relative; gap: 6px; }
.bar-v { width: 28px; background: #F0F4F8; border-radius: 12px 12px 0 0; border: 1.5px solid #E2E8F0; transition: height 1s cubic-bezier(0.16, 1, 0.3, 1); }
.bar-g { width: 20px; background: #68D391; border-radius: 12px 12px 0 0; transition: height 1s 0.2s cubic-bezier(0.16, 1, 0.3, 1); border: 1.5px solid #48BB78; }
.bar-info { text-align: center; }
.dia-name { display: block; font-size: 0.85rem; font-weight: 800; }
.dia-date { font-size: 0.75rem; color: #A0AEC0; font-weight: 600; }

/* Stock List */
.stock-list { display: flex; flex-direction: column; gap: 1.25rem; }
.stock-item { display: flex; justify-content: space-between; align-items: center; background: #F7FAFC; padding: 1.25rem; border-radius: 20px; border: 1px solid #EDF2F7; transition: transform 0.2s; }
.stock-item:hover { transform: scale(1.02); }
.stock-name { font-weight: 700; font-size: 0.95rem; margin: 0; }
.stock-badge { padding: 0.4rem 1rem; border-radius: 99px; font-weight: 800; font-size: 0.8rem; }
.warning { background: #FFFAF0; color: #C05621; border: 1px solid #FEEBC8; }
.critical { background: #FFF5F5; color: #C53030; border: 1px solid #FED7D7; }

.restock-link { background: #553C9A; color: white; text-decoration: none; padding: 0.5rem 1rem; border-radius: 10px; font-size: 0.85rem; font-weight: 700; transition: all 0.2s; }
.restock-link:hover { background: #44337A; transform: scale(1.05); }

.empty-state { text-align: center; color: #A0AEC0; padding: 3rem 0; }
.check-icon { font-size: 3rem; margin-bottom: 1rem; display: block; }

/* Loading States */
.loading-full { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 10rem 0; }
.spinner-big { width: 60px; height: 60px; border: 6px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.animate-fade-in { animation: fadeIn 0.6s cubic-bezier(0.23, 1, 0.32, 1); }
@keyframes fadeIn { from { opacity: 0; transform: translateY(15px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1100px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .main-grid { grid-template-columns: 1fr; }
}
</style>
