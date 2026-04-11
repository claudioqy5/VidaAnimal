<template>
  <div class="whatsapp-float">
    <!-- Burbuja de Mensaje -->
    <div class="whatsapp-tooltip glass">
      <p><strong>¿No encuentras lo que buscas?</strong><br>Escríbenos y te ayudamos al instante.</p>
    </div>

    <!-- Botón Flotante -->
    <a 
      href="https://wa.me/51975418965?text=Hola%20Vida%20Animal!%20%F0%9F%91%8B%20No%20encuentro%20el%20producto%20que%20busco,%20%C2%BFme%20podr%C3%ADan%20ayudar?" 
      target="_blank" 
      class="whatsapp-btn pulse"
      aria-label="Contactar por WhatsApp"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" width="30" height="30" fill="white">
        <path d="M380.9 97.1C339 55.1 283.2 32 223.9 32c-122.4 0-222 99.6-222 222 0 39.1 10.2 77.3 29.6 111L0 480l117.7-30.9c32.7 17.7 69.4 26.9 106.2 26.9 122.4 0 222-99.6 222-222 0-59.3-23.1-115.1-65.1-157.1zM223.9 446.7c-33.1 0-65.7-8.9-94.1-25.7l-6.7-4-69.9 18.3 18.7-68.1-4.4-7c-18.5-29.4-28.2-63.3-28.2-98.2 0-101.7 82.8-184.5 184.5-184.5 49.3 0 95.6 19.2 130.4 54.1 34.8 34.9 54 81.2 54.1 130.5 0 101.7-82.8 184.5-184.6 184.5zm101.2-138.2c-5.5-2.8-32.8-16.2-37.9-18-5.1-1.9-8.8-2.8-12.5 2.8-3.7 5.6-14.3 18-17.6 21.8-3.2 3.7-6.5 4.2-12 1.4-5.6-2.8-23.5-8.7-44.8-27.7-16.6-14.8-27.8-33.1-31.1-38.6-3.2-5.6-.3-8.6 2.5-11.4 2.5-2.5 5.6-6.5 8.3-9.7 2.8-3.2 3.7-5.6 5.6-9.3 1.9-3.7.9-6.9-.5-9.7-1.4-2.8-12.5-30.1-17.1-41.2-4.5-10.8-9.1-9.3-12.5-9.5-3.2-.2-6.9-.2-10.6-.2-3.7 0-9.7 1.4-14.8 6.9-5.1 5.6-19.4 19-19.4 46.3 0 27.3 19.9 53.7 22.6 57.4 2.8 3.7 39.1 59.7 94.8 83.8 13.2 5.7 23.5 9.2 31.6 11.8 13.3 4.2 25.4 3.6 35 2.2 10.7-1.5 32.8-13.4 37.4-26.4 4.6-13 4.6-24.1 3.2-26.4-1.3-2.5-5-3.9-10.5-6.6z"/>
      </svg>
    </a>
  </div>
</template>

<style scoped>
.whatsapp-float {
  position: fixed;
  bottom: 30px;
  right: 30px;
  z-index: 99999;
  display: flex;
  align-items: center;
  gap: 15px;
}

.whatsapp-btn {
  width: 65px;
  height: 65px;
  background: #25D366;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 30px rgba(37, 211, 102, 0.4);
  text-decoration: none;
  transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.whatsapp-btn:hover {
  transform: scale(1.1) rotate(5deg);
}

.whatsapp-tooltip {
  background: white;
  padding: 12px 20px;
  border-radius: 18px 18px 0 18px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  max-width: 250px;
  animation: slideIn 0.5s ease;
}

.whatsapp-tooltip p {
  margin: 0;
  font-size: 0.9rem;
  color: var(--text-dark);
  line-height: 1.4;
}

.whatsapp-tooltip strong {
  color: var(--primary);
}

@keyframes slideIn {
  from { opacity: 0; transform: translateX(20px); }
  to { opacity: 1; transform: translateX(0); }
}

@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(37, 211, 102, 0.6); }
  70% { box-shadow: 0 0 0 15px rgba(37, 211, 102, 0); }
  100% { box-shadow: 0 0 0 0 rgba(37, 211, 102, 0); }
}

.pulse {
  animation: pulse 2s infinite;
}

@media (max-width: 768px) {
  .whatsapp-tooltip {
    display: none; /* Ocultar burbuja en móvil para no tapar contenido */
  }
  .whatsapp-float {
    bottom: 15px;
    right: 15px;
  }
  .whatsapp-btn {
    width: 50px;
    height: 50px;
  }
  .whatsapp-btn svg {
    width: 24px;
    height: 24px;
  }
}
</style>
