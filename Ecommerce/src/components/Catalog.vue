<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import api from '../utils/api'
import ProductCard from './ProductCard.vue'

// Importación de Logos Personalizados
import iconHuella from '../assets/logohuella.png'
import iconGato from '../assets/logogato.JPG'
import iconPollo from '../assets/logopollo.JPG'
import iconCuy from '../assets/logocuy.JPG'
import iconChancho from '../assets/logochancho.JPG'
import iconConejo from '../assets/logoconejo.jpg'
import iconPerro from '../assets/logoperro.jpg'

const props = defineProps({
  speciesFilter: String,
  categoryFilter: String,
  searchQuery: String // Nueva prop desde el Header
})

const productos = ref([])
const categorias = ref([])
const especies = ref([])
const loading = ref(true)

// Estado de filtros internos
const search = ref('')
const selectedCategory = ref(null)
const selectedSpecies = ref(null)

// Mapeo automático de iconos
const getSpeciesIcon = (nombre) => {
  const n = (nombre || '').toLowerCase()
  if (n.includes('perro')) return iconPerro
  if (n.includes('gato')) return iconGato
  if (n.includes('pollo')) return iconPollo
  if (n.includes('cuy')) return iconCuy
  if (n.includes('chancho')) return iconChancho
  if (n.includes('conejo')) return iconConejo
  return iconHuella // Fallback si no hay icono específico
}

// Sincronizar filtros externos del Header con el Catálogo
watch(() => [props.speciesFilter, props.categoryFilter], ([newSpec, newCat]) => {
  if (newSpec) {
    if (newSpec === 'others') {
      selectedSpecies.value = 'others'
    } else {
      const found = especies.value.find(e => e.nombre.toLowerCase().includes(newSpec.toLowerCase()))
      selectedSpecies.value = found ? found.especieId : null
    }
  }
  
  if (newCat) {
    const found = categorias.value.find(c => c.nombre.toLowerCase().includes(newCat.toLowerCase()))
    selectedCategory.value = found ? found.categoriaId : null
  } else if (newCat === '') {
    selectedCategory.value = null
  }
}, { immediate: true })

onMounted(async () => {
  try {
    const [prodRes, catRes, specRes] = await Promise.all([
      api.get('/Ecommerce/Productos'),
      api.get('/Ecommerce/Categorias'),
      api.get('/Ecommerce/Especies')
    ])
    productos.value = prodRes.data.data
    categorias.value = catRes.data.data
    especies.value = specRes.data.data
    
    // Re-ejecutar sincronización después de cargar datos
    if (props.speciesFilter) {
      if (props.speciesFilter === 'others') {
        selectedSpecies.value = 'others'
      } else {
        const found = especies.value.find(e => e.nombre.toLowerCase().includes(props.speciesFilter.toLowerCase()))
        selectedSpecies.value = found ? found.especieId : null
      }
    }
    if (props.categoryFilter) {
      const found = categorias.value.find(c => c.nombre.toLowerCase().includes(props.categoryFilter.toLowerCase()))
      selectedCategory.value = found ? found.categoriaId : null
    }
  } catch (error) {
    console.error("Error al cargar datos:", error)
  } finally {
    loading.value = false
  }
})

const filteredProducts = computed(() => {
  return productos.value.filter(p => {
    const matchesSearch = p.nombre.toLowerCase().includes(search.value.toLowerCase()) || 
                          p.descripcion?.toLowerCase().includes(search.value.toLowerCase())
    
    const matchesCategory = !selectedCategory.value || p.categoriaId === selectedCategory.value
    
    let matchesSpecies = true
    if (selectedSpecies.value === 'others') {
      // Mostrar si el producto tiene AL MENOS UNA especie que NO sea Perro, Gato o Pollo
      matchesSpecies = p.especies?.some(e => 
        !['perro', 'gato', 'pollo'].includes(e.nombre.toLowerCase())
      )
    } else if (selectedSpecies.value) {
      matchesSpecies = p.especies?.some(e => e.especieId === selectedSpecies.value)
    }
                           
    return matchesSearch && matchesCategory && matchesSpecies
  })
})

// Sincronización con búsqueda global del Header
watch(() => props.searchQuery, (newVal) => {
  search.value = newVal || ''
}, { immediate: true })

const toggleSpecies = (id) => {
  selectedSpecies.value = selectedSpecies.value === id ? null : id
  // Al cambiar de especie, si la categoría actual no existe para esa especie, la limpiamos
  if (selectedCategory.value && !availableCategories.value.some(c => c.categoriaId === selectedCategory.value)) {
    selectedCategory.value = null
  }
}

const toggleCategory = (id) => {
  selectedCategory.value = selectedCategory.value === id ? null : id
}

// Categorías que realmente tienen productos según la especie seleccionada
const availableCategories = computed(() => {
  let relevantProducts = productos.value
  
  if (selectedSpecies.value) {
    relevantProducts = relevantProducts.filter(p => 
      p.especies?.some(e => e.especieId === selectedSpecies.value)
    )
  }
  
  const idsInStock = [...new Set(relevantProducts.map(p => p.categoriaId))].filter(id => id !== null)
  return categorias.value.filter(c => idsInStock.includes(c.categoriaId))
})

const selectedProduct = ref(null)
const qty = ref(1)
const emit = defineEmits(['add-to-cart'])

// --- LÓGICA CALCULADORA NUTRICIONAL ---
const petWeight = ref(10)
const activityLevel = ref(1.2) // 1.0 (bajo), 1.2 (normal), 1.5 (activo)

const parseProductWeight = (name) => {
  const match = name.match(/(\d+)\s*kg/i)
  return match ? parseInt(match[1]) : null
}

const calculatedRation = computed(() => {
  // Fórmula base: 25g por kilo para actividad normal
  return Math.round(petWeight.value * 22 * activityLevel.value)
})

const durationInDays = computed(() => {
  if (!selectedProduct.value) return 0
  const bagKg = parseProductWeight(selectedProduct.value.nombre)
  if (!bagKg) return 0
  return Math.floor((bagKg * 1000) / calculatedRation.value)
})

const selectProduct = (product) => {
  selectedProduct.value = product
  qty.value = 1 
  petWeight.value = 10 // Reset a valor promedio
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const clearSelection = () => {
  selectedProduct.value = null
}

// Cerrar detalle si se cambia cualquier filtro
watch([selectedSpecies, selectedCategory, search], () => {
  selectedProduct.value = null
})

const formatPrice = (price) => {
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(price)
}

const getWhatsAppUrl = (product) => {
  const phone = "+51975418965"
  const message = `Hola Vida Animal! 👋 Me interesa el producto: *${product.nombre}* que tiene un precio de *${formatPrice(product.precioVenta)}*. Tienen stock disponible?`
  return `https://wa.me/${phone}?text=${encodeURIComponent(message)}`
}

const getImageUrl = (url) => {
  if (!url) return 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?q=80&w=800&auto=format&fit=crop'
  if (url.startsWith('/')) {
     return `https://vidaanimal.helifyferdigital.cloud/api${url}`
  }
  return url
}
</script>

<template>
  <section class="catalog-section">
    <div class="catalog-container">
      <!-- SIDEBAR DE FILTROS -->
      <aside class="catalog-sidebar">
        <div class="sidebar-block">
          <h3>Buscar</h3>
          <div class="search-bar glass">
            <span class="search-icon">🔍</span>
            <input type="text" v-model="search" placeholder="Ej: Correa nylon...">
          </div>
        </div>

        <div class="sidebar-block">
          <h3>Mascotas</h3>
          <div class="species-list">
            <button 
              class="side-filter-btn" 
              :class="{ active: !selectedSpecies }"
              @click="selectedSpecies = null"
            >
              <img :src="iconHuella" class="species-icon" alt="Huella">
              Todas las Mascotas
            </button>
            <button 
              v-for="esp in especies" 
              :key="esp.especieId"
              class="side-filter-btn"
              :class="{ active: selectedSpecies === esp.especieId }"
              @click="toggleSpecies(esp.especieId)"
            >
              <img :src="getSpeciesIcon(esp.nombre)" class="species-icon" :alt="esp.nombre">
              {{ esp.nombre }}
            </button>
          </div>
        </div>

        <!-- Lista de Categorías Inteligente -->
        <div class="sidebar-block">
          <h3>Categorías {{ selectedSpecies ? 'para ' + especies.find(e => e.especieId === selectedSpecies)?.nombre : '' }}</h3>
          <div class="category-list">
             <button 
              v-for="cat in availableCategories" 
              :key="cat.categoriaId"
              class="category-filter-btn"
              :class="{ active: selectedCategory === cat.categoriaId }"
              @click="toggleCategory(cat.categoriaId)"
            >
              {{ cat.nombre }}
              <span class="count" v-if="selectedCategory !== cat.categoriaId">→</span>
              <span class="close-icon" v-else>✕</span>
            </button>
            <p v-if="!availableCategories.length" class="no-cats">No hay categorías disponibles</p>
          </div>
        </div>
      </aside>

      <!-- ÁREA DE PRODUCTOS -->
      <main class="catalog-main">
        <!-- VISTA DE GRILLA -->
        <div v-if="!selectedProduct" class="catalog-results-wrapper">
          <header class="main-header">
            <h2>Resultados <span>{{ selectedSpecies ? 'para ' + especies.find(e => e.especieId === selectedSpecies)?.nombre : '' }}</span></h2>
            <p v-if="filteredProducts.length">{{ filteredProducts.length }} productos</p>
          </header>

          <div v-if="loading" class="loading-state">
             <div class="spinner"></div>
             <p>Cargando lo mejor para tu mascota...</p>
          </div>

          <div v-else-if="filteredProducts.length" class="products-grid">
            <ProductCard 
              v-for="prod in filteredProducts" 
              :key="prod.productoID" 
              :producto="prod"
              @select="selectProduct"
            />
          </div>

          <div v-else class="empty-state">
             <span class="empty-icon">🏜️</span>
             <h3>Sin resultados</h3>
             <p>Prueba con otros filtros o nombres.</p>
          </div>
        </div>

        <!-- VISTA DE DETALLE -->
        <div v-else class="product-detail-view fade-in">
          <button class="back-link" @click="clearSelection">
            <span class="arrow-back">←</span> Volver al Catálogo
          </button>

          <div class="detail-container glass" :class="{ 'has-calc': parseProductWeight(selectedProduct.nombre) }">
            <div class="detail-image">
              <img :src="getImageUrl(selectedProduct.imagenURL)" :alt="selectedProduct.nombre">
            </div>

            <div class="detail-info">
              <div class="detail-category-tag">{{ selectedProduct.categoria?.nombre || 'General' }}</div>
              <h1>{{ selectedProduct.nombre }}</h1>
              <div class="detail-price-big">{{ formatPrice(selectedProduct.precioVenta) }}</div>
              
              <div class="detail-description-section">
                <h4>Descripción del producto</h4>
                <p>{{ selectedProduct.descripcion || 'Garantizamos la mejor calidad y nutrición para tus engreídos con este producto seleccionado.' }}</p>
              </div>

              <div class="detail-species-list" v-if="selectedProduct.especies?.length">
                <h4>Recomendado para</h4>
                <div class="species-badges-grid">
                  <span v-for="esp in selectedProduct.especies" :key="esp.especieId" class="s-badge">
                    {{ esp.nombre }}
                  </span>
                </div>
              </div>

              <div class="detail-footer">
                <div class="qty-selector">
                  <button @click="qty > 1 ? qty-- : null" class="qty-btn">-</button>
                  <span class="qty-num">{{ qty }}</span>
                  <button @click="qty++" class="qty-btn">+</button>
                </div>

                <button @click="emit('add-to-cart', selectedProduct, qty); clearSelection()" class="btn-add-cart-large">
                  Añadir al Carrito
                </button>
                <p class="store-info">📦 Disponible en nuestro local de Aucayacu.</p>
              </div>
            </div>

            <!-- PANEL LATERAL DE CALCULADORA (MOVIDO A LA DERECHA) -->
            <div v-if="parseProductWeight(selectedProduct.nombre)" class="detail-sidebar-calc">
              <div class="nutrition-calculator glass">
                <div class="calc-header">
                  <span class="calc-icon">⚖️</span>
                  <h4>Tu Asistente Nutricional</h4>
                </div>
                
                <div class="calc-inputs">
                  <div class="input-group">
                    <label>Peso mascota: </label>
                    <div class="weight-manual-box">
                      <input type="number" v-model.number="petWeight" min="1" max="100" class="weight-num-input">
                      <span class="weight-unit">kg</span>
                    </div>
                    <input type="range" v-model.number="petWeight" min="1" max="100" step="1">
                  </div>
                  <div class="input-group">
                    <label>Actividad: </label>
                    <select v-model="activityLevel">
                      <option :value="1.0">Sedentario</option>
                      <option :value="1.2">Normal</option>
                      <option :value="1.5">Activo</option>
                    </select>
                  </div>
                </div>

                <div class="calc-results-sidebar">
                  <div class="res-card">
                    <span class="res-label">Dosis Diaria</span>
                    <span class="res-val">{{ calculatedRation }}g</span>
                  </div>
                  <div class="res-card highlight">
                    <span class="res-label">Duración Saco</span>
                    <span class="res-val">{{ durationInDays }} días</span>
                  </div>
                </div>
                <p class="calc-hint">Ahorra comprando sacos más grandes.</p>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  </section>
</template>

<style scoped>
.catalog-section {
  padding: 0;
}

.catalog-container {
  width: 100%;
  max-width: 1800px;
  margin: 0 auto;
  display: flex;
  gap: 3rem;
  padding: 0 1rem;
}

/* SIDEBAR */
.catalog-sidebar {
  width: 280px;
  flex-shrink: 0;
  position: sticky;
  top: 140px;
  max-height: calc(100vh - 160px);
  overflow-y: auto;
  padding-right: 1.5rem;
  scrollbar-width: thin;
  scrollbar-color: rgba(61, 30, 30, 0.2) transparent;
}

/* Custom scrollbar para navegadores Webkit (Chrome, Safari, Edge) */
.catalog-sidebar::-webkit-scrollbar {
  width: 4px;
}

.catalog-sidebar::-webkit-scrollbar-track {
  background: transparent;
}

.catalog-sidebar::-webkit-scrollbar-thumb {
  background: rgba(61, 30, 30, 0.1);
  border-radius: 10px;
}

.catalog-sidebar::-webkit-scrollbar-thumb:hover {
  background: rgba(61, 30, 30, 0.2);
}

.sidebar-block {
  margin-bottom: 2.5rem;
}

.sidebar-block h3 {
  font-size: 1.1rem;
  margin-bottom: 1.2rem;
  color: #333;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.search-bar {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  padding: 0.8rem 1.2rem;
  border-radius: 12px;
  background: white;
  border: 1px solid #eee;
}

.search-bar input {
  background: transparent;
  border: none;
  outline: none;
  width: 100%;
}

.species-list, .category-list {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.side-filter-btn, .category-filter-btn {
  background: transparent;
  border: 1px solid transparent;
  padding: 0.6rem 1rem;
  border-radius: 10px;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
  color: #666;
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.species-icon {
  width: 28px;
  height: 28px;
  object-fit: contain;
  border-radius: 6px;
  flex-shrink: 0;
}

.side-filter-btn:hover, .category-filter-btn:hover {
  background: rgba(61, 30, 30, 0.05);
  color: var(--primary);
}

.side-filter-btn.active, .category-filter-btn.active {
  background: var(--primary);
  color: white;
  box-shadow: 0 4px 15px rgba(61, 30, 30, 0.2);
}

.count, .close-icon {
  font-size: 0.8rem;
  opacity: 0.6;
}

.no-cats {
  font-size: 0.9rem;
  color: #999;
  padding: 1rem;
  font-style: italic;
}

/* MAIN AREA */
.catalog-main {
  flex: 1;
}

.main-header {
  margin-bottom: 2.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}

.main-header h2 {
  font-size: 2.5rem;
  margin: 0;
}

.main-header span {
  color: var(--primary);
}

.main-header p {
  margin: 0;
  color: #888;
  font-weight: 500;
}

/* Grid */
.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

/* States */
.loading-state, .empty-state {
  text-align: center;
  padding: 8rem 0;
}

.spinner {
  width: 45px;
  height: 45px;
  border: 4px solid #F3F3F3;
  border-top: 4px solid var(--primary);
  border-radius: 50%;
  margin: 0 auto 1.5rem;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Responsividad */
@media (max-width: 1024px) {
  .catalog-container {
    flex-direction: column;
    padding: 0 1.5rem;
  }
  
  .catalog-sidebar {
    width: 100%;
    position: static;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
  }
}

@media (max-width: 640px) {
  .catalog-sidebar {
    grid-template-columns: 1fr;
  }
}

/* PRODUCT DETAIL VIEW STYLES */
.product-detail-view {
  animation: fadeIn 0.4s ease;
}

.back-link {
  background: none;
  border: none;
  color: var(--primary);
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 2rem;
  font-size: 1rem;
  transition: transform 0.2s;
}

.back-link:hover {
  transform: translateX(-5px);
}

.detail-container {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 3rem;
  background: white;
  border-radius: 32px;
  padding: 3rem;
  box-shadow: 0 20px 60px rgba(61, 30, 30, 0.05);
  margin-bottom: 2rem;
}

.detail-container.has-calc {
  grid-template-columns: 0.9fr 1.1fr 1.3fr;
  gap: 1.5rem;
  max-width: 1800px;
  width: 100%;
  padding: 1.5rem 1rem;
  margin-left: auto;
  margin-right: auto;
}

.detail-image {
  background: #fdfdfd;
  border-radius: 24px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.detail-image img {
  max-width: 100%;
  height: auto;
  border-radius: 24px;
  transition: transform 0.5s ease;
}

.detail-image:hover img {
  transform: scale(1.05);
}

.detail-info {
  display: flex;
  flex-direction: column;
}

.detail-category-tag {
  background: var(--secondary);
  color: white;
  padding: 0.4rem 1.2rem;
  border-radius: 50px;
  font-size: 0.8rem;
  font-weight: 800;
  display: inline-block;
  width: fit-content;
  margin-bottom: 0.8rem;
}

.detail-info h1 {
  font-size: 2.2rem;
  color: var(--primary);
  margin-bottom: 0.5rem;
  line-height: 1.1;
}

.detail-price-big {
  font-size: 2.4rem;
  font-weight: 900;
  color: #261313;
  margin-bottom: 1rem;
}

.detail-description-section h4 {
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  font-size: 0.85rem;
  letter-spacing: 1px;
  color: var(--primary);
}

.detail-description-section p {
  color: var(--text-light);
  line-height: 1.5;
  margin-bottom: 1.5rem;
  font-size: 1rem;
}

.detail-species-list h4 {
  margin-bottom: 0.5rem;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 1px;
  color: var(--primary);
}

.species-badges-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 1.5rem;
}

.s-badge {
  background: #FEEED9;
  color: var(--primary);
  padding: 0.6rem 1.2rem;
  border-radius: 12px;
  font-weight: 800;
  font-size: 0.8rem;
  text-transform: capitalize;
}

/* Selector de Cantidad */
.qty-selector {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
  background: #f9f9f9;
  width: fit-content;
  padding: 0.5rem 1rem;
  border-radius: 50px;
  border: 1px solid #eee;
}

.qty-btn {
  background: white;
  border: 1px solid #ddd;
  width: 35px;
  height: 35px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 1.2rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.qty-btn:hover {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

.qty-num {
  font-size: 1.2rem;
  font-weight: 900;
  color: #261313;
  min-width: 20px;
  text-align: center;
}

/* Calculadora Nutricional Styles */
.nutrition-calculator {
  background: linear-gradient(135deg, #FEEED9 0%, #FFF8F0 100%);
  border-radius: 20px;
  padding: 1.5rem;
  margin-bottom: 2.5rem;
  border: 1px solid #FAD7A0;
}

.calc-header {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  margin-bottom: 1.5rem;
}

.calc-header h4 {
  margin: 0;
  color: var(--primary);
  font-size: 1rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.calc-inputs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.input-group label {
  display: block;
  font-size: 0.85rem;
  margin-bottom: 0.5rem;
  color: #666;
}

.weight-manual-box {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.8rem;
}

.weight-num-input {
  width: 70px;
  padding: 0.4rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-weight: 800;
  font-size: 1.1rem;
  color: var(--primary);
  text-align: center;
}

.weight-unit {
  font-weight: 700;
  color: #888;
}

.input-group input[type="range"] {
  width: 100%;
  accent-color: var(--primary);
}

.input-group select {
  width: 100%;
  padding: 0.5rem;
  border-radius: 8px;
  border: 1px solid #ddd;
  background: white;
}

.calc-results {
  background: white;
  border-radius: 12px;
  padding: 1rem;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.res-item {
  display: flex;
  flex-direction: column;
}

.res-item span {
  font-size: 0.75rem;
  color: #888;
}

.res-item strong {
  font-size: 1.2rem;
  color: var(--primary);
}

.res-item .highlight {
  color: #261313;
  font-weight: 900;
}

.calc-hint {
  font-size: 0.7rem;
  color: #aaa;
  margin-top: 1rem;
  text-align: center;
}

/* Sidebar Calc específica */
.detail-sidebar-calc {
  border-left: 1px solid #f0f0f0;
  padding-left: 2rem;
}

.calc-results-sidebar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.res-card {
  background: white;
  padding: 1rem;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.res-card.highlight {
  background: var(--primary);
  color: white;
}

.res-label {
  font-size: 0.7rem;
  text-transform: uppercase;
  margin-bottom: 0.3rem;
  opacity: 0.8;
}

.res-val {
  font-size: 1.5rem;
  font-weight: 900;
}

@media (max-width: 1200px) {
  .detail-container.has-calc {
    grid-template-columns: 1fr 1.2fr;
  }
  .detail-sidebar-calc {
    border-left: none;
    padding-left: 0;
    grid-column: span 2;
  }
}

@media (max-width: 768px) {
  .detail-container, .detail-container.has-calc {
    grid-template-columns: 1fr;
    padding: 2rem;
    gap: 2.5rem;
    border-radius: 20px;
  }
  .detail-sidebar-calc {
    grid-column: span 1;
  }
}

.btn-add-cart-large {
  display: block;
  width: 100%;
  background: var(--primary);
  color: white;
  border: none;
  padding: 1.4rem;
  border-radius: 50px;
  font-weight: 900;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 10px 30px rgba(61, 30, 30, 0.2);
}

.btn-add-cart-large:hover {
  transform: translateY(-5px);
  background: var(--secondary);
  box-shadow: 0 15px 40px rgba(61, 30, 30, 0.3);
}

.store-info {
  text-align: center;
  margin-top: 1.2rem;
  font-size: 0.85rem;
  color: #999;
}

@media (max-width: 768px) {
  .detail-container {
    grid-template-columns: 1fr;
    padding: 2rem;
    gap: 2.5rem;
    border-radius: 20px;
  }
  
  .detail-info h1 {
    font-size: 1.8rem;
  }
  
  .detail-price-big {
    font-size: 2rem;
  }
}
</style>
