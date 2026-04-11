<template>
  <div class="cart-overlay" :class="{ active: isOpen }" @click="$emit('close')">
    <div class="cart-drawer" :class="{ active: isOpen }" @click.stop>
      <!-- Header -->
      <div class="cart-header">
        <h2>Tu Carrito <span>({{ totalItems }})</span></h2>
        <button class="close-drawer" @click="$emit('close')">✕</button>
      </div>

      <!-- Items List -->
      <div class="cart-body">
        <div v-if="items.length === 0" class="empty-cart">
          <span class="empty-icon">🛒</span>
          <p>Tu carrito está vacío.</p>
          <button @click="$emit('close')" class="btn-start-shopping">Empezar a comprar</button>
        </div>

        <div v-else class="cart-items">
          <div v-for="item in items" :key="item.productoId" class="cart-item glass">
            <img :src="getImageUrl(item.imagen)" :alt="item.nombre" class="item-img">
            <div class="item-details">
              <h4>{{ item.nombre }}</h4>
              <p class="item-price">{{ formatPrice(item.precio) }}</p>
              
              <div class="item-actions">
                <div class="qty-control">
                  <button @click="$emit('update-qty', item.productoId, item.quantity - 1)">-</button>
                  <span>{{ item.quantity }}</span>
                  <button @click="$emit('update-qty', item.productoId, item.quantity + 1)">+</button>
                </div>
                <button class="remove-btn" @click="$emit('remove', item.productoId)">Eliminar</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div v-if="items.length > 0" class="cart-footer">
        <div class="checkout-details glass">
          <div class="pickup-info">
            <label>📅 ¿Cuándo pasarás por tu pedido?</label>
            <input type="date" v-model="pickupDate" :min="minDate" class="date-input">
            <p class="pickup-address">📍 Recojo en: <b>Local Aucayacu</b></p>
          </div>

          <div class="reservation-box">
            <div class="res-header">
              <span class="yape-icon">✨</span>
              <span>Separa tu pedido con el 20%</span>
            </div>
            <div class="res-row">
              <span>Adelanto x Yape:</span>
              <strong>{{ formatPrice(reservationAmount) }}</strong>
            </div>
            <div class="res-row">
              <span>Saldo a pagar en local:</span>
              <span style="font-size: 0.9rem; font-weight: 600;">{{ formatPrice(pendingAmount) }}</span>
            </div>
            <p class="yape-hint">Recuerda enviar el comprobante por este chat.</p>
          </div>
        </div>

        <div class="total-row">
          <span>Total del pedido</span>
          <span class="total-price">{{ formatPrice(totalPrice) }}</span>
        </div>
        <button @click="sendWhatsApp" class="btn-confirm-order">
          Confirmar Pedido por WhatsApp
        </button>
        <p class="footer-hint">Te redirigiremos a WhatsApp para finalizar.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  isOpen: Boolean,
  items: Array
})

const emit = defineEmits(['close', 'remove', 'update-qty'])

const totalItems = computed(() => props.items.reduce((acc, item) => acc + item.quantity, 0))
const totalPrice = computed(() => props.items.reduce((acc, item) => acc + (item.precio * item.quantity), 0))
const reservationAmount = computed(() => totalPrice.value * 0.20)
const pendingAmount = computed(() => totalPrice.value - reservationAmount.value)

// Fecha mínima: mañana
const tomorrow = new Date()
tomorrow.setDate(tomorrow.getDate() + 1)
const minDate = tomorrow.toISOString().split('T')[0]
const pickupDate = ref(minDate)

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const [y, m, d] = dateStr.split('-')
  return `${d}/${m}/${y}`
}

const formatPrice = (price) => {
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: 'PEN'
  }).format(price)
}

const getImageUrl = (url) => {
  if (!url) return 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?q=80&w=200'
  if (url.startsWith('/')) return `https://vidaanimal.helifyferdigital.cloud/api${url}`
  return url
}

const sendWhatsApp = () => {
  const phone = "+51975418965"
  let message = "¡Hola Vida Animal! 👋 Quiero realizar el siguiente pedido:\n\n"
  
  props.items.forEach(item => {
    message += `🔹 ${item.quantity}x ${item.nombre} - ${formatPrice(item.precio * item.quantity)}\n`
  })
  
  message += `\n💰 *DETALLES DEL PAGO:*`
  message += `\n-----------------------`
  message += `\n*TOTAL DEL PEDIDO:* ${formatPrice(totalPrice.value)}`
  message += `\n*ADELANTO (20%) x Yape:* ${formatPrice(reservationAmount.value)}`
  message += `\n*SALDO PENDIENTE:* ${formatPrice(pendingAmount.value)}`
  message += `\n-----------------------`
  message += `\n\n🗓️ *Fecha de recojo:* ${formatDate(pickupDate.value)}`
  message += "\n\n¿Tienen todo en stock para enviarles la captura del Yape? 👋"

  const url = `https://wa.me/${phone}?text=${encodeURIComponent(message)}`
  window.open(url, '_blank')
}
</script>

<style scoped>
.cart-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
  backdrop-filter: blur(4px);
  z-index: 10000;
  opacity: 0;
  visibility: hidden;
  transition: all 0.4s;
}

.cart-overlay.active {
  opacity: 1;
  visibility: visible;
}

.cart-drawer {
  position: absolute;
  top: 0;
  right: -400px;
  width: 400px;
  height: 100%;
  background: white;
  box-shadow: -20px 0 60px rgba(0,0,0,0.2);
  display: flex;
  flex-direction: column;
  transition: right 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.cart-drawer.active {
  right: 0;
}

.cart-header {
  padding: 2rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.cart-header h2 {
  font-size: 1.5rem;
  color: var(--primary);
  font-weight: 800;
}

.cart-header h2 span {
  font-weight: 400;
  color: #888;
}

.close-drawer {
  background: none;
  border: none;
  font-size: 1.8rem;
  cursor: pointer;
  color: #ccc;
  transition: color 0.3s;
}

.close-drawer:hover {
  color: var(--primary);
}

.cart-body {
  flex: 1;
  padding: 2rem;
  overflow-y: auto;
}

.empty-cart {
  text-align: center;
  margin-top: 5rem;
}

.empty-icon {
  font-size: 4rem;
  display: block;
  margin-bottom: 1rem;
  opacity: 0.3;
}

.cart-items {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.cart-item {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  border-radius: 16px;
  border: 1px solid #eee;
}

.item-img {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 12px;
}

.item-details {
  flex: 1;
}

.item-details h4 {
  font-size: 0.95rem;
  margin-bottom: 0.3rem;
  color: var(--text-dark);
}

.item-price {
  font-weight: 800;
  color: var(--primary);
  margin-bottom: 0.8rem;
}

.item-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.qty-control {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  background: #f5f5f5;
  padding: 0.3rem 0.6rem;
  border-radius: 50px;
}

.qty-control button {
  background: white;
  border: 1px solid #ddd;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  cursor: pointer;
  font-weight: 700;
}

.remove-btn {
  background: none;
  border: none;
  color: #ff4444;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
}

.cart-footer {
  padding: 1.5rem;
  border-top: 1px solid #eee;
  background: #fdfdfd;
}

.checkout-details {
  background: #fff;
  padding: 1rem;
  border-radius: 16px;
  margin-bottom: 1.5rem;
  border: 1px solid #f0f0f0;
}

.pickup-info {
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px dashed #eee;
}

.pickup-info label {
  display: block;
  font-size: 0.85rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  color: var(--text-dark);
}

.date-input {
  width: 100%;
  padding: 0.6rem;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-family: inherit;
  margin-bottom: 0.5rem;
}

.pickup-address {
  font-size: 0.8rem;
  color: #666;
}

.reservation-box {
  background: #fdf8f0;
  padding: 0.8rem;
  border-radius: 12px;
}

.res-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.8rem;
  font-weight: 800;
  color: #b37400;
  margin-bottom: 0.5rem;
}

.res-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.3rem;
}

.res-row span {
  font-size: 0.85rem;
  color: #666;
}

.res-row strong {
  color: #261313;
  font-size: 1.1rem;
}

.yape-hint {
  font-size: 0.7rem;
  color: #999;
}

.total-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  font-size: 1.1rem;
  font-weight: 700;
}

.total-price {
  font-size: 1.5rem;
  color: #261313;
}

.btn-confirm-order {
  width: 100%;
  background: var(--whatsapp);
  color: white;
  border: none;
  padding: 1.2rem;
  border-radius: 50px;
  font-weight: 900;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 10px 25px rgba(37, 211, 102, 0.2);
}

.btn-confirm-order:hover {
  transform: translateY(-3px);
  background: #1e3d1a;
}

.footer-hint {
  text-align: center;
  font-size: 0.8rem;
  color: #888;
  margin-top: 1rem;
}

@media (max-width: 450px) {
  .cart-drawer {
    width: 100%;
    right: -100%;
  }
}
</style>
