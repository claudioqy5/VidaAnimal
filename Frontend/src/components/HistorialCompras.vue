<template>
  <div class="historial-module fade-in">
    <div class="header">
      <div>
        <h2 class="title">Historial de Abastecimiento</h2>
        <p class="subtitle">Consulta las facturas de mercadería que han ingresado al sistema</p>
      </div>
    </div>

    <!-- Filtros y Búsqueda -->
    <div class="filters-card glass">
      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input type="text" v-model="busqueda" placeholder="Buscar por Nro. Comprobante o Proveedor..." />
      </div>
    </div>

    <!-- Tabla de Compras -->
    <div class="table-container shadow-sm">
      <div v-if="cargando" class="loading-state">
        <div class="spinner"></div>
        <p>Obteniendo facturas...</p>
      </div>

      <table v-else class="main-table">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Proveedor</th>
            <th>Nro. Comprobante</th>
            <th>Total Soles</th>
            <th class="actions-head">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in comprasFiltradas" :key="c.compraID" class="row-hover">
            <td class="date-col">
              <strong>{{ formatearFecha(c.fechaCompra) }}</strong>
              <span>{{ formatearHora(c.fechaCompra) }}</span>
            </td>
            <td>
              <div class="provider-info">
                <span class="provider-icon">🏢</span>
                {{ c.proveedor?.nombre || 'Proveedor Desconocido' }}
              </div>
            </td>
            <td>
              <span class="invoice-badge">{{ c.numeroComprobante }}</span>
            </td>
            <td class="total-col">S/ {{ c.total.toFixed(2) }}</td>
            <td class="actions-col">
              <button class="btn-detail" @click="verDetalle(c)">
                <span>👁️</span> Ver Detalle
              </button>
            </td>
          </tr>
          <tr v-if="comprasFiltradas.length === 0">
            <td colspan="5" class="empty-state">No se encontraron registros de compras.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Detalle de Compra -->
    <div v-if="mostrarModalDetalle" class="modal-overlay" @click.self="mostrarModalDetalle = false">
      <div class="modal-content detail-modal">
        <div class="modal-header">
          <div>
            <h3>Detalle de Factura: {{ compraSeleccionada.numeroComprobante }}</h3>
            <p class="modal-subtitle">Proveedor: {{ compraSeleccionada.proveedor?.nombre }}</p>
          </div>
          <button class="close-btn" @click="mostrarModalDetalle = false">✕</button>
        </div>

        <div class="modal-body">
          <table class="detail-table">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Código</th>
                <th>Cantidad</th>
                <th>Costo Unitario</th>
                <th>Subtotal</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in compraSeleccionada.detalles" :key="d.compraDetalleID">
                <td class="prod-name">{{ d.producto?.nombre }}</td>
                <td><small>{{ d.producto?.codigo }}</small></td>
                <td class="qty-cell">{{ d.cantidad }}</td>
                <td>S/ {{ d.precioCostoUnitario.toFixed(2) }}</td>
                <td class="subtotal-cell">S/ {{ d.subTotal.toFixed(2) }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <td colspan="4" class="total-label">TOTAL FACTURA:</td>
                <td class="total-value">S/ {{ compraSeleccionada.total.toFixed(2) }}</td>
              </tr>
            </tfoot>
          </table>
        </div>

        <div class="modal-footer">
          <button class="primary-btn" @click="mostrarModalDetalle = false">Cerrar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

const compras = ref([])
const cargando = ref(true)
const busqueda = ref('')
const mostrarModalDetalle = ref(false)
const compraSeleccionada = ref(null)

const cargarCompras = async () => {
  cargando.value = true
  try {
    const res = await fetch('/api/Compras', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` }
    })
    const data = await res.json()
    if (data.success) {
      compras.value = data.data
    }
  } catch (err) {
    console.error("Error al cargar historial:", err)
  } finally {
    cargando.value = false
  }
}

const comprasFiltradas = computed(() => {
  if (!busqueda.value) return compras.value
  const t = busqueda.value.toLowerCase()
  return compras.value.filter(c => 
    c.numeroComprobante.toLowerCase().includes(t) || 
    (c.proveedor && c.proveedor.nombre.toLowerCase().includes(t))
  )
})

const formatearFecha = (f) => new Date(f).toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' })
const formatearHora = (f) => new Date(f).toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit' })

const verDetalle = (c) => {
  compraSeleccionada.value = c
  mostrarModalDetalle.value = true
}

onMounted(() => cargarCompras())
</script>

<style scoped>
.historial-module { animation: fadeIn 0.4s ease; }
.header { margin-bottom: 2rem; }
.title { font-size: 1.75rem; font-weight: 700; color: #1A202C; margin: 0 0 0.25rem 0; }
.subtitle { color: #718096; margin: 0; font-size: 0.95rem; }

.filters-card { background: white; padding: 1.25rem; border-radius: 16px; margin-bottom: 1.5rem; border: 1px solid #E2E8F0; }
.search-box { display: flex; align-items: center; background: #F7FAFC; border: 1px solid #E2E8F0; border-radius: 12px; padding: 0.5rem 1rem; }
.search-box input { border: none; background: transparent; padding: 0.5rem; width: 100%; outline: none; color: #2D3748; }

.table-container { background: white; border-radius: 16px; border: 1px solid #E2E8F0; overflow: hidden; }
.main-table { width: 100%; border-collapse: collapse; }
.main-table th { background: #F8FAFC; padding: 1rem; text-align: left; font-size: 0.75rem; text-transform: uppercase; color: #718096; border-bottom: 1px solid #E2E8F0; }
.main-table td { padding: 1rem; border-bottom: 1px solid #EDF2F7; color: #4A5568; font-size: 0.95rem; }

.date-col { display: flex; flex-direction: column; }
.date-col span { font-size: 0.75rem; color: #A0AEC0; }
.provider-info { display: flex; align-items: center; gap: 0.5rem; font-weight: 600; color: #2D3748; }
.invoice-badge { background: #EBF8FF; color: #2B6CB0; padding: 0.25rem 0.6rem; border-radius: 6px; font-weight: 700; font-family: monospace; font-size: 0.9rem; }
.total-col { font-weight: 800; color: #2D3748; font-size: 1.1rem; }

.btn-detail { background: #EDF2F7; border: none; padding: 0.5rem 1rem; border-radius: 8px; color: #4A5568; cursor: pointer; transition: all 0.2s; font-weight: 600; display: flex; align-items: center; gap: 0.4rem; }
.btn-detail:hover { background: #E2E8F0; color: #2D3748; }

/* Modal Detalle */
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.4); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 2000; }
.detail-modal { background: white; padding: 0; width: 90%; max-width: 800px; border-radius: 20px; overflow: hidden; display: flex; flex-direction: column; max-height: 90vh; }
.modal-header { padding: 1.5rem; border-bottom: 1px solid #E2E8F0; display: flex; justify-content: space-between; align-items: center; background: #F8FAFC; }
.modal-subtitle { color: #718096; margin: 0.25rem 0 0 0; font-size: 0.9rem; font-weight: 500; }
.modal-body { padding: 1.5rem; overflow-y: auto; flex: 1; }

.detail-table { width: 100%; border-collapse: collapse; }
.detail-table th { text-align: left; font-size: 0.75rem; color: #A0AEC0; border-bottom: 2px solid #F0F4F8; padding-bottom: 0.75rem; }
.detail-table td { padding: 1rem 0; border-bottom: 1px solid #EDF2F7; }
.prod-name { font-weight: 700; color: #2D3748; }
.qty-cell { font-weight: 700; color: #3182CE; }
.subtotal-cell { font-weight: 800; text-align: right; }

.total-label { text-align: right; padding: 1.5rem 1rem; font-weight: 800; color: #718096; }
.total-value { text-align: right; font-size: 1.5rem; font-weight: 800; color: #2D3748; padding: 1.5rem 0; }

.modal-footer { padding: 1.5rem; text-align: right; border-top: 1px solid #E2E8F0; }
.close-btn { background: none; border: none; font-size: 1.5rem; color: #A0AEC0; cursor: pointer; }
.primary-btn { background: #2D3748; color: white; border: none; padding: 0.75rem 2rem; border-radius: 10px; font-weight: 700; cursor: pointer; transition: opacity 0.2s; }
.primary-btn:hover { opacity: 0.9; }

.loading-state { text-align: center; padding: 4rem; color: #A0AEC0; }
.spinner { width: 40px; height: 40px; border: 4px solid #F3F3F3; border-top: 4px solid #F6AD55; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
