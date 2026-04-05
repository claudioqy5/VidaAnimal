<template>
  <div class="dash-container animate-fade-in">

    <!-- HEADER -->
    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Inteligencia de Negocio</h1>
        <p class="page-subtitle">Analítica avanzada de rendimiento y rentabilidad</p>
      </div>
      <div class="header-actions">
        <div class="period-switcher">
          <button :class="{ active: periodo === 'semana' }" @click="periodo = 'semana'">Semanal</button>
          <button :class="{ active: periodo === 'mes' }" @click="periodo = 'mes'">Mensual</button>
        </div>
        <button class="refresh-btn" @click="cargar" :disabled="loading">🔄</button>
      </div>
    </div>

    <div v-if="loading" class="loading-full">
      <div class="spinner-big"></div>
      <p>Procesando métricas...</p>
    </div>

    <template v-else>
      <!-- KPIs SUPERIORES -->
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
            <p class="kpi-label">Utilidad Real Hoy</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
          </div>
        </div>
        <div class="kpi-card glass k4">
          <div class="kpi-icon-wrap">💎</div>
          <div class="kpi-body">
            <p class="kpi-label">Total Histórico</p>
            <p class="kpi-value">S/ {{ formatMoney(stats.ventasMes) }}</p>
          </div>
        </div>
      </div>

      <!-- GRID PRINCIPAL -->
      <div class="main-layout">
        
        <!-- SECCIÓN GRÁFICO (Eje Y + Grid Lineas) -->
        <div class="card chart-section">
          <div class="card-header-v2">
            <h2 class="card-title-v2">📈 Fluctuación del Rendimiento</h2>
            <div class="chart-legend">
              <span class="leg-item"><i class="dot-v"></i> Venta</span>
              <span class="leg-item"><i class="dot-g"></i> Ganancia</span>
            </div>
          </div>
          
          <div class="chart-wrapper">
            <!-- Eje Y Referencial -->
            <div class="axis-y">
              <span v-for="val in yAxisLevels" :key="val">S/ {{ formatMoney(val) }}</span>
            </div>

            <div class="chart-area focus-mode">
              <!-- Lineas de Fondo (Grid) -->
              <div class="chart-grid">
                <div v-for="n in 5" :key="n" class="grid-line"></div>
              </div>

              <!-- VISTA SEMANAL (BARRAS) -->
              <div v-if="periodo === 'semana'" class="chart-canvas grid-mode animate-fade-in">
                <div v-for="item in graficoSemanal" :key="item.dia" class="bar-unit">
                  <div class="bar-labels">
                    <span class="v-val">S/ {{ formatMoney(item.totalVentas) }}</span>
                    <span class="g-val">S/ {{ formatMoney(item.totalGanancia) }}</span>
                  </div>
                  <div class="bar-pair">
                    <div class="bar-v" :style="{ height: getBarHeight(item.totalVentas, maxChartVal) + '%' }"></div>
                    <div class="bar-g" :style="{ height: getBarHeight(item.totalGanancia, maxChartVal) + '%' }"></div>
                  </div>
                  <div class="bar-footer">
                    <strong class="f-main">{{ item.dia }}</strong>
                    <small class="f-sub">{{ item.fecha }}</small>
                  </div>
                </div>
              </div>

              <!-- VISTA MENSUAL (LÍNEAS) - REORDENADO ETIQUETAS DEBAJO -->
              <div v-else class="chart-canvas line-mode animate-fade-in">
                 <div class="line-container">
                   <svg class="line-svg" viewBox="0 0 1000 300" preserveAspectRatio="none">
                     <path :d="createLinePath(graficoMensual, 'totalVentas')" class="l-v" />
                     <path :d="createLinePath(graficoMensual, 'totalGanancia')" class="l-g" />
                   </svg>
                   <div v-for="(item, idx) in graficoMensual" :key="item.semana" class="line-node-unit" :style="{ left: (idx * (100 / (graficoMensual.length - 1))) + '%' }">
                     <!-- ETIQUETAS SOBRE NODOS (DINÁMICAS) -->
                     <div class="node-data-top">
                       <span class="v-tag-v2">S/ {{ formatMoney(item.totalVentas) }}</span>
                       <span class="g-tag-v2">S/ {{ formatMoney(item.totalGanancia) }}</span>
                     </div>
                     
                     <div class="point-v" :style="{ bottom: getBarHeight(item.totalVentas, maxChartVal) + '%' }"></div>
                     <div class="point-g" :style="{ bottom: getBarHeight(item.totalGanancia, maxChartVal) + '%' }"></div>

                     <!-- ETIQUETAS DEBAJO (ORDENADITO) -->
                     <div class="node-footer-v2">
                       <strong class="f-main">{{ item.semana }}</strong>
                       <small class="f-sub">{{ item.rango }}</small>
                     </div>
                   </div>
                 </div>
              </div>
            </div>
          </div>
        </div>

        <!-- SECCIÓN RANKING (Nombres Completos) -->
        <div class="sidebar">
          <div class="card ranking-full">
            <h2 class="card-title-v2">🏆 Ranking {{ periodoLabel }}</h2>
            <div class="top-list">
              <div v-for="(p, i) in currentTop" :key="p.nombre" class="top-row animate-pop-in" :style="{ animationDelay: (i*0.1)+'s' }">
                <div class="top-number" :class="'nr-'+i">{{ i + 1 }}</div>
                <div class="top-info">
                  <p class="p-name-full">{{ p.nombre }}</p>
                  <div class="p-stats">
                    <span><strong>Monto:</strong> S/ {{ formatMoney(p.totalMonto) }}</span>
                    <span><strong>Cant:</strong> {{ p.totalUnidades }}</span>
                  </div>
                </div>
              </div>
              <div v-if="currentTop.length === 0" class="empty">Sin ventas registradas.</div>
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
  const vals = data.map(d => Math.max(d.totalVentas, d.totalGanancia));
  return Math.max(...vals, 1);
});

const yAxisLevels = computed(() => {
  const max = maxChartVal.value;
  return [max, max * 0.75, max * 0.5, max * 0.25, 0];
});

const getBarHeight = (val, max) => Math.min((val / max) * 100, 100);
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
/* REUSO DE ESTILOS CORPORATIVOS */
.dash-container { padding: 1.5rem; max-width: 1550px; margin: 0 auto; color: #2D3748; }

.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.page-title { font-size: 1.8rem; font-weight: 900; letter-spacing: -1px; margin: 0; }
.period-switcher { background: #EDF2F7; padding: 4px; border-radius: 12px; display: flex; }
.period-switcher button { border: none; background: transparent; padding: 0.4rem 1.2rem; border-radius: 9px; cursor: pointer; font-size: 0.8rem; font-weight: 800; color: #718096; }
.period-switcher button.active { background: white; color: #553C9A; box-shadow: 0 2px 6px rgba(0,0,0,0.08); }
.refresh-btn { background: #EDF2F7; border: none; padding: 0.5rem; border-radius: 10px; cursor: pointer; }

.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; margin-bottom: 2rem; }
.kpi-card { border-radius: 20px; padding: 1.25rem; display: flex; align-items: center; gap: 1rem; color: white; border: 1px solid rgba(255,255,255,0.1); }
.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.k2 { background: linear-gradient(135deg, #48BB78 0%, #38A169 100%); }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%); }
.k4 { background: linear-gradient(135deg, #ED8936 0%, #DD6B20 100%); }
.kpi-icon-wrap { font-size: 1.6rem; background: rgba(255,255,255,0.15); width: 42px; height: 42px; display: flex; align-items: center; justify-content: center; border-radius: 12px; }
.kpi-label { font-size: 0.65rem; font-weight: 800; text-transform: uppercase; margin: 0; }
.kpi-value { font-size: 1.35rem; font-weight: 900; margin: 0; }

.main-layout { display: grid; grid-template-columns: 1fr 420px; gap: 1.5rem; align-items: stretch; }
.card { background: white; border-radius: 28px; border: 1px solid #EDF2F7; box-shadow: 0 4px 25px rgba(0,0,0,0.03); }

/* SECCIÓN GRÁFICO AVANZADO */
.chart-section { padding: 2rem; }
.card-header-v2 { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }
.card-title-v2 { font-size: 1.15rem; font-weight: 900; }
.chart-legend { display: flex; gap: 1rem; }
.leg-item { display: flex; align-items: center; gap: 0.4rem; font-size: 0.7rem; font-weight: 800; color: #A0AEC0; }
.dot-v { width: 9px; height: 9px; background: #CBD5E0; border-radius: 50%; opacity: 0.6; }
.dot-g { width: 9px; height: 9px; background: #68D391; border-radius: 50%; }

.chart-wrapper { display: flex; gap: 1.5rem; height: 380px; position: relative; }
.axis-y { display: flex; flex-direction: column; justify-content: space-between; font-size: 0.65rem; color: #A0AEC0; text-align: right;}

.chart-area { flex: 1; position: relative; }
.chart-grid { position: absolute; top: 0; left: 0; width: 100%; height: 320px; display: flex; flex-direction: column; justify-content: space-between; z-index: 1; }
.grid-line { width: 100%; height: 1px; background: #F7FAFC; }

.chart-canvas { position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 2; display: flex; }

/* BARRAS */
.grid-mode { align-items: flex-end; justify-content: space-around; padding-top: 40px; }
.bar-unit { flex: 1; display: flex; flex-direction: column; align-items: center; height: 320px; position: relative; border-radius: 12px; }
.bar-labels { position: absolute; top: -50px; display: flex; flex-direction: column; align-items: center; gap: 2px; }
.v-val { font-size: 0.6rem; font-weight: 900; color: #4A5568; background: #EDF2F7; padding: 2px 5px; border-radius: 4px; }
.g-val { font-size: 0.6rem; font-weight: 900; color: #38A169; }
.bar-pair { display: flex; align-items: flex-end; gap: 6px; height: 100%; }
.bar-v { width: 22px; background: #CBD5E0; border-radius: 8px 8px 0 0; transition: height 1s; opacity: 0.4; }
.bar-g { width: 14px; background: #68D391; border-radius: 8px 8px 0 0; transition: height 1s 0.2s; }

/* LÍNEAS - REDISEÑO TOTAL (ETIQUETAS ABAJO) */
.line-mode { padding: 40px 10px; }
.line-container { width: 100%; height: 100%; position: relative; }
.line-svg { position: absolute; top: 0; left: 0; width: 100%; height: 260px; overflow: visible; z-index: 1; pointer-events: none; }
.l-v { fill: none; stroke: #CBD5E0; stroke-width: 4; stroke-linecap: round; stroke-linejoin: round; opacity: 0.5; }
.l-g { fill: none; stroke: #68D391; stroke-width: 4; stroke-linecap: round; stroke-linejoin: round; }

.line-node-unit { position: absolute; top: 0; height: 320px; display: flex; flex-direction: column; align-items: center; z-index: 2; pointer-events: none; }
.node-data-top { position: absolute; top: -50px; display: flex; flex-direction: column; align-items: center; gap: 2px; }
.v-tag-v2 { font-size: 0.6rem; font-weight: 900; color: #718096; background: #F7FAFC; padding: 1px 5px; border-radius: 4px; border: 1px solid #EDF2F7; }
.g-tag-v2 { font-size: 0.6rem; font-weight: 900; color: #38A169; }

.point-v { width: 10px; height: 10px; border-radius: 50%; background: white; border: 3px solid #CBD5E0; position: absolute; pointer-events: auto; }
.point-v:hover { transform: scale(1.5); box-shadow: 0 0 10px rgba(0,0,0,0.1); }
.point-g { width: 10px; height: 10px; border-radius: 50%; background: white; border: 3px solid #68D391; position: absolute; }

/* FOOTERS UNIFICADOS (BARRAS Y LÍNEAS) */
.bar-footer, .node-footer-v2 { position: absolute; bottom: -85px; display: flex; flex-direction: column; align-items: center; text-align: center; width: 120px; z-index: 5; }
.f-main { font-size: 0.75rem; font-weight: 900; color: #1A202C; text-transform: capitalize; margin-bottom: 2px; }
.f-sub { font-size: 0.65rem; font-weight: 800; color: #A0AEC0; white-space: nowrap; }

/* SECCIÓN RANKING */
.sidebar { display: flex; flex-direction: column; }
.ranking-full { padding: 1.5rem; background: #F8FAFC; border-radius: 28px; }
.top-list { display: flex; flex-direction: column; gap: 0.8rem; margin-top: 1rem; }
.top-row { display: flex; gap: 1rem; background: white; padding: 1.25rem; border-radius: 22px; border: 1px solid #EDF2F7; box-shadow: 0 4px 12px rgba(0,0,0,0.02); align-items: center; }
.top-number { width: 30px; height: 30px; min-width: 30px; display: flex; align-items: center; justify-content: center; font-weight: 900; color: white; border-radius: 12px; background: #CBD5E0; font-size: 0.75rem; }
.nr-0 { background: #FFD700; box-shadow: 0 4px 10px rgba(255,215,0,0.3); }
.p-name-full { font-weight: 900; font-size: 0.9rem; color: #1A202C; margin: 0 0 4px 0; }
.p-stats { display: flex; gap: 0.8rem; font-size: 0.7rem; color: #718096; font-weight: 800; }

.loading-full { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 10rem 0; }
.spinner-big { width: 50px; height: 50px; border: 4px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
