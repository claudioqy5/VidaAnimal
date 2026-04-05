<template>
  <div class="inicio-container animate-fade-in">

    <!-- HEADER SECCIÓN -->
    <div class="page-header">
      <div class="welcome-section">
        <h1 class="page-title">🏠 Resumen del Día</h1>
        <p class="page-date">{{ fechaActualDisplay }}</p>
      </div>
      <div class="header-status">
        <span class="status-indicator active"></span>
        <span class="status-text">Sistema Activo</span>
        <button class="refresh-btn" @click="cargarResumen" :disabled="loading">🔄 Recargar</button>
      </div>
    </div>

    <!-- CARDS DE MÉTRICAS (KPIs) -->
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
          <p class="kpi-value">{{ totalVentasDiarias }}</p>
        </div>
      </div>
      <div class="kpi-card k3">
        <div class="icon-wrap">👥</div>
        <div class="kpi-content">
          <p class="kpi-label">Clientes</p>
          <p class="kpi-value">{{ totalClientesUnicos }}</p>
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

    <div v-if="loading && !stats.ventasHoy" class="loading-state">
      <div class="spinner"></div>
      <p>Actualizando indicadores...</p>
    </div>

    <template v-else>
      <!-- LAYOUT CENTRAL -->
      <div class="main-dashboard">
        
        <!-- COLUMNA IZQUIERDA: TOPS -->
        <div class="left-col">
          <!-- TOP PRODUCTOS HOY -->
          <div class="card ranking-card">
            <h3 class="card-title">🏆 Top Productos Hoy</h3>
            <div v-if="topProductosHoy.length === 0" class="empty-state">No hay ventas registradas aún.</div>
            <div v-else class="top-list">
              <div v-for="(p, i) in topProductosHoy" :key="p.producto" class="top-item-h animate-pop-in" :style="{ animationDelay: (i*0.1)+'s' }">
                <span class="top-rank">{{ i + 1 }}</span>
                <div class="top-info">
                  <p class="p-name">{{ p.producto }}</p>
                  <p class="p-meta">Monto: S/ {{ formatMoney(p.total) }} | {{ p.cantidad }} uds</p>
                </div>
              </div>
            </div>
          </div>

          <!-- TOP PROVEEDORES DEL MES -->
          <div class="card ranking-card">
            <h3 class="card-title">🤝 Top Proveedores (Mes)</h3>
            <div v-if="topProveedores.length === 0" class="empty-state">No hay registros de compras este mes.</div>
            <div v-else class="top-list">
              <div v-for="(prov, i) in topProveedores" :key="prov.nombre" class="top-item-h animate-pop-in" :style="{ animationDelay: (i*0.1)+'s' }">
                <span class="top-rank prov">{{ i + 1 }}</span>
                <div class="top-info">
                  <p class="p-name">{{ prov.nombre }}</p>
                  <p class="p-meta">Total Invertido: S/ {{ formatMoney(prov.totalInvertido) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- COLUMNA DERECHA: FLUJO & INVENTARIO -->
        <div class="right-col">

          <!-- FLUJO DE VENTAS -->
          <div class="card flow-card">
             <h3 class="card-title">🕒 Flujo de Ventas (Hoy)</h3>
             <div v-if="!hasHourlyData" class="empty-state">Esperando primera venta para trazar curva...</div>
             <div v-else class="chart-hourly-v2">
                <div v-for="h in hourlyChart" :key="h.hora" class="hour-unit">
                   <div class="hour-bar" :style="{ height: (h.total / maxHourlyTotal * 100) + '%' }">
                      <span class="hour-pop">S/ {{ h.total }}</span>
                   </div>
                   <span class="hour-label">{{ h.hora }}h</span>
                </div>
             </div>
          </div>
          
          <!-- ATENCIÓN DE INVENTARIO (VERTICAL ELEGANTE) -->
          <div class="card inventory-alert">
            <div class="card-header-v2">
              <h3 class="card-title-v2">🛑 Atención de Inventario</h3>              
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
                  <span>Stock Actual: <strong>{{ p.stockActual }} {{ p.unidadMedida }}</strong></span>
                  <span>Mínimo: {{ p.stockMinimo }}</span>
                </div>
              </div>
              <div v-if="stockBajo.length === 0" class="empty-state">✅ Todo en orden</div>
            </div>
          </div>         

        </div>

      </div>
    </template>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const stats = ref({ ventasHoy: 0, gananciaHoy: 0 });
const topProductosHoy = ref([]);
const stockBajo = ref([]);
const topProveedores = ref([]);
const ventasPorHora = ref([]);
const totalVentasDiarias = ref(0);
const totalClientesUnicos = ref(0);
const loading = ref(true);

const fechaActualDisplay = computed(() => {
  return new Intl.DateTimeFormat('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }).format(new Date());
});

const cargarResumen = async () => {
  loading.value = true;
  try {
    const res = await fetch('/api/Dashboard/resumen', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
    });
    const data = await res.json();
    if (data.success) {
      stats.value = data.stats;
      stockBajo.value = data.stockBajo;
      topProveedores.value = data.topProveedores || [];
      
      // Consultar separadamente ventas de hoy para el flow si es necesario o unificar
      await cargarVentasHoy();
    }
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const cargarVentasHoy = async () => {
  try {
     const res = await fetch('/api/Ventas/historico', {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
     });
     const ventas = await res.json();
     const hoy = new Date().toISOString().split('T')[0];
     const hoyVentas = ventas.filter(v => v.fecha.startsWith(hoy) && v.estado === 'Completada');
     
     totalVentasDiarias.value = hoyVentas.length;
     totalClientesUnicos.value = new Set(hoyVentas.map(v => v.clienteID)).size;

     // Agrupar por hora
     const flow = Array(17).fill(0).map((_, i) => ({ hora: i + 6, total: 0 }));
     hoyVentas.forEach(v => {
        const h = new Date(v.fecha).getHours();
        const f = flow.find(f => f.hora === h);
        if (f) f.total += v.total;
     });
     ventasPorHora.value = flow;

     // Top Productos Hoy
     const prods = {};
     hoyVentas.forEach(v => {
        v.ventaDetalles.forEach(d => {
           if (!prods[d.nombreProducto]) prods[d.nombreProducto] = { total: 0, cantidad: 0 };
           prods[d.nombreProducto].total += d.subTotal;
           prods[d.nombreProducto].cantidad += d.cantidad;
        });
     });
     topProductosHoy.value = Object.entries(prods)
        .map(([nombre, s]) => ({ producto: nombre, ...s }))
        .sort((a,b) => b.total - a.total).slice(0, 5);

  } catch (e) { console.error(e); }
};

const hasHourlyData = computed(() => ventasPorHora.value.some(h => h.total > 0));
const hourlyChart = computed(() => ventasPorHora.value);
const maxHourlyTotal = computed(() => Math.max(...ventasPorHora.value.map(h => h.total), 1));

const getStockPercent = (p) => Math.min((p.stockActual / p.stockMinimo) * 100, 100);
const formatMoney = (n) => Number(n || 0).toLocaleString('es-PE', { minimumFractionDigits: 2 });

onMounted(cargarResumen);
</script>

<style scoped>
.inicio-container { padding: 1.5rem; max-width: 1400px; margin: 0 auto; }

.page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem; }
.page-title { font-size: 2.2rem; font-weight: 900; letter-spacing: -1.5px; margin: 0; color: #2D3748; }
.page-date { color: #718096; font-size: 1rem; font-weight: 700; text-transform: capitalize; margin: 0; }

.header-status { display: flex; align-items: center; gap: 0.8rem; background: white; padding: 0.5rem 1.2rem; border-radius: 14px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.status-indicator { width: 10px; height: 10px; border-radius: 50%; }
.status-indicator.active { background: #48BB78; box-shadow: 0 0 10px rgba(72,187,120,0.5); }
.status-text { font-size: 0.85rem; color: #4A5568; }
.refresh-btn { border: none; background: #EDF2F7; padding: 0.4rem 0.8rem; border-radius: 8px; font-weight: 800; color: #553C9A; cursor: pointer; }

/* KPIs */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
.kpi-card { padding: 1.5rem; border-radius: 24px; display: flex; align-items: center; gap: 1.2rem; border: 1px solid rgba(255,255,255,0.2); transition: 0.3s; box-shadow: 0 10px 25px rgba(0,0,0,0.06); }
.kpi-card:hover { transform: translateY(-5px); }

.k1 { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; }
.k2 { background: linear-gradient(135deg, #ff758c 0%, #ff7eb3 100%); color: white; }
.k3 { background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%); color: white; }
.k4 { background: linear-gradient(135deg, #48BB78 0%, #44E2BB 100%); color: white; }

.icon-wrap { font-size: 2rem; background: rgba(255,255,255,0.2); width: 50px; height: 50px; display: flex; align-items: center; justify-content: center; border-radius: 16px; }
.kpi-label { font-size: 0.75rem; font-weight: 800; margin: 0; text-transform: uppercase; opacity: 0.8; }
.kpi-value { font-size: 1.6rem; font-weight: 900; margin: 0; }

/* DASHBOARD MAIN */
.main-dashboard { display: grid; grid-template-columns: 420px 1fr; gap: 2rem; }

.card { background: white; border-radius: 28px; padding: 2rem; border: 1px solid #F0F4F8; box-shadow: 0 4px 20px rgba(0,0,0,0.02); margin-bottom: 2rem; }
.card-title { font-size: 1.2rem; font-weight: 900; margin: 0 0 1.5rem 0; color: #2D3748; }

/* TOP LISTS */
.top-list { display: flex; flex-direction: column; gap: 1rem; }
.top-item-h { display: flex; align-items: center; gap: 1.2rem; background: #F8FAFC; padding: 1rem; border-radius: 20px; border: 1px solid #EDF2F7; }
.top-rank { width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-weight: 900; color: #553C9A; background: #E9D8FD; font-size: 0.8rem; }
.top-rank.prov { background: #B2F5EA; color: #319795; }
.p-name { font-size: 0.95rem; margin: 0; color: #1A202C; }
.p-meta { font-size: 0.75rem; font-weight: 700; color: #718096; margin: 0; }

/* INVENTORY ALERT (VERTICAL ELEGANTE) */
.inventory-alert { border-left: 8px solid #F6AD55; }
.card-header-v2 { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.card-title-v2 { font-size: 1.3rem; font-weight: 900; margin: 0; color: #2D3748; }


.inventory-list-v { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.2rem; }
.inv-item-v { background: #FFF9F2; padding: 1.2rem; border-radius: 22px; border: 1px solid #FED7D7; }
.inv-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.p-prod { font-size: 0.9rem; color: #2D3748; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 250px; }
.p-status { font-size: 0.65rem; font-weight: 900; padding: 4px 10px; border-radius: 20px; }
.critico { background: #E53E3E; color: white; }
.bajo { background: #F6AD55; color: white; }

.inv-bar-container { background: #EDF2F7; height: 8px; border-radius: 4px; overflow: hidden; margin-bottom: 0.8rem; }
.inv-bar { height: 100%; border-radius: 4px; transition: 1s cubic-bezier(0.175, 0.885, 0.32, 1.275); }

.inv-footer { display: flex; justify-content: space-between; font-size: 0.75rem; color: #718096; }

/* FLOW CHART HOURLY */
.chart-hourly-v2 { display: flex; align-items: flex-end; justify-content: space-between; height: 180px; padding-top: 30px; gap: 5px; }
.hour-unit { flex: 1; height: 100%; display: flex; flex-direction: column; align-items: center; position: relative; }
.hour-bar { width: 100%; background: #E2E8F0; border-radius: 6px 6px 0 0; transition: 1s; position: relative; }
.hour-bar:hover { background: #553C9A; transform: scaleX(1.1); }
.hour-bar:hover .hour-pop { opacity: 1; }
.hour-pop { position: absolute; top: -25px; left: 50%; transform: translateX(-50%); background: #2D3748; color: white; font-size: 0.6rem; padding: 2px 4px; border-radius: 4px; opacity: 0; transition: 0.2s; white-space: nowrap; }
.hour-label { font-size: 0.6rem; font-weight: 800; color: #A0AEC0; margin-top: 5px; }

/* ANIMACIONES */
.animate-fade-in { animation: fadeIn 0.4s ease-out forwards; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
.animate-pop-in { animation: popIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards; opacity: 0; }
@keyframes popIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }

@media (max-width: 1100px) {
  .main-dashboard { grid-template-columns: 1fr; }
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .inventory-list-v { grid-template-columns: 1fr; }
}
</style>
