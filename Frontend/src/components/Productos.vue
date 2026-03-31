<template>
  <div class="productos-module">
    <div class="module-header">
      <div>
        <h2 class="title">Gestión de Productos</h2>
        <p class="subtitle">Consulta y edita tu catálogo de ventas</p>
      </div>
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
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="prod in productosFiltrados" :key="prod.productoID" :class="{ 'inactive-row': !prod.activo, 'loss-alert-row': (prod.activo && prod.precioVenta < prod.precioCosto) }">
            <td class="img-cell">
              <img v-if="prod.imagenURL" :src="`${IMAGE_BASE}${prod.imagenURL}`" class="prod-thumb" alt="Product thumbnail" />
              <div v-else class="img-placeholder">🐾</div>
            </td>
            <td class="font-medium text-muted">{{ prod.codigo }}</td>
            <td class="font-medium">
              <div class="prod-name">
                {{ prod.nombre }}
                <span v-if="prod.stockActual <= prod.stockMinimo" class="alert-badge stock-badge" title="Stock Bajo">⚠️ Bajo Stock</span>
                <span v-if="prod.activo && prod.precioVenta < prod.precioCosto" class="alert-badge loss-badge" title="Venta por debajo del costo">🚨 Pérdida</span>
              </div>
            </td>
            <td class="text-muted">S/ {{ prod.precioCosto.toFixed(2) }}</td>
            <td class="price-text">S/ {{ prod.precioVenta.toFixed(2) }}</td>
            <td>
              <span :class="{'text-danger': prod.stockActual <= prod.stockMinimo, 'text-success': prod.stockActual > prod.stockMinimo}">
                {{ prod.stockActual }} {{ prod.unidadMedida }}
              </span>
            </td>
            <td>
              <span class="status-dot" :class="prod.activo ? 'dot-active' : 'dot-inactive'"></span>
              {{ provStatus(prod.activo) }}
            </td>
            <td class="actions-cell">
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

    <!-- Modal Formulario -->
    <div v-if="mostrarModal" class="modal-overlay" @click.self="cerrarModal">
      <div class="modal-content modal-large">
        <div class="modal-header">
          <h3>{{ modoEdicion ? 'Editar Producto' : 'Crear Producto' }}</h3>
          <button class="close-btn" @click="cerrarModal">✕</button>
        </div>
        
        <form @submit.prevent="guardarProducto" class="modal-form-pro">
          <div v-if="errorFormulario" class="form-error">{{ errorFormulario }}</div>

          <!-- CONTENIDO SCROLLABLE -->
          <div class="modal-body-scroll">
            
            <div class="form-section">
              <h4 class="section-badge">📦 Información General</h4>
              <div class="compact-grid">
                <div class="form-group lg">
                  <label>Nombre del Producto *</label>
                  <input type="text" v-model="formProd.nombre" required placeholder="Ej. Ricocan Adultos 15Kg" />
                </div>
                <div class="form-group">
                  <label>Código / SKU *</label>
                  <input type="text" v-model="formProd.codigo" required placeholder="Ej. 1029384" />
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
                  <textarea v-model="formProd.descripcion" rows="2" placeholder="Características..."></textarea>
                </div>
              </div>
            </div>

            <div class="form-section">
              <h4 class="section-badge color-price">💰 Precios e Inventario</h4>
              <div class="compact-grid">
                <div class="form-group">
                  <label>Precio Costo (S/)</label>
                  <input type="number" step="0.01" min="0" v-model="formProd.precioCosto" required />
                </div>
                <div class="form-group">
                  <label>Precio Venta (S/)</label>
                  <input type="number" step="0.01" min="0" v-model="formProd.precioVenta" required />
                </div>
                <div class="form-group">
                  <label>Unidad Medida</label>
                  <select v-model="formProd.unidadMedida" required>
                    <option value="UND">Unidad</option>
                    <option value="KG">Kilogramos</option>
                    <option value="SACO">Saco</option>
                    <option value="LTS">Litros</option>
                    <option value="PQTE">Paquete</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Stock Mínimo</label>
                  <input type="number" min="0" v-model="formProd.stockMinimo" required />
                </div>
              </div>
            </div>

            <div class="form-section wholesale-bg">
              <h4 class="section-badge color-wholesale">🏗️ Venta por Mayor (Opcional)</h4>
              <p class="section-help">Usa esto para productos que vendes por saco y por kilo simultáneamente.</p>
              <div class="compact-grid">
                <div class="form-group lg">
                  <label>Nombre de Unidad (Ej. Saco de 50kg)</label>
                  <input type="text" v-model="formProd.nombreUnidadMayorista" placeholder="Nombre que verá el cajero" />
                </div>
                <div class="form-group">
                  <label>Cant. Equiv. (Kilos/Unid)</label>
                  <input type="number" step="0.01" min="0" v-model="formProd.cantidadMayorista" placeholder="e.g. 50" />
                </div>
                <div class="form-group">
                  <label>Precio Saco (S/)</label>
                  <input type="number" step="0.01" min="0" v-model="formProd.precioMayorista" placeholder="e.g. 45.00" />
                </div>
              </div>
            </div>

            <div class="form-section">
              <h4 class="section-badge">🖼️ Imagen del Producto</h4>
              <div class="image-upload-wrapper">
                <div class="file-drop-area" :class="{'has-file': filePreview}">
                  <input type="file" @change="manejarImagen" accept="image/png, image/jpeg, image/webp" class="file-input" />
                  <div v-if="!filePreview" class="file-msg">
                    <span class="upload-icon">📸</span>
                    <p>Subir o arrastrar imagen</p>
                  </div>
                  <img v-else :src="filePreview" class="preview-img" />
                </div>
              </div>
            </div>

          </div> <!-- FIN SCROLL -->

          <div class="modal-footer-fixed">
            <button type="button" class="cancel-btn" @click="cerrarModal">Cancelar</button>
            <button type="submit" class="primary-btn-pro" :disabled="guardando">
              {{ guardando ? 'Guardando...' : (modoEdicion ? 'Actualizar Producto' : 'Crear Producto') }}
            </button>
          </div>
        </form>
      </div>
    </div>
    <!-- Modal de Eliminación Segura -->
    <div v-if="mostrarModalEliminar" class="modal-overlay" @click.self="cerrarModalEliminar">
      <div class="modal-content" style="max-width: 450px; background: white; border-radius: 20px; overflow: hidden; box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);">
        <div class="modal-header" style="background-color: #FFF5F5; border-bottom: 2px solid #FED7D7; border-radius: 20px 20px 0 0;">
          <h3 style="color: #C53030;">⚠️ Alerta Crítica</h3>
          <button class="close-btn" @click="cerrarModalEliminar">✕</button>
        </div>
        
        <form @submit.prevent="confirmarEliminar" class="modal-form">
          <p style="font-weight: 700; color: #2D3748; margin-bottom: 0.5rem; text-align: center;">¿Estás completamente seguro?</p>
          <p style="color: #4A5568; font-size: 0.95rem; line-height: 1.5; text-align: center; margin-top: 0; margin-bottom: 1.5rem;">
            Se va a eliminar definitivamente el producto <strong style="color: #E53E3E;">{{ prodAEliminar?.nombre }}</strong> y todos sus datos del sistema. Para recuperarlo tendrás que crearlo nuevamente desde cero.
          </p>
          
          <div v-if="errorEliminar" class="form-error" style="text-align: center;">{{ errorEliminar }}</div>

          <div class="form-group" style="background: #F7FAFC; padding: 1rem; border-radius: 10px; border: 1px dashed #CBD5E0;">
            <label style="text-align: center; color: #4A5568;">Por seguridad, ingresa tu contraseña para confirmar:</label>
            <input type="password" v-model="passwordEliminar" required placeholder="Tu contraseña de usuario" style="text-align: center; margin-top: 0.5rem;" />
          </div>

          <div class="modal-footer" style="justify-content: center; gap: 1rem;">
            <button type="button" class="cancel-btn" @click="cerrarModalEliminar">Cancelar</button>
            <button type="submit" class="primary-btn" :disabled="eliminando" style="background-color: #E53E3E; color: white;">
              {{ eliminando ? 'Verificando...' : 'Sí, Eliminar Definitivamente' }}
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
const IMAGE_BASE = '/'

const productos = ref([])
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
  let resultado = productos.value

  // Filtrado por nombre o código
  if (busqueda.value.trim() !== '') {
    const q = busqueda.value.toLowerCase()
    resultado = resultado.filter(p => 
      p.nombre.toLowerCase().includes(q) || 
      p.codigo.toLowerCase().includes(q)
    )
  }

  // Ordenamiento (asumiendo que el ID incremental equivale a fecha de creación reciente si son consecutivos)
  resultado = resultado.slice().sort((a, b) => {
    // Si viene fechaCreacion en formato ISO desde C#, ordenaríamos así:
    const timeA = new Date(a.fechaCreacion || 0).getTime();
    const timeB = new Date(b.fechaCreacion || 0).getTime();
    
    // Fallback: ordenar por ID si no tuvieran fecha (productoID más alto = más reciente)
    const valA = timeA > 0 ? timeA : a.productoID;
    const valB = timeB > 0 ? timeB : b.productoID;

    if (ordenFecha.value === 'desc') return valB - valA;
    return valA - valB;
  })

  return resultado
})

const getToken = () => localStorage.getItem('jwt_token')

// Carga Inicial
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
  }
}

onMounted(() => cargarDatos())

const provStatus = (activo) => activo ? 'Activo' : 'Inactivo'

// Lógica de Imagen
const manejarImagen = (event) => {
  const file = event.target.files[0]
  if (file) {
    archivoSeleccionado.value = file
    filePreview.value = URL.createObjectURL(file)
  }
}

// Modal
const abrirModalNuevo = () => {
  modoEdicion.value = false
  errorFormulario.value = ''
  archivoSeleccionado.value = null
  filePreview.value = null
  formProd.value = { 
    codigo: '', nombre: '', descripcion: '', proveedorID: 0, unidadMedida: 'UND', 
    precioCosto: 0, precioVenta: 0, stockActual: 0, stockMinimo: 5,
    precioMayorista: 0, cantidadMayorista: 0, nombreUnidadMayorista: ''
  }
  mostrarModal.value = true
}

const abrirModalEditar = (prod) => {
  modoEdicion.value = true
  productToEditId.value = prod.productoID
  errorFormulario.value = ''
  archivoSeleccionado.value = null
  filePreview.value = prod.imagenURL ? IMAGE_BASE + prod.imagenURL : null
  formProd.value = { ...prod } // Copiamos info
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

/* MODAL PRO REDESIGN */
.modal-content.modal-large {
  max-width: 900px;
  max-height: 95vh;
  display: flex;
  flex-direction: column;
}

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

/* Grid Compacto y Alineado */
.compact-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.25rem;
}

.form-group.lg {
  grid-column: span 2;
}

/* Footer Fijo */
.modal-footer-fixed {
  padding: 1.25rem 1.5rem;
  border-top: 1px solid #E2E8F0;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  background: #F8FAFC;
  border-radius: 0 0 20px 20px;
}

.primary-btn-pro {
  background-color: #2D3748;
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 10px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.primary-btn-pro:hover:not(:disabled) {
  background-color: #1A202C;
  transform: translateY(-1px);
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
