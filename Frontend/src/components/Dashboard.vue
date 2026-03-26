<template>
  <div class="dashboard fade-in">
    <header class="header flex justify-between items-center mb-6">
      <div>
        <h1 class="text-2xl font-bold">Resumen Diario</h1>
        <p class="text-muted">¡Hola! Aquí tienes el estado de tu tienda "Vida Animal".</p>
      </div>
      <div class="date-badge glass">
        📅 {{ new Date().toLocaleDateString('es-ES', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}
      </div>
    </header>

    <div class="stats-grid mb-6">
      <div class="stat-card" v-for="stat in stats" :key="stat.title">
        <div class="stat-icon" :style="{ background: stat.color }">{{ stat.icon }}</div>
        <div class="stat-content">
          <p class="stat-title">{{ stat.title }}</p>
          <h2 class="stat-value">{{ stat.value }}</h2>
        </div>
      </div>
    </div>

    <div class="grid-cols-2">
      <div class="card">
        <h3 class="font-bold mb-4">Productos con Stock Bajo ⚠️</h3>
        <ul class="flex-col gap-2">
          <li v-for="prod in lowStock" :key="prod.id" class="stock-item flex justify-between items-center">
            <div class="flex items-center gap-2">
              <span class="product-icon">📦</span>
              <span>{{ prod.name }}</span>
            </div>
            <span class="stock-badge">{{ prod.stock }} {{ prod.unit }} left</span>
          </li>
        </ul>
      </div>
      <div class="card">
        <h3 class="font-bold mb-4">Últimas Ventas ⭐</h3>
        <ul class="flex-col gap-2">
          <li v-for="sale in recentSales" :key="sale.id" class="sale-item flex justify-between items-center">
            <div class="flex flex-col">
              <span class="font-semibold">{{ sale.ticket }}</span>
              <span class="text-sm text-muted">{{ sale.time }} - {{ sale.user }}</span>
            </div>
            <span class="font-bold amount">${{ sale.amount.toFixed(2) }}</span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
const stats = [
  { title: 'Ventas de Hoy', value: '$1,250.00', icon: '💰', color: 'rgba(16, 185, 129, 0.15)' },
  { title: 'Tickets', value: '34', icon: '📝', color: 'rgba(59, 130, 246, 0.15)' },
  { title: 'Nuevos Productos', value: '8', icon: '🐕', color: 'rgba(245, 158, 11, 0.15)' },
  { title: 'Alertas Inventario', value: '3', icon: '⚠️', color: 'rgba(239, 68, 68, 0.15)' }
]

const lowStock = [
  { id: 1, name: 'Croquetas Cachorro Premium', stock: 2.5, unit: 'KG' },
  { id: 2, name: 'Collar Azul Mediano', stock: 1, unit: 'UNIDAD' },
  { id: 3, name: 'Patitas de Pollo Deshidratadas', stock: 0.5, unit: 'KG' }
]

const recentSales = [
  { id: 1, ticket: 'TKT-0042', time: '13:02', amount: 85.50, user: 'Carlos' },
  { id: 2, ticket: 'TKT-0041', time: '12:45', amount: 210.00, user: 'Ana' },
  { id: 3, ticket: 'TKT-0040', time: '11:30', amount: 45.00, user: 'Carlos' }
]
</script>

<style scoped>
.fade-in { animation: fadeIn 0.4s ease; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

.date-badge {
  padding: 0.5rem 1rem;
  border-radius: var(--radius-full);
  font-weight: 500;
  font-size: 0.875rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.5rem;
}
.stat-card {
  background: var(--surface);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1.25rem;
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border);
  transition: transform 0.2s;
}
.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}
.stat-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}
.stat-title {
  color: var(--text-muted);
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 0.25rem;
}
.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
}

.stock-item {
  padding: 0.75rem;
  background: var(--bg-color);
  border-radius: var(--radius-md);
}
.stock-badge {
  background-color: rgba(239, 68, 68, 0.1);
  color: var(--danger);
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-lg);
  font-size: 0.75rem;
  font-weight: 600;
}
.product-icon {
  width: 32px;
  height: 32px;
  background: white;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-sm);
}
.sale-item {
  padding: 0.75rem;
  border-bottom: 1px solid var(--border);
}
.sale-item:last-child {
  border-bottom: none;
}
.amount {
  color: var(--primary);
}
</style>
