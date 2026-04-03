<template>
  <div class="productos-module">
    <div class="module-header" style="display: flex; justify-content: space-between; align-items: flex-start;">
      <div>
        <h2 class="title">Gestión de Productos</h2>
        <p class="subtitle">Consulta y edita tu catálogo de ventas</p>
      </div>
      <button v-if="usuarioRol === 'ADMINISTRADOR'" @click="abrirModalNuevo" class="primary-btn-pro">
        📦 Nuevo Producto
      </button>
    </div>

    <div v-if="errorGlobal" class="error-banner">{{ errorGlobal }}</div>
    <div v-if="successGlobal" class="success-banner">{{ successGlobal }}</div>

    <div class="filters-bar">
      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input type="text" v-model="busqueda" placeholder="Buscar por nombre o código..." />
      </div>
      <div class="sort-box">
        <label>Ordenar por fecha:</label>
        <select v-model="ordenFecha">
          <option value="desc">Más recientes primero</option>
          <option value="asc">Más antiguos primero</option>
        </select>
      </div>
    </div>

    <div v-if="cargando" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando productos...</p>
    </div>

    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>Imagen</th>
            <th>Código</th>
            <th>Nombre del Producto</th>
            <th>P. Costo</th>
            <th>P. Venta</th>
            <th>Stock Actual</th>
            <th>Estado</th>
            <th v-if="usuarioRol === 'ADMINISTRADOR'" class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="prod in productosFiltrados" :key="prod.productoID" :class="{ 'inactive-row': !prod.activo, 'loss-alert-row': evaluarPerdida(prod) }">
            <td class="img-cell">
              <img v-if="prod.imagenURL" :src="`${IMAGE_BASE}${prod.imagenURL}`" class="prod-thumb" alt="Product thumbnail" />
              <div v-else class="img-placeholder">🐾</div>
            </td>
            <td class="font-medium text-muted">{{ prod.codigo }}</td>
            <td class="font-medium">
              <div class="prod-name">
                {{ prod.nombre }}
                <span v-if="prod.stockActual <= prod.stockMinimo" class="alert-badge stock-badge" title="Stock Bajo">⚠️ Bajo Stock</span>
                <span v-if="evaluarPerdida(prod)" class="alert-badge loss-badge" title="Venta por debajo del costo de compra">🚨 Pérdida</span>
              </div>
            </td>
            <td class="text-muted">
              S/ {{ (prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') && prod.cantidadMayorista > 0 ? (prod.precioCosto / prod.cantidadMayorista).toFixed(2) : prod.precioCosto.toFixed(2) }}
            </td>
            <td class="price-text">S/ {{ prod.precioVenta.toFixed(2) }}</td>
            <td>
              <span :class="{'text-danger': prod.stockActual <= prod.stockMinimo, 'text-success': prod.stockActual > prod.stockMinimo}">
                {{ formatStock(prod) }}
              </span>
            </td>
            <td>
              <span class="status-dot" :class="prod.activo ? 'dot-active' : 'dot-inactive'"></span>
              {{ provStatus(prod.activo) }}
            </td>
            <td v-if="usuarioRol === 'ADMINISTRADOR'" class="actions-cell">
              <button class="action-btn edit-btn" @click="abrirModalEditar(prod)" title="Editar">✏️</button>
              <button class="action-btn toggle-btn" @click="abrirModalToggle(prod)" :title="prod.activo ? 'Desactivar' : 'Activar'">
                {{ prod.activo ? '🚫' : '✅' }}
              </button>
              <button class="action-btn delete-btn" @click="abrirModalEliminar(prod)" title="Eliminar definitivamente">
                🗑️
              </button>
            </td>
          </tr>
          <tr v-if="productosFiltrados.length === 0">
            <td colspan="8" class="empty-state">No se encontraron productos que coincidan con tu búsqueda.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL PRINCIPAL (CREAR/EDITAR) -->
    <div v-if="mostrarModal" class="modal-overlay" @click.self="cerrarModal">
      <div class="modal-content modal-large">
        <div class="modal-header">
          <h3>{{ modoEdicion ? '📋 Editar Producto' : '➕ Nuevo Producto' }}</h3>
          <button class="close-btn" @click="cerrarModal">✕</button>
        </div>
        
        <form @submit.prevent="guardarProducto" class="modal-form-pro">
          <div v-if="errorFormulario" class="form-error">{{ errorFormulario }}</div>

          <div class="modal-body-scroll">
            <div class="form-section">
              <h4 class="section-badge">📦 Información General</h4>
              <div class="compact-grid">
                <div class="form-group lg">
                  <label>Nombre del Producto *</label>
                  <input type="text" v-model="formProd.nombre" required />
                </div>
                <div class="form-group">
                  <label>Código / SKU *</label>
                  <input type="text" v-model="formProd.codigo" required />
                </div>
                <div class="form-group">
                  <label>Proveedor *</label>
                  <select v-model="formProd.proveedorID" required>
                    <option value="0" disabled>Selecciona uno...</option>
                    <option v-for="p in proveedores" :key="p.proveedorID" :value="p.proveedorID">{{ p.nombre }}</option>
                  </select>
                </div>
                <div class="form-group lg">
                  <label>Descripción</label>
                  <textarea v-model="formProd.descripcion" rows="2"></textarea>
                </div>
              </div>
            </div>

            <div class="form-section">
              <h4 class="section-badge color-price">⚙️ Sistema de Unidades</h4>
              <div class="compact-grid">
                <div class="form-group lg">
                  <label>Unidad Medida</label>
                  <select v-model="formProd.unidadMedida" required :disabled="modoEdicion">
                    <option value="UND">Unidad o Pieza Entera</option>
                    <option value="SACO">Saco (Stock controlable por Kilos y Bultos)</option>
                    <option value="BALDE">Balde o Bolsa Granel (Stock controlable por Unidades)</option>
                  </select>
                </div>
              </div>
            </div>

            <!-- ======== RENDERING PARA UNIDAD ======== -->
            <template v-if="formProd.unidadMedida === 'UND'">
              <div class="form-section compact">
                <h4 class="section-badge color-price">💰 Precios e Inventario (Unidad)</h4>
                <div class="compact-grid">
                  <div class="form-group">
                    <label>Precio Costo Compra (S/)</label>
                    <input type="number" step="0.01" v-model="formProd.precioCosto" />
                  </div>
                  <div class="form-group">
                    <label>Precio Venta Público (S/) *</label>
                    <input type="number" step="0.01" v-model="formProd.precioVenta" required />
                  </div>
                  <div class="form-group">
                    <label>Stock Actual (Cant.) *</label>
                    <input type="number" step="0.01" v-model="formProd.stockActual" required />
                  </div>
                  <div class="form-group">
                    <label>Stock Mínimo (Alerta) *</label>
                    <input type="number" v-model="formProd.stockMinimo" required />
                  </div>
                </div>
              </div>
            </template>

            <!-- ======== RENDERING PARA SACO Y BALDE ======== -->
            <template v-if="formProd.unidadMedida === 'SACO' || formProd.unidadMedida === 'BALDE'">
              <div class="form-section wholesale-bg animate-fade-in" style="background: #F7FAFC; border: 1px solid #CBD5E0;">
                <h4 class="section-badge" style="background-color: #4A5568;">📦 Configuración por {{ formProd.unidadMedida === 'SACO' ? 'Saco' : 'Balde/Bolsa' }}</h4>
                <p style="font-size: 0.8rem; color: #718096; margin-top: -5px; margin-bottom: 1rem;">Ingresa los costos y detalles del bulto entero.</p>
                <div class="compact-grid">
                  <div class="form-group">
                    <label>Precio Costo Compra (S/) *</label>
                    <input type="number" step="0.01" v-model="formProd.precioCosto" required />
                  </div>
                  <div class="form-group">
                    <label>{{ formProd.unidadMedida === 'SACO' ? 'Peso del Saco (Kg)' : 'Unidades por Balde' }} *</label>
                    <input type="number" step="0.01" v-model="formProd.cantidadMayorista" required />
                  </div>
                  <div class="form-group">
                    <label>Precio Venta Público (Entero) (S/) *</label>
                    <input type="number" step="0.01" v-model="formProd.precioMayorista" required />
                  </div>
                  <div class="form-group">
                    <label>Stock Actual (Bultos) *</label>
                    <input type="number" step="0.01" v-model="formProd.stockActual" required />
                  </div>
                  <div class="form-group">
                    <label>Min. Alerta (En bultos) *</label>
                    <input type="number" v-model="formProd.stockMinimo" required />
                  </div>
                </div>
              </div>

              <div class="form-section animate-fade-in" style="background: #EBF8FF; border: 1px solid #90CDF4; margin-top: -0.5rem;">
                <h4 class="section-badge" style="background-color: #2B6CB0;">⚖️ Configuración de Fracción</h4>
                <p style="font-size: 0.8rem; color: #4A5568; margin-top: -5px; margin-bottom: 1rem;">Precio al que venderás cada {{ formProd.unidadMedida === 'SACO' ? 'Kilo' : 'Unidad suelta' }} si el bulto se fracciona.</p>
                <div class="compact-grid">
                  <div class="form-group">
                    <label>Precio Venta Público por {{ formProd.unidadMedida === 'SACO' ? 'Kilo' : 'Unidad' }} (S/) *</label>
                    <input type="number" step="0.01" v-model="formProd.precioVenta" required />
                  </div>
                </div>
              </div>
            </template>

            <div class="form-section">
              <h4 class="section-badge">🖼️ Imagen</h4>
              <div class="image-upload-wrapper">
                <div class="file-drop-area" :class="{'has-file': filePreview}">
                  <input type="file" @change="manejarImagen" accept="image/*" class="file-input" />
                  <img v-if="filePreview" :src="filePreview" class="preview-img" />
                  <button v-if="filePreview" type="button" @click.stop.prevent="eliminarImagenActual" class="delete-img-btn" style="position: absolute; top: 5px; right: 5px; background: rgba(255, 0, 0, 0.8); color: white; border: none; border-radius: 50%; width: 25px; height: 25px; cursor: pointer; z-index: 10;">✕</button>
                  <div v-else class="file-msg">📸 Subir Foto</div>
                </div>
              </div>
            </div>
          </div>

          <div class="modal-footer-fixed">
            <button type="button" class="cancel-btn" @click="cerrarModal">Cancelar</button>
            <button type="submit" class="primary-btn-pro" :disabled="guardando">
              {{ guardando ? '...' : (modoEdicion ? 'Actualizar' : 'Crear') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de Eliminación Segura (REESTRUCTURADO) -->
    <div v-if="mostrarModalEliminar" class="modal-overlay" @click.self="cerrarModalEliminar">
      <div class="modal-content" style="max-width: 450px; background: white; border-radius: 20px; overflow: hidden; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.4);">
        <div class="modal-header" style="background-color: #FFF5F5; border-bottom: 2px solid #FED7D7; padding: 1.5rem;">
          <h3 style="color: #C53030; display: flex; align-items: center; gap: 0.5rem; margin: 0; font-size: 1.3rem;">
            ⚠️ Alerta Crítica
          </h3>
          <button class="close-btn" @click="cerrarModalEliminar">✕</button>
        </div>
        
        <form @submit.prevent="confirmarEliminar" style="display: flex; flex-direction: column;">
          <div style="padding: 2rem;">
            <p style="font-weight: 800; color: #1A202C; text-align: center; font-size: 1.1rem; margin-bottom: 1rem;">
              ¿Confirmas la eliminación definitiva?
            </p>
            <p style="color: #4A5568; font-size: 0.95rem; line-height: 1.6; text-align: center; margin: 0 0 1.5rem 0;">
              Se borrará el producto <strong style="color: #E53E3E;">{{ prodAEliminar?.nombre }}</strong>. Esta acción no tiene vuelta atrás en el inventario.
            </p>
            
            <div style="background: #F7FAFC; padding: 1.25rem; border-radius: 12px; border: 1px dashed #CBD5E0;">
              <label style="display: block; font-size: 0.85rem; font-weight: 700; color: #4A5568; margin-bottom: 0.75rem; text-align: center;">
                Ingresa tu contraseña de administrador:
              </label>
              <input type="password" v-model="passwordEliminar" required placeholder="Tu clave de seguridad" style="width: 100%; padding: 0.75rem; border-radius: 8px; border: 1.5px solid #E2E8F0; text-align: center;" />
            </div>
            
            <div v-if="errorEliminar" class="form-error" style="margin-top: 1rem; text-align: center;">{{ errorEliminar }}</div>
          </div>

          <div class="modal-footer" style="padding: 1.25rem 2rem; background: #FFF5F5; border-top: 1px solid #FED7D7; display: flex; justify-content: center; gap: 1rem;">
            <button type="button" @click="cerrarModalEliminar" class="cancel-btn" style="padding: 0.75rem 1.5rem; font-weight: 700;">
              Cancelar
            </button>
            <button type="submit" class="primary-btn-pro" :disabled="eliminando" style="background-color: #E53E3E; padding: 0.75rem 1.5rem;">
              {{ eliminando ? '🗑️ Eliminando...' : '🗑️ Sí, Eliminar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de Cambio de Estado (Activar/Desactivar) -->
    <div v-if="mostrarModalToggle" class="modal-overlay" @click.self="cerrarModalToggle">
      <div class="modal-content" style="max-width: 400px; background: white; border-radius: 20px; overflow: hidden; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);">
        <div class="modal-header" :style="prodAToggle?.activo ? 'background-color: #FFF5F5; border-bottom: 2px solid #FED7D7;' : 'background-color: #F0FFF4; border-bottom: 2px solid #C6F6D5;'">
          <h3 :style="prodAToggle?.activo ? 'color: #C53030;' : 'color: #2F855A;'">
            {{ prodAToggle?.activo ? '🚫 Desactivar Producto' : '✅ Activar Producto' }}
          </h3>
          <button class="close-btn" @click="cerrarModalToggle">✕</button>
        </div>
        
        <div class="modal-form">
          <p style="color: #4A5568; font-size: 1rem; line-height: 1.5; text-align: center; margin-bottom: 1.5rem;">
            Se cambiará el estado del producto <strong>{{ prodAToggle?.nombre }}</strong>.
            <br/><br/>
            <span v-if="prodAToggle?.activo">Pasará a estado Inactivo y ya no podrá ser utilizado para registros de ventas o compras.</span>
            <span v-else>Volverá a estar Activo y disponible para comercializar.</span>
          </p>

          <div class="modal-footer" style="justify-content: center; gap: 1rem; padding-top: 0; border-top: none;">
            <button type="button" class="cancel-btn" @click="cerrarModalToggle">Cancelar</button>
            <button type="button" class="primary-btn" @click="confirmarToggle" :disabled="togglando" :style="prodAToggle?.activo ? 'background-color: #E53E3E; color: white;' : 'background-color: #48BB78; color: white;'">
              {{ togglando ? 'Procesando...' : 'Confirmar Cambio' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const API_URL = '/api'
const IMAGE_BASE = '/api'

const productos = ref([])
const usuarioRol = ref('')

const obtenerRol = () => {
    const token = localStorage.getItem('jwt_token');
    if (!token) return '';
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        // Buscamos el rol en los claims estándar de ASP.NET Core
        return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role || '';
    } catch (e) {
        return '';
    }
}

const categorias = ref([])
const proveedores = ref([])

const cargando = ref(true)
const guardando = ref(false)
const errorGlobal = ref('')
const successGlobal = ref('')
const errorFormulario = ref('')

// Para eliminación
const mostrarModalEliminar = ref(false)
const prodAEliminar = ref(null)
const passwordEliminar = ref('')
const errorEliminar = ref('')
const eliminando = ref(false)

// Para Cambiar Estado
const mostrarModalToggle = ref(false)
const prodAToggle = ref(null)
const togglando = ref(false)

const mostrarModal = ref(false)
const modoEdicion = ref(false)
const productToEditId = ref(0) // ID original de edicion para el PUT
const formProd = ref({
  codigo: '', nombre: '', descripcion: '', proveedorID: 0, 
  unidadMedida: 'UND', precioCosto: 0, precioVenta: 0, stockActual: 0, stockMinimo: 5,
  precioMayorista: 0, cantidadMayorista: 0, nombreUnidadMayorista: ''
})

const archivoSeleccionado = ref(null)
const filePreview = ref(null)

const busqueda = ref('')
const ordenFecha = ref('desc')

const productosFiltrados = computed(() => {
  if (!productos.value) return []
  
  let resultado = [...productos.value]

  // Filtrado por nombre o código
  if (busqueda.value.trim() !== '') {
    const q = busqueda.value.toLowerCase()
    resultado = resultado.filter(p => 
      (p.nombre?.toLowerCase().includes(q)) || 
      (p.codigo?.toLowerCase().includes(q))
    )
  }

  // Ordenamiento seguro
  resultado.sort((a, b) => {
    const idA = a.productoID || 0
    const idB = b.productoID || 0
    
    if (ordenFecha.value === 'desc') return idB - idA
    return idA - idB
  })

  return resultado
})

const getToken = () => localStorage.getItem('jwt_token')

const formatStock = (prod) => {
  if ((prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') && prod.cantidadMayorista > 0) {
    const totalInner = prod.stockActual * prod.cantidadMayorista;
    const formatted = parseFloat(totalInner.toFixed(2));
    
    let unidad = prod.nombreUnidadMayorista;
    if (!unidad || unidad === '0' || unidad === '') {
       unidad = prod.unidadMedida === 'SACO' ? 'KG' : 'UND';
    }
    
    return `${formatted} ${unidad}`;
  }
  return `${prod.stockActual} ${prod.unidadMedida}`;
}// Carga Inicial
const cargarDatos = async () => {
  cargando.value = true
  errorGlobal.value = ''
  try {
    const headers = { 'Authorization': `Bearer ${getToken()}` }
    
    // Concurrent requests for fast load
    const [resProd, resProv] = await Promise.all([
      fetch('/api/Productos', { headers }),
      fetch(`${API_URL}/Proveedores`, { headers })
    ])

    const dProd = await resProd.json()
    const dProv = await resProv.json()

    if (dProd.success) productos.value = dProd.data
    if (dProv.success) proveedores.value = dProv.data.filter(p => p.activo)

  } catch (err) {
    errorGlobal.value = 'Error cargando datos del Catálogo.'
  } finally {
    cargando.value = false
    usuarioRol.value = obtenerRol()
  }
}

onMounted(() => cargarDatos())

const provStatus = (activo) => activo ? 'Activo' : 'Inactivo'

const evaluarPerdida = (prod) => {
  if (!prod.activo) return false;
  
  if (prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') {
    // Calculamos si hay perdida en el granel (Vender bulto más barato que costo)
    const perdidaEnMayorista = prod.precioMayorista < prod.precioCosto;
    
    // Calculamos el costo por fracción = Costo Bulto / Unidades que trae el bulto
    const costoFraccion = (prod.cantidadMayorista > 0) ? (prod.precioCosto / prod.cantidadMayorista) : prod.precioCosto;
    // Calculamos si hay pérdida al vender por fracción (Vender kilo/unidad más barato de lo que costó el kilo/unidad en el interior del bulto)
    const perdidaEnFraccion = prod.precioVenta < costoFraccion;
    
    return perdidaEnMayorista || perdidaEnFraccion;
  }
  
  // Para productos de unidad normal
  return prod.precioVenta < prod.precioCosto;
}

// Lógica de Imagen
const manejarImagen = (event) => {
  const file = event.target.files[0]
  if (file) {
    archivoSeleccionado.value = file
    filePreview.value = URL.createObjectURL(file)
    formProd.value.eliminarImagen = false
  }
}

const eliminarImagenActual = () => {
  filePreview.value = null;
  archivoSeleccionado.value = null;
  formProd.value.eliminarImagen = true;
};

// Modal
const abrirModalNuevo = () => {
  modoEdicion.value = false
  errorFormulario.value = ''
  archivoSeleccionado.value = null
  filePreview.value = null
  formProd.value = { 
    codigo: '', nombre: '', descripcion: '', proveedorID: 0, unidadMedida: 'UND', 
    precioCosto: 0, precioVenta: 0, stockActual: 0, stockMinimo: 5,
    precioMayorista: 0, cantidadMayorista: 0, nombreUnidadMayorista: '',
    eliminarImagen: false
  }
  mostrarModal.value = true
}

const abrirModalEditar = (prod) => {
  modoEdicion.value = true
  productToEditId.value = prod.productoID
  errorFormulario.value = ''
  archivoSeleccionado.value = null
  filePreview.value = prod.imagenURL ? IMAGE_BASE + prod.imagenURL : null
  
  // Mapeo manual para asegurar que los campos nuevos no sean null
  formProd.value = {
    ...prod,
    precioMayorista: prod.precioMayorista ?? 0,
    cantidadMayorista: prod.cantidadMayorista ?? 0,
    nombreUnidadMayorista: prod.nombreUnidadMayorista ?? '',
    eliminarImagen: false
  }
  
  mostrarModal.value = true
}

const cerrarModal = () => mostrarModal.value = false

// Guardar usando FormData (para el archivo IFormFile)
const guardarProducto = async () => {
  if (formProd.value.proveedorID === 0) {
    errorFormulario.value = 'Debes seleccionar un proveedor válido.'
    return
  }

  guardando.value = true
  errorFormulario.value = ''

  const formData = new FormData()
  // Agregar todos los campos al FormData respetando valores numéricos como 0
  Object.keys(formProd.value).forEach(key => {
    const val = formProd.value[key];
    formData.append(key, val !== null && val !== undefined && val !== '' ? val : 0);
  });
  
  // Agregar imagen si fue seleccionada
  if (archivoSeleccionado.value) {
    formData.append('ImagenFoto', archivoSeleccionado.value)
  }

  try {
    let url = `${API_URL}/Productos`
    let method = 'POST'
    if (modoEdicion.value) {
      url = `${API_URL}/Productos/${productToEditId.value}`
      method = 'PUT'
    }

    const res = await fetch(url, {
      method: method,
      headers: { 'Authorization': `Bearer ${getToken()}` },
      // Omitir Content-Type para automática inserción de Boundary
      body: formData 
    })
    
    if (res.status === 400) {
      const data = await res.json();
      if (data.errors) {
        // Errores de validación ASP.NET Core
        const msgs = [];
        for (let prop in data.errors) {
          msgs.push(`${prop}: ${data.errors[prop].join(', ')}`);
        }
        errorFormulario.value = "Por favor, corrige estos campos incompletos o inválidos: " + msgs.join(" | ");
      } else {
        errorFormulario.value = data.mensaje || 'Información incongruente al tratar de guardar.';
      }
      return;
    }

    const data = await res.json()
    if (data.success) {
      cerrarModal()
      successGlobal.value = modoEdicion.value ? '¡Producto actualizado correctamente!' : '¡Producto creado correctamente!'
      setTimeout(() => { successGlobal.value = '' }, 3000)
      cargarDatos()
    } else {
      errorFormulario.value = data.mensaje || 'Error interno al guardar.';
    }
  } catch (err) {
    errorFormulario.value = 'Fallo de red al guardar producto.'
  } finally {
    guardando.value = false
  }
}

// LOGICA DE ELIMINACION DEFINITIVA
const abrirModalEliminar = (prod) => {
  prodAEliminar.value = prod
  passwordEliminar.value = ''
  errorEliminar.value = ''
  mostrarModalEliminar.value = true
}

const cerrarModalEliminar = () => {
  mostrarModalEliminar.value = false
  prodAEliminar.value = null
}

const confirmarEliminar = async () => {
  if (!passwordEliminar.value) {
    errorEliminar.value = 'Debes ingresar tu contraseña de usuario.'
    return
  }

  eliminando.value = true
  errorEliminar.value = ''

  try {
    const res = await fetch(`${API_URL}/Productos/${prodAEliminar.value.productoID}/eliminar`, {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}` 
      },
      body: JSON.stringify({ password: passwordEliminar.value })
    })
    const data = await res.json()
    if (data.success) {
      cerrarModalEliminar()
      cargarDatos()
    } else {
      errorEliminar.value = data.mensaje || 'Error al validar contraseña o eliminar el producto.'
    }
  } catch (err) {
    errorEliminar.value = 'Fallo de red al intentar eliminar.'
  } finally {
    eliminando.value = false
  }
}
const abrirModalToggle = (prod) => {
  prodAToggle.value = prod
  mostrarModalToggle.value = true
}
const cerrarModalToggle = () => {
  mostrarModalToggle.value = false
  prodAToggle.value = null
}

const confirmarToggle = async () => {
  if (!prodAToggle.value) return;
  togglando.value = true;
  
  try {
    const res = await fetch(`${API_URL}/Productos/${prodAToggle.value.productoID}/toggle-activo`, {
      method: 'PATCH',
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    if ((await res.json()).success) {
      cargarDatos()
      cerrarModalToggle()
    }
  } catch (err) {
    errorGlobal.value = 'Error al cambiar el estado del producto.'
    cerrarModalToggle()
  } finally {
    togglando.value = false
  }
}
</script>

<style scoped>
/* Filter bar */
.filters-bar { display: flex; justify-content: space-between; align-items: center; background: white; padding: 1rem 1.25rem; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03); border: 1px solid #E2E8F0; margin-bottom: 2rem; gap: 1rem; flex-wrap: wrap;}
.search-box { display: flex; flex: 1; align-items: center; background: #F7FAFC; padding: 0.75rem 1rem; border-radius: 10px; border: 1px solid #E2E8F0; min-width: 250px; transition: border-color 0.2s; }
.search-box:focus-within { border-color: #A3E4D7; box-shadow: 0 0 0 3px rgba(163, 228, 215, 0.2); }
.search-icon { margin-right: 0.5rem; opacity: 0.4; }
.search-box input { background: transparent; border: none; font-size: 0.95rem; width: 100%; color: #2D3748; outline: none; }
.sort-box { display: flex; align-items: center; gap: 0.75rem; }
.sort-box label { font-size: 0.9rem; color: #718096; font-weight: 500; }
.sort-box select { padding: 0.6rem 1rem; border-radius: 8px; border: 1px solid #E2E8F0; background: #F7FAFC; color: #2D3748; outline: none; font-weight: 500; cursor: pointer; transition: border-color 0.2s;}
.sort-box select:focus { border-color: #A3E4D7; }

/* Aprovechamos la UI común */
.productos-module { animation: fadeIn 0.4s ease; }
.module-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem; }
.title { font-size: 1.75rem; font-weight: 700; color: #1A202C; margin: 0 0 0.25rem 0; }
.subtitle { color: #718096; margin: 0; font-size: 0.95rem; }

/* Botones con color primario general (Turquesa sutil) */
.primary-btn {
  background-color: #A3E4D7; 
  color: #0E6251; 
  border: none; padding: 0.75rem 1.25rem; border-radius: 10px; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 0.5rem; transition: all 0.2s; box-shadow: 0 4px 10px rgba(163, 228, 215, 0.4);
}
.primary-btn:hover:not(:disabled) { background-color: #76D7C4; transform: translateY(-1px); }
.cancel-btn { background-color: transparent; color: #4A5568; border: 1px solid #CBD5E0; padding: 0.75rem 1.25rem; border-radius: 10px; font-weight: 600; cursor: pointer; }
.cancel-btn:hover { background-color: #F7FAFC; }

/* Tabla Estilo */
.table-container { background: white; border-radius: 16px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03); border: 1px solid #E2E8F0; overflow: hidden; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th { background-color: #F8FAFC; padding: 1rem 1.25rem; text-align: left; font-size: 0.8rem; text-transform: uppercase; color: #4A5568; border-bottom: 1px solid #E2E8F0; }
.data-table td { padding: 1rem 1.25rem; border-bottom: 1px solid #E2E8F0; color: #2D3748; font-size: 0.95rem; vertical-align: middle; }

/* Fotos miniatura */
.img-cell { width: 60px; }
.prod-thumb { width: 45px; height: 45px; border-radius: 8px; object-fit: cover; border: 1px solid #E2E8F0; }
.img-placeholder { width: 45px; height: 45px; border-radius: 8px; background: #EDF2F7; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; }

.font-medium { font-weight: 600; }
.text-muted { color: #718096; }
.price-text { font-weight: 700; color: #2F855A; }
.text-danger { color: #E53E3E; font-weight: 600; }
.text-success { color: #38A169; font-weight: 600; }
.alert-badge { background: #FFF5F5; padding: 0.2rem 0.4rem; border-radius: 6px; font-size: 0.75rem; margin-left: 0.5rem; display: inline-flex; align-items: center; gap: 0.2rem; }
.stock-badge { color: #9C4221; font-weight: 700; border: 1px solid #FBD38D; background-color: #FFFAF0; }
.loss-badge { color: #C53030; font-weight: 700; border: 1px solid #FEB2B2; background-color: #FEFCBF; }

.status-dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 6px; }
.dot-active { background-color: #48BB78; }
.dot-inactive { background-color: #F56565; }
.inactive-row td { opacity: 0.6; background-color: #FAFAFA; }
.loss-alert-row td { background-color: #FFF5F5; /* Light red tint for the whole row */ }

.actions-cell { text-align: right; white-space: nowrap; }
.action-btn { background: transparent; border: none; font-size: 1.1rem; padding: 0.4rem; cursor: pointer; border-radius: 6px; opacity: 0.7; }
.action-btn:hover { background: #EDF2F7; opacity: 1; }

.loading-state, .empty-state { padding: 4rem; text-align: center; color: #718096; }
.spinner { width: 40px; height: 40px; border: 4px solid #E2E8F0; border-top-color: #A3E4D7; border-radius: 50%; animation: spin 1s infinite linear; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Error y Success Banner */
.error-banner { background-color: #FFF5F5; color: #C53030; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; }
.success-banner { background-color: #F0FFF4; color: #2F855A; padding: 1rem; border-radius: 10px; margin-bottom: 1.5rem; font-weight: 500; border: 1px solid #C6F6D5; animation: fadeIn 0.3s; }

/* MODAL OVERLAY (LO QUE HACE QUE SE VEA) */
.modal-overlay { 
  position: fixed !important; 
  top: 0 !important; 
  left: 0 !important; 
  right: 0 !important; 
  bottom: 0 !important; 
  background-color: rgba(0, 0, 0, 0.6) !important; 
  backdrop-filter: blur(8px) !important; 
  display: flex !important; 
  align-items: center !important; 
  justify-content: center !important; 
  z-index: 999999 !important; /* Capa superior absoluta */
  animation: fadeIn 0.3s ease; 
}

/* MODAL PRO REDESIGN */
.modal-content.modal-large {
  background: white;
  width: 95%;
  max-width: 900px;
  max-height: 90vh; /* Ajustado para laptops pequeñas */
  border-radius: 24px;
  box-shadow: 0 25px 70px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

.modal-form-pro {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden; /* Evita scroll doble */
}

.modal-body-scroll {
  padding: 1.5rem;
  overflow-y: auto;
  max-height: 65vh; /* Altura ideal para laptops */
  scrollbar-width: thin;
  scrollbar-color: #A3E4D7 #F8FAFC;
}

/* Secciones Agrupadas */
.form-section {
  background: white;
  border: 1px solid #EDF2F7;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  position: relative;
}

.section-badge {
  position: absolute;
  top: -12px;
  left: 15px;
  background: #2D3748;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 700;
  margin: 0;
}

.color-price { background: #2F855A; }
.color-wholesale { background: #805AD5; }

.wholesale-bg {
  background: linear-gradient(to bottom right, #F7FAFC, #EDF2F7);
  border: 1.5px dashed #CBD5E0;
}

.section-help {
  font-size: 0.8rem;
  color: #718096;
  margin-bottom: 1rem;
}

/* Grid y Grupos de Formulario (Limpieza Total) */
.compact-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  width: 100%;
}

.form-group label {
  display: block;
  font-size: 0.85rem;
  font-weight: 800;
  color: #334155;
  margin: 0;
  text-align: left;
}

.form-group input, 
.form-group select, 
.form-group textarea {
  width: 100%;
  padding: 0.8rem 1rem;
  border: 1.5px solid #CBD5E1;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.2s;
  background-color: #FDFDFD;
  box-sizing: border-box;
  color: #1E293B;
  outline: none;
}

.form-group input:focus, 
.form-group select:focus, 
.form-group textarea:focus {
  border-color: #2D3748;
  background-color: white;
  box-shadow: 0 0 0 4px rgba(45, 55, 72, 0.08);
}

.form-group.lg {
  grid-column: span 2;
}

.form-group textarea {
  min-height: 90px;
  resize: vertical;
}

/* Footer Fijo */
.modal-footer-fixed {
  padding: 1.25rem 1.5rem;
  border-top: 1px solid #E2E8F0;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  background: #F8FAFC;
  border-radius: 0 0 24px 24px;
}

.primary-btn-pro {
  background-color: #2D3748;
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.primary-btn-pro:hover:not(:disabled) {
  background-color: #1A202C;
  transform: translateY(-1px);
}

/* Header Estilizado */
.modal-header {
  padding: 1.75rem 2rem;
  background: white;
  border-bottom: 1px solid #F1F5F9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h3 {
  font-size: 1.4rem;
  font-weight: 800;
  color: #1E293B;
  margin: 0;
}

.close-btn {
  background: #F1F5F9;
  border: none;
  font-size: 1.2rem;
  width: 36px;
  height: 36px;
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  color: #64748B;
}

.close-btn:hover {
  background: #E2E8F0;
  color: #0F172A;
  transform: rotate(90deg);
}

/* Imagen Upload Wrapper */
.image-upload-wrapper {
  max-width: 400px;
  margin-top: 0.5rem;
}

/* Responsive fixes */
@media (max-width: 768px) {
  .form-group.lg {
    grid-column: span 1;
  }
  .modal-body-scroll {
    max-height: 70vh;
  }
}
</style>
