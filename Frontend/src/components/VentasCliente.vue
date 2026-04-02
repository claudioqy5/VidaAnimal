<template>
  <div class="ventas-container animate-fade-in">
    <div class="header-section">
      <div>
        <h1>Historial de Ventas</h1>
        <p class="subtitle">Consulta las ventas realizadas por cada cliente</p>
      </div>
    </div>

    <!-- Selector de Filtros y Orden -->
    <div class="filters-card glass">
      <div class="filter-group">
        <label>Filtrar por Cliente:</label>
        <select v-model="selectedClienteID" @change="fetchVentas">
          <option value="">Todos los clientes</option>
          <option v-for="c in clientes" :key="c.clienteID" :value="c.clienteID">
            {{ c.nombreCompleto }} — {{ c.documentoIdentidad }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label>Ordenar por:</label>
        <select v-model="orderBy">
          <option value="fecha_desc">📌 Fecha: Recientes primero</option>
          <option value="fecha_asc">📌 Fecha: Antiguas primero</option>
          <option value="monto_desc">💰 Monto: Mayor a Menor</option>
          <option value="monto_asc">💰 Monto: Menor a Mayor</option>
        </select>
      </div>

      <div class="filter-group">
        <label>Filtrar por Fecha:</label>
        <div class="date-filter-row">
          <input type="date" v-model="selectedFecha" @change="fetchVentas" class="date-input" />
          <button v-if="selectedFecha" class="btn-clear-filter" @click="verTodoElHistorial" title="Ver todo el historial">
            Ver Todo
          </button>
        </div>
      </div>

      <div class="stats-mini">
        <div class="stat-item">
          <span class="label">Monto Total:</span>
          <span class="value highlight">S/ {{ totalGeneral.toFixed(2) }}</span>
        </div>
        <button class="btn-reporte" @click="descargarReporteVentas" :disabled="ventasOrdenadas.length === 0">
          📄 Generar Reporte
        </button>
      </div>
    </div>

    <!-- Lista de Ventas -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando ventas...</p>
    </div>

    <div v-else-if="ventas.length === 0" class="empty-state">
      <p>📋 No hay ventas para mostrar.</p>
    </div>

    <div v-else class="ventas-grid">
      <div 
        class="venta-card glass" 
        v-for="v in ventasOrdenadas" 
        :key="v.ventaID"
        :class="{ 'is-expanded': expandedVentas.has(v.ventaID), 'is-anulada': v.estado === 'Anulada' }"
        @click="toggleVenta(v.ventaID)"
      >
        <!-- CABECERA RESUMIDA -->
        <div class="card-summary">
          <div class="summary-main">
            <span class="comprobante-badge">{{ v.serieComprobante }}-{{ v.numeroComprobante }}</span>
              <div class="main-info">
                <span class="cliente-name">{{ v.cliente?.nombreCompleto || 'Consumidor Final' }}</span>
                <span class="fecha-text">{{ formatDate(v.fecha) }} • 👤 {{ v.cajero || 'Sistema' }}</span>
              </div>
          </div>
          <div class="summary-price">
            <span class="metodo-badge" :class="(v.metodoPago || 'Efectivo').toLowerCase()">{{ v.metodoPago || 'Efectivo' }}</span>
            <span class="total-label" v-if="v.estado !== 'Anulada'">Pagado:</span>
            <span class="total-label" v-else style="color: #E53E3E; font-weight: bold;">ANULADA:</span>
            <span class="total-value" :style="v.estado === 'Anulada' ? 'text-decoration: line-through; opacity: 0.5;' : ''">S/ {{ Number(v.total).toFixed(2) }}</span>
            <button class="btn-pdf" title="Descargar Comprobante PDF" @click.stop="descargarPDF(v)">📄 PDF</button>
            <span class="expand-icon">{{ expandedVentas.has(v.ventaID) ? '▲' : '▼' }}</span>
          </div>
        </div>

        <!-- DETALLE EXPANDIBLE -->
        <div class="card-details-wrapper" v-if="expandedVentas.has(v.ventaID)">
          <div class="details-content animate-slide-down">
            <!-- PRODUCTOS PRIMERO -->
            <div class="items-list">
              <p class="section-title">📦 Detalle de Productos:</p>
              <table class="details-table">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Cant.</th>
                    <th>P. Unit</th>
                    <th style="text-align: right;">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="d in v.detalleVentas" :key="d.ventaDetalleID">
                    <td>{{ d.producto?.nombre || 'Producto' }}</td>
                    <td>{{ d.cantidad }}</td>
                    <td>S/ {{ Number(d.precioUnitario).toFixed(2) }}</td>
                    <td class="item-sub">S/ {{ (d.cantidad * d.precioUnitario).toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- RESUMEN FINANCIERO DESPUÉS -->
            <div class="financial-breakdown">
              <div class="breakdown-row">
                <span>Subtotal Bruto</span>
                <span>S/ {{ Number(v.subTotal || 0).toFixed(2) }}</span>
              </div>
              <div class="breakdown-row discount" v-if="v.descuento > 0">
                <span>Descuento aplicado</span>
                <span>- S/ {{ Number(v.descuento).toFixed(2) }}</span>
              </div>
              <div class="breakdown-row final">
                <span>Total Recaudado</span>
                <span>S/ {{ Number(v.total).toFixed(2) }}</span>
              </div>
            </div>

            <div class="extra-info" v-if="v.observaciones">
              <p><strong>Nota:</strong> {{ v.observaciones }}</p>
            </div>
            
            <div class="actions-footer" style="margin-top: 1.5rem; text-align: right;">
              <button class="btn-anular" v-if="v.estado !== 'Anulada'" @click.stop="anularVenta(v)">
                ⛔ Anular Boleta y Devolver Stock
              </button>
              <div v-else class="anulado-badge">🚫 ESTA VENTA FUE ANULADA - STOCK DEVUELTO</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL DE DESACTIVACION DE BOLETA / ANULACION -->
    <div class="modal-overlay" v-if="showAnularModal" @click.self="cerrarAnularModal">
      <div class="modal-content glass-modal bounce-in" style="max-width: 400px; text-align: center;">
        <div style="font-size: 3rem; margin-bottom: 1rem;">🛡️</div>
        <h2 style="color: #742A2A; margin-bottom: 0.5rem; font-size: 1.2rem;">Seguridad Requerida</h2>
        <p style="color: #4A5568; font-size: 0.85rem; margin-bottom: 1.5rem;">
          Estás a punto de anular la boleta <b>{{ ventaToAnular?.serieComprobante }}-{{ ventaToAnular?.numeroComprobante }}</b>.<br>
          Esto devolverá la mercancía al Kardex y restará los ingresos de tu Dashboard contable.
        </p>
        
        <div class="form-group" style="text-align: left; margin-bottom: 1.5rem;">
          <label style="font-size: 0.75rem; color: #718096; font-weight: 700;">INGRESE SU CONTRASEÑA:</label>
          <input type="password" v-model="adminPassword" placeholder="Contraseña de Administrador" @keyup.enter="confirmAnularVenta"
                 style="width: 100%; padding: 0.75rem; border: 2px solid #E2E8F0; border-radius: 8px; margin-top: 0.3rem;" />
        </div>

        <div class="modal-actions" style="display: flex; gap: 0.5rem; justify-content: center;">
          <button @click="cerrarAnularModal" class="btn-secondary" style="flex: 1; padding: 0.75rem;">Cancelar</button>
          <button @click="confirmAnularVenta" class="btn-danger" style="flex: 1; padding: 0.75rem; background: #E53E3E; color: white; border: none; border-radius: 8px; font-weight: 700;" :disabled="loadingAnular">
            {{ loadingAnular ? 'Validando...' : 'Anular Venta' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

const API_BASE = '/api';
const getToken = () => localStorage.getItem('jwt_token');

const getTodayInFormat = () => new Date().toLocaleDateString('en-CA');

const ventas = ref([]);
const clientes = ref([]);
const loading = ref(false);
const showAnularModal = ref(false);
const adminPassword = ref('');
const ventaToAnular = ref(null);
const loadingAnular = ref(false);
const selectedClienteID = ref('');
const selectedFecha = ref(getTodayInFormat());
const orderBy = ref('fecha_desc');
const expandedVentas = ref(new Set());

const verTodoElHistorial = () => {
  selectedFecha.value = '';
  fetchVentas();
};

const toggleVenta = (id) => {
  if (expandedVentas.value.has(id)) {
    expandedVentas.value.delete(id);
  } else {
    expandedVentas.value.add(id);
  }
};

const descargarPDF = (v) => {
  const doc = new jsPDF();
  
  // Titulo general
  doc.setFontSize(22);
  doc.setTextColor(43, 108, 176); // Azul logo 
  doc.text("VIDA ANIMAL", 105, 20, null, null, "center");
  
  doc.setFontSize(14);
  doc.setTextColor(0, 0, 0);
  doc.text("Comprobante de Venta", 105, 30, null, null, "center");
  
  // Info de la Venta
  doc.setFontSize(11);
  doc.text(`Comprobante: ${v.serieComprobante}-${v.numeroComprobante}`, 15, 45);
  doc.text(`Fecha: ${formatDate(v.fecha)}`, 15, 52);
  doc.text(`Cliente: ${v.cliente?.nombreCompleto || 'Consumidor Final'}`, 15, 59);
  doc.text(`DNI/RUC: ${v.cliente?.documentoIdentidad || '---'}`, 15, 66);
  doc.text(`Cajero: ${v.cajero || 'Sistema'}`, 130, 45);
  doc.text(`M. Pago: ${v.metodoPago || 'Efectivo'}`, 130, 52);

  // Detalles de Venta con AutoTable
  const tableColumn = ["Producto", "U. Venta", "Cantidad", "P. Unitario", "Subtotal"];
  const tableRows = [];

  v.detalleVentas.forEach(d => {
    const pName = d.producto?.nombre || 'Producto';
    const cant = d.cantidad;
    const pu = `S/ ${Number(d.precioUnitario).toFixed(2)}`;
    const sub = `S/ ${(d.cantidad * d.precioUnitario).toFixed(2)}`;
    const uv = d.unidadVenta || 'UND';
    tableRows.push([pName, uv, cant, pu, sub]);
  });

  autoTable(doc, {
    head: [tableColumn],
    body: tableRows,
    startY: 75,
    theme: 'grid',
    styles: { fontSize: 9, cellPadding: 3 },
    headStyles: { fillColor: [43, 108, 176], textColor: [255, 255, 255] }
  });

  // Totales
  const finalY = doc.lastAutoTable.finalY || 75;
  doc.setFontSize(11);
  doc.text(`Subtotal: S/ ${Number(v.subTotal || 0).toFixed(2)}`, 140, finalY + 10);
  const desc = Number(v.descuento || 0);
  if (desc > 0) {
    doc.text(`Descuento: - S/ ${desc.toFixed(2)}`, 140, finalY + 17);
  }
  
  doc.setFontSize(13);
  doc.setFont(undefined, 'bold');
  const totalY = desc > 0 ? finalY + 25 : finalY + 18;
  doc.text(`TOTAL PAGADO: S/ ${Number(v.total).toFixed(2)}`, 140, totalY);
  
  if (v.observaciones) {
    doc.setFont(undefined, 'normal');
    doc.setFontSize(10);
    doc.text("Notas:", 15, finalY + 15);
    doc.text(v.observaciones, 15, finalY + 22, { maxWidth: 100 });
  }

  // Footer
  doc.setFontSize(9);
  doc.setTextColor(150, 150, 150);
  doc.text("Gracias por su preferencia. - Sistema Vida Animal", 105, 280, null, null, "center");

  // Save the PDF
  doc.save(`Venta_${v.serieComprobante}-${v.numeroComprobante}.pdf`);
};

const apiFetch = async (endpoint) => {
  const res = await fetch(`${API_BASE}${endpoint}`, {
    headers: { 'Authorization': `Bearer ${getToken()}` }
  });
  return res.json();
};

const fetchClientes = async () => {
  try {
    const data = await apiFetch('/clientes');
    if (data.success) clientes.value = data.data;
  } catch (e) { console.error(e); }
};

const fetchVentas = async () => {
  loading.value = true;
  try {
    let url = `${API_BASE}/ventas?`;
    if (selectedClienteID.value) url += `clienteId=${selectedClienteID.value}&`;
    if (selectedFecha.value) url += `fecha=${selectedFecha.value}&`;
    
    const res = await fetch(url, {
        headers: { 'Authorization': `Bearer ${getToken()}` }
    });
    const data = await res.json();
    if (data.success) ventas.value = data.data;
  } catch (e) {
    console.error('Error al cargar ventas', e);
  } finally {
    loading.value = false;
  }
};

const anularVenta = (venta) => {
  ventaToAnular.value = venta;
  adminPassword.value = '';
  showAnularModal.value = true;
};

const cerrarAnularModal = () => {
  showAnularModal.value = false;
  ventaToAnular.value = null;
  adminPassword.value = '';
};

const confirmAnularVenta = async () => {
  if (!adminPassword.value) {
    alert("❌ Debes ingresar tu contraseña de seguridad.");
    return;
  }
  
  loadingAnular.value = true;
  try {
    const res = await fetch(`${API_BASE}/Ventas/${ventaToAnular.value.ventaID}/anular`, {
      method: 'POST',
      headers: { 
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ password: adminPassword.value })
    });
    
    if (res.ok) {
      alert("✅ Venta anulada con éxito. El stock ha sido devuelto al Kardex.");
      cerrarAnularModal();
      fetchVentas(); // Recargar la lista
    } else {
      const errorData = await res.json();
      alert("❌ Error: " + (errorData.mensaje || "Error al anular"));
    }
  } catch (error) {
    alert("❌ Error de comunicación al intentar anular la venta.");
  } finally {
    loadingAnular.value = false;
  }
};

const ventasOrdenadas = computed(() => {
  return [...ventas.value].sort((a, b) => {
    if (orderBy.value === 'fecha_desc') return new Date(b.fecha) - new Date(a.fecha);
    if (orderBy.value === 'fecha_asc') return new Date(a.fecha) - new Date(b.fecha);
    if (orderBy.value === 'monto_desc') return Number(b.total) - Number(a.total);
    if (orderBy.value === 'monto_asc') return Number(a.total) - Number(b.total);
    return 0;
  });
});

const totalGeneral = computed(() => 
  ventas.value.filter(v => v.estado !== 'Anulada').reduce((sum, v) => sum + Number(v.total || 0), 0)
);

const formatDate = (dateStr) => {
  if (!dateStr) return '---';
  return new Date(dateStr).toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
};

const descargarReporteVentas = () => {
  const doc = new jsPDF();
  
  doc.setFontSize(22);
  doc.setTextColor(43, 108, 176); 
  doc.text("VIDA ANIMAL", 105, 20, null, null, "center");
  
  doc.setFontSize(14);
  doc.setTextColor(0, 0, 0);
  doc.text("Reporte General de Ventas", 105, 30, null, null, "center");
  
  doc.setFontSize(11);
  let fechaFiltroStr = selectedFecha.value ? `${selectedFecha.value}` : 'Todo el historial';
  let clienteFiltroStr = selectedClienteID.value ? 'Cliente filtrado' : 'Todos los clientes';
  
  doc.text(`Filtro Fecha: ${fechaFiltroStr}`, 15, 45);
  doc.text(`Filtro Cliente: ${clienteFiltroStr}`, 15, 52);
  doc.text(`N° Ventas Mostradas: ${ventasOrdenadas.value.length}`, 130, 45);

  const tableColumn = ["Fecha/Hora", "Comprobante", "Cliente", "Cajero", "TotalPagado"];
  const tableRows = [];

  ventasOrdenadas.value.forEach(v => {
    tableRows.push([
      formatDate(v.fecha),
      `${v.serieComprobante}-${v.numeroComprobante}`,
      v.cliente?.nombreCompleto || 'Consumidor Final',
      v.cajero || 'Sistema',
      `S/ ${Number(v.total).toFixed(2)}`
    ]);
  });

  autoTable(doc, {
    head: [tableColumn],
    body: tableRows,
    startY: 60,
    theme: 'grid',
    styles: { fontSize: 8, cellPadding: 3 },
    headStyles: { fillColor: [43, 108, 176], textColor: [255, 255, 255] }
  });

  const finalY = doc.lastAutoTable.finalY || 60;
  doc.setFontSize(13);
  doc.setFont(undefined, 'bold');
  doc.text(`MONTO TOTAL: S/ ${totalGeneral.value.toFixed(2)}`, 120, finalY + 15);
  
  doc.setFontSize(9);
  doc.setFont(undefined, 'normal');
  doc.setTextColor(150, 150, 150);
  doc.text("Generado por el Sistema Vida Animal.", 105, 280, null, null, "center");

  doc.save(`Reporte_Ventas_${new Date().toISOString().split('T')[0]}.pdf`);
};

onMounted(async () => {
  await fetchClientes();
  await fetchVentas();
});
</script>

<style scoped>
.ventas-container { padding: 2rem; max-width: 1200px; margin: 0 auto; }
.header-section { margin-bottom: 2rem; }
.header-section h1 { font-size: 1.8rem; color: #2D3748; margin-bottom: 0.25rem; }
.subtitle { color: #718096; font-size: 0.9rem; }

.filters-card {
  padding: 1.25rem 1.5rem; margin-bottom: 2rem;
  display: flex; justify-content: space-between; align-items: center;
  border-radius: 16px; background: white; 
  border: 1px solid #E2E8F0; box-shadow: 0 2px 8px rgba(0,0,0,0.04);
  flex-wrap: wrap; gap: 1rem;
}
.filter-group { display: flex; flex-direction: column; gap: 0.4rem; }
.filter-group label { font-size: 0.75rem; font-weight: 700; color: #718096; text-transform: uppercase; letter-spacing: 0.05em;}
.filter-group select, .date-input {
  padding: 0.7rem 1rem; border-radius: 10px; min-width: 240px;
  border: 1px solid #E2E8F0; font-family: inherit; outline: none; background: #F8FAFC;
  transition: all 0.2s;
}

.date-filter-row { display: flex; gap: 0.5rem; align-items: center; }

.btn-clear-filter {
  padding: 0.7rem 1rem; border-radius: 10px; border: 1px solid #A7C7E7;
  background: #EBF8FF; color: #2C5282; font-weight: 700; font-size: 0.8rem;
  cursor: pointer; transition: 0.2s; white-space: nowrap;
}
.btn-clear-filter:hover { background: #BEE3F8; border-color: #63B3ED; }

.stats-mini { display: flex; align-items: flex-end; }
.stat-item { display: flex; flex-direction: column; align-items: flex-end;}
.stat-item .label { color: #718096; font-size: 0.8rem; font-weight: 600; }
.stat-item .value { font-weight: 800; color: #2D3748; font-size: 1.4rem; }
.stat-item .value.highlight { 
  background: linear-gradient(135deg, #276749, #48BB78);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.btn-reporte { background: #2D3748; color: white; border: none; border-radius: 10px; padding: 0.8rem 1.25rem; font-weight: 700; font-size: 0.85rem; cursor: pointer; transition: 0.2s ease; margin-left: 1.5rem; white-space: nowrap; align-self: center; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
.btn-reporte:hover { background: #1A202C; transform: translateY(-2px); box-shadow: 0 6px 14px rgba(0,0,0,0.15);}
.btn-reporte:disabled { opacity: 0.5; cursor: not-allowed; transform: none; box-shadow: none; }

.ventas-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.venta-card {
  background: white; border-radius: 12px; padding: 0.25rem;
  border: 1px solid #E2E8F0; box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  transition: all 0.2s; animation: slideUp 0.3s ease;
  cursor: pointer;
}
.venta-card:hover { border-color: #A7C7E7; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.venta-card.is-expanded { border-color: #A7C7E7; box-shadow: 0 8px 24px rgba(0,0,0,0.08); }

/* CONTENIDO RESUMIDO */
.card-summary {
  display: flex; justify-content: space-between; align-items: center;
  padding: 1rem 1.25rem;
}
.summary-main { display: flex; align-items: center; gap: 1.5rem; }
.comprobante-badge {
  background: #EDF2F7; color: #4A5568; padding: 0.35rem 0.75rem;
  border-radius: 8px; font-weight: 700; font-size: 0.9rem; font-family: 'Syncopate', sans-serif;
}
.main-info { display: flex; flex-direction: column; }
.cliente-name { font-weight: 600; color: #2D3748; font-size: 1rem; }
.fecha-text { font-size: 0.8rem; color: #A0AEC0; }

.summary-price { display: flex; align-items: center; gap: 1rem; }
.btn-pdf { background: #EBF8FF; color: #2B6CB0; border: 1px solid #90CDF4; border-radius: 6px; padding: 0.4rem 0.6rem; font-size: 0.75rem; font-weight: 700; cursor: pointer; transition: 0.2s ease; display: inline-flex; align-items: center; gap: 0.25rem;}
.btn-pdf:hover { background: #BEE3F8; transform: translateY(-2px); box-shadow: 0 4px 10px rgba(43,108,176, 0.15);}
.btn-pdf:active { transform: translateY(0); }
.total-label { font-size: 0.8rem; color: #718096; }
.total-value { 
  font-size: 1.25rem; font-weight: 800; color: #2C5282;
  background: linear-gradient(135deg, #A7C7E7, #C3B1E1);
  background-clip: text;
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
}
.metodo-badge { font-size: 0.7rem; padding: 0.2rem 0.5rem; border-radius: 6px; font-weight: 700; text-transform: uppercase; margin-right: 0.5rem; }
.efectivo { background: #E6FFFA; color: #234E52; border: 1px solid #81E6D9; }
.yape { background: #FAF5FF; color: #44337A; border: 1px solid #D6BCFA; }
.plin { background: #ebf8ff; color: #2C5282; border: 1px solid #90CDF4; }
.transferencia { background: #EDF2F7; color: #2D3748; border: 1px solid #CBD5E0; }
.tarjeta { background: #FFF5F5; color: #742A2A; border: 1px solid #FEB2B2; }
.expand-icon { color: #CBD5E0; font-size: 0.8rem; }
.btn-anular { background: #FFF5F5; color: #E53E3E; border: 1px solid #FEB2B2; border-radius: 8px; padding: 0.6rem 1rem; font-weight: 700; font-size: 0.8rem; cursor: pointer; transition: 0.2s; }
.btn-anular:hover { background: #FED7D7; transform: translateY(-2px); box-shadow: 0 4px 10px rgba(229, 62, 62, 0.2); }
.anulado-badge { color: #E53E3E; font-weight: 800; font-size: 0.9rem; padding: 0.5rem; background: #FFF5F5; border-radius: 8px; border: 1px dashed #FC8181; display: inline-block; }
.is-anulada { background: #FAFAFA; opacity: 0.8; border-color: #FED7D7; }
.is-anulada:hover { border-color: #FC8181; }
/* EFECTO BOUNCE */
@keyframes bounceIn {
  0% { transform: scale(0.9); opacity: 0; }
  50% { transform: scale(1.02); opacity: 1; }
  100% { transform: scale(1); opacity: 1; }
}
.bounce-in { animation: bounceIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1); }

.glass-modal { background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px); border: 1px solid rgba(255, 255, 255, 0.3); }
.btn-secondary { background: #EDF2F7; color: #4A5568; border: none; border-radius: 8px; font-weight: 700; cursor: pointer; }
.btn-secondary:hover { background: #E2E8F0; }
.btn-danger:hover { background: #C53030 !important; }

/* CONTENIDO EXPANDIDO */
.card-details-wrapper {
  border-top: 1px solid #F0F4F8;
  padding: 1.5rem;
  background: #FAFBFC;
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
}

.financial-breakdown {
  display: flex; justify-content: space-around;
  margin-bottom: 2rem; padding: 1.25rem;
  background: white; border-radius: 12px; border: 1px solid #E2E8F0;
}
.breakdown-row { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; }
.breakdown-row span:first-child { font-size: 0.75rem; color: #A0AEC0; font-weight: 600; text-transform: uppercase; }
.breakdown-row span:last-child { font-size: 1.1rem; font-weight: 700; color: #4A5568; }
.breakdown-row.discount span:last-child { color: #E53E3E; }
.breakdown-row.final span:last-child { color: #2C5282; font-size: 1.3rem; }

.items-list { background: white; border-radius: 12px; padding: 1.25rem; border: 1px solid #E2E8F0; }
.section-title { font-weight: 700; color: #4A5568; font-size: 0.9rem; margin-bottom: 1rem; border-left: 4px solid #A7C7E7; padding-left: 0.75rem;}

.details-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
.details-table th { text-align: left; padding: 0.75rem; color: #718096; font-weight: 600; border-bottom: 2px solid #F7FAFC; }
.details-table td { padding: 0.75rem; border-bottom: 1px solid #F7FAFC; color: #4A5568; }
.item-sub { font-weight: 700; color: #2D3748; text-align: right; }

.animate-slide-down { animation: slideDown 0.3s ease-out; }
@keyframes slideDown { from { transform: translateY(-10px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }

.loading-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; padding: 4rem; gap: 1rem; color: #718096;
}
.spinner {
  width: 40px; height: 40px; border: 4px solid #E2E8F0;
  border-top-color: #A7C7E7; border-radius: 50%;
  animation: spin 1s linear infinite;
}
.empty-state {
  text-align: center; padding: 4rem; color: #A0AEC0;
  background: white; border-radius: 16px; border: 1px dashed #CBD5E0;
}
@keyframes spin { to { transform: rotate(360deg); } }
@keyframes slideUp { from { transform: translateY(10px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.glass { backdrop-filter: blur(10px); }
</style>
