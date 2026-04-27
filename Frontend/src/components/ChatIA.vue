<template>
  <div class="chat-wrapper">
    <!-- Chat Bubble (Toggle Button) -->
    <button class="chat-bubble-btn" @click="toggleChat" :class="{ 'chat-open': isChatOpen }">
      <div v-if="!isChatOpen" class="bot-face-container">
        <img :src="BotFaceImg" class="bot-face-img animate-float" alt="Fer IA" />
      </div>
      <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" class="close-icon">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>

    <!-- Chat Window -->
    <div v-if="isChatOpen" class="chat-window shadow-xl">
      <div class="chat-header">
        <div class="chat-title">
          <img :src="BotFaceImg" class="bot-icon-small" alt="Fer IA" />
          <h4>Fer IA</h4>
        </div>
        <p class="chat-subtitle">Asistente Inteligente de Vida Animal</p>
      </div>

      <div class="chat-body" ref="chatBody">
        <div v-if="messages.length === 0" class="empty-chat">
          <p>¡Hola! Soy Fer, tu asistente de IA. Puedo ayudarte con dudas sobre el sistema o darte datos sobre tus ventas, stock y productos.</p>
        </div>
        
        <div v-for="(msg, index) in messages" :key="index" :class="['message-row', msg.role === 'user' ? 'row-user' : 'row-bot']">
          <div :class="['message-bubble', msg.role === 'user' ? 'msg-user' : 'msg-bot']">
            <span v-if="msg.role === 'bot'" v-html="formatMessage(msg.content)"></span>
            <span v-else>{{ msg.content }}</span>
          </div>
        </div>

        <!-- Typing Indicator -->
        <div v-if="isLoading" class="message-row row-bot">
          <div class="message-bubble msg-bot typing-indicator">
            <span></span><span></span><span></span>
          </div>
        </div>
      </div>

      <div class="chat-footer">
        <input 
          type="text" 
          v-model="newMessage" 
          @keyup.enter="sendMessage"
          placeholder="Pregúntale a Fersito..." 
          :disabled="isLoading"
        />
        <button @click="sendMessage" :disabled="isLoading || !newMessage.trim()">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onUpdated, nextTick } from 'vue';
import BotFaceImg from '../assets/micarapensando.png';

const isChatOpen = ref(false);
const messages = ref([]);
const newMessage = ref('');
const isLoading = ref(false);
const chatBody = ref(null);

const toggleChat = () => {
  isChatOpen.value = !isChatOpen.value;
  if (isChatOpen.value && messages.value.length === 0) {
    // Primer mensaje de bienvenida opcional o cargar historial guardado
  }
};

const formatMessage = (text) => {
  // Convertir asteriscos **texto** a bold y saltos de línea a <br>
  let formatted = text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
  formatted = formatted.replace(/\n/g, '<br/>');
  return formatted;
};

const scrollToBottom = () => {
  if (chatBody.value) {
    chatBody.value.scrollTop = chatBody.value.scrollHeight;
  }
};

onUpdated(() => {
  scrollToBottom();
});

const getToken = () => localStorage.getItem('jwt_token');

const sendMessage = async () => {
  if (!newMessage.value.trim() || isLoading.value) return;

  const userText = newMessage.value.trim();
  messages.value.push({ role: 'user', content: userText });
  newMessage.value = '';
  isLoading.value = true;

  // Preparar historial reciente para dar contexto a la IA (últimos 4 mensajes)
  let historyText = '';
  const recentMsgs = messages.value.slice(-5, -1);
  recentMsgs.forEach(m => {
    historyText += `${m.role === 'user' ? 'Usuario' : 'Gemma'}: ${m.content}\n`;
  });

  try {
    const response = await fetch('/api/IA/Chatbot', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getToken()}`
      },
      body: JSON.stringify({ 
        mensaje: userText,
        historial: historyText 
      })
    });

    const data = await response.json();
    
    if (data.success) {
      messages.value.push({ role: 'bot', content: data.respuesta });
    } else {
      messages.value.push({ role: 'bot', content: `Error: ${data.mensaje}` });
    }
  } catch (error) {
    messages.value.push({ role: 'bot', content: "Hubo un problema de conexión con el servidor IA." });
  } finally {
    isLoading.value = false;
    nextTick(() => {
      scrollToBottom();
    });
  }
};
</script>

<style scoped>
.chat-wrapper {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.chat-bubble-btn {
  width: 65px;
  height: 65px;
  border-radius: 50%;
  background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%);
  color: white;
  border: none;
  box-shadow: 0 10px 25px rgba(66, 153, 225, 0.4), 0 0 0 0 rgba(66, 153, 225, 0.7);
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  animation: pulse-ring 2s infinite;
}

@keyframes pulse-ring {
  0% { box-shadow: 0 10px 25px rgba(66, 153, 225, 0.4), 0 0 0 0 rgba(66, 153, 225, 0.7); }
  70% { box-shadow: 0 10px 25px rgba(66, 153, 225, 0.4), 0 0 0 15px rgba(66, 153, 225, 0); }
  100% { box-shadow: 0 10px 25px rgba(66, 153, 225, 0.4), 0 0 0 0 rgba(66, 153, 225, 0); }
}

.bot-face-container {
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  border-radius: 50%;
  overflow: hidden;
  background: white; /* Contrast background for the image */
  border: 3px solid #EBF8FF;
}

.bot-face-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  animation: bounce-subtle 3s ease-in-out infinite;
}

@keyframes bounce-subtle {
  0%, 100% { transform: translateY(0) scale(1); }
  50% { transform: translateY(-3px) scale(1.05); }
}

.close-icon {
  width: 30px;
  height: 30px;
}

.chat-bubble-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 15px 35px rgba(66, 153, 225, 0.5);
  animation: none;
}

.chat-bubble-btn.chat-open {
  background: #E2E8F0;
  color: #4A5568;
  box-shadow: none;
  animation: none;
  border: none;
}

.chat-window {
  width: 380px;
  height: 550px;
  background: white;
  border-radius: 20px;
  margin-bottom: 20px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #E2E8F0;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 20px 40px rgba(0,0,0,0.15);
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.95); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.chat-header {
  background: linear-gradient(135deg, #4299E1 0%, #3182CE 100%);
  padding: 20px;
  color: white;
}

.chat-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.bot-icon-small {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: white;
  border: 2px solid rgba(255,255,255,0.4);
  object-fit: cover;
}

.chat-title h4 {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 700;
}

.chat-subtitle {
  margin: 5px 0 0 0;
  font-size: 0.8rem;
  opacity: 0.9;
}

.chat-body {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
  background: #F7FAFC;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.empty-chat {
  text-align: center;
  color: #718096;
  font-size: 0.9rem;
  margin-top: 20px;
  background: white;
  padding: 15px;
  border-radius: 12px;
  box-shadow: 0 2px 5px rgba(0,0,0,0.02);
}

.message-row {
  display: flex;
  width: 100%;
}

.row-user { justify-content: flex-end; }
.row-bot { justify-content: flex-start; }

.message-bubble {
  max-width: 80%;
  padding: 12px 16px;
  border-radius: 16px;
  font-size: 0.9rem;
  line-height: 1.4;
}

.msg-user {
  background: #3182CE;
  color: white;
  border-bottom-right-radius: 4px;
}

.msg-bot {
  background: white;
  color: #2D3748;
  border-bottom-left-radius: 4px;
  border: 1px solid #E2E8F0;
  box-shadow: 0 2px 5px rgba(0,0,0,0.02);
}

.chat-footer {
  padding: 15px;
  background: white;
  border-top: 1px solid #E2E8F0;
  display: flex;
  gap: 10px;
  align-items: center;
}

.chat-footer input {
  flex: 1;
  border: 1px solid #E2E8F0;
  padding: 12px 15px;
  border-radius: 999px;
  outline: none;
  font-size: 0.9rem;
  transition: border-color 0.2s;
  background: #F7FAFC;
}

.chat-footer input:focus {
  border-color: #4299E1;
  background: white;
}

.chat-footer button {
  background: #3182CE;
  color: white;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  transition: background 0.2s;
}

.chat-footer button svg {
  width: 20px;
  height: 20px;
  transform: rotate(90deg);
}

.chat-footer button:hover:not(:disabled) {
  background: #2B6CB0;
}

.chat-footer button:disabled {
  background: #CBD5E0;
  cursor: not-allowed;
}

/* Typing indicator */
.typing-indicator {
  display: flex;
  gap: 4px;
  padding: 12px 16px;
}
.typing-indicator span {
  width: 6px;
  height: 6px;
  background: #A0AEC0;
  border-radius: 50%;
  animation: bounce 1.4s infinite ease-in-out both;
}
.typing-indicator span:nth-child(1) { animation-delay: -0.32s; }
.typing-indicator span:nth-child(2) { animation-delay: -0.16s; }
@keyframes bounce {
  0%, 80%, 100% { transform: scale(0); }
  40% { transform: scale(1); }
}
</style>
