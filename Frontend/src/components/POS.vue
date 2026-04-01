<template>
  <div class="pos-module">
    <div class="pos-header">
      <div>
        <h2 class="title">Punto de Venta (POS)</h2>
        <p class="subtitle">Módulo de caja para facturación rápida y salida de mercadería</p>
      </div>
      <div>
        <div class="status-badge" :class="conexionExitosa ? 'badge-on' : 'badge-off'">
          <span class="pulse-dot"></span>
          {{ conexionExitosa ? 'Sistema En Línea' : 'Verificando red...' }}
        </div>
      </div>
    </div>

    <!-- Mensajes Globales -->
    <div v-if="errorGlobal" class="error-banner">{{ errorGlobal }}</div>
    <div v-if="successGlobal" class="success-banner">{{ successGlobal }}</div>
    <div v-if="cumpleaneroHoy" class="birthday-alert animate-pop-in">
      <div class="bday-icon">🎂</div>
      <div class="bday-text">
        <strong>¡Hoy es el onomástico de {{ cumpleaneroHoy.nombreCompleto }}!</strong>
        <p>No olvides saludarlo y ofrecerle un descuento especial 🎉</p>
      </div>
    </div>

    <div class="pos-layout">
      <!-- PANEL IZQUIERDO: Buscador y Catálogo Visual -->
      <div class="panel-catalogo">
        <!-- Buscador -->
        <div class="search-bar">
          <input type="text" v-model="busqueda" placeholder="🔍 Buscar por nombre, código o categoría..." class="search-input" />
        </div>

        <!-- Grilla de productos (12 por página) -->
        <div class="products-grid" v-for="dummy in [1]" :key="dummy" v-if="!cargandoProductos">
          <div 
            class="product-card" 
            v-for="prod in productosPaginados" 
            :key="prod.productoID"
            @click="agregarAlCarrito(prod)"
            :class="{ 'out-of-stock': prod.stockActual <= 0 }"
          >
            <div class="card-img-wrapper">
              <img v-if="prod.imagenURL" :src="`${IMAGE_BASE}${prod.imagenURL}`" class="prod-img" alt="Foto producto" />
              <div v-else class="no-img"><span>📦</span></div>
              
              <!-- Badge de stock superpuesto -->
              <div class="stock-badge" :class="getStockClass(prod)">
                Stock: {{ prod.stockActual }}
              </div>
            </div>
            <div class="card-info">
              <h4 class="prod-name">{{ prod.nombre }}</h4>
              <p class="prod-code">Cod: {{ prod.codigo }}</p>
              <div class="prod-price">S/ {{ prod.precioVenta.toFixed(2) }}</div>
            </div>
          </div>
          <div v-if="productosFiltrados.length === 0" class="empty-products">
            <p>No se encontraron productos disponibles.</p>
          </div>
        </div>
        
        <div v-else class="loading-products">
          <div class="spinner"></div>
          <p>Cargando catálogo en línea...</p>
        </div>

        <!-- Controles de Paginación -->
        <div class="pagination-controls" v-if="totalPages > 1">
          <button @click="prevPage" :disabled="currentPage === 1" class="page-btn">
            ← Anterior
          </button>
          <span class="page-indicator">Página {{ currentPage }} de {{ totalPages }}</span>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="page-btn">
            Siguiente →
          </button>
        </div>
      </div>

      <!-- PANEL DERECHO: Carrito (Ticket de Venta) -->
      <div class="panel-ticket">
        <h3 class="ticket-title">Comprobante de Venta</h3>

        <!-- Datos del Comprobante -->
        <div class="ticket-header-form">
          <div class="form-group row-inline">
            <div style="flex: 1;">
              <label>Serie *</label>
              <input type="text" v-model="ticket.serie" placeholder="B001" required />
            </div>
            <div style="flex: 1;">
              <label>Número *</label>
              <input type="text" v-model="ticket.numero" placeholder="0001294" required />
            </div>
          </div>

          <div class="form-group" style="margin-top: 0.5rem;">
            <div class="label-with-action">
              <label>Cliente (Opcional)</label>
              <button class="btn-text-link" @click="mostrarModalNuevoCliente = true">+ Nuevo Cliente</button>
            </div>
            <select v-model="ticket.clienteID" @change="verificarCumpleanos">
              <option value="0">Cliente Varios (Consumidor Final)</option>
              <option v-for="c in clientes" :key="c.clienteID" :value="c.clienteID">
                {{ c.nombreCompleto }} - {{ c.documentoIdentidad }}
              </option>
            </select>
          </div>
        </div>

        <div class="ticket-divider"></div>

        <!-- Lista del carrito -->
        <div class="cart-items-container">
          <div v-if="carrito.length === 0" class="empty-cart">
            <span class="cart-icon-big">🛒</span>
            <p>El carrito está vacío.<br/>Haz clic en un producto para añadirlo.</p>
          </div>
          
          <div class="cart-item" v-for="(item, index) in carrito" :key="index">
            <div class="item-main">
              <div>
                <p class="item-name">{{ item.producto.nombre }}</p>
                <!-- Selector de Unidad (Solo si el producto es SACO o BALDE) -->
                <div v-if="item.producto.unidadMedida === 'SACO'" class="unit-selector">
                  <select v-model="item.tipoVenta" @change="cambiarTipoVenta(index)" class="select-unit-mini">
                    <option value="KG">Vender por Kilo (Kg) - S/ {{ item.producto.precioVenta.toFixed(2) }}</option>
                    <option value="SACO">Vender por Saco (Bulto) - S/ {{ (item.producto.precioMayorista || 0).toFixed(2) }}</option>
                  </select>
                </div>
                <div v-else-if="item.producto.unidadMedida === 'BALDE'" class="unit-selector">
                  <select v-model="item.tipoVenta" @change="cambiarTipoVenta(index)" class="select-unit-mini">
                    <option value="UND">Vender por Unidad Suelta - S/ {{ item.producto.precioVenta.toFixed(2) }}</option>
                    <option value="BALDE">Vender Balde Entero - S/ {{ (item.producto.precioMayorista || 0).toFixed(2) }}</option>
                  </select>
                </div>
                <p v-else class="item-price-unit">S/ {{ item.precioVentaUnitario.toFixed(2) }} x {{ item.producto.unidadMedida === 'UND' ? 'Unidad' : item.producto.unidadMedida }}</p>
              </div>
            </div>
            
            <div class="item-actions-grouped">
              <div class="action-column">
                <span class="action-caption">
                  {{ item.tipoVenta === 'KG' ? 'Ingresar Cant. (KG)' : (item.tipoVenta === 'SACO' ? 'Nro. de Sacos' : (item.tipoVenta === 'BALDE' ? 'Nro. de Baldes' : 'Ingresar Cantidad')) }}
                </span>
                <div class="qty-control">
                  <button @click="restarCantidad(index)" class="qty-btn">-</button>
                  <input type="number" v-model.number="item.cantidad" class="qty-input editable" step="0.001" min="0.001" />
                  <button @click="sumarCantidad(index)" class="qty-btn">+</button>
                </div>
              </div>

              <!-- Botón para calcular por montos exactos SOLO SI SE VENDE POR KILO -->
              <div class="action-column" v-if="item.tipoVenta === 'KG'">
                <span class="action-caption">Monto Exacto</span>
                <button class="calc-btn-labeled" @click="abrirModalSoles(index)" title="Vender ingresando Soles (S/) exactos">
                  💰 S/
                </button>
              </div>

              <div class="action-column subtotal-col">
                <span class="action-caption">Subtotal</span>
                <div class="item-subtotal">
                  S/ {{ (item.cantidad * item.precioVentaUnitario).toFixed(2) }}
                </div>
              </div>
              <button class="remove-btn" @click="eliminarDelCarrito(index)" title="Quitar item">✕</button>
            </div>
          </div>
        </div>

        <div class="ticket-divider"></div>

        <!-- Totales y Cobro -->
        <div class="ticket-footer">
          <div class="total-row subtotal">
            <span style="font-size: 0.9rem; color: #718096;">Subtotal</span>
            <span style="font-size: 1rem; color: #4A5568;">S/ {{ subtotalVenta.toFixed(2) }}</span>
          </div>
          <div class="total-row discount-row" style="margin-top: -0.5rem; border-bottom: 1px dashed #E2E8F0; padding-bottom: 0.5rem;">
            <span style="font-size: 0.9rem; color: #E53E3E;">Descuento Global</span>
            <div style="display: flex; align-items: center; gap: 0.5rem;">
              <span style="font-size: 0.9rem; color: #E53E3E;">- S/ </span>
              <input type="number" v-model.number="ticket.descuento" min="0" step="0.5" 
                     style="width: 80px; text-align: right; border: 1px solid #FED7D7; border-radius: 6px; padding: 0.25rem; font-weight: 700; color: #C53030; background: #FFF5F5;" />
            </div>
          </div>

          <div class="total-row" style="margin-top: 0.5rem; margin-bottom: 0.5rem;">
             <div style="width: 100%;">
                <label style="font-size: 0.75rem; font-weight: 600; color: #718096; display: block; margin-bottom: 0.2rem;">Notas / Observaciones (Opcional)</label>
                <textarea v-model="ticket.observaciones" placeholder="Ej: Pago con Yape, recoge productos mañana..." 
                          style="width: 100%; height: 40px; border-radius: 8px; border: 1px solid #E2E8F0; padding: 0.4rem; font-family: inherit; font-size: 0.8rem; resize: none; color: #4A5568;"></textarea>
             </div>
          </div>

          <div class="total-row">
            <span>Total Recaudado</span>
            <span class="total-monto">S/ {{ totalVenta.toFixed(2) }}</span>
          </div>

          <button class="checkout-btn" :disabled="carrito.length === 0 || vendiendo" @click="procesarVenta">
            <span v-if="vendiendo" class="spinner-small"></span>
            {{ vendiendo ? 'Procesando Tarjeta/Efectivo...' : 'Generar Venta 💳' }}
          </button>
        </div>

      </div>
    </div>
    
    <!-- Modal Calculadora de Soles -->
    <div v-if="mostrarModalSoles" class="modal-overlay" @click.self="cerrarModalSoles">
      <div class="modal-card animate-slide-up" style="max-width: 380px; padding: 1.5rem;">
        <div class="modal-header" style="margin-bottom: 1rem;">
          <h3 style="font-size: 1.15rem;">💰 Vender por Monto (S/)</h3>
          <button class="btn-close" @click="cerrarModalSoles">×</button>
        </div>
        <p style="color: #4A5568; font-size: 0.9rem; margin-bottom: 1.5rem;">
          ¿Cuánto comprará el cliente en Soles de:<br/>
          <strong style="color: #2D3748; font-size: 0.95rem;">{{ itemSolesNombre }}</strong>?
        </p>
        <div class="form-group" style="margin-bottom: 1.5rem;">
          <input type="number" v-model="inputSoles" step="0.50" min="0.10" 
                 @keyup.enter="confirmarCalculoSoles"
                 id="inputMontoSoles"
                 style="font-size: 1.8rem; text-align: center; padding: 1rem; font-weight: 800; color: #1A365D; border: 2px solid #A7C7E7; border-radius: 12px; transition: all 0.2s; outline: none;" 
                 onfocus="this.style.boxShadow='0 0 0 4px rgba(167, 199, 231, 0.3)'"
                 onblur="this.style.boxShadow='none'" />
        </div>
        <div class="modal-footer" style="margin-top: 0;">
          <button class="secondary-btn" @click="cerrarModalSoles">Cancelar</button>
          <button class="primary-btn" @click="confirmarCalculoSoles">Aplicar</button>
        </div>
      </div>
    </div>

    <!-- Modal de Venta Exitosa -->
    <div v-if="mostrarModalVenta" class="modal-overlay">
      <div class="modal-success">
        <div class="success-icon-container">
          <div class="check-animation">✅</div>
        </div>
        <h3>¡Transacción Aprobada!</h3>
        <p>La venta se registró correctamente. Se ha descontado el stock del inventario.</p>
        <button class="primary-btn" @click="cerrarModalNuevoCliente">Emitir Nuevo Ticket</button>
      </div>
    </div>
    <!-- Modal Nuevo Cliente -->
    <div v-if="mostrarModalNuevoCliente" class="modal-overlay" @click.self="mostrarModalNuevoCliente = false">
      <div class="modal-card animate-slide-up">
        <div class="modal-header">
          <h3>Registrar Nuevo Cliente</h3>
          <button class="btn-close" @click="mostrarModalNuevoCliente = false">×</button>
        </div>
        <div class="form-grid">
          <div class="form-group full">
            <label>Nombre Completo *</label>
            <input type="text" v-model="nuevoCliente.nombreCompleto" placeholder="Ej. Juan Perez" />
          </div>
          <div class="form-group">
            <label>DNI / RUC *</label>
            <input type="text" v-model="nuevoCliente.documentoIdentidad" placeholder="DNI o RUC" />
          </div>
          <div class="form-group">
            <label>Fecha Nacimiento</label>
            <input type="date" v-model="nuevoCliente.fechaNacimiento" />
          </div>
          <div class="form-group">
            <label>Teléfono</label>
            <input type="text" v-model="nuevoCliente.telefono" />
          </div>
          <div class="form-group">
            <label>Correo</label>
            <input type="email" v-model="nuevoCliente.correo" />
          </div>
          <div class="form-group full">
            <label>Dirección</label>
            <input type="text" v-model="nuevoCliente.direccion" placeholder="Calle, Urb, Distrito..." />
          </div>
        </div>
        <div class="modal-footer">
          <button class="secondary-btn" @click="mostrarModalNuevoCliente = false">Cancelar</button>
          <button class="primary-btn" @click="guardarNuevoCliente" :disabled="creandoCliente">
            {{ creandoCliente ? 'Guardando...' : 'Registrar y Seleccionar' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'

const API_URL = '/api'
const IMAGE_BASE = ''

const productos = ref([])
const clientes = ref([])
const cargandoProductos = ref(true)
const conexionExitosa = ref(false)
const errorGlobal = ref('')
const successGlobal = ref('')

const busqueda = ref('')
const currentPage = ref(1)
const itemsPerPage = 12

// Carrito y Estado de Venta
const carrito = ref([])
const vendiendo = ref(false)
const mostrarModalVenta = ref(false)

const ticket = ref({
  serie: 'B001',
  numero: '',
  clienteID: 0,
  descuento: 0,
  observaciones: ''
})

const mostrarModalSoles = ref(false)
const inputSoles = ref('')
const itemSolesIdx = ref(-1)
const itemSolesNombre = ref('')

const cumpleaneroHoy = ref(null)
const mostrarModalNuevoCliente = ref(false)
const creandoCliente = ref(false)
const nuevoCliente = ref({
  nombreCompleto: '',
  documentoIdentidad: '',
  fechaNacimiento: '',
  telefono: '',
  correo: '',
  direccion: '',
  activo: true
})

const getToken = () => localStorage.getItem('jwt_token')

const cargarDatos = async () => {
  errorGlobal.value = ''
  try {
    const resP = await fetch(`${API_URL}/Productos`, {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const resC = await fetch(`${API_URL}/Clientes`, {
        headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    
    // Fallback if Clientes endpoint does not exist yet
    if (resC.ok) {
        const dataC = await resC.json();
        if (dataC.success) clientes.value = dataC.data;
    }

    const dataP = await resP.json()
    if (dataP.success) {
      // Filtrar solo los productos ACTIVOS
      productos.value = dataP.data.filter(p => p.activo === true)
      conexionExitosa.value = true
    } else {
      errorGlobal.value = dataP.mensaje
    }
  } catch (err) {
    errorGlobal.value = 'Error al conectar con la base de datos (Inventario / Clientes).'
  } finally {
    cargandoProductos.value = false
  }
}

onMounted(() => {
  cargarDatos()
  generarCorrelativo()
})

const generarCorrelativo = () => {
  // Simulacion de generador de numero de boleta aleatorio para esta demo
  const r = Math.floor(Math.random() * 900000) + 100000
  ticket.value.numero = r.toString()
}

const verificarCumpleanos = () => {
  if (ticket.value.clienteID == 0) {
    cumpleaneroHoy.value = null
    return
  }
  const cliente = clientes.value.find(c => c.clienteID == ticket.value.clienteID)
  if (cliente && cliente.fechaNacimiento) {
    const bday = new Date(cliente.fechaNacimiento)
    const today = new Date()
    if (bday.getDate() === today.getDate() && bday.getMonth() === today.getMonth()) {
      cumpleaneroHoy.value = cliente
    } else {
      cumpleaneroHoy.value = null
    }
  } else {
    cumpleaneroHoy.value = null
  }
}

const guardarNuevoCliente = async () => {
  if (!nuevoCliente.value.nombreCompleto || !nuevoCliente.value.documentoIdentidad) {
    alert("❌ El nombre y el documento de identidad son obligatorios.");
    return
  }
  
  // Asignar fecha de registro interna
  nuevoCliente.value.fechaRegistro = new Date().toISOString().split('T')[0];
  
  // 🧹 Limpieza: Convertir "" a null para evitar 400 Bad Request
  const payload = { ...nuevoCliente.value };
  if (!payload.fechaNacimiento) payload.fechaNacimiento = null;

  creandoCliente.value = true;
  try {
    const res = await fetch(`${API_URL}/clientes`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(payload)
    })
    const data = await res.json()
    if (data.success) {
      successGlobal.value = "Cliente registrado."
      setTimeout(() => successGlobal.value = '', 3000)
      await cargarDatos()
      ticket.value.clienteID = data.data.clienteID
      verificarCumpleanos()
      mostrarModalNuevoCliente.value = false;
      nuevoCliente.value = { 
        nombreCompleto: '', 
        documentoIdentidad: '', 
        fechaNacimiento: '', 
        telefono: '', 
        correo: '', 
        direccion: '', 
        activo: true,
        fechaRegistro: ''
      };
    }
  } catch (e) {
    alert("❌ Error al registrar cliente. Verifica los datos o la conexión.");
  } finally {
    creandoCliente.value = false;
  }
}

const productosFiltrados = computed(() => {
  if (busqueda.value.trim() === '') return productos.value
  const q = busqueda.value.toLowerCase()
  return productos.value.filter(p => 
    p.nombre.toLowerCase().includes(q) || 
    p.codigo.toLowerCase().includes(q)
  )
})

const productosPaginados = computed(() => {
  const skip = (currentPage.value - 1) * itemsPerPage
  return productosFiltrados.value.slice(skip, skip + itemsPerPage)
})

const totalPages = computed(() => Math.ceil(productosFiltrados.value.length / itemsPerPage))

const nextPage = () => { if (currentPage.value < totalPages.value) currentPage.value++ }
const prevPage = () => { if (currentPage.value > 1) currentPage.value-- }

// Resetear página al buscar
watch(busqueda, () => {
  currentPage.value = 1
})

const subtotalVenta = computed(() => {
  return carrito.value.reduce((sum, item) => sum + (item.cantidad * item.precioVentaUnitario), 0)
})

const totalVenta = computed(() => {
  const d = Number(ticket.value.descuento) || 0;
  return Math.max(subtotalVenta.value - d, 0);
})

const getStockClass = (prod) => {
  if (prod.stockActual <= 0) return 'stock-none'
  if (prod.stockActual <= prod.stockMinimo) return 'stock-low'
  return 'stock-ok'
}

const agregarAlCarrito = (prod) => {
  if (prod.stockActual <= 0) {
    alert(`El producto ${prod.nombre} está agotado.`);
    return;
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
}

const cambiarTipoVenta = (idx) => {
  const item = carrito.value[idx];
  if (item.tipoVenta === 'SACO' || item.tipoVenta === 'BALDE') {
    item.precioVentaUnitario = item.producto.precioMayorista || 0;
    item.cantidad = 1;
  } else if (item.tipoVenta === 'KG' || item.tipoVenta === 'UND') {
    item.precioVentaUnitario = item.producto.precioVenta;
    item.cantidad = 1;
  }
}

const sumarCantidad = (idx) => {
  const item = carrito.value[idx]
  
  const udsEnBulto = item.producto.cantidadMayorista || 1;
  const esFraccion = (item.producto.unidadMedida === 'SACO' && item.tipoVenta === 'KG') || 
                     (item.producto.unidadMedida === 'BALDE' && item.tipoVenta === 'UND');
                     
  const decrementoFisico = esFraccion ? (1 / udsEnBulto) : 1;
  const cantidadNuevaEnSacos = (item.cantidad + 1) * decrementoFisico;
  
  if (cantidadNuevaEnSacos <= item.producto.stockActual) {
    item.cantidad++
  } else {
    alert(`Límite de stock físico alcanzado. Quedan ${item.producto.stockActual.toFixed(3)} ${item.producto.unidadMedida === 'SACO' ? 'sacos' : 'bultos'} disponibles.`)
  }
}

const restarCantidad = (idx) => {
  const item = carrito.value[idx]
  if (item.cantidad > 1) {
    item.cantidad--
  } else {
    eliminarDelCarrito(idx)
  }
}

const eliminarDelCarrito = (idx) => {
  carrito.value.splice(idx, 1)
}

const abrirModalSoles = (idx) => {
  const item = carrito.value[idx]
  itemSolesIdx.value = idx
  itemSolesNombre.value = item.producto.nombre
  inputSoles.value = '1.00'
  mostrarModalSoles.value = true
  // Autofocus con pequeño rebote
  setTimeout(() => {
    const input = document.getElementById('inputMontoSoles')
    if (input) {
      input.focus()
      input.select()
    }
  }, 100)
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

const procesarVenta = async () => {
  if (!ticket.value.serie || !ticket.value.numero) {
    errorGlobal.value = "Por favor, completa la Serie y Número de Comprobante."
    setTimeout(() => errorGlobal.value='', 3500)
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
    detalles: carrito.value.map(item => {
      // El backend ahora recibe cantidad nominal y unidad de venta, manejando él mismo la división del stock
      return {
        productoID: item.productoID,
        cantidad: item.cantidad,
        precioVentaUnitario: item.precioVentaUnitario,
        unidadVenta: item.tipoVenta // 'KG', 'SACO' o 'UND'
      }
    })
  }

  try {
    const res = await fetch(`${API_URL}/Ventas`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(payload)
    })
    
    const data = await res.json()
    if (data.success) {
      mostrarModalVenta.value = true
    } else {
      errorGlobal.value = data.mensaje || 'Error al procesar la venta. Revisa el stock disponible.'
    }
  } catch (error) {
    errorGlobal.value = 'Fallo de red al intentar registrar la venta.'
  } finally {
    vendiendo.value = false
  }
}

const cerrarModalNuevoCliente = () => {
  mostrarModalVenta.value = false
  carrito.value = []
  ticket.value.descuento = 0
  ticket.value.observaciones = ''
  generarCorrelativo()
  cargarDatos() // Recargar para actualizar los nuevos niveles de stock en la grilla visual
}
</script>

<style scoped>
.pos-module { animation: fadeIn 0.4s ease; padding-bottom: 2rem; }

.pos-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
.title { font-size: 1.75rem; font-weight: 700; color: #1A202C; margin: 0 0 0.25rem 0; }
.subtitle { color: #718096; margin: 0; font-size: 0.95rem; }

.status-badge { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; border-radius: 20px; font-size: 0.85rem; font-weight: 600; }
.badge-on { background-color: #F0FFF4; color: #2F855A; border: 1px solid #C6F6D5; }
.badge-off { background-color: #FFF5F5; color: #C53030; border: 1px solid #FED7D7; }

.pulse-dot { width: 8px; height: 8px; border-radius: 50%; background-color: currentColor; animation: pulse 2s infinite; }
@keyframes pulse { 0% { box-shadow: 0 0 0 0 rgba(0,0,0, 0.4); } 70% { box-shadow: 0 0 0 6px rgba(0,0,0, 0); } 100% { box-shadow: 0 0 0 0 rgba(0,0,0, 0); } }

.error-banner { background-color: #FFF5F5; color: #C53030; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; border: 1px solid #FED7D7;}
.success-banner { background-color: #F0FFF4; color: #2F855A; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; border: 1px solid #C6F6D5;}

/* LAYOUT PRINCIPAL DEL POS */
.pos-layout { display: flex; gap: 2rem; height: calc(100vh - 120px); min-height: 500px;}

/* Panel Catálogo */
.panel-catalogo { flex: 65; display: flex; flex-direction: column; background: white; border-radius: 16px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03); border: 1px solid #E2E8F0; padding: 1.5rem; overflow: hidden;}
.search-bar { margin-bottom: 1.5rem; }
.search-input { width: 100%; padding: 1rem 1.25rem; border-radius: 12px; border: 2px solid #E2E8F0; font-size: 1rem; color: #2D3748; outline: none; transition: border-color 0.2s, box-shadow 0.2s; background: #F7FAFC; font-family: inherit;}
.search-input:focus { border-color: #A7C7E7; background: white; box-shadow: 0 0 0 4px rgba(167, 199, 231, 0.15); }

/* Paginacion */
.pagination-controls {
  display: flex; justify-content: center; align-items: center; gap: 1.5rem;
  margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid #EDF2F7;
}
.page-btn {
  padding: 0.6rem 1.25rem; border-radius: 10px; border: 1px solid #E2E8F0;
  background: white; color: #4A5568; font-weight: 600; cursor: pointer;
  transition: all 0.2s;
}
.page-btn:hover:not(:disabled) { border-color: #A7C7E7; color: #2C5282; background: #F8FAFC; }
.page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.page-indicator { font-size: 0.9rem; font-weight: 700; color: #718096; }

/* Grilla Productos */
.products-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1.25rem; overflow-y: auto; padding-right: 0.5rem; }
.products-grid::-webkit-scrollbar { width: 6px; }
.products-grid::-webkit-scrollbar-thumb { background: #CBD5E0; border-radius: 10px; }

.product-card { background: white; border: 1px solid #E2E8F0; border-radius: 14px; overflow: hidden; cursor: pointer; transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1); display: flex; flex-direction: column; position: relative;}
.product-card:hover:not(.out-of-stock) { transform: translateY(-4px); box-shadow: 0 12px 25px -5px rgba(0,0,0,0.1); border-color: #A7C7E7; }
.product-card:active:not(.out-of-stock) { transform: translateY(0); }

.out-of-stock { opacity: 0.5; filter: grayscale(1); cursor: not-allowed; }

.card-img-wrapper { height: 140px; background: #F8FAFC; position: relative; overflow: hidden; display: flex; align-items: center; justify-content: center;}
.prod-img { width: 100%; height: 100%; object-fit: cover; }
.no-img { font-size: 3rem; opacity: 0.2; }

/* Status stock en miniatura */
.stock-badge { position: absolute; top: 8px; right: 8px; padding: 0.25rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 700; color: white; box-shadow: 0 2px 4px rgba(0,0,0,0.2);}
.stock-ok { background-color: #48BB78; }
.stock-low { background-color: #ED8936; }
.stock-none { background-color: #E53E3E; }

.card-info { padding: 1rem; display: flex; flex-direction: column; flex: 1; justify-content: space-between;}
.prod-name { margin: 0 0 0.25rem 0; font-size: 0.95rem; font-weight: 700; color: #2D3748; line-height: 1.2;}
.prod-code { margin: 0; font-size: 0.75rem; color: #A0AEC0; }
.prod-price { margin-top: 0.75rem; font-size: 1.1rem; font-weight: 800; color: #1A365D; }

/* PANEL COMANDA/TICKET */
.panel-ticket { flex: 35; display: flex; flex-direction: column; background: #F8FAFC; border-radius: 16px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05); border: 1px solid #E2E8F0; overflow: hidden; }

.ticket-title { margin: 0; padding: 0.75rem 1.25rem; background: white; border-bottom: 1px solid #E2E8F0; font-size: 1rem; font-weight: 700; color: #2D3748;}
.ticket-header-form { padding: 0.75rem 1.25rem; background: white; }

.row-inline { display: flex; gap: 1rem; }
.form-group label { display: block; font-size: 0.75rem; font-weight: 600; color: #718096; margin-bottom: 0.2rem;}
.form-group input, .form-group select { width: 100%; box-sizing: border-box; padding: 0.45rem; border-radius: 8px; border: 1px solid #CBD5E0; font-family: inherit; font-size: 0.85rem; outline: none; transition: border-color 0.2s;}
.form-group input:focus, .form-group select:focus { border-color: #A7C7E7; }

.ticket-divider { height: 4px; background: url('data:image/svg+xml;utf8,<svg width="10" height="4" xmlns="http://www.w3.org/2000/svg"><path d="M0 2h5" stroke="%23CBD5E0" stroke-width="2" fill="none"/></svg>') repeat-x; margin: 0; }

.cart-items-container { flex: 1; overflow-y: auto; padding: 0.75rem 1.25rem; background: #F8FAFC; display: flex; flex-direction: column; gap: 0.5rem;}
.cart-items-container::-webkit-scrollbar { width: 4px; }
.cart-items-container::-webkit-scrollbar-thumb { background: #CBD5E0; border-radius: 10px; }

.empty-cart { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #A0AEC0; text-align: center;}
.cart-icon-big { font-size: 3rem; margin-bottom: 1rem; opacity: 0.5;}

.cart-item { background: white; border-radius: 8px; padding: 0.5rem 0.75rem; border: 1px solid #E2E8F0; display: flex; flex-direction: column; gap: 0.35rem; animation: slideInX 0.2s ease;}
@keyframes slideInX { from { transform: translateX(20px); opacity: 0; } }

.item-main { display: flex; justify-content: space-between; align-items: flex-start;}
.item-name { margin: 0; font-weight: 600; color: #2D3748; font-size: 0.9rem; }
.item-price-unit { margin: 0; font-size: 0.8rem; color: #718096; }

.item-actions-grouped { display: flex; align-items: flex-end; gap: 0.5rem; margin-top: 0.25rem; justify-content: space-between; }
.action-column { display: flex; flex-direction: column; gap: 0.25rem; }
.action-caption { font-size: 0.6rem; color: #718096; text-transform: uppercase; font-weight: 800; letter-spacing: 0.05em; line-height: 1; }
.subtotal-col { align-items: flex-end; flex: 1; margin-right: 0.25rem; }

.qty-control { display: flex; align-items: center; background: #EDF2F7; border-radius: 6px; overflow: hidden; border: 1px solid #E2E8F0; height: 26px;}
.qty-btn { width: 24px; height: 100%; border: none; background: transparent; cursor: pointer; font-weight: bold; font-size: 0.9rem; color: #4A5568; display: flex; align-items: center; justify-content: center; padding: 0;}
.qty-btn:hover { background: #E2E8F0; }
.qty-input { width: 50px; height: 100%; border: none; text-align: center; background: transparent; font-size: 0.85rem; font-weight: 600; font-family: inherit; -moz-appearance: textfield; appearance: textfield; }
.qty-input.editable { background-color: #fff; cursor: text; border-radius: 4px; box-shadow: inset 0 1px 2px rgba(0,0,0,0.1); width: 60px;}
.qty-input::-webkit-outer-spin-button, .qty-input::-webkit-inner-spin-button { -webkit-appearance: none; appearance: none; margin: 0; }

.calc-btn-labeled { 
  background: #EBF8FF; border: 1px solid #BEE3F8; color: #2B6CB0; 
  border-radius: 6px; height: 26px; padding: 0 0.5rem;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; font-size: 0.75rem; font-weight: 700; gap: 0.25rem;
}
.calc-btn-labeled:hover { background: #BEE3F8; }

.unit-selector { margin: 0.3rem 0; }
.select-unit-mini { 
  width: 100%; 
  padding: 0.2rem; 
  font-size: 0.7rem; 
  border-radius: 4px; 
  border: 1px solid #CBD5E0; 
  background-color: #F7FAFC;
  font-weight: 600;
  color: #2D3748;
}

.item-subtotal { font-weight: 700; color: #2D3748; font-size: 0.95rem;}
.remove-btn { background: #FFF5F5; border: 1px solid #FED7D7; color: #C53030; border-radius: 6px; width: 24px; height: 24px; cursor: pointer; transition: 0.2s; display: flex; justify-content: center; align-items: center; font-size: 0.9rem;}
.remove-btn:hover { background: #FED7D7; }

.ticket-footer { background: white; padding: 1rem 1.25rem; display: flex; flex-direction: column; gap: 0.75rem;}
.total-row { display: flex; justify-content: space-between; align-items: center; font-size: 1.15rem; font-weight: 700; color: #2D3748;}
.total-monto { font-size: 1.5rem; color: #1A365D; }

.checkout-btn { background-color: #A7C7E7; color: #1A365D; border: none; padding: 0.9rem; border-radius: 10px; font-size: 1rem; font-weight: 700; cursor: pointer; transition: all 0.2s; display: flex; justify-content: center; align-items: center; box-shadow: 0 4px 15px rgba(167, 199, 231, 0.4);}
.checkout-btn:hover:not(:disabled) { transform: translateY(-2px); background-color: #8BA9C7; }
.checkout-btn:active:not(:disabled) { transform: translateY(0); }
.checkout-btn:disabled { opacity: 0.6; cursor: not-allowed; box-shadow: none; background-color: #E2E8F0; color: #A0AEC0;}

.spinner-small { width: 20px; height: 20px; border: 3px solid rgba(26, 54, 93, 0.2); border-top-color: #1A365D; border-radius: 50%; animation: spin 1s infinite linear; margin-right: 0.5rem; }

/* MODAL EXITO */
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0, 0, 0, 0.6); backdrop-filter: blur(5px); display: flex; align-items: center; justify-content: center; z-index: 999; animation: fadeIn 0.3s ease; }
.modal-success { background: white; padding: 2.5rem; border-radius: 24px; text-align: center; max-width: 400px; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.4); animation: scaleUp 0.4s cubic-bezier(0.16, 1, 0.3, 1);}
.success-icon-container { width: 80px; height: 80px; background: #F0FFF4; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 1.5rem auto; border: 4px solid #C6F6D5;}
.check-animation { font-size: 2.5rem; animation: popIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards; transform: scale(0); }
.modal-success h3 { margin: 0 0 1rem 0; font-size: 1.5rem; color: #2D3748; }
.modal-success p { color: #718096; line-height: 1.6; margin-bottom: 2rem; }
.modal-success .primary-btn { width: 100%; background-color: #48BB78; color: white; padding: 1rem; border: none; border-radius: 12px; font-weight: 700; font-size: 1rem; cursor: pointer; transition: 0.2s; }
.modal-success .primary-btn:hover { background-color: #38A169; transform: translateY(-2px); }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes scaleUp { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }
@keyframes popIn { 0% { transform: scale(0); } 70% { transform: scale(1.2); } 100% { transform: scale(1); } }

/* Estilos Adicionales Cumpleaños y Modal */
.birthday-alert {
  background: linear-gradient(135deg, #FFF5F7 0%, #FED7E2 100%);
  border: 1px solid #F6AD55;
  padding: 1rem 1.5rem;
  border-radius: 16px;
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 4px 15px rgba(213, 63, 140, 0.15);
}

.bday-icon { font-size: 2.5rem; }
.bday-text strong { color: #D53F8C; font-size: 1.1rem; }
.bday-text p { margin: 0.25rem 0 0 0; color: #702459; font-size: 0.95rem; }

.label-with-action { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.25rem; }
.btn-text-link { background: none; border: none; color: #553C9A; font-weight: 600; font-size: 0.75rem; cursor: pointer; padding: 0; text-decoration: underline; }

/* Reutilizar modal card de estilos generales si es posible o definir aqui */
.modal-card { background: white; width: 100%; max-width: 500px; border-radius: 20px; padding: 2rem; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1); }
.modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.modal-header h3 { margin: 0; color: #2D3748; }
.modal-footer { margin-top: 2rem; display: flex; justify-content: flex-end; gap: 1rem; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.5rem; }
.form-group.full { grid-column: span 2; }
.btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; }
.secondary-btn { background: #EDF2F7; color: #4A5568; border: none; padding: 0.75rem 1.5rem; border-radius: 10px; font-weight: 600; cursor: pointer; }

.animate-slide-up { animation: slideUp 0.3s ease-out; }
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
</style>
