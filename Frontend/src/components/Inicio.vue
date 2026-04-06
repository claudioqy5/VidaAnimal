<template>
  <div class="inicio-container animate-fade-in">

    <!-- HEADER -->
    <div class="page-header">
      <div class="welcome-section">
        <h1 class="page-title">🏠 Resumen del Día</h1>
        <p class="page-date">{{ fechaActualDisplay }}</p>
      </div>
      <div class="header-status">
        <span class="status-indicator active"></span>
        <span class="status-text">Sistema Sincronizado (Perú)</span>
        <button class="refresh-btn" @click="cargarResumen" :disabled="loading">🔄 Recargar</button>
      </div>
    </div>

    <!-- KPIs CORE -->
    <div class="kpi-grid">
      <div class="kpi-card k1">
        <div class="icon-wrap">💰</div>
        <div class="kpi-content">
          <p class="kpi-label">Ventas Hoy</p>
          <p class="kpi-value">S/ {{ formatMoney(stats.ventasHoy) }}</p>
        </div>
      </div>
      <div class="kpi-card k2">
        <div class="icon-wrap">📃</div>
        <div class="kpi-content">
          <p class="kpi-label">Transacciones</p>
          <p class="kpi-value">{{ stats.transaccionesHoy || 0 }}</p>
        </div>
      </div>
      <div class="kpi-card k3">
        <div class="icon-wrap">👥</div>
        <div class="kpi-content">
          <p class="kpi-label">Clientes</p>
          <p class="kpi-value">{{ stats.clientesHoy || 0 }}</p>
        </div>
      </div>
      <div class="kpi-card k4">
        <div class="icon-wrap">📊</div>
        <div class="kpi-content">
          <p class="kpi-label">Utilidad Real</p>
          <p class="kpi-value">S/ {{ formatMoney(stats.gananciaHoy) }}</p>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Actualizando indicadores...</p>
    </div>

    <template v-else>
      <div class="main-dashboard">
        
        <!-- COLUMNA IZQUIERDA -->
        <div class="left-col">
          <!-- TOP PRODUCTOS HOY (Desde Backend) -->
          <div class="card ranking-card">
            <h3 class="card-title">🏆 Top Productos Hoy</h3>
            <div v-if="topProductosHoy.length === 0" class="empty-state">No hay ventas registradas aún.</div>
            <div v-else class="top-list">
              <div v-for="(p, i) in topProductosHoy" :key="p.producto" class="top-item-h animate-pop-in">
                <span class="top-rank">{{ i + 1 }}</span>
                <div class="top-info">
                  <p class="p-name">{{ p.producto }}</p>
                  <p class="p-meta">Incurrido: S/ {{ formatMoney(p.total) }} | {{ p.cantidad }} uds</p>
                </div>
              </div>
            </div>
          </div>

          <!-- TOP PROVEEDORES (Mes) -->
          <div class="card ranking-card">
            <h3 class="card-title">🤝 Top Proveedores (Mes)</h3>
            <div v-if="topProveedores.length === 0" class="empty-state">Sin compras este mes.</div>
            <div v-else class="top-list">
              <div v-for="(prov, i) in topProveedores" :key="prov.nombre" class="top-item-h animate-pop-in">
                <span class="top-rank prov">{{ i + 1 }}</span>
                <div class="top-info">
                  <p class="p-name">{{ prov.nombre }}</p>
                  <p class="p-meta">S/ {{ formatMoney(prov.totalInvertido) }} invertido</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUMNA DERECHA -->
        <div class="right-col">
          
          <!-- FLUJO DE VENTAS (Desde Backend) -->
          <div class="card flow-card">
             <h3 class="card-title">🕒 Flujo de Ventas (Hoy)</h3>
             <div v-if="!hasHourlyData" class="empty-state">Trazando curva de ventas...</div>
             <div v-else class="chart-hourly-v2">
                <div v-for="h in flujos" :key="h.hora" class="hour-unit">
                   <div class="hour-bar" :style="{ height: (h.total / maxHourlyTotal * 100) + '%' }">
                      <span class="hour-pop">S/ {{ h.total }}</span>
                   </div>
                   <span class="hour-label">{{ h.hora }}h</span>
                </div>
             </div>
          </div>

          <!-- ATENCIÓN DE INVENTARIO -->
          <div class="card inventory-alert">
            <div class="card-header-v2">
              <h3 class="card-title-v2">🛑 Atención de Inventario</h3>
              <router-link to="/productos" class="view-more">Ver Todo →</router-link>
            </div>
            <div class="inventory-list-v">
              <div v-for="p in stockBajo" :key="p.nombre" class="inv-item-v">
                <div class="inv-head">
                  <p class="p-prod">{{ p.nombre }}</p>
                  <span class="p-status" :class="p.stockActual <= 0 ? 'critico' : 'bajo'">
                    {{ p.stockActual <= 0 ? 'Agotado' : 'Bajo' }}
                  </span>
                </div>
                <div class="inv-bar-container">
                  <div class="inv-bar" :style="{ width: getStockPercent(p) + '%', backgroundColor: p.stockActual <= 0 ? '#E53E3E' : '#F6AD55' }"></div>
                </div>
                <div class="inv-footer">
                   <span>Stock: <strong>{{ p.stockActual }}</strong></span>
                   <span>Min: {{ p.stockMinimo }}</span>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </template>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const stats = ref({ ventasHoy: 0, gananciaHoy: 0, transaccionesHoy: 0, clientesHoy: 0 });
const topProductosHoy = ref([]);
const flujos = ref([]);
const stockBajo = ref([]);
const topProveedores = ref([]);
const loading = ref(true);

const fechaActualDisplay = computed(() => new Intl.DateTimeFormat('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }).format(new Date()));

const cargarResumen = async () => {
  loading.value = true;
  try {
    const res = await fetch('/api/Dashboard/resumen', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
    });
    const data = await res.json();
    if (data.success) {
      stats.value = data.stats;
      topProductosHoy.value = data.topProductosHoy || [];
      flujos.value = data.flujoHoras || [];
      stockBajo.value = data.stockBajo || [];
      topProveedores.value = data.topProveedores || [];
    }
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const hasHourlyData = computed(() => flujos.value.some(h => h.total > 0));
const maxHourlyTotal = computed(() => Math.max(...flujos.value.map(h => h.total), 1));
const getStockPercent = (p) => Math.min((p.stockActual / p.stockMinimo) * 100, 100);
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

onMounted(cargarResumen);
</script>

<style scoped>
/* REUSO DE ESTILOS PREVIOS (PREMIUM) */
.inicio-container { padding: 1.5rem; max-width: 1400px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem; }
.page-title { font-size: 2.2rem; font-weight: 900; letter-spacing: -1.5px; margin: 0; }
.header-status { display: flex; align-items: center; gap: 0.8rem; background: white; padding: 0.5rem 1.2rem; border-radius: 12px; }
.status-indicator { width: 10px; height: 10px; border-radius: 50%; background: #48BB78; }
.refresh-btn { border: none; background: #EDF2F7; padding: 0.4rem 0.8rem; border-radius: 8px; font-weight: 800; cursor: pointer; }

.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.kpi-card { padding: 1.25rem; border-radius: 22px; display: flex; align-items: center; gap: 1.2rem; color: white; }
.k1 { background: linear-gradient(135deg, #667eea, #764ba2); }
.k2 { background: linear-gradient(135deg, #ff758c, #ff7eb3); }
.k3 { background: linear-gradient(135deg, #4299E1, #3182CE); }
.k4 { background: linear-gradient(135deg, #48BB78, #44E2BB); }
.icon-wrap { font-size: 2rem; background: rgba(255,255,255,0.2); width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; border-radius: 14px; }
.kpi-value { font-size: 1.5rem; font-weight: 900; margin: 0; }

.main-dashboard { display: grid; grid-template-columns: 420px 1fr; gap: 2rem; }
.card { background: white; border-radius: 28px; padding: 2rem; border: 1px solid #F0F4F8; box-shadow: 0 4px 20px rgba(0,0,0,0.02); margin-bottom: 1.5rem; }
.card-title { font-size: 1.2rem; font-weight: 900; margin-bottom: 1.5rem; }

.top-item-h { display: flex; align-items: center; gap: 1rem; background: #F8FAFC; padding: 0.8rem; border-radius: 18px; border: 1px solid #EDF2F7; margin-bottom: 0.8rem; }
.top-rank { width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-weight: 900; font-size: 0.75rem; background: #E9D8FD; color: #553C9A; }
.top-rank.prov { background: #B2F5EA; color: #319795; }
.p-name { font-weight: 800; font-size: 0.9rem; margin: 0; }

.inventory-list-v { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }
.inv-item-v { background: #FFF9F2; padding: 1rem; border-radius: 18px; border: 1px solid #FED7D7; }
.inv-bar-container { background: #EDF2F7; height: 6px; border-radius: 3px; overflow: hidden; margin-top: 0.5rem; margin-bottom: 0.5rem; }
.inv-bar { height: 100%; transition: 1s; }

.chart-hourly-v2 { display: flex; align-items: baseline; justify-content: space-between; height: 150px; gap: 4px; }
.hour-unit { flex: 1; height: 100%; display: flex; flex-direction: column; align-items: center; }
.hour-bar { width: 100%; background: #E2E8F0; border-radius: 4px 4px 0 0; position: relative; transition: 0.3s; }
.hour-bar:hover { background: #553C9A; }
.hour-pop { position: absolute; top: -20px; left: 50%; transform: translateX(-50%); font-size: 0.55rem; background: #2D3748; color: white; padding: 2px 4px; border-radius: 4px; opacity: 0; }
.hour-bar:hover .hour-pop { opacity: 1; }
.hour-label { font-size: 0.55rem; font-weight: 800; color: #718096; margin-top: 4px; }

.loading-state { text-align: center; padding: 10rem 0; }
.spinner { width: 40px; height: 40px; border: 4px solid #EDF2F7; border-top-color: #553C9A; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }

.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
.animate-pop-in { animation: popIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards; }
@keyframes popIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>
