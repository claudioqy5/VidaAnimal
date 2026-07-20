<template>
  <div>
    <!-- Filtros -->
    <div class="filters-bar">
      <input
        type="text"
        v-model="busqueda"
        placeholder="🔍 Buscar por nombre o código..."
        class="input"
      />
      <div class="filter-row">
        <select v-model="filtroCategoriaID" class="input">
          <option :value="null">🏷️ Todas las categorías</option>
          <option v-for="c in categorias" :key="c.categoriaID" :value="c.categoriaID">{{ c.nombre }}</option>
        </select>
        <select v-model="filtroEspecieID" class="input">
          <option :value="null">🐾 Todas las mascotas</option>
          <option v-for="e in especies" :key="e.especieID" :value="e.especieID">{{ e.nombre }}</option>
        </select>
      </div>
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <span style="font-size: 0.78rem; color: var(--text-muted);">
          {{ productosFiltrados.length }} de {{ productos.length }} productos
        </span>
        <button
          v-if="filtroCategoriaID || filtroEspecieID"
          class="btn btn-danger"
          style="font-size: 0.72rem; padding: 0.2rem 0.6rem; min-height: unset; height: 28px;"
          @click="filtroCategoriaID = null; filtroEspecieID = null"
        >
          ✕ Limpiar filtros
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="cargando" class="loading-screen">
      <div class="spinner"></div>
      <span>Cargando inventario...</span>
    </div>

    <!-- Lista de productos -->
    <div v-else class="inv-list">
      <div
        class="inv-item"
        v-for="prod in productosFiltrados"
        :key="prod.productoID"
        @click="verDetalle(prod)"
      >
        <!-- Imagen miniatura -->
        <img v-if="prod.imagenURL" :src="`/api${prod.imagenURL}`" class="inv-item-img" :alt="prod.nombre" />
        <div v-else class="inv-item-no-img">🐾</div>

        <!-- Info -->
        <div class="inv-item-info">
          <div class="inv-item-name">{{ prod.nombre }}</div>
          <div class="inv-item-code">Cod: {{ prod.codigo }}</div>
          <div class="inv-item-tags">
            <span v-for="esp in prod.especies" :key="esp.especieID" class="inv-tag">🐾 {{ esp.nombre }}</span>
            <span v-if="prod.categoria" class="inv-tag inv-tag-cat">🏷️ {{ prod.categoria.nombre }}</span>
            <span v-if="!prod.activo" class="badge badge-danger" style="font-size: 0.62rem;">Inactivo</span>
          </div>
        </div>

        <!-- Precios y stock -->
        <div class="inv-item-right">
          <div class="inv-price">S/ {{ prod.precioVenta.toFixed(2) }}</div>
          <div class="inv-stock" :class="getStockClass(prod)">
            {{ formatStock(prod) }}
          </div>
          <!-- Alertas -->
          <div v-if="prod.stockActual <= prod.stockMinimo && prod.activo" style="font-size: 0.65rem; color: var(--warning); margin-top: 0.2rem;">⚠️ Bajo</div>
          <div v-if="evaluarPerdida(prod)" style="font-size: 0.65rem; color: var(--danger); margin-top: 0.1rem;">🚨 Pérdida</div>
        </div>
      </div>

      <div v-if="productosFiltrados.length === 0" style="text-align: center; color: var(--text-muted); padding: 2rem; font-size: 0.9rem;">
        No se encontraron productos.
      </div>
    </div>

    <!-- =================== MODAL DETALLE PRODUCTO =================== -->
    <div v-if="productoDetalle" class="modal-overlay" @click.self="productoDetalle = null">
      <div class="modal-sheet">
        <div class="modal-handle"></div>

        <!-- Imagen grande -->
        <img
          v-if="productoDetalle.imagenURL"
          :src="`/api${productoDetalle.imagenURL}`"
          class="detail-img"
          :alt="productoDetalle.nombre"
          @click="verImagenCompleta = true"
        />
        <div v-else class="detail-no-img">🐾</div>

        <!-- Nombre y código -->
        <div class="detail-name">{{ productoDetalle.nombre }}</div>
        <div class="detail-code">Código: {{ productoDetalle.codigo }}</div>

        <!-- Estado -->
        <div style="display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem;">
          <span class="badge" :class="productoDetalle.activo ? 'badge-success' : 'badge-danger'">
            {{ productoDetalle.activo ? '✓ Activo' : '✕ Inactivo' }}
          </span>
          <span v-if="productoDetalle.stockActual <= productoDetalle.stockMinimo && productoDetalle.activo" class="badge badge-warning">⚠️ Stock Bajo</span>
          <span v-if="evaluarPerdida(productoDetalle)" class="badge badge-danger">🚨 Precio < Costo</span>
        </div>

        <!-- Grid de datos -->
        <div class="detail-grid">
          <!-- Precio de venta -->
          <div class="detail-cell">
            <div class="detail-cell-label">Precio Venta</div>
            <div class="detail-cell-value price-green">S/ {{ productoDetalle.precioVenta.toFixed(2) }}</div>
          </div>

          <!-- Precio de costo (unitario) -->
          <div class="detail-cell">
            <div class="detail-cell-label">Precio Costo</div>
            <div class="detail-cell-value">
              S/ {{
                (productoDetalle.unidadMedida === 'SACO' || productoDetalle.unidadMedida === 'BALDE') && productoDetalle.cantidadMayorista > 0
                  ? (productoDetalle.precioCosto / productoDetalle.cantidadMayorista).toFixed(2)
                  : productoDetalle.precioCosto.toFixed(2)
              }}
            </div>
          </div>

          <!-- Stock actual -->
          <div class="detail-cell">
            <div class="detail-cell-label">Stock Actual</div>
            <div class="detail-cell-value" :class="getStockClass(productoDetalle)">
              {{ formatStock(productoDetalle) }}
            </div>
          </div>

          <!-- Stock mínimo -->
          <div class="detail-cell">
            <div class="detail-cell-label">Stock Mínimo</div>
            <div class="detail-cell-value">{{ productoDetalle.stockMinimo }}</div>
          </div>

          <!-- Unidad de medida -->
          <div class="detail-cell">
            <div class="detail-cell-label">Unidad</div>
            <div class="detail-cell-value">{{ productoDetalle.unidadMedida }}</div>
          </div>

          <!-- Precio mayorista (si aplica) -->
          <div class="detail-cell" v-if="productoDetalle.unidadMedida === 'SACO' || productoDetalle.unidadMedida === 'BALDE'">
            <div class="detail-cell-label">P. Bulto Entero</div>
            <div class="detail-cell-value price-green">S/ {{ productoDetalle.precioMayorista?.toFixed(2) ?? '—' }}</div>
          </div>

          <!-- Cantidad por bulto -->
          <div class="detail-cell" v-if="productoDetalle.unidadMedida === 'SACO' || productoDetalle.unidadMedida === 'BALDE'">
            <div class="detail-cell-label">{{ productoDetalle.unidadMedida === 'SACO' ? 'Kg/Saco' : 'Und/Balde' }}</div>
            <div class="detail-cell-value">{{ productoDetalle.cantidadMayorista }}</div>
          </div>
        </div>

        <!-- Clasificaciones -->
        <div v-if="(productoDetalle.especies && productoDetalle.especies.length > 0) || productoDetalle.categoria" style="margin-bottom: 1rem;">
          <div class="label" style="margin-bottom: 0.5rem;">Clasificación</div>
          <div style="display: flex; flex-wrap: wrap; gap: 0.4rem;">
            <span v-for="esp in productoDetalle.especies" :key="esp.especieID" class="badge badge-green">🐾 {{ esp.nombre }}</span>
            <span v-if="productoDetalle.categoria" class="badge badge-blue">🏷️ {{ productoDetalle.categoria.nombre }}</span>
          </div>
        </div>

        <!-- Proveedor -->
        <div v-if="productoDetalle.proveedor" style="margin-bottom: 1rem;">
          <div class="label" style="margin-bottom: 0.3rem;">Proveedor</div>
          <div style="background: var(--bg-surface); border-radius: var(--radius-sm); padding: 0.6rem 0.75rem; font-size: 0.88rem; color: var(--text-secondary); font-weight: 600;">
            {{ productoDetalle.proveedor.nombre || productoDetalle.proveedor }}
          </div>
        </div>

        <!-- Descripción -->
        <div v-if="productoDetalle.descripcion">
          <div class="label" style="margin-bottom: 0.3rem;">Descripción</div>
          <div class="detail-desc">{{ productoDetalle.descripcion }}</div>
        </div>

        <button class="btn btn-secondary btn-full" style="margin-top: 1.25rem;" @click="productoDetalle = null">
          Cerrar
        </button>
      </div>
    </div>

    <!-- Vista imagen completa -->
    <div v-if="productoDetalle && verImagenCompleta" class="modal-overlay" @click="verImagenCompleta = false" style="align-items: center; justify-content: center;">
      <div style="padding: 1rem; max-width: 100%; max-height: 100%;" @click.stop>
        <img
          :src="`/api${productoDetalle.imagenURL}`"
          style="max-width: 100%; max-height: 85vh; border-radius: var(--radius); object-fit: contain;"
          :alt="productoDetalle.nombre"
        />
        <button class="btn btn-secondary btn-full" style="margin-top: 0.75rem;" @click="verImagenCompleta = false">Cerrar</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const API_URL = '/api'

const productos = ref([])
const categorias = ref([])
const especies = ref([])
const cargando = ref(true)

const busqueda = ref('')
const filtroCategoriaID = ref(null)
const filtroEspecieID = ref(null)

const productoDetalle = ref(null)
const verImagenCompleta = ref(false)

const getToken = () => localStorage.getItem('jwt_token')

const cargarDatos = async () => {
  cargando.value = true
  try {
    const headers = { Authorization: `Bearer ${getToken()}` }
    const [resProd, resEsp, resCat] = await Promise.all([
      fetch(`${API_URL}/Productos`, { headers }),
      fetch(`${API_URL}/Clasificacion/especies`, { headers }),
      fetch(`${API_URL}/Clasificacion/categorias`, { headers })
    ])
    const dProd = await resProd.json()
    const dEsp = await resEsp.json()
    const dCat = await resCat.json()
    if (dProd.success) productos.value = dProd.data
    if (dEsp.success) especies.value = dEsp.data
    if (dCat.success) categorias.value = dCat.data
  } catch {
    // silencioso, la lista quedará vacía
  } finally {
    cargando.value = false
  }
}

onMounted(() => cargarDatos())

const productosFiltrados = computed(() => {
  let res = [...productos.value]

  if (busqueda.value.trim()) {
    const palabras = busqueda.value.toLowerCase().split(/\s+/).filter(w => w.length > 0)
    res = res.filter(p => {
      const texto = `${p.nombre ?? ''} ${p.codigo ?? ''}`.toLowerCase()
      return palabras.every(pal => texto.includes(pal))
    })
  }

  if (filtroCategoriaID.value !== null) {
    res = res.filter(p => p.categoriaID === filtroCategoriaID.value)
  }

  if (filtroEspecieID.value !== null) {
    res = res.filter(p => p.especies && p.especies.some(e => e.especieID === filtroEspecieID.value))
  }

  // Más recientes primero
  res.sort((a, b) => (b.productoID || 0) - (a.productoID || 0))

  return res
})

const formatStock = (prod) => {
  if ((prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') && prod.cantidadMayorista > 0) {
    const totalInner = prod.stockActual * prod.cantidadMayorista
    const formatted = parseFloat(totalInner.toFixed(2))
    let unidad = prod.nombreUnidadMayorista
    if (!unidad || unidad === '0' || unidad === '') unidad = prod.unidadMedida === 'SACO' ? 'KG' : 'UND'
    return `${formatted} ${unidad}`
  }
  return `${prod.stockActual} ${prod.unidadMedida}`
}

const getStockClass = (prod) => {
  if (prod.stockActual <= 0) return 'stock-out-text'
  if (prod.stockActual <= prod.stockMinimo) return 'stock-low-text'
  return 'stock-ok-text'
}

const evaluarPerdida = (prod) => {
  if (!prod.activo) return false
  if (prod.unidadMedida === 'SACO' || prod.unidadMedida === 'BALDE') {
    const perdidaMayorista = prod.precioMayorista < prod.precioCosto
    const costoFraccion = prod.cantidadMayorista > 0 ? prod.precioCosto / prod.cantidadMayorista : prod.precioCosto
    return perdidaMayorista || prod.precioVenta < costoFraccion
  }
  return prod.precioVenta < prod.precioCosto
}

const verDetalle = (prod) => {
  productoDetalle.value = prod
  verImagenCompleta.value = false
}
</script>
