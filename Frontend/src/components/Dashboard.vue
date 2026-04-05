<template>
  <div class="dash-container animate-fade-in">

    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Inteligencia de Negocio</h1>
        <p class="page-subtitle">Monitoreo de ingresos y rentabilidad</p>
      </div>
      <div class="header-actions">
        <div class="period-switcher">
          <button :class="{ active: periodo === 'semana' }" @click="periodo = 'semana'">Semana</button>
          <button :class="{ active: periodo === 'mes' }" @click="periodo = 'mes'">Mensual</button>
        </div>
        <button class="refresh-btn" @click="cargar" :disabled="loading">
          <span v-if="loading" class="spinner-small"></span>
          <span v-else>🔄</span>
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading-full">
      <div class="spinner-big"></div>
      <p>Consultando analíticas...</p>
    </div>

    <template v-else>
      <!-- KPIs DINÁMICOS -->
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
            <p class="kpi-label">Ganancia {{ periodoLabel }}</p>
            <p class="kpi-value">S/ {{ formatMoney(currentStats.ganancia) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k3">
          <div class="kpi-icon-wrap">ATM</div>
          <div class="kpi-body">
            <p class="kpi-label">Margen de Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k4">
          <div class="kpi-icon-wrap">💎</div>
          <div class="kpi-body">
            <p class="kpi-label">Ingreso Total Histórico</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasMes) }}</p>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT -->
      <div class="main-layout">
        
        <!-- COLUMNA IZQUIERDA: GRÁFICO -->
        <div class="card chart-section">
          <div class="card-header-v2">
            <h2 class="card-title-v2">📈 Rendimiento {{ periodo === 'semana' ? 'Semanal' : 'Mensual' }}</h2>
            <div class="chart-legend">
              <span class="leg-item"><i class="dot-v"></i> Venta Bruta</span>
              <span class="leg-item"><i class="dot-g"></i> Ganancia Neta</span>
            </div>
          </div>
          
          <div class="chart-canvas-v2">
            <div v-for="item in currentChartData" :key="item.dia || item.semana" class="bar-unit">
              <!-- VALORES SOBRE LAS BARRAS -->
              <div class="bar-labels-top">
                <span class="v-val">S/ {{ formatMoney(item.totalVentas) }}</span>
                <span class="g-val">S/ {{ formatMoney(item.totalGanancia) }}</span>
              </div>
              <div class="bar-pair">
                <div class="bar-v-v2" :style="{ height: getBarHeight(item.totalVentas, maxChartVal) + '%' }"></div>
                <div class="bar-g-v2" :style="{ height: getBarHeight(item.totalGanancia, maxChartVal) + '%' }"></div>
              </div>
              <div class="bar-bottom-info">
                <strong class="b-main-label">{{ item.dia || item.semana }}</strong>
                <span class="b-sub-label">{{ item.fecha || item.rango }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUMNA DERECHA: TOPS & STOCK -->
        <div class="sidebar-info">
          
          <!-- TOP PRODUCTOS -->
          <div class="card ranking-card">
            <h2 class="card-title-v2">🏆 Top {{ periodoLabel }}</h2>
            <div v-if="currentTop.length === 0" class="empty-state">Sin datos aún.</div>
            <div class="top-list" v-else>
              <div v-for="(p, i) in currentTop" :key="p.nombre" class="top-item">
                <div class="top-rank" :class="'rank-'+i">{{ i + 1 }}</div>
                <div class="top-body">
                  <p class="top-name">{{ p.nombre }}</p>
                  <p class="top-meta">S/ {{ formatMoney(p.totalMonto) }} - ({{ p.totalUnidades }} uds)</p>
                </div>
              </div>
            </div>
          </div>

          <!-- REPOSICIÓN STOCK (OPTIMIZADA) -->
          <div class="card stock-alert-v2">
            <h2 class="card-title-v2">⚠️ Reposición Crítica</h2>
            <div class="stock-list-v2">
              <div v-for="s in stockBajo" :key="s.nombre" class="stock-row-v2">
                <div class="s-main">
                  <p class="s-name-v2">{{ s.nombre }}</p>
                  <div class="s-badges">
                    <span class="s-badge" :class="s.stockActual <= 0 ? 'red' : 'orange'">
                      Stock: {{ s.stockActual }} {{ s.unidadMedida }}
                    </span>
                  </div>
                </div>
                <router-link to="/compras" class="btn-pedir-v2">Pedir</router-link>
              </div>
              <div v-if="stockBajo.length === 0" class="empty-state">✅ Stock OK</div>
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
const topSemanal = ref([]);
const topMensual = ref([]);
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
      topSemanal.value = data.topSemanal || [];
      topMensual.value = data.topMensual || [];
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
const currentTop = computed(() => periodo.value === 'semana' ? topSemanal.value : topMensual.value);

const maxChartVal = computed(() => {
  const vals = currentChartData.value.map(d => d.totalVentas);
  return Math.max(...vals, 1);
});

const getBarHeight = (val, max) => Math.max((val / max) * 100, 3);
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

onMounted(cargar);
</script>

<style scoped>
.dash-container { padding: 1.5rem; max-width: 1500px; margin: 0 auto; color: #2D3748; }

.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.page-title { font-size: 1.8rem; font-weight: 900; letter-spacing: -1px; margin: 0; }
.page-subtitle { color: #718096; font-size: 0.95rem; margin: 0; }

.header-actions { display: flex; align-items: center; gap: 1rem; }
.period-switcher { background: #EDF2F7; padding: 4px; border-radius: 12px; display: flex; }
.period-switcher button { border: none; background: transparent; padding: 0.4rem 1.2rem; border-radius: 9px; cursor: pointer; font-size: 0.8rem; font-weight: 800; color: #718096; transition: 0.2s; }
.period-switcher button.active { background: white; color: #553C9A; box-shadow: 0 2px 6px rgba(0,0,0,0.06); }

.refresh-btn { background: #EDF2F7; border: none; padding: 0.5rem; border-radius: 10px; cursor: pointer; }

/* KPIs Modernos */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; margin-bottom: 1.5rem; }
.kpi-card { border-radius: 20px; padding: 1.25rem; display: flex; align-items: center; gap: 1rem; border: 1px solid rgba(255,255,255,0.2); box-shadow: 0 4px 20px rgba(0,0,0,0.05); color: white; transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
.kpi-card:hover { transform: translateY(-5px); }

.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.k2 { background: linear-gradient(135deg, #48BB78 0%, #38A169 100%); }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%); }
.k4 { background: linear-gradient(135deg, #F6AD55 0%, #DD6B20 100%); }

.kpi-icon-wrap { font-size: 1.8rem; background: rgba(255,255,255,0.2); width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; border-radius: 14px; }
.kpi-label { font-size: 0.7rem; font-weight: 700; text-transform: uppercase; margin: 0; opacity: 0.85; }
.kpi-value { font-size: 1.4rem; font-weight: 900; margin: 0; }

/* Layout Design */
.main-layout { display: grid; grid-template-columns: 2.2fr 1fr; gap: 1.5rem; }
.card { background: white; border-radius: 24px; padding: 1.75rem; border: 1px solid #F0F4F8; box-shadow: 0 4px 20px rgba(0,0,0,0.02); }

.card-header-v2 { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.card-title-v2 { font-size: 1.1rem; font-weight: 800; margin: 0; }

.chart-legend { display: flex; gap: 1rem; }
.leg-item { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; font-weight: 700; color: #718096; }
.dot-v { width: 8px; height: 8px; background: #EDF2F7; border-radius: 50%; }
.dot-g { width: 8px; height: 8px; background: #68D391; border-radius: 50%; }

/* Canvas Gráfico Avanzado */
.chart-canvas-v2 { display: flex; justify-content: space-around; align-items: flex-end; height: 320px; padding-top: 30px; position: relative; }
.bar-unit { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; position: relative; }

.bar-labels-top { position: absolute; top: -35px; display: flex; flex-direction: column; align-items: center; gap: 2px; }
.v-val { font-size: 0.65rem; font-weight: 800; color: #718096; background: #F7FAFC; padding: 2px 4px; border-radius: 4px; }
.g-val { font-size: 0.65rem; font-weight: 800; color: #38A169; }

.bar-pair { flex: 1; display: flex; align-items: flex-end; width: 100%; justify-content: center; gap: 6px; }
.bar-v-v2 { width: 22px; background: #F0F4F8; border-radius: 6px 6px 0 0; border: 1.5px solid #E2E8F0; transition: height 1s; }
.bar-g-v2 { width: 14px; background: #68D391; border-radius: 6px 6px 0 0; border: 1.5px solid #48BB78; transition: height 1s 0.2s; }

.bar-bottom-info { display: flex; flex-direction: column; align-items: center; margin-top: 1rem; }
.b-main-label { font-size: 0.8rem; text-transform: capitalize; }
.b-sub-label { font-size: 0.65rem; color: #A0AEC0; }

/* Sidebar Sections */
.sidebar-info { display: flex; flex-direction: column; gap: 1.5rem; }

/* Ranking List */
.ranking-card { background: linear-gradient(135deg, white 0%, #FAFBFC 100%); }
.top-list { display: flex; flex-direction: column; gap: 0.75rem; }
.top-item { display: flex; align-items: center; gap: 1rem; background: white; padding: 0.75rem; border-radius: 16px; border: 1px solid #EDF2F7; }
.top-rank { width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-weight: 900; font-size: 0.8rem; color: white; background: #CBD5E0; }
.rank-0 { background: #FFD700; transform: scale(1.1); }
.rank-1 { background: #C0C0C0; }
.rank-2 { background: #CD7F32; }

.top-name { font-weight: 700; font-size: 0.85rem; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 150px; }
.top-meta { font-size: 0.7rem; color: #718096; font-weight: 600; margin: 0; }

/* Stock Alert */
.stock-alert-v2 { border-left: 5px solid #E53E3E; }
.stock-list-v2 { display: flex; flex-direction: column; gap: 0.75rem; }
.stock-row-v2 { display: flex; justify-content: space-between; align-items: center; padding: 0.75rem; background: #FFF5F5; border-radius: 14px; border: 1px solid #FED7D7; }
.s-name-v2 { font-weight: 800; font-size: 0.85rem; margin: 0 0 4px 0; color: #2D3748; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 180px; }
.s-badge { font-size: 0.7rem; font-weight: 700; padding: 2px 8px; border-radius: 6px; }
.orange { background: #FFFAF0; color: #C05621; }
.red { background: #FFF5F5; color: #E53E3E; }

.btn-pedir-v2 { background: #553C9A; color: white; text-decoration: none; padding: 0.4rem 0.8rem; border-radius: 9px; font-size: 0.75rem; font-weight: 800; transition: all 0.2s; }
.btn-pedir-v2:hover { background: #44337A; transform: scale(1.05); }

.loading-full { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 8rem 0; }
.spinner-big { width: 50px; height: 50px; border: 5px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1200px) {
  .main-layout { grid-template-columns: 1fr; }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>
