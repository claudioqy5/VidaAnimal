<template>
  <div class="dash-container animate-fade-in">

    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Análisis de Negocio</h1>
        <p class="page-subtitle">Rendimiento y rentabilidad neta</p>
      </div>
      <div class="header-actions">
        <div class="period-switcher">
          <button :class="{ active: periodo === 'semana' }" @click="periodo = 'semana'">Vista Semanal</button>
          <button :class="{ active: periodo === 'mes' }" @click="periodo = 'mes'">Vista Mensual</button>
        </div>
        <button class="refresh-btn" @click="cargar" :disabled="loading">
          <span v-if="loading" class="spinner-small"></span>
          <span v-else>🔄</span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading-full">
      <div class="spinner-big"></div>
      <p>Sincronizando finanzas...</p>
    </div>

    <template v-else>
      <!-- KPIs DINÁMICOS (Adaptan su valor al periodo seleccionado) -->
      <div class="kpi-grid">
        <div class="kpi-card glass k1">
          <div class="kpi-icon-wrap">💵</div>
          <div class="kpi-body">
            <p class="kpi-label">Ventas {{ periodoLabel }}</p>
            <p class="kpi-value">S/ {{ formatMoney(currentStats.ventas) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k2">
          <div class="kpi-icon-wrap">📈</div>
          <div class="kpi-body">
            <p class="kpi-label">Utilidad {{ periodoLabel }}</p>
            <p class="kpi-value">S/ {{ formatMoney(currentStats.ganancia) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k3">
          <div class="kpi-icon-wrap">🏧</div>
          <div class="kpi-body">
            <p class="kpi-label">Utilidad Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k4">
          <div class="kpi-icon-wrap">💎</div>
          <div class="kpi-body">
            <p class="kpi-label">Ventas del Mes</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasMes) }}</p>
          </div>
        </div>
      </div>

      <div class="main-grid">
        <!-- GRÁFICO (REDUCIDO) -->
        <div class="card chart-card">
          <div class="card-header-compact">
            <h2 class="card-title-compact">📅 Rendimiento {{ periodo === 'semana' ? 'Lunes a Domingo' : 'Semanas del Mes' }}</h2>
            <div class="chart-legend">
              <span class="leg-item"><i class="dot-v"></i> Venta</span>
              <span class="leg-item"><i class="dot-g"></i> Ganancia</span>
            </div>
          </div>
          <div class="weekly-bars-container">
            <div v-for="item in currentChartData" :key="item.dia || item.semana" class="bar-column">
              <div class="bar-stack">
                <div class="bar-v" :style="{ height: getBarHeight(item.totalVentas, maxChartVal) + '%' }" :title="'Venta: S/ ' + item.totalVentas"></div>
                <div class="bar-g" :style="{ height: getBarHeight(item.totalGanancia, maxChartVal) + '%' }" :title="'Ganancia: S/ ' + item.totalGanancia"></div>
              </div>
              <div class="bar-info">
                <span class="dia-name">{{ item.dia || item.semana }}</span>
                <span class="dia-date text-truncate">{{ item.fecha || item.rango }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- REPOSICIÓN MÁS COMPACTA -->
        <div class="card stock-card">
          <h2 class="card-title-compact">⚠️ Stock para Reponer</h2>
          <div v-if="stockBajo.length === 0" class="empty-state">📦 Stock OK</div>
          <div class="stock-list-compact" v-else>
            <div v-for="item in stockBajo" :key="item.nombre" class="stock-item-compact">
              <div class="s-info">
                <p class="s-name-compact text-truncate">{{ item.nombre }}</p>
                <span class="s-badge-compact" :class="item.stockActual < 1 ? 'crit' : 'warn'">
                  {{ item.stockActual }} {{ item.unidadMedida }}
                </span>
              </div>
              <router-link to="/compras" class="btn-pedir">Pedir</router-link>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const stats = ref({ ventasHoy: 0, gananciaHoy: 0, ventasSemana: 0, gananciaSemana: 0, ventasMes: 0, gananciaMes: 0 });
const graficoSemanal = ref([]);
const graficoMensual = ref([]);
const stockBajo = ref([]);
const loading = ref(true);
const periodo = ref('semana');

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
      graficoMensual.value = data.graficoMensual;
      stockBajo.value = data.stockBajo;
    }
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const periodoLabel = computed(() => periodo.value === 'semana' ? 'Semanal' : 'Mensual');

const currentStats = computed(() => {
  if (periodo.value === 'semana') {
    return { ventas: stats.value.ventasSemana, ganancia: stats.value.gananciaSemana };
  }
  return { ventas: stats.value.ventasMes, ganancia: stats.value.gananciaMes };
});

const currentChartData = computed(() => periodo.value === 'semana' ? graficoSemanal.value : graficoMensual.value);

const maxChartVal = computed(() => {
  const vals = currentChartData.value.map(d => d.totalVentas);
  return Math.max(...vals, 1);
});

const getBarHeight = (val, max) => (val / max) * 100;
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

onMounted(cargar);
</script>

<style scoped>
.dash-container { padding: 1rem; max-width: 1400px; margin: 0 auto; color: #2D3748; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.page-title { font-size: 1.5rem; font-weight: 900; letter-spacing: -0.5px; }
.page-subtitle { color: #718096; font-size: 0.85rem; }

.header-actions { display: flex; align-items: center; gap: 0.75rem; }
.period-switcher { display: flex; background: #EDF2F7; padding: 2px; border-radius: 10px; }
.period-switcher button { border: none; background: transparent; padding: 0.3rem 0.8rem; border-radius: 8px; font-size: 0.75rem; font-weight: 800; color: #718096; cursor: pointer; transition: 0.2s; }
.period-switcher button.active { background: white; color: #553C9A; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }

.refresh-btn { background: #EDF2F7; border: none; padding: 0.4rem; border-radius: 8px; cursor: pointer; }

/* KPIs Compactos */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; margin-bottom: 1rem; }
.kpi-card { border-radius: 16px; padding: 1rem; display: flex; align-items: center; gap: 0.75rem; color: white; box-shadow: 0 4px 10px rgba(0,0,0,0.04); }
.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.k2 { background: linear-gradient(135deg, #48BB78 0%, #68D391 100%); }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #63B3ED 100%); }
.k4 { background: linear-gradient(135deg, #F6AD55 0%, #FBD38D 100%); }

.kpi-icon-wrap { font-size: 1.5rem; background: rgba(255,255,255,0.2); width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; border-radius: 12px; }
.kpi-label { font-size: 0.65rem; font-weight: 700; text-transform: uppercase; margin: 0; opacity: 0.8; }
.kpi-value { font-size: 1.25rem; font-weight: 900; }

/* Main Design */
.main-grid { display: grid; grid-template-columns: 2fr 1fr; gap: 1rem; }
.card { background: white; border-radius: 20px; padding: 1.25rem; box-shadow: 0 4px 15px rgba(0,0,0,0.02); border: 1px solid #F0F4F8; }
.card-header-compact { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; }
.card-title-compact { font-size: 0.9rem; font-weight: 800; margin: 0; }

.chart-legend { display: flex; gap: 0.75rem; }
.leg-item { display: flex; align-items: center; gap: 0.3rem; font-size: 0.7rem; font-weight: 700; color: #718096; }
.dot-v { width: 7px; height: 7px; background: #EDF2F7; border-radius: 50%; }
.dot-g { width: 7px; height: 7px; background: #68D391; border-radius: 50%; }

/* Chart Bars */
.weekly-bars-container { display: flex; justify-content: space-around; align-items: flex-end; height: 200px; padding: 0.5rem 0; }
.bar-column { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; gap: 0.5rem; }
.bar-stack { flex: 1; display: flex; align-items: flex-end; justify-content: center; width: 100%; gap: 3px; }
.bar-v { width: 18px; background: #F0F4F8; border-radius: 6px 6px 0 0; border: 1.2px solid #E2E8F0; transition: height 0.8s; }
.bar-g { width: 12px; background: #68D391; border-radius: 6px 6px 0 0; transition: height 0.8s 0.1s; border: 1.2px solid #48BB78; }
.dia-name { font-size: 0.75rem; font-weight: 800; }
.dia-date { font-size: 0.6rem; color: #A0AEC0; }
.text-truncate { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 60px; text-align: center; }

/* Stock Compact */
.stock-list-compact { display: flex; flex-direction: column; gap: 0.5rem; }
.stock-item-compact { display: flex; justify-content: space-between; align-items: center; background: #F8FAFC; padding: 0.5rem 0.75rem; border-radius: 12px; border: 1px solid #EDF2F7; }
.s-name-compact { font-weight: 700; font-size: 0.8rem; margin: 0; }
.s-badge-compact { padding: 0.2rem 0.5rem; border-radius: 6px; font-weight: 800; font-size: 0.65rem; }
.crit { background: #FFF5F5; color: #C53030; }
.warn { background: #FFFAF0; color: #C05621; }
.btn-pedir { background: #553C9A; color: white; text-decoration: none; padding: 0.3rem 0.6rem; border-radius: 8px; font-size: 0.7rem; font-weight: 700; }

.loading-full { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 6rem 0; }
.spinner-big { width: 40px; height: 40px; border: 4px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1000px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .main-grid { grid-template-columns: 1fr; }
}
</style>
