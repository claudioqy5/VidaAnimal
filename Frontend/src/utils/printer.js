// ============================================================
//  printer.js — Motor de Impresión Térmica 80mm · Vida Animal
// ============================================================

const STORE_INFO = {
  name: 'VIDA ANIMAL',
  businessName: 'VIDA ANIMAL',
  address: 'Jr. Atahualpa N° 291 - Aucayacu',
  phone: '975 418 965',
  ruc: '10764194883',
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
  }).toUpperCase()
}

/**
 * Convierte números a letras (para el total en soles)
 */
function numeroALetras(num) {
  const unidades = ['CERO', 'UNO', 'DOS', 'TRES', 'CUATRO', 'CINCO', 'SEIS', 'SIETE', 'OCHO', 'NUEVE'];
  const decenas = ['DIEZ', 'ONCE', 'DOCE', 'TRECE', 'CATORCE', 'QUINCE', 'DIECISEIS', 'DIECISIETE', 'DIECIOCHO', 'DIECINUEVE', 'VEINTE'];
  const decenas2 = ['VENTI', 'TREINTA', 'CUARENTA', 'CINCUENTA', 'SESENTA', 'SETENTA', 'OCHENTA', 'NOVENTA'];
  const centenas = ['CIEN', 'CIENTO', 'DOSCIENTOS', 'TRESCIENTOS', 'CUATROCIENTOS', 'QUINIENTOS', 'SEISCIENTOS', 'SETECIENTOS', 'OCHOCIENTOS', 'NOVECIENTOS'];

  function dec(n) {
    if (n < 10) return unidades[n];
    if (n <= 20) return decenas[n - 10];
    if (n < 30) return n === 20 ? 'VEINTE' : 'VEINTI' + unidades[n % 10];
    let d = Math.floor(n / 10);
    let u = n % 10;
    return decenas2[d - 1] + (u > 0 ? ' Y ' + unidades[u] : '');
  }

  function cen(n) {
    if (n < 100) return dec(n);
    if (n === 100) return 'CIEN';
    let c = Math.floor(n / 100);
    let r = n % 100;
    return centenas[c] + (r > 0 ? ' ' + dec(r) : '');
  }

  function mil(n) {
    if (n < 1000) return cen(n);
    let m = Math.floor(n / 1000);
    let r = n % 1000;
    let m_str = m === 1 ? 'MIL' : cen(m) + ' MIL';
    return m_str + (r > 0 ? ' ' + cen(r) : '');
  }

  let enteros = Math.floor(num);
  let centavos = Math.round((num - enteros) * 100);
  let centavosStr = centavos.toString().padStart(2, '0');
  
  if (enteros === 0) return 'CERO CON ' + centavosStr + '/100 SOLES';
  return mil(enteros) + ' CON ' + centavosStr + '/100 SOLES';
}

/**
 * Hoja de estilos CSS interna para el ticket de 80mm.
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
    -webkit-font-smoothing: none;
    -moz-osx-font-smoothing: grayscale;
    font-smooth: never;
    text-rendering: optimizeSpeed;
    color: #000 !important;
  }
  body {
    width: 76mm;
    font-size: 11px;
    background: #fff;
    padding-bottom: 20px;
  }
  
  .text-center { text-align: center; }
  .text-left { text-align: left; }
  .text-right { text-align: right; }
  .bold { font-weight: 700; }
  .bolder { font-weight: 900; }

  /* ── Header ── */
  .store-logo-container { text-align: center; margin-bottom: 5px; margin-top: 10px; }
  .store-logo {
    width: 60px;
    object-fit: contain;
    filter: grayscale(100%) contrast(200%) brightness(1.2);
  }
  .store-name { font-size: 14px; font-weight: 900; margin-bottom: 2px; }
  .store-info-text { font-size: 11px; font-weight: 600; line-height: 1.2; }
  
  /* ── Comprobante info ── */
  .comp-title-container { margin: 10px 0; }
  .comp-title { font-size: 13px; font-weight: 900; text-align: center; text-transform: uppercase; }
  .comp-number { font-size: 14px; font-weight: 900; text-align: center; margin-top: 2px; letter-spacing: 1px; }
  
  .client-info { font-size: 10px; font-weight: 600; line-height: 1.3; margin-bottom: 6px; }
  .client-row { display: flex; }
  .client-label { width: 120px; flex-shrink: 0; }

  /* ── Separadores ── */
  .section-div {
    width: 100%;
    border-top: 1px dashed #000;
    margin: 6px 0;
  }

  /* ── Tabla de productos ── */
  table { width: 100%; border-collapse: collapse; margin-bottom: 4px; }
  thead th { font-size: 9px; font-weight: 800; border-bottom: 1px dashed #000; padding-bottom: 4px; }
  th.left, td.left { text-align: left; }
  th.right, td.right { text-align: right; }
  th.center, td.center { text-align: center; }
  tbody td { font-size: 10px; padding: 4px 0; vertical-align: top; font-weight: 600; }
  .prod-name { max-width: 40mm; word-break: break-word; padding-right: 4px; padding-left: 4px; }
  
  .totals-container { width: 100%; display: flex; flex-direction: column; align-items: flex-end; font-size: 11px; font-weight: 700; margin-top: 4px; }
  .totals-row { display: flex; justify-content: flex-end; width: 100%; gap: 10px; }
  
  .amount-words { font-size: 10px; font-weight: 700; margin-top: 8px; line-height: 1.3; }

  /* ── QR y Footer ── */
  .qr-section { display: flex; align-items: flex-start; gap: 10px; margin-top: 10px; }
  .qr-code { width: 80px; height: 80px; }
  .obs-box { flex: 1; font-size: 10px; font-weight: 600; }
  
  .footer-legal { text-align: center; font-size: 10px; font-weight: 600; margin-top: 15px; line-height: 1.3; }
  
  /* Anulada */
  .anulada-stamp {
    text-align: center; font-size: 15px; font-weight: 900; letter-spacing: 2px;
    border: 2px solid #000; padding: 2px 8px; display: inline-block;
    margin: 4px auto; transform: rotate(-5deg);
  }
`

function openPrintWindow(bodyHtml, title = 'Ticket Vida Animal') {
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

  iframe.onload = () => {
    setTimeout(() => {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();
      setTimeout(() => {
        if (document.body.contains(iframe)) document.body.removeChild(iframe);
      }, 5000);
    }, 300);
  };
}

export function imprimirTicketVenta(venta) {
  const isAnulada = venta.estado === 'Anulada'

  const filasProductos = (venta.detalleVentas || []).map(d => `
    <tr>
      <td class="center">${Number(d.cantidad).toFixed(0)}</td>
      <td class="center">NIU</td>
      <td class="left prod-name">${d.producto?.nombre || 'PRODUCTO'}</td>
      <td class="right">${Number(d.precioUnitario).toFixed(2)}</td>
      <td class="right">${(Number(d.cantidad) * Number(d.precioUnitario)).toFixed(2)}</td>
    </tr>
  `).join('')

  const subtotal = Number(venta.subTotal || venta.total || 0)
  const descuento = Number(venta.descuento || 0)
  const total = Number(venta.total || 0)
  
  let isBoleta = venta.serieComprobante?.startsWith('B');
  let isFactura = venta.serieComprobante?.startsWith('F');
  
  let tipoComprobante = 'NOTA DE VENTA';
  if (isBoleta) tipoComprobante = 'BOLETA DE VENTA ELECTRONICA';
  if (isFactura) tipoComprobante = 'FACTURA ELECTRONICA';

  let docLabel = isFactura ? 'R.U.C.' : 'D.N.I.';
  let clientDoc = venta.cliente?.documento || '00000000';
  let clientName = venta.cliente?.nombreCompleto || 'PUBLICO GENERAL';
  
  // Generar QR Dinámico
  // Formato: RUC | Tipo Doc | Serie | Numero | IGV | Total | Fecha | Tipo Doc Cliente | Numero Doc Cliente
  let qrData = \`\${STORE_INFO.ruc}|\${isBoleta ? '03' : (isFactura ? '01' : '00')}|\${venta.serieComprobante || '000'}|\${venta.numeroComprobante || '000'}|0.00|\${total.toFixed(2)}|\${(venta.fecha || '').substring(0,10)}|\${isFactura ? '6' : '1'}|\${clientDoc}\`;
  let qrUrl = \`https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=\${encodeURIComponent(qrData)}\`;

  const body = `
    <div class="store-logo-container">
      <img class="store-logo" src="${STORE_INFO.logoUrl}" alt="Logo" onerror="this.style.display='none'" />
    </div>
    
    <div class="text-center store-info-text">
      <div class="store-name">${STORE_INFO.businessName}</div>
      <div>R.U.C: ${STORE_INFO.ruc}</div>
      <div>${STORE_INFO.address}</div>
      <div>Telf.: ${STORE_INFO.phone}</div>
    </div>

    <div class="comp-title-container">
      <div class="comp-title">${tipoComprobante}</div>
      <div class="comp-number">${venta.serieComprobante || 'N001'} - ${venta.numeroComprobante || ''}</div>
    </div>

    <div class="client-info">
      <div class="client-row">
        <span class="client-label">FECHA DE EMISIÓN:</span>
        <span>${formatDateTime(venta.fecha)}</span>
      </div>
      <div class="client-row">
        <span class="client-label">SEÑOR (ES):</span>
        <span>${clientName.toUpperCase()}</span>
      </div>
      <div class="client-row">
        <span class="client-label">${docLabel}:</span>
        <span>${clientDoc}</span>
      </div>
      <div class="client-row">
        <span class="client-label">DIREC:</span>
        <span>${venta.cliente?.direccion ? venta.cliente.direccion.toUpperCase() : '-'}</span>
      </div>
      <div class="client-row">
        <span class="client-label">FORMA DE PAGO:</span>
        <span>CONTADO - ${venta.metodoPago ? venta.metodoPago.toUpperCase() : 'EFECTIVO'}</span>
      </div>
    </div>

    ${isAnulada ? '<div style="text-align:center"><div class="anulada-stamp">⛔ ANULADA ⛔</div></div>' : ''}

    <div class="section-div"></div>

    <table>
      <thead>
        <tr>
          <th class="center">CT.</th>
          <th class="center">U.M</th>
          <th class="left" style="padding-left:4px;">DESCRIPCIÓN</th>
          <th class="right">P.U</th>
          <th class="right">IMP.</th>
        </tr>
      </thead>
      <tbody>${filasProductos}</tbody>
    </table>

    <div class="section-div"></div>

    <div class="totals-container">
      ${descuento > 0 ? `<div class="totals-row"><span>Descuento:</span><span>S/ ${descuento.toFixed(2)}</span></div>` : ''}
      <div class="totals-row">
        <span>Exonerado:</span>
        <span>S/ ${subtotal.toFixed(2)}</span>
      </div>
      <div class="totals-row">
        <span>Total a Pagar:</span>
        <span>S/ ${total.toFixed(2)}</span>
      </div>
    </div>

    <div class="section-div"></div>
    
    <div class="amount-words">
      SON ${numeroALetras(total)}
    </div>

    <div class="qr-section">
      <img class="qr-code" src="${qrUrl}" alt="QR" />
      <div class="obs-box">
        <div class="bold" style="margin-bottom: 2px;">Observación:</div>
        <div>${venta.observaciones ? venta.observaciones.toUpperCase() : ''}</div>
      </div>
    </div>

    <div class="footer-legal">
      Representación Impresa del Comprobante Electrónico<br>
      Consulte su Documento en:<br>
      https://${STORE_INFO.web}<br>
      <div style="margin-top: 4px;">HASH: ${venta.sunatHash || ''}</div>
      <div style="margin-top: 2px;">VENDEDOR: ${venta.cajero ? venta.cajero.toUpperCase() : STORE_INFO.businessName}</div>
    </div>
  `

  openPrintWindow(body, `Ticket ${venta.serieComprobante}-${venta.numeroComprobante}`)
}

export function imprimirCierreCaja(ventas, filtros = {}) {
  // (Cierre de caja sin cambios, solo adaptado al nuevo header)
  const ventasActivas = ventas.filter(v => v.estado !== 'Anulada')
  const totalEfectivo = ventasActivas.filter(v => (v.metodoPago || 'Efectivo').toLowerCase() === 'efectivo').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalYape = ventasActivas.filter(v => (v.metodoPago || '').toLowerCase() === 'yape').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalPlin = ventasActivas.filter(v => (v.metodoPago || '').toLowerCase() === 'plin').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalGeneral = ventasActivas.reduce((s, v) => s + Number(v.total || 0), 0)

  const body = `
    <div class="store-logo-container"><img class="store-logo" src="${STORE_INFO.logoUrl}" onerror="this.style.display='none'" /></div>
    <div class="text-center store-info-text">
      <div class="store-name">${STORE_INFO.businessName}</div>
      <div>R.U.C: ${STORE_INFO.ruc}</div>
    </div>
    <div class="comp-title-container">
      <div class="comp-title">CIERRE DE CAJA</div>
      <div class="comp-number">${formatDateTime(new Date())}</div>
    </div>
    <div class="section-div"></div>
    <table style="font-size: 12px; margin: 10px 0;">
      <tr><td class="bold">💵 Efectivo:</td><td class="right bolder">S/ ${totalEfectivo.toFixed(2)}</td></tr>
      <tr><td class="bold">📱 Yape:</td><td class="right bolder">S/ ${totalYape.toFixed(2)}</td></tr>
      <tr><td class="bold">📱 Plin:</td><td class="right bolder">S/ ${totalPlin.toFixed(2)}</td></tr>
      <tr><td colspan="2"><div class="section-div"></div></td></tr>
      <tr><td class="bold" style="font-size: 14px;">TOTAL:</td><td class="right bolder" style="font-size: 14px;">S/ ${totalGeneral.toFixed(2)}</td></tr>
    </table>
  `
  openPrintWindow(body, 'Cierre de Caja - Vida Animal')
}

