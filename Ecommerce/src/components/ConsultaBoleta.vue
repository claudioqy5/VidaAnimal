<script setup>
import { ref } from 'vue'
import api from '../utils/api.js'
import { generateA4PDF, generateTicketPDF } from '../utils/pdfGenerator.js'

const tipoDoc = ref('Boleta')
const serie = ref('B001')
const numero = ref('')
const fechaEmision = ref('')
const montoTotal = ref('')
const loading = ref(false)
const resultado = ref(null)
const error = ref('')

const consultar = async () => {
  error.value = ''
  resultado.value = null

  if (!tipoDoc.value || !serie.value || !numero.value || !fechaEmision.value || !montoTotal.value) {
    error.value = 'Debe ingresar el tipo, serie, número, fecha de emisión y monto total.'
    return
  }

  loading.value = true
  try {
    const res = await api.get('/ecommerce/ConsultaBoleta', {
      params: { 
        tipoDoc: tipoDoc.value, 
        serie: serie.value.trim(), 
        numero: numero.value.trim(), 
        fechaEmision: fechaEmision.value,
        montoTotal: montoTotal.value
      }
    })
    if (res.data.success) {
      resultado.value = res.data.data
    } else {
      error.value = res.data.message || 'No se encontró el comprobante.'
    }
  } catch (err) {
    if (err.response?.status === 404) {
      error.value = 'No se encontró la boleta con la serie y número proporcionados.'
    } else if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else {
      error.value = 'Error de conexión. Intente nuevamente.'
    }
  } finally {
    loading.value = false
  }
}

const formatFecha = (fecha) => {
  const d = new Date(fecha)
  return d.toLocaleDateString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric' }) +
    ' ' + d.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit', hour12: true })
}

const descargarA4 = async () => {
  if (resultado.value) {
    await generateA4PDF(resultado.value);
  }
}

const descargarTicketLocal = () => {
  if (resultado.value) {
    generateTicketPDF(resultado.value);
  }
}

const emit = defineEmits(['go-home'])
</script>

<template>
  <div class="consulta-wrapper">
    <div class="consulta-container">
      <!-- Header -->
      <div class="consulta-header">
        <h1>Consulta de Documentos Electrónicos</h1>
        <p>Busque y verifique su boleta de venta electrónica</p>
      </div>

      <!-- Formulario -->
      <div class="consulta-form light-card">
        <div class="form-row">
          <div class="form-group">
            <label>Tipo de Documento</label>
            <select v-model="tipoDoc">
              <option value="Boleta">Boleta</option>
              <option value="Factura">Factura</option>
              <option value="NotaDebito">Nota Débito</option>
              <option value="NotaCredito">Nota Crédito</option>
            </select>
          </div>
          <div class="form-group">
            <label>Fecha de Emisión</label>
            <input 
              v-model="fechaEmision" 
              type="date" 
            />
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label>Serie y Número del Comprobante</label>
            <div class="serie-numero-group">
              <input 
                v-model="serie" 
                type="text" 
                placeholder="B001" 
                maxlength="4"
                @input="serie = serie.toUpperCase()"
                class="input-serie"
              />
              <span class="separator">-</span>
              <input 
                v-model="numero" 
                type="text" 
                placeholder="00000034"
                @keyup.enter="consultar"
                class="input-numero"
              />
            </div>
          </div>
          <div class="form-group">
            <label>Monto Total (S/)</label>
            <input 
              v-model="montoTotal" 
              type="number" 
              step="0.01"
              placeholder="Ej: 185.00"
              @keyup.enter="consultar"
            />
          </div>
        </div>

        <button class="btn-consultar" @click="consultar" :disabled="loading">
          <span v-if="!loading">VER DOCUMENTO</span>
          <span v-else>Buscando...</span>
        </button>
      </div>

      <!-- Error -->
      <div v-if="error" class="error-card">
        <p>{{ error }}</p>
      </div>

      <!-- Resultado -->
      <div v-if="resultado" class="resultado-card light-card">
        <div class="resultado-header">
          <div class="status-badge success">BOLETA ACEPTADA POR SUNAT</div>
          <p class="status-detail">Estado: <strong>{{ resultado.sunatStatus || 'ACEPTADO' }}</strong></p>
        </div>

        <!-- Info del comprobante -->
        <div class="resultado-info">
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Serie - Número:</span>
              <span class="info-value">{{ resultado.serie }} - {{ resultado.numero }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Fecha de Emisión:</span>
              <span class="info-value">{{ formatFecha(resultado.fecha) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Cliente:</span>
              <span class="info-value">{{ resultado.cliente?.nombre || 'PÚBLICO GENERAL' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Método de Pago:</span>
              <span class="info-value">{{ resultado.metodoPago || 'Efectivo' }}</span>
            </div>
          </div>
        </div>

        <!-- Tabla de productos -->
        <div class="resultado-tabla">
          <table>
            <thead>
              <tr>
                <th>CANT.</th>
                <th>DESCRIPCIÓN</th>
                <th>P.U.</th>
                <th>IMPORTE</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(d, i) in resultado.detalles" :key="i">
                <td class="center">{{ d.cantidad }}</td>
                <td>{{ d.producto }}</td>
                <td class="right">S/ {{ Number(d.precioUnitario).toFixed(2) }}</td>
                <td class="right">S/ {{ Number(d.subTotal).toFixed(2) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Total -->
        <div class="resultado-total">
          <div class="total-row">
            <span>Exonerado:</span>
            <span>S/ {{ Number(resultado.subTotal).toFixed(2) }}</span>
          </div>
          <div class="total-row total-final">
            <span>Total a Pagar:</span>
            <span>S/ {{ Number(resultado.total).toFixed(2) }}</span>
          </div>
        </div>

        <!-- Botones de descarga -->
        <div class="resultado-descargas">
          <!-- Opciones Locales -->
          <button @click="descargarA4" class="btn-download local-a4">
            📄 Descargar A4
          </button>
          <button @click="descargarTicketLocal" class="btn-download local-ticket">
            🧾 Descargar Ticket
          </button>

          <!-- Opciones SUNAT (Solo visibles si fueron aceptadas) -->
          <a v-if="resultado.sunatXmlUrl" :href="resultado.sunatXmlUrl" target="_blank" class="btn-download xml">
            📋 XML SUNAT
          </a>
          <a v-if="resultado.sunatCdrUrl" :href="resultado.sunatCdrUrl" target="_blank" class="btn-download cdr">
            📜 CDR SUNAT
          </a>
        </div>
      </div>

      <!-- Volver al inicio -->
      <button class="btn-volver" @click="$emit('go-home')">
        Volver al catálogo
      </button>
    </div>
  </div>
</template>

<style scoped>
.consulta-wrapper {
  min-height: 80vh;
  padding: 20vh 20px 60px;
  background: linear-gradient(180deg, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0) 120px), #ffffff;
}

.consulta-container {
  max-width: 700px;
  margin: 0 auto;
}

.consulta-header {
  text-align: center;
  margin-bottom: 2rem;
}

.consulta-header h1 {
  font-size: 1.8rem;
  font-weight: 500;
  color: #333333;
  margin-bottom: 0.5rem;
}

.consulta-header p {
  color: #666666;
  font-size: 1rem;
}

.light-card {
  background: #f9f9f9;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 2rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  color: #555555;
  font-size: 0.9rem;
  font-weight: 400;
  margin-bottom: 0.5rem;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.8rem 1rem;
  border-radius: 6px;
  border: 1px solid #cccccc;
  background: #ffffff;
  color: #333333;
  font-size: 1rem;
  font-weight: 400;
  outline: none;
  transition: border 0.3s;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #f59e0b;
}

.serie-numero-group {
  display: flex;
  align-items: center;
  gap: 10px;
}

.input-serie {
  width: 100px !important;
  text-align: center;
  text-transform: uppercase;
}

.separator {
  font-weight: bold;
  font-size: 1.5rem;
  color: #666;
}

.input-numero {
  flex: 1;
}

.form-group input::placeholder {
  color: #999999;
}

.btn-consultar {
  width: 100%;
  padding: 1rem;
  border: none;
  border-radius: 6px;
  background: #f59e0b;
  color: #ffffff;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-consultar:hover:not(:disabled) {
  background: #d97706;
}

.btn-consultar:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Error */
.error-card {
  margin-top: 1.5rem;
  padding: 1.2rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  display: flex;
  align-items: center;
}

.error-card p {
  color: #b91c1c;
  font-weight: 400;
  font-size: 0.95rem;
  margin: 0;
}

/* Resultado */
.resultado-card {
  margin-top: 1.5rem;
  overflow: hidden;
}

.resultado-header {
  text-align: center;
  margin-bottom: 1.5rem;
}

.status-badge {
  display: inline-block;
  padding: 0.5rem 1.5rem;
  border-radius: 4px;
  font-weight: 500;
  font-size: 0.9rem;
}

.status-badge.success {
  background: #dcfce7;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.status-detail {
  color: #666666;
  font-size: 0.85rem;
  margin-top: 0.5rem;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.8rem;
  margin-bottom: 1.5rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.85rem;
  color: #777777;
  font-weight: 400;
}

.info-value {
  font-size: 0.95rem;
  color: #333333;
  font-weight: 500;
}

/* Tabla */
.resultado-tabla {
  margin-bottom: 1rem;
}

.resultado-tabla table {
  width: 100%;
  border-collapse: collapse;
}

.resultado-tabla th {
  text-align: left;
  font-size: 0.8rem;
  font-weight: 500;
  color: #555555;
  padding: 0.6rem 0.5rem;
  border-bottom: 1px solid #dddddd;
}

.resultado-tabla td {
  padding: 0.6rem 0.5rem;
  color: #333333;
  font-size: 0.9rem;
  font-weight: 400;
  border-bottom: 1px solid #eeeeee;
}

.resultado-tabla .center { text-align: center; }
.resultado-tabla .right { text-align: right; }

/* Totales */
.resultado-total {
  padding: 1rem 0;
  border-top: 1px dashed #cccccc;
}

.total-row {
  display: flex;
  justify-content: space-between;
  padding: 0.3rem 0;
  color: #555555;
  font-size: 0.95rem;
  font-weight: 400;
}

.total-final {
  font-size: 1.1rem;
  font-weight: 500;
  color: #333333;
  padding-top: 0.5rem;
  border-top: 1px solid #dddddd;
  margin-top: 0.3rem;
}

/* Descargas */
.resultado-descargas {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
  margin-top: 1.5rem;
  justify-content: center;
}

.btn-download {
  padding: 0.7rem 1.2rem;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  text-decoration: none;
  color: #ffffff;
  transition: all 0.3s;
  display: inline-flex;
  align-items: center;
}

.btn-download:hover {
  opacity: 0.9;
}

.btn-download.local-a4 { background: #0ea5e9; }
.btn-download.local-ticket { background: #f59e0b; }
.btn-download.xml { background: #2563eb; }
.btn-download.cdr { background: #7c3aed; }

/* Volver */
.btn-volver {
  display: block;
  margin: 2rem auto 0;
  padding: 0.7rem 2rem;
  background: transparent;
  border: 1px solid #cccccc;
  color: #555555;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 400;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-volver:hover {
  border-color: #999999;
  color: #333333;
  background: #f9f9f9;
}

@media (max-width: 600px) {
  .form-row { grid-template-columns: 1fr; }
  .info-grid { grid-template-columns: 1fr; }
  .consulta-header h1 { font-size: 1.4rem; }
  .resultado-descargas { flex-direction: column; }
  .btn-download { text-align: center; justify-content: center; }
}
</style>
