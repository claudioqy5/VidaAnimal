<template>
  <div class="compras-module">
    <div class="module-header">
      <div>
        <h2 class="title">Abastecimiento / Compras</h2>
        <p class="subtitle">Registra la llegada de mercadería y aumenta tu stock</p>
      </div>
    </div>

    <div v-if="errorGlobal" class="error-banner">{{ errorGlobal }}</div>
    <div v-if="successGlobal" class="success-banner">{{ successGlobal }}</div>

    <div v-if="cargando" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando datos del sistema...</p>
    </div>

    <div v-else class="pos-layout">
      <!-- Panel Izquierdo: Formulario de Ingreso -->
      <div class="panel panel-left">
        <h3 class="panel-title">1. Datos del Proveedor y Factura</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Proveedor *</label>
            <div class="select-with-button">
              <select v-model="compraHeader.proveedorID">
                <option value="0" disabled>Selecciona el proveedor...</option>
                <option v-for="p in proveedores" :key="p.proveedorID" :value="p.proveedorID">{{ p.nombre }}</option>
              </select>
              <button class="quick-btn" @click="abrirModalProveedor" title="Crear Proveedor Nuevo">
                ➕
              </button>
            </div>
          </div>
          <div class="form-group">
            <label>Nro. Comprobante / Factura *</label>
            <input type="text" v-model="compraHeader.numeroComprobante" placeholder="Ej. F001-0002934" />
          </div>
        </div>

        <div class="divider"></div>

        <h3 class="panel-title" :style="{ opacity: puedeUsarPanelSecundario ? 1 : 0.5 }">2. Agregar Productos al Lote</h3>
        <fieldset :disabled="!puedeUsarPanelSecundario" class="fieldset-no-border">
          <p v-if="!puedeUsarPanelSecundario" style="color: #E53E3E; font-size: 0.85rem; margin-top: -10px; margin-bottom: 1rem;">
            * Completa los datos del paso 1 para habilitar este panel.
          </p>

          <div class="form-group" style="margin-bottom: 1rem;">
            <label>Producto a ingresar *</label>
            <div class="select-with-button">
              <div class="custom-combobox">
                <input 
                  type="text"
                  v-model="busquedaDetalleTemp"
                  placeholder="Busca por nombre o código..."
                  class="search-input-select"
                  @focus="abrirDropdownAlEnfocar"
                  @input="onInputBusqueda"
                  @click.stop="toggleDropdownProductos"
                />
                <!-- Flechita para indicar que es un select -->
                <span class="combobox-arrow" @click.stop="toggleDropdownProductos">▼</span>
                
                <div v-if="mostrarDropdownProductos" class="custom-dropdown custom-scrollbar" v-click-outside="cerrarDropdownSafe">
                  <div v-if="productosFiltradosBusqueda.length > 0">
                    <div 
                      v-for="prod in productosFiltradosBusqueda" 
                      :key="prod.productoID" 
                      class="dropdown-option"
                      @click.stop="seleccionarProductoDropdown(prod)"
                    >
                      <div class="option-main">
                        <span class="option-code">[{{ prod.codigo }}]</span>
                        <span class="option-name">{{ prod.nombre }}</span>
                      </div>
                      <span class="option-price">Costo: S/ {{ prod.precioCosto }}</span>
                    </div>
                  </div>
                  <div v-else class="custom-dropdown-empty">
                    No se encontró el producto 🐾
                  </div>
                </div>
              </div>
              <button class="quick-btn" @click.prevent="abrirModalProducto" title="Crear Producto Nuevo">
                ➕
              </button>
            </div>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label>Cantidad *</label>
              <input type="number" min="1" step="0.01" v-model="detalleTemp.cantidad" :disabled="detalleTemp.productoID === 0" :placeholder="detalleTemp.productoID === 0 ? 'Selecciona un prod. primero' : '0'" />
            </div>
            <div class="form-group">
              <label>Precio Costo Unitario (S/) *</label>
              <input type="number" min="0" step="0.01" v-model="detalleTemp.precioCostoUnitario" :disabled="detalleTemp.productoID === 0" :placeholder="detalleTemp.productoID === 0 ? 'Selecciona un prod. primero' : '0.00'" />
            </div>
          </div>
          
          <button class="add-btn" @click="agregarAlCarrito" :disabled="!puedeAgregarAlCarrito">
            <span>⬇️</span> Añadir a la lista
          </button>
        </fieldset>
      </div>

      <!-- Panel Derecho: Lista de la Compra -->
      <div class="panel panel-right">
        <h3 class="panel-title">Resumen del Lote a Ingresar</h3>
        
        <div class="cart-container">
          <table class="cart-table">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Cant.</th>
                <th>Costo U.</th>
                <th>Subtotal</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in carrito" :key="index">
                <td class="product-col">
                  <strong>{{ item.nombre }}</strong>
                </td>
                <td>{{ item.cantidad }}</td>
                <td>S/ {{ item.precioCostoUnitario.toFixed(2) }}</td>
                <td class="font-medium">S/ {{ item.subTotal.toFixed(2) }}</td>
                <td>
                  <button class="remove-btn" @click="removerDelCarrito(index)" title="Quitar">✖</button>
                </td>
              </tr>
              <tr v-if="carrito.length === 0">
                <td colspan="5" class="empty-cart">No has agregado ningún producto todavía.</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="cart-footer">
          <div class="total-row">
            <span>Total factura:</span>
            <span class="total-amount">S/ {{ totalCompra.toFixed(2) }}</span>
          </div>
          
          <button class="checkout-btn" @click="registrarCompra" :disabled="guardando || !puedeRegistrarCompra">
            {{ guardando ? 'Guardando Entrada...' : 'Registrar Ingreso de Mercadería' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Formulario Rápido de Producto -->
    <div v-if="mostrarModalProducto" class="modal-overlay" @click.self="cerrarModalProducto">
      <div class="modal-content">
        <div class="modal-header">
          <h3>Crear Producto Rápido</h3>
          <button class="close-btn" @click="cerrarModalProducto">✕</button>
        </div>
        
        <form @submit.prevent="guardarProductoRapido" class="modal-form">
          <div v-if="errorModalProd" class="form-error">{{ errorModalProd }}</div>

          <p class="modal-note">El producto quedará enganchado al proveedor de la factura principal.</p>

          <div class="form-grid">
            <div class="form-group">
              <label>Código de Barras *</label>
              <input type="text" v-model="nuevoProd.codigo" required />
            </div>
            <div class="form-group">
              <label>Nombre *</label>
              <input type="text" v-model="nuevoProd.nombre" required />
            </div>
            <div class="form-group" style="grid-column: span 2;">
              <label>Descripción</label>
              <input type="text" v-model="nuevoProd.descripcion" />
            </div>
            <div class="form-group">
              <label>Unidad Medida *</label>
              <select v-model="nuevoProd.unidadMedida" required>
                <option value="UND">Unidad</option>
                <option value="SACO">Saco o costal</option>
                <option value="BALDE">Balde o bolsa granel</option>
              </select>
            </div>
            <div class="form-group">
              <label>Cantidad (La que acaba de llegar) *</label>
              <input type="number" step="0.01" min="0.01" v-model="nuevoProd.cantidadLlegando" required />
            </div>

            <!-- ======== RENDERING PARA UNIDAD ======== -->
            <template v-if="nuevoProd.unidadMedida === 'UND'">
              <div class="form-group">
                <label>Precio Costo de Compra (S/)</label>
                <input type="number" step="0.01" min="0" v-model="nuevoProd.precioCosto" />
              </div>
              <div class="form-group">
                <label>Stock Mínimo Alerta *</label>
                <input type="number" min="0" v-model="nuevoProd.stockMinimo" required />
              </div>
              <div class="form-group" style="grid-column: span 2;">
                <label>Precio Venta Público (S/ - Opcional)</label>
                <input type="number" step="0.01" min="0" v-model="nuevoProd.precioVenta" placeholder="0.00" />
              </div>
            </template>

            <!-- ======== RENDERING PARA SACO Y BALDE ======== -->
            <template v-if="nuevoProd.unidadMedida === 'SACO' || nuevoProd.unidadMedida === 'BALDE'">
              <div class="form-group" style="grid-column: span 2; margin-top: 0.5rem;">
                <div class="wholesale-box animate-fade-in" style="background: #F7FAFC; border-color: #CBD5E0;">
                  <p class="box-title" style="color: #2D3748; display: flex; align-items: center; gap: 0.5rem; font-size: 0.85rem;">📦 Configuración por {{ nuevoProd.unidadMedida === 'SACO' ? 'Saco' : 'Balde/Bolsa' }}</p>
                  <p style="font-size: 0.8rem; color: #718096; margin-top: -10px; margin-bottom: 1rem;">Ingresa los costos y detalles del bulto entero.</p>
                  <div class="form-row-custom" style="grid-template-columns: 1fr 1fr; margin-bottom: 1rem;">
                    <div class="form-group">
                      <label>Precio Costo Compra (S/) *</label>
                      <input type="number" step="0.01" min="0" v-model="nuevoProd.precioCosto" required placeholder="Costo del bulto entero" />
                    </div>
                    <div class="form-group">
                      <label>{{ nuevoProd.unidadMedida === 'SACO' ? 'Peso del Saco (Kg)' : 'Cant. Unidades por Balde' }} *</label>
                      <input type="number" step="0.01" v-model="nuevoProd.cantidadMayorista" required placeholder="Ej. 40" />
                    </div>
                  </div>
                  <div class="form-row-custom" style="grid-template-columns: 1fr 1fr;">
                    <div class="form-group">
                      <label>Precio Venta Público Bulto (S/) *</label>
                      <input type="number" step="0.01" v-model="nuevoProd.precioMayorista" required placeholder="S/ 0.00" />
                    </div>
                    <div class="form-group">
                      <label>Stock Mínimo Alerta (En bultos) *</label>
                      <input type="number" min="0" v-model="nuevoProd.stockMinimo" required />
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-group" style="grid-column: span 2; margin-top: -0.5rem;">
                <div class="wholesale-box animate-fade-in" style="background: #EBF8FF; border-color: #90CDF4;">
                  <p class="box-title" style="color: #2B6CB0; display: flex; align-items: center; gap: 0.5rem; font-size: 0.85rem;">⚖️ Configuración de Fracción</p>
                  <p style="font-size: 0.8rem; color: #4A5568; margin-top: -10px; margin-bottom: 1rem;">¿A cuánto venderás cada {{ nuevoProd.unidadMedida === 'SACO' ? 'kilo' : 'unidad' }} si se fracciona el bulto?</p>
                  <div class="form-row-custom" style="grid-template-columns: 1fr;">
                    <div class="form-group">
                      <label>Precio Venta Público por {{ nuevoProd.unidadMedida === 'SACO' ? 'Kilo' : 'Unidad' }} (S/) *</label>
                      <input type="number" step="0.01" min="0" v-model="nuevoProd.precioVenta" required placeholder="0.00" />
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <div class="form-group" style="grid-column: span 2;">
              <label>Fotografía (Opcional)</label>
              <input type="file" @change="onFileChangeProd" accept="image/png, image/jpeg, image/jpg" style="padding: 0.6rem; cursor: pointer;" />
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="cancel-btn" @click="cerrarModalProducto">Cancelar</button>
            <button type="submit" class="primary-btn" :disabled="guardandoProd">
              {{ guardandoProd ? 'Creando...' : 'Guardar y Seleccionar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal Formulario Rápido de Proveedor -->
    <div v-if="mostrarModalProveedor" class="modal-overlay" @click.self="cerrarModalProveedor">
      <div class="modal-content">
        <div class="modal-header">
          <h3>Crear Proveedor Rápido</h3>
          <button class="close-btn" @click="cerrarModalProveedor">✕</button>
        </div>
        
        <form @submit.prevent="guardarProveedorRapido" class="modal-form">
          <div v-if="errorModalProv" class="form-error">{{ errorModalProv }}</div>

          <div class="form-group">
            <label>Razón Social / Nombre *</label>
            <input type="text" v-model="nuevoProv.nombre" required placeholder="Ej. Purina S.A." />
          </div>
          
          <div class="form-grid">
            <div class="form-group">
              <label>Teléfono de Contacto</label>
              <input type="text" v-model="nuevoProv.telefono" placeholder="Ej. 987654321" />
            </div>
            <div class="form-group">
              <label>Correo Electrónico</label>
              <input type="email" v-model="nuevoProv.email" placeholder="ventas@proveedor.com" />
            </div>
            <div class="form-group" style="grid-column: span 2;">
              <label>Dirección Física Principal</label>
              <input type="text" v-model="nuevoProv.direccion" placeholder="Av. Principal 123" />
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="cancel-btn" @click="cerrarModalProveedor">Cancelar</button>
            <button type="submit" class="primary-btn" :disabled="guardandoProv">
              {{ guardandoProv ? 'Creando...' : 'Guardar y Seleccionar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de xito de Compra -->
    <div v-if="mostrarModalExitoCompra" class="modal-overlay" @click.self="cerrarModalExito">
      <div class="modal-content" style="max-width: 400px; text-align: center;">
        <div class="modal-header" style="justify-content: center; border-bottom: none; padding-bottom: 0;">
          <div style="font-size: 3rem; margin-bottom: 0.5rem;">🎉</div>
        </div>
        
        <div class="modal-form" style="padding-top: 0; align-items: center;">
          <h3 style="color: #2F855A; font-weight: 800; margin: 0 0 1rem 0; font-size: 1.4rem;">¡Compra Registrada!</h3>
          <p style="color: #4A5568; margin: 0 0 1.5rem 0; line-height: 1.5;">El inventario de todos los productos y los precios de costo han sido actualizados exitosamente en la base de datos.</p>
          
          <button type="button" class="primary-btn" @click="cerrarModalExito" style="width: 100%; justify-content: center; font-size: 1.1rem; padding: 0.8rem;">
            Aceptar y Continuar
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'

const proveedores = ref([])
const productos = ref([])
const cargando = ref(true)
const guardando = ref(false)
const errorGlobal = ref('')
const successGlobal = ref('')

// LÓGICA DE COMBOBOX PREMIUM
const busquedaDetalleTemp = ref('')
const mostrarDropdownProductos = ref(false)

const productosFiltradosBusqueda = computed(() => {
  if (!busquedaDetalleTemp.value) return productos.value;
  const find = busquedaDetalleTemp.value.toLowerCase();
  return productos.value.filter(p => 
    p.nombre.toLowerCase().includes(find) || 
    p.codigo.toLowerCase().includes(find)
  );
})

const onInputBusqueda = () => {
  mostrarDropdownProductos.value = true;
  if (!busquedaDetalleTemp.value) {
    detalleTemp.value.productoID = 0;
  }
}

const toggleDropdownProductos = () => {
  mostrarDropdownProductos.value = !mostrarDropdownProductos.value;
}

const abrirDropdownAlEnfocar = () => {
  setTimeout(() => {
    mostrarDropdownProductos.value = true;
  }, 100);
}

const cerrarDropdownSafe = () => {
  mostrarDropdownProductos.value = false;
}

const seleccionarProductoDropdown = (prod) => {
  detalleTemp.value.productoID = prod.productoID;
  detalleTemp.value.precioCostoUnitario = prod.precioCosto;
  busquedaDetalleTemp.value = `[${prod.codigo}] ${prod.nombre}`;
  mostrarDropdownProductos.value = false;
}

// Directiva para cerrar al hacer clic afuera
const vClickOutside = {
  mounted(el, binding) {
    el.clickOutsideEvent = (event) => {
      if (!(el === event.target || el.contains(event.target))) {
        binding.value(event);
      }
    };
    document.addEventListener("click", el.clickOutsideEvent);
  },
  unmounted(el) {
    document.removeEventListener("click", el.clickOutsideEvent);
  },
};

const mostrarModalExitoCompra = ref(false)
const cerrarModalExito = () => {
  mostrarModalExitoCompra.value = false
}

// Headers del formulario
const compraHeader = ref({
  proveedorID: 0,
  numeroComprobante: ''
})

// Item temporal
const detalleTemp = ref({
  productoID: 0,
  cantidad: '',
  precioCostoUnitario: ''
})

// Lista que va llenandose
const carrito = ref([])

const puedeUsarPanelSecundario = computed(() => {
  return compraHeader.value.proveedorID !== 0 && compraHeader.value.numeroComprobante.trim() !== ''
})

const getToken = () => localStorage.getItem('jwt_token')

const cargarDatos = async () => {
  cargando.value = true
  try {
    const headers = { 'Authorization': `Bearer ${getToken()}` }
    const [resProv, resProd] = await Promise.all([
      fetch('/api/Proveedores', { headers }),
      fetch('/api/Productos', { headers })
    ])
    const dProv = await resProv.json()
    const dProd = await resProd.json()
    
    if (dProv.success) proveedores.value = dProv.data.filter(p => p.activo)
    if (dProd.success) productos.value = dProd.data.filter(p => p.activo)
  } catch (err) {
    errorGlobal.value = 'Error al cargar catálogos.'
  } finally {
    cargando.value = false
  }
}

onMounted(() => cargarDatos())

// Watcher para auto-llenar precio costo
watch(() => detalleTemp.value.productoID, (newVal) => {
  if (newVal !== 0) {
    const prodSeleccionado = productos.value.find(p => p.productoID === newVal)
    if (prodSeleccionado) {
      detalleTemp.value.precioCostoUnitario = prodSeleccionado.precioCosto
    }
  }
})

const puedeAgregarAlCarrito = computed(() => {
  return detalleTemp.value.productoID !== 0 && detalleTemp.value.cantidad > 0 && detalleTemp.value.precioCostoUnitario >= 0
})

const agregarAlCarrito = () => {
  const prod = productos.value.find(p => p.productoID === detalleTemp.value.productoID)
  
  carrito.value.push({
    productoID: prod.productoID,
    nombre: prod.nombre,
    cantidad: parseFloat(detalleTemp.value.cantidad),
    precioCostoUnitario: parseFloat(detalleTemp.value.precioCostoUnitario),
    subTotal: parseFloat(detalleTemp.value.cantidad) * parseFloat(detalleTemp.value.precioCostoUnitario)
  })

  detalleTemp.value.productoID = 0
  detalleTemp.value.cantidad = ''
  detalleTemp.value.precioCostoUnitario = ''
}

// LÓGICA DE PROVEEDOR RÁPIDO
const mostrarModalProveedor = ref(false)
const guardandoProv = ref(false)
const errorModalProv = ref('')
const nuevoProv = ref({ nombre: '', telefono: '', email: '', direccion: '' })

const abrirModalProveedor = () => {
  errorModalProv.value = ''
  nuevoProv.value = { nombre: '', telefono: '', email: '', direccion: '' }
  mostrarModalProveedor.value = true
}
const cerrarModalProveedor = () => { mostrarModalProveedor.value = false }

const guardarProveedorRapido = async () => {
  guardandoProv.value = true
  errorModalProv.value = ''

  try {
    const res = await fetch('/api/Proveedores', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(nuevoProv.value)
    })
    
    const data = await res.json()
    if (data.success) {
      await cargarDatos()
      compraHeader.value.proveedorID = data.data.proveedorID
      cerrarModalProveedor()
    } else {
      errorModalProv.value = data.mensaje
    }
  } catch(err) {
    errorModalProv.value = 'Fallo al conectar al guardar proveedor.'
  } finally {
    guardandoProv.value = false
  }
}

// ==================== LÓGICA DE PRODUCTO RÁPIDO ====================

const mostrarModalProducto = ref(false)
const guardandoProd = ref(false)
const errorModalProd = ref('')

// Variables para imagen (corregidas y mejoradas)
const archivoProd = ref(null)
const filePreviewProd = ref(null)

const nuevoProd = ref({
  codigo: '', nombre: '', descripcion: '', proveedorID: 0, unidadMedida: 'UND', 
  precioCosto: 0, precioVenta: '', stockActual: 0, stockMinimo: 5, cantidadLlegando: 1,
  precioMayorista: 0, cantidadMayorista: 0, nombreUnidadMayorista: ''
})

// Lógica de imagen (igual que en Gestión de Productos)
const manejarImagenProducto = (event) => {
  const file = event.target.files[0]
  if (file) {
    archivoProd.value = file
    filePreviewProd.value = URL.createObjectURL(file)
  }
}

const eliminarImagenProducto = () => {
  filePreviewProd.value = null
  archivoProd.value = null
}

const abrirModalProducto = () => {
  if (compraHeader.value.proveedorID === 0) {
    alert("Por favor, selecciona primero un Proveedor en el paso 1.")
    return
  }
  errorModalProd.value = ''
  archivoProd.value = null
  filePreviewProd.value = null   // Reinicia vista previa

  nuevoProd.value = {
    codigo: '', nombre: '', descripcion: '', proveedorID: compraHeader.value.proveedorID,
    unidadMedida: 'UND', precioCosto: 0, precioVenta: '', stockActual: 0, stockMinimo: 5, 
    cantidadLlegando: 1, precioMayorista: 0, cantidadMayorista: 0, nombreUnidadMayorista: ''
  }
  mostrarModalProducto.value = true
}

const cerrarModalProducto = () => {
  mostrarModalProducto.value = false
}

const guardarProductoRapido = async () => {
  guardandoProd.value = true
  errorModalProd.value = ''

  const formData = new FormData()

  Object.keys(nuevoProd.value).forEach(key => {
    if (key !== 'cantidadLlegando') {
      const val = nuevoProd.value[key];
      formData.append(key, val !== null && val !== undefined && val !== '' ? val : 0);
    }
  });
  
  // Imagen - Nombre correcto del campo (igual que en el otro componente)
  if (archivoProd.value) {
    formData.append('ImagenFoto', archivoProd.value)
  }

  try {
    const res = await fetch('/api/Productos', {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${getToken()}` },
      body: formData
    })
    
    const data = await res.json()
    if (data.success) {
      await cargarDatos()
      
      const cLlegada = parseFloat(nuevoProd.value.cantidadLlegando) || 1
      const pCosto = parseFloat(nuevoProd.value.precioCosto) || 0

      carrito.value.push({
        productoID: data.data.productoID,
        nombre: data.data.nombre,
        cantidad: cLlegada,
        precioCostoUnitario: pCosto,
        subTotal: cLlegada * pCosto
      })

      cerrarModalProducto()
    } else {
      errorModalProd.value = data.mensaje || 'Error al guardar el producto.'
    }
  } catch(err) {
    errorModalProd.value = 'Fallo al conectar al guardar el producto.'
  } finally {
    guardandoProd.value = false
  }
}

const removerDelCarrito = (index) => {
  carrito.value.splice(index, 1)
}

const totalCompra = computed(() => {
  return carrito.value.reduce((acc, current) => acc + current.subTotal, 0)
})

const puedeRegistrarCompra = computed(() => {
  return compraHeader.value.proveedorID !== 0 && compraHeader.value.numeroComprobante.trim() !== '' && carrito.value.length > 0
})

const registrarCompra = async () => {
  guardando.value = true
  errorGlobal.value = ''
  successGlobal.value = ''

  const payload = {
    proveedorID: compraHeader.value.proveedorID,
    numeroComprobante: compraHeader.value.numeroComprobante,
    total: totalCompra.value,
    detalles: carrito.value.map(c => ({
      productoID: c.productoID,
      cantidad: c.cantidad,
      precioCostoUnitario: c.precioCostoUnitario,
      subTotal: c.subTotal
    }))
  }

  try {
    const res = await fetch('/api/Compras', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify(payload)
    })
    
    const data = await res.json()
    if (data.success) {
      compraHeader.value = { proveedorID: 0, numeroComprobante: '' }
      carrito.value = []
      mostrarModalExitoCompra.value = true;
    } else {
      errorGlobal.value = data.mensaje
    }
  } catch (err) {
    errorGlobal.value = 'Error de red al intentar registrar compra.'
  } finally {
    guardando.value = false
  }
}
</script>

<style scoped>
.compras-module { animation: fadeIn 0.4s ease; padding-bottom: 2rem; }
.module-header { margin-bottom: 2rem; }
.title { font-size: 1.75rem; font-weight: 700; color: #1A202C; margin: 0 0 0.25rem 0; }
.subtitle { color: #718096; margin: 0; font-size: 0.95rem; }

.pos-layout { display: flex; gap: 2rem; align-items: flex-start; }
.panel { background: white; border-radius: 16px; padding: 2rem; box-shadow: 0 4px 20px rgba(0,0,0,0.03); border: 1px solid #E2E8F0; }
.panel-left { flex: 1; max-width: 45%; }
.panel-right { flex: 1; display: flex; flex-direction: column; min-height: 600px; }

.panel-title { margin-top: 0; margin-bottom: 1.5rem; font-size: 1.1rem; color: #2D3748; border-bottom: 2px solid #F0F4F8; padding-bottom: 0.8rem; }
.divider { height: 1px; background-color: #E2E8F0; margin: 2rem 0; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; }
.form-group label { font-size: 0.85rem; font-weight: 600; color: #4A5568; }
.form-group input, .form-group select { width: 100%; box-sizing: border-box; padding: 0.85rem 1rem; border: 1.5px solid #E2E8F0; border-radius: 10px; color: #2D3748; font-family: inherit; font-size: 0.95rem; transition: border-color 0.2s;}
.form-group input:focus, .form-group select:focus { border-color: #F6AD55; box-shadow: 0 0 0 3px rgba(246, 173, 85, 0.2); outline: none; }
.fieldset-no-border { border: none; padding: 0; margin: 0; }
.fieldset-no-border:disabled { opacity: 0.6; cursor: not-allowed; }

.add-btn { width: 100%; padding: 1rem; background-color: #EDF2F7; color: #2D3748; border: none; border-radius: 10px; font-weight: 600; cursor: pointer; transition: all 0.2s; margin-top: 1.5rem; display: flex; justify-content: center; align-items: center; gap: 0.5rem; }
.add-btn:hover:not(:disabled) { background-color: #E2E8F0; }
.add-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* Panel Derecho - Carrito */
.cart-container { flex: 1; overflow-y: auto; margin-bottom: 1rem; border: 1px solid #E2E8F0; border-radius: 8px; }
.cart-table { width: 100%; border-collapse: collapse; }
.cart-table th { background-color: #F8FAFC; padding: 0.75rem 1rem; text-align: left; font-size: 0.75rem; text-transform: uppercase; color: #718096; position: sticky; top: 0; }
.cart-table td { padding: 1rem; border-bottom: 1px solid #EDF2F7; font-size: 0.9rem; color: #4A5568; }
.product-col { max-width: 200px; }
.font-medium { font-weight: 700; color: #2D3748; }

.remove-btn { background: none; border: none; color: #E53E3E; cursor: pointer; font-size: 1.2rem; display: flex; align-items: center; justify-content: center; width: 30px; height: 30px; border-radius: 6px; transition: background 0.2s;}
.remove-btn:hover { background-color: #FFF5F5; }

.empty-cart { text-align: center; color: #A0AEC0; padding: 4rem 1rem !important; font-style: italic; }

.cart-footer { background-color: #F8FAFC; padding: 1.5rem; border-radius: 12px; border: 1px dashed #CBD5E0; }
.total-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; font-size: 1.2rem; font-weight: 600; color: #4A5568; }
.total-amount { font-size: 1.75rem; color: #2D3748; font-weight: 800; }

.checkout-btn { width: 100%; padding: 1.2rem; background-color: #F6AD55; color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 1.1rem; cursor: pointer; transition: all 0.2s; box-shadow: 0 4px 15px rgba(246, 173, 85, 0.4); }
.checkout-btn:hover:not(:disabled) { background-color: #DD6B20; transform: translateY(-2px); }
.checkout-btn:disabled { opacity: 0.6; cursor: not-allowed; transform: none; box-shadow: none; }

.error-banner { background-color: #FFF5F5; color: #C53030; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; }
/* Modal extra y botones inline */
.select-with-button { display: flex; gap: 0.5rem; align-items: stretch; }
.select-with-button select { flex: 1; }
.quick-btn { background-color: #EDF2F7; border: 1.5px solid #E2E8F0; border-radius: 10px; width: 45px; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; cursor: pointer; transition: background 0.2s; }
.quick-btn:hover { background-color: #E2E8F0; }

/* COMBOBOX PERSONALIZADO (STYLE VIDA ANIMAL) */
.custom-combobox { position: relative; flex: 1; display: flex; align-items: center; }
.search-input-select { width: 100% !important; padding-right: 2.5rem !important; }
.combobox-arrow { position: absolute; right: 12px; font-size: 0.7rem; color: #A0AEC0; cursor: pointer; user-select: none; }

.custom-dropdown {
  position: absolute; top: calc(100% + 5px); left: 0; right: 0;
  background: white; border-radius: 14px; border: 1px solid #E2E8F0;
  box-shadow: 0 10px 30px rgba(0,0,0,0.1); z-index: 999;
  max-height: 320px; overflow-y: auto; overflow-x: hidden;
  animation: slideInDown 0.2s ease-out;
}

@keyframes slideInDown {
  from { opacity: 0; transform: translateY(-8px); }
  to { opacity: 1; transform: translateY(0); }
}

.dropdown-option {
  padding: 0.85rem 1.2rem; display: flex; justify-content: space-between; align-items: center;
  border-bottom: 1px solid #F7FAFC; cursor: pointer; transition: all 0.2s;
}

.dropdown-option:last-child { border-bottom: none; }
.dropdown-option:hover { background: #E6FFFA; border-left: 5px solid #4FD1C5; }

.option-main { display: flex; flex-direction: column; gap: 0.1rem; }
.option-code { font-size: 0.75rem; color: #718096; font-weight: 700; text-transform: uppercase; }
.option-name { font-size: 0.95rem; color: #2D3748; font-weight: 500; line-height: 1.2; }
.option-price { font-size: 0.85rem; color: #F6AD55; font-weight: 750; white-space: nowrap; }

.custom-dropdown-empty {
  position: absolute; top: calc(100% + 5px); left: 0; right: 0;
  background: #F7FAFC; padding: 1.5rem; text-align: center; border-radius: 14px;
  color: #A0AEC0; font-style: italic; z-index: 999; font-size: 0.9rem;
}

.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #E2E8F0; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #CBD5E0; }


/* MODAL STANDARD PARA PRODUCTO RÁPIDO */
.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0, 0, 0, 0.4); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000; animation: fadeIn 0.2s ease; }
.modal-content { 
  background: white; 
  width: 90%; 
  max-width: 650px; 
  max-height: 90vh; 
  border-radius: 20px; 
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25); 
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } }

.modal-header { display: flex; justify-content: space-between; align-items: center; padding: 1.5rem; border-bottom: 1px solid #E2E8F0; flex-shrink: 0; }
.modal-header h3 { margin: 0; font-size: 1.25rem; font-weight: 700; color: #2D3748; }
.close-btn { background: none; border: none; font-size: 1.25rem; color: #A0AEC0; cursor: pointer; }

.modal-form { padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; overflow-y: auto; flex: 1; }

/* Wholesale box */
.wholesale-box { background: #F7FAFC; padding: 1rem; border-radius: 12px; border: 1px solid #E2E8F0; grid-column: span 2 !important; }
.box-title { font-size: 0.75rem; font-weight: 800; color: #4A5568; margin: 0 0 1rem 0; text-transform: uppercase; }
.form-row-custom { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 0.75rem; }
.modal-note { font-size: 0.85rem; color: #4A5568; background-color: #EBF8FF; border-left: 4px solid #4299E1; padding: 0.75rem; border-radius: 4px; margin: 0; }
.form-error { background: #FFF5F5; color: #E53E3E; padding: 0.75rem; border-radius: 8px; font-size: 0.85rem; }

.primary-btn { background-color: #F6AD55; color: white; border: none; padding: 0.75rem 1.25rem; border-radius: 10px; font-weight: 600; cursor: pointer; transition: all 0.2s; }
.primary-btn:hover:not(:disabled) { background-color: #DD6B20; }
.primary-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.cancel-btn { background-color: transparent; color: #4A5568; border: 1px solid #CBD5E0; padding: 0.75rem 1.25rem; border-radius: 10px; font-weight: 600; cursor: pointer; }
.cancel-btn:hover { background-color: #F7FAFC; }

.modal-footer { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid #E2E8F0; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>
