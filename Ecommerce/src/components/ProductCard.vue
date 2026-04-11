<script setup>
const props = defineProps({
  producto: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['select'])

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
  if (!url) return 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?q=80&w=500&auto=format&fit=crop'
  // Si la URL es relativa, le pegamos el dominio del VPS con todo y /api
  if (url.startsWith('/')) {
     return `https://vidaanimal.helifyferdigital.cloud/api${url}`
  }
  return url
}
</script>

<template>
  <div class="product-card glass fade-in" @click="$emit('select', props.producto)" style="cursor: pointer;">
    <div class="image-container">
      <img 
        :src="getImageUrl(props.producto.imagenURL)" 
        :alt="props.producto.nombre"
        loading="lazy"
      >
      <div v-if="props.producto.especies && props.producto.especies.length" class="species-badges">
        <span v-for="esp in props.producto.especies" :key="esp.especieID" class="species-badge">
           {{ esp.nombre }}
        </span>
      </div>
    </div>
    
    <div class="product-info">
      <div class="category-tag">{{ props.producto.categoria?.nombre || 'General' }}</div>
      <h3>{{ props.producto.nombre }}</h3>
      <p class="description">{{ props.producto.descripcion || 'Sin descripción disponible' }}</p>
      
      <div class="card-footer">
        <span class="price">{{ formatPrice(props.producto.precioVenta) }}</span>
        <a :href="getWhatsAppUrl(props.producto)" target="_blank" class="buy-btn" @click.stop>
          Comprar
        </a>
      </div>
    </div>
  </div>
</template>

<style scoped>
.product-card {
  border-radius: 24px;
  overflow: hidden;
  transition: transform 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  display: flex;
  flex-direction: column;
  height: 100%;
}

.product-card:hover {
  transform: translateY(-10px);
}

.image-container {
  height: 180px;
  position: relative;
  overflow: hidden;
}

.image-container img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s;
}

.product-card:hover .image-container img {
  transform: scale(1.1);
}

.species-badges {
  position: absolute;
  bottom: 1rem;
  left: 1rem;
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.species-badge {
  background: var(--primary);
  color: white;
  padding: 0.3rem 0.8rem;
  border-radius: 50px;
  font-size: 0.7rem;
  font-weight: 600;
  backdrop-filter: blur(4px);
}

.product-info {
  padding: 1.2rem;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
}

.category-tag {
  font-size: 0.75rem;
  color: var(--secondary);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 0.5rem;
}

.product-info h3 {
  font-size: 1rem;
  margin-bottom: 0.5rem;
  color: var(--text-dark);
}

.description {
  font-size: 0.9rem;
  color: var(--text-light);
  margin-bottom: 1rem;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-footer {
  margin-top: auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.price {
  font-size: 1.2rem;
  font-weight: 700;
  color: var(--primary);
}

.buy-btn {
  background: var(--primary);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
  transition: all 0.3s;
}

.buy-btn:hover {
  background: #2a1515;
  transform: translateY(-2px);
}
</style>
