<template>
  <section id="sobre-nosotros" class="about-section fade-in">
    <div class="container about-container glass">
      <div class="about-content">
        <span class="about-tag">Nuestra Misión</span>
        <h2>Cuidamos a los que más amas</h2>
        <p class="about-description">
          En <strong>Vida Animal</strong>, no solo vendemos productos; compartimos el mismo amor que sientes por tus mascotas. 
          Nacimos con un propósito claro: transformar y profesionalizar el cuidado animal en <strong>Aucayacu</strong>, la selva central y todo el Perú.
        </p>
        <p class="about-sub">
          Seleccionamos cada alimento y accesorio como si fuera para nuestros propios compañeros, garantizando calidad, salud y felicidad en cada hogar.
        </p>
        
        <div class="values-row">
          <div class="value-item">
            <span class="value-icon">❤️</span>
            <span>Pasión Real</span>
          </div>
          <div class="value-item">
            <span class="value-icon">🏅</span>
            <span>Calidad Premium</span>
          </div>
          <div class="value-item">
            <span class="value-icon">📍</span>
            <span>Espíritu Local</span>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.about-section {
  padding: 4rem 0 8rem;
  background: transparent;
}

.about-container {
  max-width: 900px !important;
  margin: 0 auto;
  padding: 5rem 4rem;
  background: white;
  border-radius: 40px;
  text-align: center;
  box-shadow: 0 40px 100px rgba(61, 30, 30, 0.08);
  border: 1px solid rgba(0,0,0,0.02);
}

.about-tag {
  display: inline-block;
  background: var(--secondary);
  color: white;
  padding: 0.5rem 1.5rem;
  border-radius: 50px;
  font-weight: 800;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 2px;
  margin-bottom: 2rem;
}

h2 {
  font-size: 2.8rem;
  color: var(--primary);
  margin-bottom: 1.5rem;
  font-weight: 900;
}

.about-description {
  font-size: 1.3rem;
  color: var(--text-dark);
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.about-sub {
  font-size: 1.1rem;
  color: var(--text-light);
  line-height: 1.7;
  max-width: 700px;
  margin: 0 auto 3rem;
}

.values-row {
  display: flex;
  justify-content: center;
  gap: 3rem;
  border-top: 1px solid rgba(0,0,0,0.05);
  padding-top: 3rem;
}

.value-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.8rem;
  font-weight: 800;
  color: var(--primary);
  font-size: 1rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.value-icon {
  font-size: 2rem;
}

@media (max-width: 768px) {
  .about-container {
    padding: 3rem 2rem;
    border-radius: 24px;
  }
  h2 { font-size: 2rem; }
  .values-row {
    flex-direction: column;
    gap: 2rem;
  }
}
</style>
