<template>
  <section id="ubicacion-central" class="map-section fade-in">
    <div class="container map-header">
      <h2>Encuéntranos en Aucayacu</h2>
      <p>Visítanos en nuestro local principal y recibe la mejor atención para tus mascotas.</p>
      <div class="header-line"></div>
    </div>

    <div class="container map-container glass">
      <iframe 
        src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d492.6786934144267!2d-76.11899362410372!3d-8.932505323029968!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x91a60b0001d313a3%3A0x7bef527addb7a4a0!2sVida%20animal!5e0!3m2!1ses-419!2spe!4v1775878173277!5m2!1ses-419!2spe" 
        width="100%" 
        height="450" 
        style="border:0;" 
        allowfullscreen="" 
        loading="lazy" 
        referrerpolicy="no-referrer-when-downgrade">
      </iframe>
    </div>
  </section>
</template>

<style scoped>
.map-section {
  padding: 6rem 0;
  background: transparent;
}

.map-header {
  text-align: center;
  margin-bottom: 3.5rem;
}

.map-header h2 {
  font-size: 2rem;
  color: var(--primary);
  margin-bottom: 1rem;
  font-weight: 800;
}

.map-header p {
  color: var(--text-light);
  font-size: 1.1rem;
}

.header-line {
  width: 80px;
  height: 4px;
  background: var(--secondary);
  margin: 1.5rem auto;
  border-radius: 10px;
}

.map-container {
  overflow: hidden;
  border-radius: 32px;
  padding: 10px;
  background: white;
  box-shadow: 0 30px 100px rgba(61, 30, 30, 0.1);
}

.map-container iframe {
  border-radius: 24px;
  display: block;
}

@media (max-width: 768px) {
  .map-header h2 {
    font-size: 1.6rem;
  }
  .map-container {
    border-radius: 20px;
  }
}
</style>
