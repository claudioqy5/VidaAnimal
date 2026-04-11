<template>
  <section class="features-section fade-in">
    <div class="container features-grid">
      
      <!-- CARD 1: HORARIO -->
      <div class="feature-card glass">
        <div class="icon-wrapper">
          <span class="feature-icon">🕒</span>
        </div>
        <h3>Horario de Atención</h3>
        <p>Te esperamos todos los días de <strong>7:00 AM a 9:30 PM</strong> en nuestro local y las <strong>24 horas del dia los 7 dias de la semana</strong> online para atender a tus engreídos con amor.</p>
      </div>

      <!-- CARD 2: PRODUCTOS -->
      <div class="feature-card glass">
        <div class="icon-wrapper">
          <span class="feature-icon">🐾</span>
        </div>
        <h3>Calidad Garantizada</h3>
        <p>Vendemos <strong>alimentos y productos premium</strong> seleccionados para el bienestar de tus animalitos.</p>
      </div>

      <!-- CARD 3: UBICACIÓN -->
      <div class="feature-card glass">
        <div class="icon-wrapper">
          <span class="feature-icon">📍</span>
        </div>
        <h3>¿Donde estamos?</h3>
        <p>Visítanos en nuestro local central en Jiron Atahualpa N° 291 -<strong>Aucayacu</strong>. <br>Referencia: frente al mercado de Aucayacu<br>¡Estamos muy cerca de ti!</p>
      </div>

    </div>
  </section>
</template>

<style scoped>
.features-section {
  position: relative;
  z-index: 10;
  margin-top: 4rem; /* Separación limpia debajo del hero */
  margin-bottom: 5rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
}

.feature-card {
  background: white;
  padding: 3.5rem 2.5rem;
  border-radius: 32px;
  text-align: center;
  box-shadow: 0 20px 60px rgba(61, 30, 30, 0.08);
  transition: all 0.4s ease;
  border: 1px solid rgba(0,0,0,0.03);
}

.feature-card:hover {
  transform: translateY(-10px);
  box-shadow: 0 30px 80px rgba(61, 30, 30, 0.12);
}

.icon-wrapper {
  width: 90px;
  height: 90px;
  background: var(--secondary);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 2rem;
  opacity: 0.9;
}

.feature-icon {
  font-size: 2.5rem;
}

h3 {
  font-size: 1.4rem;
  color: var(--primary);
  margin-bottom: 1.2rem;
  font-weight: 800;
}

p {
  color: var(--text-light);
  line-height: 1.7;
  font-size: 1.05rem;
}

p strong {
  color: var(--text-dark);
}

/* Responsividad */
@media (max-width: 992px) {
  .features-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
  .features-section {    
    padding: 0 1.5rem;
  }
}
</style>
