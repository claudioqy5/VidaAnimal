<template>
  <div>
    <!-- BANNERS GLOBALES -->
    <div v-if="errorGlobal" class="banner banner-error">{{ errorGlobal }}</div>
    <div v-if="successGlobal" class="banner banner-success">{{ successGlobal }}</div>

    <!-- ALERTA CUMPLEAÑOS -->
    <div v-if="cumpleaneroHoy" class="birthday-alert" style="margin: 0.75rem 1rem 0;">
      <div class="bday-icon">🎂</div>
      <div class="bday-text">
        <strong>¡Hoy es el cumpleaños de {{ cumpleaneroHoy.nombreCompleto }}!</strong>
        Ofrécele un descuento especial 🎉
      </div>
    </div>

    <!-- BUSCADOR -->
    <div style="padding: 1rem 1rem 0.75rem;">
      <input
        type="text"
        v-model="busqueda"
        placeholder="🔍 Buscar producto por nombre o código..."
        class="input"
      />
    </div>

    <!-- =================== CATÁLOGO PRINCIPAL =================== -->
    <div class="section-header" style="padding-top: 0; padding-bottom: 0.5rem;">
      <span class="section-title">Catálogo de Productos</span>
      <span style="font-size: 0.78rem; color: var(--text-muted);">
        {{ productosFiltrados.length }} resultado{{ productosFiltrados.length !== 1 ? 's' : '' }}
      </span>
    </div>

    <div v-if="cargandoProductos" class="loading-screen">
      <div class="spinner"></div>
      <span>Cargando catálogo...</span>
    </div>

    <div v-else class="prod-grid" style="padding-bottom: 6rem;"> <!-- Extra padding for floating bar -->
      <div
        class="prod-card"
        v-for="prod in productosPaginados"
        :key="prod.productoID"
        :class="{ 'out-of-stock': prod.stockActual <= 0 }"
        @click="agregarAlCarrito(prod)"
      >
        <!-- Imagen -->
        <img v-if="prod.imagenURL" :src="`/api${prod.imagenURL}`" class="prod-card-img" :alt="prod.nombre" />
        <div v-else class="prod-card-no-img">📦</div>

        <!-- Badge stock -->
        <span class="stock-badge-overlay" :class="getStockBadgeClass(prod)">
          {{ formatStock(prod) }}
        </span>

        <div class="prod-card-body">
          <div class="prod-card-name">{{ prod.nombre }}</div>
          <div class="prod-card-code">{{ prod.codigo }}</div>
          <div class="prod-card-price">S/ {{ prod.precioVenta.toFixed(2) }}</div>
        </div>
      </div>

      <div v-if="productosFiltrados.length === 0" style="grid-column: 1 / -1; text-align: center; color: var(--text-muted); padding: 2rem;">
        No se encontraron productos.
      </div>
    </div>

    <!-- Paginación -->
    <div class="pagination" v-if="totalPages > 1">
      <button class="btn btn-secondary" style="padding: 0.5rem 1rem; min-height: 40px;" @click="prevPage" :disabled="currentPage === 1">← Ant</button>
      <span class="page-indicator">{{ currentPage }} / {{ totalPages }}</span>
      <button class="btn btn-secondary" style="padding: 0.5rem 1rem; min-height: 40px;" @click="nextPage" :disabled="currentPage === totalPages">Sig →</button>
    </div>


    <!-- =================== BARRA FLOTANTE DE CARRITO =================== -->
    <div 
      v-if="carrito.length > 0 && !isCartOpen && !mostrarModalVenta" 
      class="floating-cart-bar fade-in"
      @click="isCartOpen = true"
    >
      <div class="fcb-left">
        <div class="fcb-count">{{ carrito.length }}</div>
        <div class="fcb-text">Ver Carrito</div>
      </div>
      <div class="fcb-right">
        S/ {{ totalVenta.toFixed(2) }}
      </div>
    </div>


    <!-- =================== DRAWER DEL CARRITO (SLIDE DESDE DERECHA) =================== -->
    <div v-if="isCartOpen" class="cart-drawer-overlay" @click.self="isCartOpen = false">
      <div class="cart-drawer">
        <div class="cart-drawer-header">
          <div class="cart-drawer-title">Carrito ({{ carrito.length }})</div>
          <div class="cart-drawer-close" @click="isCartOpen = false">✕</div>
        </div>

        <div class="cart-drawer-body">
          <div v-if="carrito.length > 0" style="padding: 0 1rem; display: flex; flex-direction: column; gap: 0.6rem;">
            <div class="cart-item fade-in" v-for="(item, index) in carrito" :key="index">
              
              <!-- Header del item -->
              <div class="cart-item-header">
                <span class="cart-item-name">{{ item.producto.nombre }}</span>
                <button class="cart-item-remove" @click="eliminarDelCarrito(index)">✕</button>
              </div>

              <!-- Selector tipo (solo SACO/BALDE) -->
              <div v-if="item.producto.unidadMedida === 'SACO' || item.producto.unidadMedida === 'BALDE'">
                <select v-model="item.tipoVenta" @change="cambiarTipoVenta(index)" class="tipo-select">
                  <option :value="item.producto.unidadMedida === 'SACO' ? 'KG' : 'UND'">
                    × {{ item.producto.unidadMedida === 'SACO' ? 'Kg (fracción)' : 'Und (fracción)' }}
                  </option>
                  <option :value="item.producto.unidadMedida">
                    × {{ item.producto.unidadMedida === 'SACO' ? 'Saco (entero)' : 'Balde/Bolsa (entero)' }}
                  </option>
                </select>
              </div>

              <!-- Precio editable -->
              <div class="cart-item-price-row">
                <span class="cart-price-label">Precio:</span>
                <span style="color: var(--text-muted); font-size: 0.85rem;">S/</span>
                <input
                  type="number"
                  v-model.number="item.precioVentaUnitario"
                  @change="validarPrecioCosto(index)"
                  class="price-input-inline"
                  step="0.10"
                  :readonly="usuarioRol === 'CAJERO' && (item.producto.unidadMedida !== 'SACO' && item.producto.unidadMedida !== 'BALDE')"
                />
                <span style="font-size: 0.8rem; color: var(--text-muted);">
                  × {{ (item.producto.unidadMedida === 'SACO' || item.producto.unidadMedida === 'BALDE') ? item.tipoVenta.toLowerCase() : item.producto.unidadMedida.toLowerCase() }}
                </span>
              </div>

              <!-- Acciones: cantidad + monto + subtotal -->
              <div class="cart-item-actions">
                <div style="display: flex; align-items: center; gap: 0.4rem;">
                  <!-- Control cantidad -->
                  <div class="qty-control">
                    <button class="qty-btn" @click="restarCantidad(index)">−</button>
                    <input type="number" v-model.number="item.cantidad" class="qty-input" step="0.001" min="0.001" />
                    <button class="qty-btn" @click="sumarCantidad(index)">+</button>
                  </div>

                  <!-- Botón monto (solo KG) -->
                  <button
                    v-if="item.tipoVenta === 'KG'"
                    class="btn-monto"
                    @click="abrirModalSoles(index)"
                    title="Vender por monto exacto en Soles"
                  >
                    💰 S/
                  </button>
                </div>

                <!-- Subtotal -->
                <div class="cart-item-subtotal">
                  <div class="subtotal-label">Subtotal</div>
                  <div class="subtotal-value">S/ {{ (item.cantidad * item.precioVentaUnitario).toFixed(2) }}</div>
                </div>
              </div>
            </div>
          </div>
          
          <div v-else class="empty-cart">
            <div class="empty-cart-icon">🛒</div>
            <p class="empty-cart-text">El carrito está vacío.</p>
          </div>
        </div>

        <!-- TICKET FOOTER: descuento, observaciones, método pago, total, botón -->
        <div class="ticket-footer" style="margin: 1rem;">
          <!-- Descuento -->
          <div class="discount-row">
            <span class="discount-left">Subtotal: <b style="color: var(--text-primary);">S/ {{ subtotalVenta.toFixed(2) }}</b></span>
            <div style="display: flex; align-items: center; gap: 0.4rem;">
              <span style="font-size: 0.82rem; color: var(--danger); font-weight: 700;">Desc. -S/</span>
              <input type="number" v-model.number="ticket.descuento" min="0" step="0.5" class="discount-input" />
            </div>
          </div>

          <!-- Observaciones -->
          <input
            type="text"
            v-model="ticket.observaciones"
            placeholder="Notas u observaciones de la venta..."
            class="input input-sm"
          />

          <!-- Método de pago -->
          <div class="pay-btns">
            <button class="pay-btn" :class="{ active: ticket.metodoPago === 'Efectivo' }" @click="ticket.metodoPago = 'Efectivo'">
              💵 Efectivo
            </button>
            <button class="pay-btn" :class="{ active: ticket.metodoPago === 'Yape' }" @click="ticket.metodoPago = 'Yape'">
              📱 Yape
            </button>
          </div>

          <!-- Total -->
          <div class="total-row">
            <span class="total-label">Total a cobrar</span>
            <span class="total-amount">S/ {{ totalVenta.toFixed(2) }}</span>
          </div>

          <!-- Botón cobrar -->
          <button
            class="btn btn-checkout"
            :disabled="carrito.length === 0 || vendiendo"
            @click="procesarVenta"
          >
            <span v-if="vendiendo" class="spinner" style="width: 20px; height: 20px; border-width: 2.5px; flex-shrink: 0;"></span>
            {{ vendiendo ? 'Procesando...' : '💳 Procesar Pago' }}
          </button>
        </div>
      </div>
    </div>


    <!-- =================== MODAL: MONTO EN SOLES =================== -->
    <div v-if="mostrarModalSoles" class="modal-overlay" style="z-index: 400;" @click.self="cerrarModalSoles">
      <div class="modal-sheet">
        <div class="modal-handle"></div>
        <h3 class="modal-title">💰 Vender por Monto (S/)</h3>

        <div class="monto-modal-content">
          <p class="monto-product-name">
            ¿Cuánto comprará el cliente de:
            <strong>{{ itemSolesNombre }}</strong>
          </p>
          <input
            type="number"
            v-model="inputSoles"
            class="monto-input"
            step="0.50"
            min="0.10"
            id="inputMontoSolesM"
            @keyup.enter="confirmarCalculoSoles"
            autofocus
          />
          <div style="display: flex; gap: 0.75rem; width: 100%;">
            <button class="btn btn-secondary" style="flex: 1;" @click="cerrarModalSoles">Cancelar</button>
            <button class="btn btn-primary" style="flex: 1;" @click="confirmarCalculoSoles">Aplicar</button>
          </div>
        </div>
      </div>
    </div>

    <!-- =================== MODAL: VENTA EXITOSA =================== -->
    <div v-if="mostrarModalVenta" class="modal-overlay" style="z-index: 500;">
      <div class="success-modal">
        <div class="success-checkmark">✅</div>
        <h3 class="success-title">¡Venta Exitosa!</h3>
        <p class="success-text">El pago ha sido procesado y el stock actualizado correctamente.</p>
        <button class="btn btn-primary btn-full" style="margin-top: 0.5rem; min-height: 52px; font-size: 1rem;" @click="cerrarModalVentaExitosa">
          Nueva Venta
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'

const props = defineProps({ usuarioRol: String })

const API_URL = '/api'

// ── Estado ──
const productos = ref([])
const clientes = ref([])
const carrito = ref([])
const cargandoProductos = ref(true)
const vendiendo = ref(false)
const errorGlobal = ref('')
const successGlobal = ref('')
const busqueda = ref('')
const currentPage = ref(1)
const itemsPerPage = 12

const isCartOpen = ref(false) // NUEVO: Estado del drawer del carrito
const cumpleaneroHoy = ref(null)

const ticket = ref({
  serie: 'B001',
  numero: '',
  clienteID: 0,
  descuento: 0,
  observaciones: '',
  metodoPago: 'Efectivo'
})

// Modales
const mostrarModalSoles = ref(false)
const inputSoles = ref('')
const itemSolesIdx = ref(-1)
const itemSolesNombre = ref('')
const mostrarModalVenta = ref(false)

// ── Helpers ──
const getToken = () => localStorage.getItem('jwt_token')

const generarCorrelativo = () => {
  ticket.value.numero = (Math.floor(Math.random() * 900000) + 100000).toString()
}

const usuarioRol = computed(() => props.usuarioRol || '')

// ── API ──
const cargarDatos = async () => {
  errorGlobal.value = ''
  try {
    const [resP, resC] = await Promise.all([
      fetch(`${API_URL}/Productos`, { headers: { Authorization: `Bearer ${getToken()}` } }),
      fetch(`${API_URL}/Clientes`, { headers: { Authorization: `Bearer ${getToken()}` } })
    ])
    const dataP = await resP.json()
    if (dataP.success) productos.value = dataP.data.filter(p => p.activo === true)

    if (resC.ok) {
      const dataC = await resC.json()
      if (dataC.success) clientes.value = dataC.data
    }
  } catch {
    errorGlobal.value = 'Error al conectar con el servidor.'
  } finally {
    cargandoProductos.value = false
  }
}

onMounted(() => {
  cargarDatos()
  generarCorrelativo()
})

// ── Filtrado y paginación ──
const productosFiltrados = computed(() => {
  if (!busqueda.value.trim()) return productos.value
  const palabras = busqueda.value.toLowerCase().split(/\s+/).filter(w => w.length > 0)
  return productos.value.filter(p => {
    const texto = `${p.nombre ?? ''} ${p.codigo ?? ''}`.toLowerCase()
    return palabras.every(pal => texto.includes(pal))
  })
})

const totalPages = computed(() => Math.ceil(productosFiltrados.value.length / itemsPerPage))
const productosPaginados = computed(() => {
  const skip = (currentPage.value - 1) * itemsPerPage
  return productosFiltrados.value.slice(skip, skip + itemsPerPage)
})
const nextPage = () => { if (currentPage.value < totalPages.value) currentPage.value++ }
const prevPage = () => { if (currentPage.value > 1) currentPage.value-- }
watch(busqueda, () => { currentPage.value = 1 })

// ── Totales ──
const subtotalVenta = computed(() =>
  carrito.value.reduce((sum, item) => sum + item.cantidad * item.precioVentaUnitario, 0)
)
const totalVenta = computed(() =>
  Math.max(subtotalVenta.value - (Number(ticket.value.descuento) || 0), 0)
)

// ── Stock badges ──
const getStockBadgeClass = (prod) => {
  if (prod.stockActual <= 0) return 'sb-out'
  if (prod.stockActual <= prod.stockMinimo) return 'sb-low'
  return 'sb-ok'
}

const formatStock = (prod) => {
  if ((prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') && prod.cantidadMayorista > 0) {
    const totalInner = prod.stockActual * prod.cantidadMayorista
    const formatted = parseFloat(totalInner.toFixed(2))
    let unidad = prod.nombreUnidadMayorista
    if (!unidad || unidad === '0' || unidad === '') unidad = prod.unidadMedida === 'SACO' ? 'KG' : 'UND'
    return `${formatted}${unidad}`
  }
  return `${prod.stockActual}`
}

// ── Carrito ──
const agregarAlCarrito = (prod) => {
  if (prod.stockActual <= 0) {
    // Alerta pequeña (o nada), pero como es stock = 0, mostramos alert
    alert(`"${prod.nombre}" está agotado.`)
    return
  }
  const existe = carrito.value.find(item => item.producto.productoID === prod.productoID)
  if (existe) {
    sumarCantidad(carrito.value.indexOf(existe))
  } else {
    carrito.value.push({
      producto: prod,
      productoID: prod.productoID,
      cantidad: 1,
      precioVentaUnitario: prod.precioVenta,
      tipoVenta: prod.unidadMedida === 'SACO' ? 'KG' : (prod.unidadMedida === 'BALDE' ? 'UND' : 'UND')
    })
  }
  // Silencioso. El drawer no se abre automáticamente. El Floating bar mostrará el item agregado.
}

const cambiarTipoVenta = (idx) => {
  const item = carrito.value[idx]
  if (item.tipoVenta === 'SACO' || item.tipoVenta === 'BALDE') {
    item.precioVentaUnitario = item.producto.precioMayorista || 0
    item.cantidad = 1
  } else {
    item.precioVentaUnitario = item.producto.precioVenta
    item.cantidad = 1
  }
}

const sumarCantidad = (idx) => {
  const item = carrito.value[idx]
  const udsEnBulto = item.producto.cantidadMayorista || 1
  const esFraccion = (item.producto.unidadMedida === 'SACO' && item.tipoVenta === 'KG') ||
                     (item.producto.unidadMedida === 'BALDE' && item.tipoVenta === 'UND')
  const decrementoFisico = esFraccion ? (1 / udsEnBulto) : 1
  if ((item.cantidad + 1) * decrementoFisico <= item.producto.stockActual) {
    item.cantidad++
  } else {
    alert(`Límite de stock alcanzado: ${item.producto.stockActual.toFixed(3)} ${item.producto.unidadMedida === 'SACO' ? 'sacos' : 'bultos'}.`)
  }
}

const restarCantidad = (idx) => {
  const item = carrito.value[idx]
  if (item.cantidad > 1) { item.cantidad-- }
  else { eliminarDelCarrito(idx) }
}

const eliminarDelCarrito = (idx) => {
  carrito.value.splice(idx, 1)
  if (carrito.value.length === 0) isCartOpen.value = false // Cierra el drawer si se vació
}

const validarPrecioCosto = (idx) => {
  const item = carrito.value[idx]
  const costoBase = item.producto.precioCosto || 0
  let costoMinimo = 0
  if (item.tipoVenta === 'SACO' || item.tipoVenta === 'BALDE') {
    costoMinimo = costoBase
  } else {
    const divisor = (item.producto.cantidadMayorista && item.producto.cantidadMayorista > 0) ? item.producto.cantidadMayorista : 1
    costoMinimo = costoBase / divisor
  }
  if (item.precioVentaUnitario < costoMinimo) {
    alert(`⚠️ Precio (S/ ${item.precioVentaUnitario.toFixed(2)}) menor al costo (S/ ${costoMinimo.toFixed(2)}). Se ajustará al costo mínimo.`)
    item.precioVentaUnitario = costoMinimo
  }
}

// ── Modal Soles ──
const abrirModalSoles = (idx) => {
  const item = carrito.value[idx]
  itemSolesIdx.value = idx
  itemSolesNombre.value = item.producto.nombre
  inputSoles.value = '1.00'
  mostrarModalSoles.value = true
  setTimeout(() => { document.getElementById('inputMontoSolesM')?.focus() }, 100)
}

const confirmarCalculoSoles = () => {
  if (itemSolesIdx.value >= 0 && inputSoles.value) {
    const monto = parseFloat(inputSoles.value)
    if (!isNaN(monto) && monto > 0) {
      const item = carrito.value[itemSolesIdx.value]
      item.cantidad = parseFloat((monto / item.precioVentaUnitario).toFixed(3))
    }
  }
  cerrarModalSoles()
}

const cerrarModalSoles = () => {
  mostrarModalSoles.value = false
  inputSoles.value = ''
  itemSolesIdx.value = -1
  itemSolesNombre.value = ''
}

// ── Cumpleaños ──
const verificarCumpleanos = () => {
  if (ticket.value.clienteID == 0) { cumpleaneroHoy.value = null; return }
  const cliente = clientes.value.find(c => c.clienteID == ticket.value.clienteID)
  if (cliente?.fechaNacimiento) {
    const bday = new Date(cliente.fechaNacimiento)
    const today = new Date()
    cumpleaneroHoy.value = (bday.getDate() === today.getDate() && bday.getMonth() === today.getMonth()) ? cliente : null
  } else {
    cumpleaneroHoy.value = null
  }
}

// ── Procesar Venta ──
const procesarVenta = async () => {
  if (!ticket.value.serie || !ticket.value.numero) {
    alert('Falta Serie y Número de Comprobante por defecto.')
    return
  }
  vendiendo.value = true
  errorGlobal.value = ''

  const payload = {
    clienteID: ticket.value.clienteID,
    serieComprobante: ticket.value.serie,
    numeroComprobante: ticket.value.numero,
    descuento: Number(ticket.value.descuento) || 0,
    observaciones: ticket.value.observaciones,
    metodoPago: ticket.value.metodoPago,
    detalles: carrito.value.map(item => ({
      productoID: item.productoID,
      cantidad: item.cantidad,
      precioVentaUnitario: item.precioVentaUnitario,
      unidadVenta: item.tipoVenta
    }))
  }

  try {
    const res = await fetch(`${API_URL}/Ventas`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${getToken()}` },
      body: JSON.stringify(payload)
    })
    const data = await res.json()
    if (data.success) {
      mostrarModalVenta.value = true
      isCartOpen.value = false // Oculta el drawer detrás del success
    } else {
      alert(data.mensaje || 'Error al procesar la venta. Verifica el stock disponible.')
    }
  } catch {
    alert('Fallo de red al registrar la venta.')
  } finally {
    vendiendo.value = false
  }
}

const cerrarModalVentaExitosa = () => {
  mostrarModalVenta.value = false
  carrito.value = []
  ticket.value.descuento = 0
  ticket.value.observaciones = ''
  ticket.value.metodoPago = 'Efectivo'
  generarCorrelativo()
  cargarDatos()
}
</script>
