// ============================================================
//  printer.js — Motor de Impresión Térmica 80mm · Vida Animal
// ============================================================

const STORE_INFO = {
  name: 'Vida Animal',
  address: 'Jr. Atahualpa N° 291 - Aucayacu, Perú',
  phone: '975 418 965',
  web: 'vidaanimal.vercel.app',
  logoUrl: window.location.origin + '/logovidaanimal.png',
}

/**
 * Formatea una fecha ISO o Date object a cadena legible en español (Perú).
 */
function formatDateTime(dateStr) {
  const d = new Date(dateStr)
  return d.toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', hour12: true,
  })
}

/**
 * Genera el HTML del encabezado del ticket con logo + datos de la tienda.
 * El logo se carga como <img> referenciando la URL del servidor.
 */
function buildHeader() {
  return `
    <div class="ticket-header">
      <div class="header-top">
        <div class="store-text">
          <div class="store-name">${STORE_INFO.name}</div>
          <div class="store-address">${STORE_INFO.address}</div>
          <div class="store-contact">Cel: ${STORE_INFO.phone}</div>
          <div class="store-web">${STORE_INFO.web}</div>
        </div>
        <img class="store-logo" src="${STORE_INFO.logoUrl}" alt="Logo Vida Animal" onerror="this.style.display='none'" />
      </div>
      <div class="header-divider">================================</div>
    </div>
  `
}

/**
 * Genera el HTML del pie del ticket.
 */
function buildFooter() {
  return `
    <div class="ticket-footer">
      <div class="footer-divider">================================</div>
      <div class="footer-thanks">¡Gracias por su compra!</div>
      <div class="footer-web">${STORE_INFO.web}</div>
    </div>
  `
}

/**
 * Hoja de estilos CSS interna para el ticket de 80mm.
 * Se usa box-sizing border-box y ancho fijo de 80mm.
 */
const TICKET_CSS = `
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

  @page {
    size: 80mm auto;
    margin: 0mm 2mm;
  }
  * {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    /* Evitar difuminado en impresoras térmicas de bajos DPI */
    -webkit-font-smoothing: none;
    -moz-osx-font-smoothing: grayscale;
    font-smooth: never;
    text-rendering: optimizeSpeed;
  }
  body {
    width: 76mm;
    font-size: 11px;
    color: #000;
    background: #fff;
  }
  
  /* Forzar que TODO sea absolutamente negro para evitar renderizado por semitonos (dithering) */
  h1, h2, h3, h4, h5, h6, p, span, div, td, th {
    color: #000 !important;
  }

  /* ── Header ── */
  .ticket-header { text-align: left; margin-bottom: 4px; }
  .header-top {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 4px;
    margin-bottom: 4px;
  }
  .store-text { flex: 1; }
  .store-name { font-size: 15px; font-weight: 800; letter-spacing: -0.3px; text-transform: uppercase; }
  .store-address { font-size: 9px; color: #444; margin-top: 2px; font-weight: 400; }
  .store-contact { font-size: 9px; color: #444; font-weight: 400; }
  .store-web { font-size: 9px; color: #444; font-weight: 400; }
  .store-logo {
    width: 40px;
    height: 40px;
    object-fit: contain;
    border-radius: 50%;
    flex-shrink: 0;
  }
  .header-divider { font-size: 9px; color: #ccc; margin: 4px 0; letter-spacing: 0.5px; border-top: 1px dashed #bbb; padding-top: 4px; }

  /* ── Comprobante info ── */
  .comp-info { margin-bottom: 6px; }
  .comp-title { font-size: 11px; font-weight: 700; text-align: center; text-transform: uppercase; letter-spacing: 0.8px; color: #111; }
  .comp-number { font-size: 12px; font-weight: 800; text-align: center; margin-top: 2px; color: #000; }
  .comp-date { font-size: 9px; color: #555; margin-top: 3px; font-weight: 400; }
  .comp-client { font-size: 9px; color: #555; margin-top: 1px; font-weight: 500; }
  .comp-pay { font-size: 9px; color: #555; margin-top: 1px; }
  .comp-obs { font-size: 9px; color: #666; margin-top: 3px; font-style: italic; }
  .section-div { margin: 5px 0; border-top: 1px dashed #bbb; }

  /* ── Tabla de productos ── */
  table { width: 100%; border-collapse: collapse; margin-bottom: 4px; }
  thead th { font-size: 8px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.4px; color: #333; border-bottom: 1px solid #ddd; padding-bottom: 3px; }
  th.left, td.left { text-align: left; }
  th.right, td.right { text-align: right; }
  th.center, td.center { text-align: center; }
  tbody td { font-size: 10px; padding: 2px 0; vertical-align: top; color: #111; }
  .prod-name { max-width: 35mm; word-break: break-word; font-weight: 500; }
  tfoot tr td { border-top: 1px solid #ddd; padding-top: 3px; }
  .total-row td { font-size: 13px; font-weight: 800; color: #000; }
  .discount-row td { font-size: 10px; color: #555; }
  .subtotal-row td { font-size: 10px; color: #555; }

  /* ── Resumen de cierre de caja ── */
  .summary-table { width: 100%; font-size: 10px; margin: 4px 0; }
  .summary-table td { padding: 2px 0; }
  .summary-label { font-weight: 600; color: #333; }
  .summary-value { text-align: right; font-weight: 700; }
  .summary-big td { font-size: 14px; font-weight: 800; border-top: 1px solid #000; padding-top: 4px; margin-top: 3px; }

  /* ── Footer ── */
  .ticket-footer { text-align: center; margin-top: 8px; }
  .footer-divider { border-top: 1px dashed #bbb; margin-bottom: 5px; }
  .footer-thanks { font-size: 11px; font-weight: 700; color: #111; margin-top: 4px; }
  .footer-web { font-size: 9px; color: #666; margin-top: 2px; font-weight: 400; }

  /* Anulada */
  .anulada-stamp {
    text-align: center;
    font-size: 14px;
    font-weight: 800;
    letter-spacing: 2px;
    border: 2px solid #c00;
    color: #c00;
    padding: 2px 8px;
    display: inline-block;
    margin: 4px auto;
    transform: rotate(-5deg);
    border-radius: 3px;
  }
`


/**
 * Abre el diálogo de impresión usando un iframe oculto para evitar abrir nuevas pestañas.
 * @param {string} bodyHtml — HTML del contenido del ticket (sin head/style).
 * @param {string} title — Título de la ventana (para el nombre del archivo PDF a generar).
 */
function openPrintWindow(bodyHtml, title = 'Ticket Vida Animal') {
  // Crear un iframe invisible
  const iframe = document.createElement('iframe');
  iframe.style.position = 'fixed';
  iframe.style.right = '0';
  iframe.style.bottom = '0';
  iframe.style.width = '0';
  iframe.style.height = '0';
  iframe.style.border = '0';
  document.body.appendChild(iframe);

  const doc = iframe.contentWindow.document;
  doc.open();
  doc.write(`
    <!DOCTYPE html>
    <html>
      <head>
        <meta charset="UTF-8" />
        <title>${title}</title>
        <style>${TICKET_CSS}</style>
      </head>
      <body>
        ${bodyHtml}
      </body>
    </html>
  `);
  doc.close();

  // Esperar a que los recursos (como el logo) carguen antes de imprimir
  iframe.onload = () => {
    setTimeout(() => {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();
      
      // Remover el iframe después de un tiempo prudencial
      setTimeout(() => {
        if (document.body.contains(iframe)) {
          document.body.removeChild(iframe);
        }
      }, 5000); // 5 segundos para asegurar que el spooler capture la vista
    }, 300);
  };
}

/**
 * Imprime el ticket de una venta individual (Nota de Venta).
 * @param {object} venta — Objeto de venta tal como viene del backend.
 */
export function imprimirTicketVenta(venta) {
  const isAnulada = venta.estado === 'Anulada'

  const filasProductos = (venta.detalleVentas || []).map(d => `
    <tr>
      <td class="left prod-name">${d.producto?.nombre || 'Producto'}</td>
      <td class="center">${Number(d.cantidad).toFixed(2)}</td>
      <td class="right">S/${Number(d.precioUnitario).toFixed(2)}</td>
      <td class="right">S/${(Number(d.cantidad) * Number(d.precioUnitario)).toFixed(2)}</td>
    </tr>
  `).join('')

  const subtotal = Number(venta.subTotal || venta.total || 0)
  const descuento = Number(venta.descuento || 0)
  const total = Number(venta.total || 0)

  const body = `
    ${buildHeader()}

    <div class="comp-info">
      <div class="comp-title">Nota de Venta</div>
      <div class="comp-number">${venta.serieComprobante || 'B001'}-${venta.numeroComprobante || ''}</div>
      <div class="comp-date">Fecha: ${formatDateTime(venta.fecha)}</div>
      <div class="comp-client">Cliente: ${venta.cliente?.nombreCompleto || 'Consumidor Final'}</div>
      <div class="comp-pay">Pago: ${venta.metodoPago || 'Efectivo'}</div>
      ${venta.cajero ? `<div class="comp-pay">Cajero: ${venta.cajero}</div>` : ''}
      ${venta.observaciones ? `<div class="comp-obs">Nota: ${venta.observaciones}</div>` : ''}
    </div>

    ${isAnulada ? '<div style="text-align:center"><div class="anulada-stamp">⛔ ANULADA ⛔</div></div>' : ''}

    <div class="section-div">--------------------------------</div>

    <table>
      <thead>
        <tr>
          <th class="left">Producto</th>
          <th class="center">Cant</th>
          <th class="right">P.U.</th>
          <th class="right">Total</th>
        </tr>
      </thead>
      <tbody>${filasProductos}</tbody>
      <tfoot>
        ${descuento > 0 ? `<tr class="subtotal-row"><td class="left" colspan="3">Subtotal</td><td class="right">S/${subtotal.toFixed(2)}</td></tr>` : ''}
        ${descuento > 0 ? `<tr class="discount-row"><td class="left" colspan="3">Descuento</td><td class="right">-S/${descuento.toFixed(2)}</td></tr>` : ''}
        <tr class="total-row">
          <td class="left" colspan="3">TOTAL</td>
          <td class="right">S/${total.toFixed(2)}</td>
        </tr>
      </tfoot>
    </table>

    ${buildFooter()}
  `

  openPrintWindow(body, `Ticket ${venta.serieComprobante}-${venta.numeroComprobante}`)
}

/**
 * Imprime un resumen de cierre de caja con las ventas visibles en el historial.
 * @param {Array} ventas — Array de ventas filtradas/mostradas en pantalla.
 * @param {object} filtros — { fecha?: string, cliente?: string, metodoPago?: string }
 */
export function imprimirCierreCaja(ventas, filtros = {}) {
  const ventasActivas = ventas.filter(v => v.estado !== 'Anulada')
  const ventasAnuladas = ventas.filter(v => v.estado === 'Anulada')

  const totalEfectivo = ventasActivas
    .filter(v => (v.metodoPago || 'Efectivo').toLowerCase() === 'efectivo')
    .reduce((s, v) => s + Number(v.total || 0), 0)

  const totalYape = ventasActivas
    .filter(v => (v.metodoPago || '').toLowerCase() === 'yape')
    .reduce((s, v) => s + Number(v.total || 0), 0)

  const totalPlin = ventasActivas
    .filter(v => (v.metodoPago || '').toLowerCase() === 'plin')
    .reduce((s, v) => s + Number(v.total || 0), 0)

  const totalGeneral = ventasActivas.reduce((s, v) => s + Number(v.total || 0), 0)

  const fechaImpresion = new Date().toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', hour12: true
  })

  const filtroPeriodo = filtros.fecha
    ? `Fecha: ${filtros.fecha}`
    : 'Período: Todo el historial'

  const body = `
    ${buildHeader()}

    <div class="comp-info">
      <div class="comp-title">Resumen de Ventas</div>
      <div class="comp-title" style="font-size:10px">Cierre de Caja</div>
      <div class="comp-date" style="margin-top:4px">Impreso: ${fechaImpresion}</div>
      <div class="comp-date">${filtroPeriodo}</div>
      ${filtros.cliente ? `<div class="comp-date">Cliente: ${filtros.cliente}</div>` : ''}
    </div>

    <div class="section-div">--------------------------------</div>

    <table class="summary-table">
      <tbody>
        <tr>
          <td class="summary-label">Ventas registradas:</td>
          <td class="summary-value">${ventasActivas.length}</td>
        </tr>
        <tr>
          <td class="summary-label">Ventas anuladas:</td>
          <td class="summary-value">${ventasAnuladas.length}</td>
        </tr>
      </tbody>
    </table>

    <div class="section-div">--------------------------------</div>

    <table class="summary-table">
      <tbody>
        ${totalEfectivo > 0 ? `<tr><td class="summary-label">💵 Efectivo:</td><td class="summary-value">S/ ${totalEfectivo.toFixed(2)}</td></tr>` : ''}
        ${totalYape > 0 ? `<tr><td class="summary-label">📱 Yape:</td><td class="summary-value">S/ ${totalYape.toFixed(2)}</td></tr>` : ''}
        ${totalPlin > 0 ? `<tr><td class="summary-label">📱 Plin:</td><td class="summary-value">S/ ${totalPlin.toFixed(2)}</td></tr>` : ''}
        <tr class="summary-big">
          <td class="summary-label">TOTAL GENERAL:</td>
          <td class="summary-value">S/ ${totalGeneral.toFixed(2)}</td>
        </tr>
      </tbody>
    </table>

    ${buildFooter()}
  `

  openPrintWindow(body, 'Cierre de Caja - Vida Animal')
}
