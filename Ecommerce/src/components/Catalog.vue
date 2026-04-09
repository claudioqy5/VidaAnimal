<script setup>
import { ref, onMounted, computed } from 'vue'
import api from '../utils/api'
import ProductCard from './ProductCard.vue'

const productos = ref([])
const categorias = ref([])
const especies = ref([])
const loading = ref(true)

// Estado de filtros
const search = ref('')
const selectedCategory = ref(null)
const selectedSpecies = ref(null)

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

const toggleSpecies = (id) => {
  selectedSpecies.value = selectedSpecies.value === id ? null : id
}
</script>

<template>
  <section class="catalog-section">
    <!-- Header del Catálogo -->
    <div class="catalog-header fade-in">
      <div class="filters-container">
        <h2>Catálogo de <span>Productos</span></h2>
        
        <div class="search-bar glass">
          <span class="search-icon">🔍</span>
          <input type="text" v-model="search" placeholder="¿Qué estás buscando?">
        </div>
      </div>

      <!-- Filtros por Especie (Pestañas Premium) -->
      <div class="species-filters fade-in" style="animation-delay: 0.1s">
        <button 
          class="filter-tab" 
          :class="{ active: !selectedSpecies }"
          @click="selectedSpecies = null"
        >
          Todos
        </button>
        <button 
          v-for="esp in especies" 
          :key="esp.especieId"
          class="filter-tab"
          :class="{ active: selectedSpecies === esp.especieId }"
          @click="toggleSpecies(esp.especieId)"
        >
          {{ esp.nombre }}
        </button>
      </div>
    </div>

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
       <h3>No encontramos productos</h3>
       <p>Prueba ajustando tus filtros o búsqueda.</p>
    </div>
  </section>
</template>

<style scoped>
.catalog-section {
  padding: 4rem 0;
}

.catalog-header {
  margin-bottom: 3rem;
}

.filters-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 2rem;
  margin-bottom: 2rem;
}

.filters-container h2 {
  font-size: 2.2rem;
}

.filters-container h2 span {
  color: var(--primary);
}

.search-bar {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  padding: 0.8rem 1.5rem;
  border-radius: 50px;
  min-width: 350px;
}

.search-bar input {
  background: transparent;
  border: none;
  outline: none;
  width: 100%;
  font-family: inherit;
  font-size: 1rem;
}

.species-filters {
  display: flex;
  gap: 1rem;
  overflow-x: auto;
  padding: 0.5rem 0 1.5rem;
  scrollbar-width: none;
}

.species-filters::-webkit-scrollbar {
  display: none;
}

.filter-tab {
  background: white;
  border: 1px solid #eee;
  padding: 0.7rem 1.8rem;
  border-radius: 50px;
  cursor: pointer;
  white-space: nowrap;
  font-weight: 500;
  transition: all 0.3s;
  color: var(--text-light);
}

.filter-tab:hover {
  border-color: var(--primary);
  color: var(--primary);
}

.filter-tab.active {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
  box-shadow: 0 8px 20px rgba(45, 90, 39, 0.2);
}

/* Grid */
.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2.5rem;
}

/* States */
.loading-state, .empty-state {
  text-align: center;
  padding: 5rem 0;
}

.spinner {
  width: 40px;
  height: 40px;
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

.empty-icon {
  font-size: 4rem;
  display: block;
  margin-bottom: 1.5rem;
}

@media (max-width: 768px) {
  .search-bar {
    min-width: 100%;
  }
  .filters-container {
    text-align: center;
    justify-content: center;
  }
}
</style>
