<template>
  <div class="dash-container animate-fade-in">

    <!-- HEADER SECCIÓN -->
    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Inteligencia de Negocio</h1>
        <p class="page-subtitle">Rendimiento estratégico del punto de venta</p>
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

    <!-- PANELES DE CARGA -->
    <div v-if="loading" class="loading-full">
      <div class="spinner-big"></div>
      <p>Procesando métricas...</p>
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
            <p class="kpi-label">Utilidad de Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k4">
          <div class="kpi-icon-wrap">💎</div>
          <div class="kpi-body">
            <p class="kpi-label">Ventas Totales (Historial No Anulado)</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasMes) }}</p>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT -->
      <div class="main-layout">
        
        <!-- COLUMNA IZQUIERDA: GRÁFICO PERSONALIZADO -->
        <div class="card chart-section">
          <div class="card-header-v2">
            <h2 class="card-title-v2">📈 Fluctuación del Rendimiento</h2>
            <div class="chart-legend">
              <span class="leg-item"><i class="dot-v"></i> Venta</span>
              <span class="leg-item"><i class="dot-g"></i> Ganancia</span>
            </div>
          </div>
          
          <!-- VISTA SEMANAL (BARRAS) -->
          <div v-if="periodo === 'semana'" class="chart-canvas-v2 bars-mode animate-fade-in">
            <div v-for="item in graficoSemanal" :key="item.dia" class="bar-unit">
              <div class="bar-labels-top">
                <span class="v-val">S/ {{ formatMoney(item.totalVentas) }}</span>
                <span class="g-val">S/ {{ formatMoney(item.totalGanancia) }}</span>
              </div>
              <div class="bar-pair">
                <div class="bar-v-v2" :style="{ height: getBarHeight(item.totalVentas, maxChartVal) + '%' }"></div>
                <div class="bar-g-v2" :style="{ height: getBarHeight(item.totalGanancia, maxChartVal) + '%' }"></div>
              </div>
              <div class="bar-bottom-info">
                <strong class="b-main-label">{{ item.dia }}</strong>
                <span class="b-sub-label">{{ item.fecha }}</span>
              </div>
            </div>
          </div>

          <!-- VISTA MENSUAL (LÍNEAS) -->
          <div v-else class="chart-canvas-v2 line-mode animate-fade-in">
             <div class="line-container">
               <svg class="line-svg" viewBox="0 0 1000 300" preserveAspectRatio="none">
                 <!-- Línea de Venta -->
                 <path :d="createLinePath(graficoMensual, 'totalVentas')" class="line-v" />
                 <!-- Línea de Ganancia -->
                 <path :d="createLinePath(graficoMensual, 'totalGanancia')" class="line-g" />
               </svg>
               <div v-for="(item, idx) in graficoMensual" :key="item.semana" class="line-point-unit" :style="{ left: (idx * (100 / (graficoMensual.length - 1))) + '%' }">
                 <div class="point-labels">
                   <span class="v-tag">S/ {{ formatMoney(item.totalVentas) }}</span>
                   <span class="g-tag">S/ {{ formatMoney(item.totalGanancia) }}</span>
                 </div>
                 <div class="node-v"></div>
                 <div class="node-g"></div>
                 <div class="line-footer">
                   <strong>{{ item.semana }}</strong>
                   <small>{{ item.rango }}</small>
                 </div>
               </div>
             </div>
          </div>
        </div>

        <!-- COLUMNA DERECHA: RANKING -->
        <div class="sidebar-info">
          <div class="card ranking-card full-height">
            <div class="card-header-v2">
              <h2 class="card-title-v2">🏆 Ranking {{ periodoLabel }}</h2>
            </div>
            <div class="top-list">
              <div v-for="(p, i) in currentTop" :key="p.nombre" class="top-item animate-pop-in" :style="{ animationDelay: (i * 0.1) + 's' }">
                <div class="top-rank" :class="'rank-'+i">{{ i + 1 }}</div>
                <div class="top-body">
                  <p class="top-name">{{ p.nombre }}</p>
                  <p class="top-meta">Monto: S/ {{ formatMoney(p.totalMonto) }} | Cant: {{ p.totalUnidades }}</p>
                </div>
              </div>
              <div v-if="currentTop.length === 0" class="empty-state">No se registran ventas para este periodo.</div>
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

const cargar = async () => {
  loading.value = true;
  try {
    const res = await fetch('/api/Dashboard/resumen', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
    });
    const data = await res.json();
    if (data.success) {
      stats.value = data.stats;
      graficoSemanal.value = data.graficoSemanal;
      graficoMensual.value = data.graficoMensual;
      topSemanal.value = data.topSemanal || [];
      topMensual.value = data.topMensual || [];
    }
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const periodoLabel = computed(() => periodo.value === 'semana' ? 'Semanal' : 'Mensual');
const currentStats = computed(() => periodo.value === 'semana' ? 
  { ventas: stats.value.ventasSemana, ganancia: stats.value.gananciaSemana } : 
  { ventas: stats.value.ventasMes, ganancia: stats.value.gananciaMes });

const currentTop = computed(() => periodo.value === 'semana' ? topSemanal.value : topMensual.value);

const maxChartVal = computed(() => {
  const data = periodo.value === 'semana' ? graficoSemanal.value : graficoMensual.value;
  const vals = data.map(d => d.totalVentas);
  return Math.max(...vals, 1);
});

const getBarHeight = (val, max) => Math.max((val / max) * 100, 3);
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

const createLinePath = (data, key) => {
  if (!data || data.length < 2) return "";
  const max = maxChartVal.value;
  const step = 1000 / (data.length - 1);
  return data.map((d, i) => {
    const x = i * step;
    const y = 300 - ( (d[key] / max) * 250 + 20 );
    return `${i === 0 ? 'M' : 'L'} ${x} ${y}`;
  }).join(' ');
};

onMounted(cargar);
</script>

<style scoped>
.dash-container { padding: 1.5rem; max-width: 1400px; margin: 0 auto; color: #2D3748; }

/* Header & Switcher */
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.page-title { font-size: 1.8rem; font-weight: 900; letter-spacing: -1px; margin: 0; }
.header-actions { display: flex; align-items: center; gap: 1rem; }
.period-switcher { background: #EDF2F7; padding: 4px; border-radius: 12px; display: flex; }
.period-switcher button { border: none; background: transparent; padding: 0.4rem 1.2rem; border-radius: 9px; cursor: pointer; font-size: 0.8rem; font-weight: 800; color: #718096; transition: 0.2s; }
.period-switcher button.active { background: white; color: #553C9A; box-shadow: 0 2px 6px rgba(0,0,0,0.06); }
.refresh-btn { background: #EDF2F7; border: none; padding: 0.5rem; border-radius: 10px; cursor: pointer; }

/* KPI CARDS */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; margin-bottom: 1.5rem; }
.kpi-card { border-radius: 20px; padding: 1.25rem; display: flex; align-items: center; gap: 1rem; color: white; border: 1px solid rgba(255,255,255,0.1); transition: 0.3s; }
.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.k2 { background: linear-gradient(135deg, #48BB78 0%, #38A169 100%); }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%); }
.k4 { background: linear-gradient(135deg, #ED8936 0%, #DD6B20 100%); }
.kpi-icon-wrap { font-size: 1.6rem; background: rgba(255,255,255,0.2); width: 42px; height: 42px; display: flex; align-items: center; justify-content: center; border-radius: 12px; }
.kpi-label { font-size: 0.65rem; font-weight: 800; text-transform: uppercase; margin: 0; opacity: 0.8; }
.kpi-value { font-size: 1.3rem; font-weight: 900; margin: 0; }

/* MAIN CONTENT LAYOUT */
.main-layout { display: grid; grid-template-columns: 1fr 380px; gap: 1.5rem; min-height: 450px; }
.card { background: white; border-radius: 24px; border: 1px solid #EDF2F7; box-shadow: 0 4px 20px rgba(0,0,0,0.02); display: flex; flex-direction: column; }
.chart-section { padding: 1.5rem; }
.card-title-v2 { font-size: 1.1rem; font-weight: 900; margin: 0; color: #2D3748; }

/* Legend */
.chart-legend { display: flex; gap: 1.2rem; }
.leg-item { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; font-weight: 800; color: #A0AEC0; }
.dot-v { width: 8px; height: 8px; background: #E2E8F0; border-radius: 50%; }
.dot-g { width: 8px; height: 8px; background: #68D391; border-radius: 50%; }

/* VISTA DE BARRAS (SEMANA) */
.chart-canvas-v2 { flex: 1; display: flex; position: relative; margin-top: 1rem; }
.bars-mode { align-items: flex-end; justify-content: space-around; padding-top: 40px; }
.bar-unit { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; position: relative; }
.bar-labels-top { position: absolute; top: -45px; display: flex; flex-direction: column; align-items: center; width: 100%; }
.v-val { font-size: 0.65rem; font-weight: 900; color: #718096; background: #F7FAFC; padding: 2px 6px; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
.g-val { font-size: 0.65rem; font-weight: 900; color: #38A169; }
.bar-pair { flex: 1; display: flex; align-items: flex-end; gap: 6px; }
.bar-v-v2 { width: 22px; background: #E2E8F0; border-radius: 8px 8px 0 0; transition: height 1s; }
.bar-g-v2 { width: 14px; background: #68D391; border-radius: 8px 8px 0 0; transition: height 1s 0.2s; }
.bar-bottom-info { display: flex; flex-direction: column; align-items: center; margin-top: 1rem; padding-bottom: 0.5rem; }
.b-main-label { font-size: 0.75rem; text-transform: capitalize; color: #4A5568; }
.b-sub-label { font-size: 0.6rem; color: #A0AEC0; font-weight: 700; }

/* VISTA DE LÍNEAS (MES) */
.line-mode { padding: 40px 30px; }
.line-container { width: 100%; height: 100%; position: relative; }
.line-svg { position: absolute; top: 0; left: 0; width: 100%; height: 260px; overflow: visible; z-index: 1; }
.line-v { fill: none; stroke: #CBD5E0; stroke-width: 4; stroke-linecap: round; stroke-linejoin: round; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.05)); }
.line-g { fill: none; stroke: #68D391; stroke-width: 4; stroke-linecap: round; stroke-linejoin: round; }
.line-point-unit { position: absolute; top: 0; height: 100%; display: flex; flex-direction: column; align-items: center; z-index: 2; pointer-events: none; }
.node-v { width: 10px; height: 10px; border-radius: 50%; background: white; border: 3px solid #CBD5E0; margin-top: auto; margin-bottom: 120px; }
.node-g { width: 10px; height: 10px; border-radius: 50%; background: white; border: 3px solid #68D391; position: absolute; bottom: 80px; }
.point-labels { position: absolute; top: -30px; display: flex; flex-direction: column; align-items: center; gap: 2px; }
.v-tag { background: #EDF2F7; font-size: 0.6rem; font-weight: 900; color: #4A5568; padding: 2px 6px; border-radius: 4px; }
.g-tag { color: #38A169; font-size: 0.6rem; font-weight: 900; }
.line-footer { position: absolute; bottom: 0; display: flex; flex-direction: column; align-items: center; width: 100px; }
.line-footer strong { font-size: 0.8rem; }
.line-footer small { font-size: 0.6rem; color: #A0AEC0; }

/* RANKING SIDEBAR */
.sidebar-info { display: flex; flex-direction: column; }
.ranking-card { padding: 1.5rem; background: #F7FAFC; }
.top-list { display: flex; flex-direction: column; gap: 0.8rem; margin-top: 1rem; }
.top-item { display: flex; align-items: center; gap: 1rem; background: white; padding: 0.8rem; border-radius: 18px; box-shadow: 0 2px 10px rgba(0,0,0,0.02); border: 1px solid #EDF2F7; }
.top-rank { width: 26px; height: 26px; display: flex; align-items: center; justify-content: center; border-radius: 8px; font-weight: 900; font-size: 0.75rem; color: white; background: #CBD5E0; }
.rank-0 { background: #F6AD55; transform: scale(1.1); box-shadow: 0 4px 10px rgba(237,137,54,0.3); }
.rank-1 { background: #B2F5EA; color: #319795; }
.rank-2 { background: #FED7D7; color: #C53030; }
.top-name { font-weight: 800; font-size: 0.85rem; margin: 0; color: #2D3748; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 200px; }
.top-meta { font-size: 0.7rem; font-weight: 700; color: #718096; margin: 0; }

/* UTILS */
.animate-fade-in { animation: fadeIn 0.4s ease-out forwards; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

.animate-pop-in { animation: popIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards; opacity: 0; }
@keyframes popIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }

.loading-full { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 8rem 0; }
.spinner-big { width: 45px; height: 45px; border: 4px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1100px) {
  .main-layout { grid-template-columns: 1fr; }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>
