<template>
  <div class="dash-container animate-fade-in">

    <div class="page-header">
      <div>
        <h1 class="page-title">📊 Dashboard Analítico</h1>
        <p class="page-subtitle">Resumen semanal y mensual del negocio</p>
      </div>
      <div class="period-tabs">
        <button :class="['tab', periodo === 'semana' && 'active']" @click="periodo = 'semana'">Esta Semana</button>
        <button :class="['tab', periodo === 'mes' && 'active']" @click="periodo = 'mes'">Este Mes</button>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div><p>Cargando datos...</p>
    </div>

    <template v-else>

      <!-- KPIs período seleccionado -->
      <div class="kpi-grid">
        <div class="kpi-card k1">
          <div class="kpi-icon">💰</div>
          <div>
            <p class="kpi-label">Total Vendido</p>
            <p class="kpi-value">S/ {{ formatMoney(periodoData.total) }}</p>
          </div>
        </div>
        <div class="kpi-card k2">
          <div class="kpi-icon">🧾</div>
          <div>
            <p class="kpi-label">Nº Ventas</p>
            <p class="kpi-value">{{ periodoData.numVentas }}</p>
          </div>
        </div>
        <div class="kpi-card k3">
          <div class="kpi-icon">📦</div>
          <div>
            <p class="kpi-label">Producto #1</p>
            <p class="kpi-value" style="font-size:1rem;">{{ periodoData.topProductos?.[0]?.nombre || '---' }}</p>
          </div>
        </div>
        <div class="kpi-card k4">
          <div class="kpi-icon">📊</div>
          <div>
            <p class="kpi-label">Ticket Promedio</p>
            <p class="kpi-value">S/ {{ ticketPromedio }}</p>
          </div>
        </div>
      </div>

      <div class="grid-two">

        <!-- Ventas por día (solo semana) -->
        <div class="card" v-if="periodo === 'semana'">
          <h2 class="card-title">📅 Ventas por Día</h2>
          <div class="dia-chart">
            <div v-for="d in diasSemana" :key="d.nombre" class="dia-col">
              <div class="dia-wrap">
                <div class="dia-bar"
                  :style="{ height: barH(d.total, maxDia) + '%' }"
                  :title="'S/ ' + formatMoney(d.total)"
                ></div>
              </div>
              <div class="dia-label">{{ d.nombre }}</div>
              <div class="dia-val">S/ {{ formatMoney(d.total) }}</div>
            </div>
          </div>
        </div>

        <!-- Top Productos -->
        <div class="card">
          <h2 class="card-title">🏆 Top Productos mas vendidos</h2>
          <div v-if="!periodoData.topProductos?.length" class="empty-msg">Sin datos</div>
          <div class="bar-chart" v-else>
            <div v-for="(p, i) in periodoData.topProductos" :key="i" class="bar-row">
              <span class="bar-rank">#{{ i + 1 }}</span>
              <div class="bar-info">
                <span class="bar-name">{{ p.nombre }}</span>
                <div class="bar-track">
                  <div class="bar-fill"
                    :class="'bc-' + i"
                    :style="{ width: barH(p.totalMonto, periodoData.topProductos[0]?.totalMonto) + '%' }"
                  ></div>
                </div>
              </div>
              <div style="text-align: right; line-height: 1.1;">
                <span class="bar-qty" style="color: #553C9A;">S/ {{ formatMoney(p.totalMonto) }}</span><br>
                <span style="font-size: 0.65rem; color: #A0AEC0; font-weight: 600;">{{ p.totalUnidades }} uds</span>
              </div>
            </div>
          </div>
        </div>

      </div>

      <div class="grid-two">

        <!-- Top Clientes (solo semana) -->
        <div class="card" v-if="periodo === 'semana'">
          <h2 class="card-title">👥 Clientes Más Frecuentes</h2>
          <div v-if="!datos?.semana?.topClientesSemana?.length" class="empty-msg">Sin datos esta semana</div>
          <div class="client-list" v-else>
            <div v-for="(c, i) in datos.semana.topClientesSemana" :key="c.clienteID" class="client-row">
              <div class="client-rank">{{ i + 1 }}</div>
              <div class="client-avatar">{{ getInitials(c.nombre) }}</div>
              <div class="client-info">
                <p class="client-name">{{ c.nombre }}</p>
                <p class="client-sub">{{ c.numVentas }} compras</p>
              </div>
              <div class="client-amount">S/ {{ formatMoney(c.totalGastado) }}</div>
            </div>
          </div>
        </div>

        <!-- Proveedores por compra total -->
        <div class="card" :class="periodo === 'mes' ? 'full-width' : ''">
          <h2 class="card-title">🏢 Proveedores por Compra Total</h2>
          <div v-if="!datos?.proveedores?.length" class="empty-msg">Sin datos</div>
          <table class="prov-table" v-else>
            <thead>
              <tr>
                <th>#</th>
                <th>Proveedor</th>
                <th>Compras</th>
                <th>Total</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(p, i) in datos.proveedores" :key="p.proveedorID">
                <td class="td-rank">{{ i + 1 }}</td>
                <td class="td-name">{{ p.nombre }}</td>
                <td>{{ p.numCompras }}</td>
                <td class="td-total">S/ {{ formatMoney(p.totalComprado) }}</td>
                <td>
                  <div class="prov-bar-wrap">
                    <div class="prov-bar"
                      :style="{ width: barH(p.totalComprado, datos.proveedores[0]?.totalComprado) + '%' }"
                    ></div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

      </div>

    </template>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const loading = ref(true);
const datos = ref(null);
const periodo = ref('semana');

const DIAS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

const periodoData = computed(() => {
  if (!datos.value) return { total: 0, numVentas: 0, topProductos: [] };
  if (periodo.value === 'semana') return {
    total: datos.value.semana.totalSemana,
    numVentas: datos.value.semana.numVentasSemana,
    topProductos: datos.value.semana.topProductosSemana
  };
  return {
    total: datos.value.mes.totalMes,
    numVentas: datos.value.mes.numVentasMes,
    topProductos: datos.value.mes.topProductosMes
  };
});

const ticketPromedio = computed(() => {
  const { total, numVentas } = periodoData.value;
  return numVentas ? formatMoney(total / numVentas) : '0.00';
});

const diasSemana = computed(() => {
  const mapa = {};
  (datos.value?.semana?.ventasPorDia || []).forEach(d => { mapa[d.dia] = d.total; });
  return DIAS.map((nombre, i) => ({ nombre, total: mapa[i] || 0 }));
});

const maxDia = computed(() => Math.max(...diasSemana.value.map(d => d.total), 1));

const barH = (val, max) => max > 0 ? Math.max((val / max) * 100, 2) : 2;
const formatMoney = n => Number(n || 0).toFixed(2);
const getInitials = name => (name || '?').split(' ').map(w => w[0]).join('').substring(0, 2).toUpperCase();

const cargar = async () => {
  loading.value = true;
  try {
    const res = await fetch(`${API_BASE}/reportes/dashboard`, {
      headers: { Authorization: `Bearer ${getToken()}` }
    });
    const json = await res.json();
    if (json.success) datos.value = json.data;
  } catch (e) { console.error(e); }
  finally { loading.value = false; }
};

onMounted(cargar);
</script>

<style scoped>
.dash-container { padding: 2rem; max-width: 1300px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; flex-wrap: wrap; gap: 1rem; }
.page-title { font-size: 1.9rem; font-weight: 800; color: #2D3748; margin: 0 0 0.25rem 0; }
.page-subtitle { color: #718096; font-size: 0.9rem; margin: 0; }

/* Tabs */
.period-tabs { display: flex; background: #F7FAFC; border-radius: 10px; padding: 4px; gap: 4px; border: 1px solid #E2E8F0; }
.tab { padding: 0.4rem 1.1rem; border: none; background: transparent; border-radius: 8px; cursor: pointer; font-size: 0.85rem; font-weight: 600; color: #718096; transition: all 0.2s; }
.tab.active { background: white; color: #553C9A; box-shadow: 0 1px 4px rgba(0,0,0,0.1); }

/* KPIs */
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.25rem; margin-bottom: 1.5rem; }
.kpi-card { border-radius: 16px; padding: 1.5rem; display: flex; align-items: center; gap: 1.25rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); color: white; transition: transform 0.2s; }
.kpi-card:hover { transform: translateY(-3px); }
.k1 { background: linear-gradient(135deg, #667eea, #764ba2); }
.k2 { background: linear-gradient(135deg, #f093fb, #f5576c); }
.k3 { background: linear-gradient(135deg, #4facfe, #00f2fe); }
.k4 { background: linear-gradient(135deg, #43e97b, #38f9d7); }
.kpi-icon { font-size: 2rem; }
.kpi-label { font-size: 0.8rem; opacity: 0.85; margin: 0; }
.kpi-value { font-size: 1.7rem; font-weight: 800; margin: 0; }

/* Grid */
.grid-two { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1.5rem; }
.full-width { grid-column: 1 / -1; }
.card { background: white; border-radius: 16px; padding: 1.5rem; border: 1px solid #E2E8F0; box-shadow: 0 2px 8px rgba(0,0,0,0.04); }
.card-title { font-size: 1rem; font-weight: 700; color: #2D3748; margin: 0 0 1.25rem 0; }
.empty-msg { color: #A0AEC0; text-align: center; padding: 2rem; font-size: 0.9rem; }

/* Gráfico de días */
.dia-chart { display: flex; gap: 8px; height: 150px; align-items: flex-end; padding-bottom: 32px; border-bottom: 2px solid #EDF2F7; position: relative; }
.dia-col { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; }
.dia-wrap { flex: 1; display: flex; align-items: flex-end; width: 100%; justify-content: center; }
.dia-bar { width: 70%; min-height: 4px; border-radius: 6px 6px 0 0;  background: linear-gradient(180deg, #667eea, #764ba2); transition: height 0.6s cubic-bezier(.25,.8,.25,1); }
.dia-label { font-size: 0.7rem; font-weight: 700; color: #718096; margin-top: 6px; }
.dia-val { font-size: 0.6rem; color: #A0AEC0; }

/* Barras de productos */
.bar-chart { display: flex; flex-direction: column; gap: 0.875rem; }
.bar-row { display: grid; grid-template-columns: 24px 1fr 85px; gap: 0.75rem; align-items: center; }
.bar-rank { font-weight: 800; color: #A0AEC0; font-size: 0.85rem; }
.bar-name { font-size: 0.8rem; color: #4A5568; font-weight: 600; margin-bottom: 4px; display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.bar-track { background: #F7FAFC; border-radius: 999px; height: 10px; overflow: hidden; }
.bar-fill { height: 100%; border-radius: 999px; transition: width 0.8s cubic-bezier(.25,.8,.25,1); }
.bc-0 { background: linear-gradient(90deg,#667eea,#764ba2); }
.bc-1 { background: linear-gradient(90deg,#f093fb,#f5576c); }
.bc-2 { background: linear-gradient(90deg,#4facfe,#00f2fe); }
.bc-3 { background: linear-gradient(90deg,#43e97b,#38f9d7); }
.bc-4 { background: linear-gradient(90deg,#fa709a,#fee140); }
.bar-qty { font-size: 0.78rem; font-weight: 700; color: #553C9A; text-align: right; }

/* Clientes */
.client-list { display: flex; flex-direction: column; gap: 0.75rem; }
.client-row { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0; border-bottom: 1px solid #EDF2F7; }
.client-row:last-child { border-bottom: none; }
.client-rank { font-weight: 800; color: #A0AEC0; width: 20px; text-align: center; font-size: 0.85rem; }
.client-avatar { width: 36px; height: 36px; min-width: 36px; background: linear-gradient(135deg, #C3B1E1, #A7C7E7); border-radius: 10px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 0.8rem; color: white; }
.client-name { font-weight: 600; color: #2D3748; font-size: 0.875rem; margin: 0; }
.client-sub { color: #A0AEC0; font-size: 0.75rem; margin: 0; }
.client-info { flex: 1; }
.client-amount { font-weight: 700; color: #553C9A; font-size: 0.9rem; }

/* Proveedores */
.prov-table { width: 100%; border-collapse: collapse; }
.prov-table th { font-size: 0.75rem; text-transform: uppercase; color: #718096; padding: 0.5rem 0.75rem; text-align: left; background: #F8FAFC; }
.prov-table td { padding: 0.75rem; border-bottom: 1px solid #EDF2F7; font-size: 0.875rem; color: #4A5568; }
.td-rank { color: #A0AEC0; font-weight: 700; }
.td-name { font-weight: 600; color: #2D3748; }
.td-total { font-weight: 700; color: #276749; }
.prov-bar-wrap { background: #EDF2F7; border-radius: 999px; height: 8px; width: 100px; overflow: hidden; }
.prov-bar { height: 100%; background: linear-gradient(90deg, #667eea, #764ba2); border-radius: 999px; transition: width 0.6s; }

/* Loading */
.loading-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 4rem; gap: 1rem; color: #718096; }
.spinner { width: 40px; height: 40px; border: 4px solid #E2E8F0; border-top-color: #667eea; border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.animate-fade-in { animation: fadeIn 0.4s ease; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .grid-two { grid-template-columns: 1fr; }
}
</style>
