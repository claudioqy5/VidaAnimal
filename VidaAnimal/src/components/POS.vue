<template>
  <div class="pos-container fade-in">
    <!-- Products Section -->
    <div class="products-section">
      <header class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold">Punto de Venta</h2>
        <div class="search-bar">
          <input type="text" placeholder="🔍 Buscar producto o código..." v-model="search" class="w-full" />
        </div>
      </header>
      
      <!-- Category filters -->
      <div class="categories flex gap-2 mb-6">
        <button 
          v-for="cat in categories" 
          :key="cat"
          :class="['btn', selectedCategory === cat ? 'btn-primary' : 'btn-outline']"
          @click="selectedCategory = cat"
        >
          {{ cat }}
        </button>
      </div>

      <!-- Products Grid -->
      <div class="products-grid">
        <div 
          class="product-card" 
          v-for="prod in filteredProducts" 
          :key="prod.id"
          @click="addToCart(prod)"
        >
          <div class="prod-image glass">{{ prod.icon }}</div>
          <div class="prod-info mt-2">
            <h4 class="font-bold text-sm">{{ prod.name }}</h4>
            <p class="text-muted text-xs">{{ prod.unit }}</p>
            <p class="price font-bold mt-1">${{ prod.price.toFixed(2) }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Cart Section -->
    <div class="cart-section card flex-col">
      <h3 class="font-bold text-xl mb-4">Ticket de Venta</h3>
      
      <div class="cart-items flex-1">
        <p v-if="cart.length === 0" class="text-muted text-center mt-10">
          🛒 El carrito está vacío
        </p>
        <div v-for="(item, index) in cart" :key="index" class="cart-item flex justify-between items-center mb-4">
          <div class="item-details">
            <p class="font-semibold text-sm">{{ item.product.name }}</p>
            <div class="qty-control flex items-center gap-2 mt-1">
              <button class="qty-btn glass" @click="updateQty(index, -1)">-</button>
              <span class="text-sm font-bold w-8 text-center">{{ item.qty }} {{ item.product.unit === 'KG' ? 'kg' : '' }}</span>
              <button class="qty-btn glass" @click="updateQty(index, 1)">+</button>
            </div>
          </div>
          <div class="item-price text-right">
            <p class="font-bold">${{ (item.product.price * item.qty).toFixed(2) }}</p>
            <button class="text-danger text-xs mt-1" @click="cart.splice(index, 1)">Eliminar</button>
          </div>
        </div>
      </div>

      <div class="cart-summary mt-4 pt-4 border-t">
        <div class="flex justify-between mb-2 text-muted">
          <span>Subtotal</span>
          <span>${{ subtotal.toFixed(2) }}</span>
        </div>
        <div class="flex justify-between mb-2 text-muted">
          <span>Impuestos (16%)</span>
          <span>${{ tax.toFixed(2) }}</span>
        </div>
        <div class="flex justify-between mb-6 text-xl font-bold">
          <span>Total</span>
          <span class="text-primary">${{ total.toFixed(2) }}</span>
        </div>
        <button class="btn btn-primary w-full text-lg py-3" :disabled="cart.length === 0">
          💳 Cobrar ${{ total.toFixed(2) }}
        </button>
        <button class="btn btn-outline w-full text-danger mt-2" @click="cart = []" v-if="cart.length > 0">
          🗑️ Cancelar Venta
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const search = ref('')
const selectedCategory = ref('Todos')
const categories = ['Todos', 'Alimentos', 'Accesorios', 'Premios', 'Farmacia']

const products = [
  { id: 1, name: 'Croquetas Perro Adulto 1kg', price: 45.00, unit: 'KG', category: 'Alimentos', icon: '🐕' },
  { id: 2, name: 'Patitas de Pollo Deshidratadas', price: 12.50, unit: 'UNIDAD', category: 'Premios', icon: '🍗' },
  { id: 3, name: 'Collar Azul Mediano', price: 120.00, unit: 'UNIDAD', category: 'Accesorios', icon: '📿' },
  { id: 4, name: 'Cama para Perro Grande', price: 450.00, unit: 'UNIDAD', category: 'Accesorios', icon: '🛏️' },
  { id: 5, name: 'Alimento Premium Gato', price: 55.00, unit: 'KG', category: 'Alimentos', icon: '🐈' },
  { id: 6, name: 'Desparasitante Puppy', price: 85.00, unit: 'UNIDAD', category: 'Farmacia', icon: '💊' },
  { id: 7, name: 'Comedero Automático', price: 299.00, unit: 'UNIDAD', category: 'Accesorios', icon: '🍽️' },
  { id: 8, name: 'Carnaza Hueca 15cm', price: 25.00, unit: 'UNIDAD', category: 'Premios', icon: '🦴' },
]

const filteredProducts = computed(() => {
  return products.filter(p => {
    const matchCat = selectedCategory.value === 'Todos' || p.category === selectedCategory.value
    const matchSearch = p.name.toLowerCase().includes(search.value.toLowerCase())
    return matchCat && matchSearch
  })
})

const cart = ref([])

const addToCart = (product) => {
  const existing = cart.value.find(item => item.product.id === product.id)
  if (existing) {
    existing.qty += product.unit === 'KG' ? 0.5 : 1
  } else {
    cart.value.push({
      product,
      qty: product.unit === 'KG' ? 1.0 : 1
    })
  }
}

const updateQty = (index, delta) => {
  const item = cart.value[index]
  const step = item.product.unit === 'KG' ? 0.25 : 1
  if (item.qty + (delta * step) > 0) {
    item.qty += (delta * step)
  }
}

const subtotal = computed(() => cart.value.reduce((sum, item) => sum + (item.product.price * item.qty), 0))
const tax = computed(() => subtotal.value * 0.16)
const total = computed(() => subtotal.value + tax.value)
</script>

<style scoped>
.pos-container {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 1.5rem;
  height: calc(100vh - 3rem); /* Full height minus layout padding */
}

.search-bar input {
  width: 300px;
  background-color: var(--surface);
}

.categories::-webkit-scrollbar {
  display: none;
}
.categories {
  overflow-x: auto;
  padding-bottom: 0.5rem;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 1rem;
  overflow-y: auto;
  padding-right: 0.5rem;
  max-height: calc(100vh - 12rem);
}

.product-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
}
.product-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
  border-color: var(--primary);
}
.prod-image {
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
  border-radius: var(--radius-md);
  background: var(--bg-color);
  margin-bottom: 0.5rem;
}
.price {
  color: var(--primary);
}

.cart-section {
  position: sticky;
  top: 0;
  height: 100%;
}
.cart-items {
  overflow-y: auto;
  padding-right: 0.5rem;
}
.cart-item {
  background: var(--bg-color);
  padding: 0.75rem;
  border-radius: var(--radius-md);
}
.qty-btn {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-md);
  color: var(--text-main);
  background: var(--surface);
  border: 1px solid var(--border);
}
.qty-btn:hover {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}
.border-t {
  border-top: 1px solid var(--border);
}
.text-primary {
  color: var(--primary);
}
</style>
