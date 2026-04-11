<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import api from '../utils/api'
import ProductCard from './ProductCard.vue'

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

// Sincronizar filtros externos del Header con el Catálogo
watch(() => [props.speciesFilter, props.categoryFilter], ([newSpec, newCat]) => {
  if (newSpec) {
    const found = especies.value.find(e => e.nombre.toLowerCase().includes(newSpec.toLowerCase()))
    selectedSpecies.value = found ? found.especieId : null
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
      const found = especies.value.find(e => e.nombre.toLowerCase().includes(props.speciesFilter.toLowerCase()))
      selectedSpecies.value = found ? found.especieId : null
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
    
    const matchesSpecies = !selectedSpecies.value || 
                           p.especies?.some(e => e.especieId === selectedSpecies.value)
                           
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
              🐾 Todas las Mascotas
            </button>
            <button 
              v-for="esp in especies" 
              :key="esp.especieId"
              class="side-filter-btn"
              :class="{ active: selectedSpecies === esp.especieId }"
              @click="toggleSpecies(esp.especieId)"
            >
              {{ esp.nombre === 'Perro' ? '🐶' : esp.nombre === 'Gato' ? '🐱' : esp.nombre === 'Pollo' ? '🐥' : '🐾' }} {{ esp.nombre }}
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
        <header class="main-header">
          <h2>Resultados <span>{{ selectedSpecies ? 'para ' + especies.find(e => e.especieId === selectedSpecies)?.nombre : '' }}</span></h2>
          <p v-if="filteredProducts.length">{{ filteredProducts.length }} productos</p>
        </header>

        <!-- Grid de Productos -->
        <div v-if="loading" class="loading-state">
           <div class="spinner"></div>
           <p>Cargando lo mejor para tu mascota...</p>
        </div>

        <div v-else-if="filteredProducts.length" class="products-grid">
          <ProductCard 
            v-for="prod in filteredProducts" 
            :key="prod.productoID" 
            :producto="prod"
          />
        </div>

        <div v-else class="empty-state">
           <span class="empty-icon">🏜️</span>
           <h3>Sin resultados</h3>
           <p>Prueba con otros filtros o nombres.</p>
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
  display: flex;
  gap: 1.5rem;
  max-width: 100%;
  margin: 0 auto;
}

/* SIDEBAR */
.catalog-sidebar {
  width: 280px;
  flex-shrink: 0;
  position: sticky;
  top: 140px;
  height: fit-content;
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
  padding: 0.7rem 1.2rem;
  border-radius: 10px;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
  color: #666;
  display: flex;
  justify-content: space-between;
  align-items: center;
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
</style>
