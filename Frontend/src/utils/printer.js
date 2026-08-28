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
 * Convierte números a letras (para el total en soles).
 * Soporta hasta 999,999.99
 */
function numeroALetras(num) {
  const unidades = ['CERO', 'UNO', 'DOS', 'TRES', 'CUATRO', 'CINCO', 'SEIS', 'SIETE', 'OCHO', 'NUEVE']
  const especiales = ['DIEZ', 'ONCE', 'DOCE', 'TRECE', 'CATORCE', 'QUINCE', 'DIECISEIS', 'DIECISIETE', 'DIECIOCHO', 'DIECINUEVE', 'VEINTE']
  const decenas2 = ['', '', 'VEINTI', 'TREINTA', 'CUARENTA', 'CINCUENTA', 'SESENTA', 'SETENTA', 'OCHENTA', 'NOVENTA']
  const centenas = ['', 'CIENTO', 'DOSCIENTOS', 'TRESCIENTOS', 'CUATROCIENTOS', 'QUINIENTOS', 'SEISCIENTOS', 'SETECIENTOS', 'OCHOCIENTOS', 'NOVECIENTOS']

  function convertirDecenas(n) {
    if (n < 10) return unidades[n]
    if (n <= 20) return especiales[n - 10]
    if (n < 30) return 'VEINTI' + unidades[n % 10]
    const d = Math.floor(n / 10)
    const u = n % 10
    return decenas2[d] + (u > 0 ? ' Y ' + unidades[u] : '')
  }

  function convertirCentenas(n) {
    if (n < 100) return convertirDecenas(n)
    if (n === 100) return 'CIEN'
    const c = Math.floor(n / 100)
    const r = n % 100
    return centenas[c] + (r > 0 ? ' ' + convertirDecenas(r) : '')
  }

  function convertirMiles(n) {
    if (n < 1000) return convertirCentenas(n)
    const m = Math.floor(n / 1000)
    const r = n % 1000
    const mStr = m === 1 ? 'MIL' : convertirCentenas(m) + ' MIL'
    return mStr + (r > 0 ? ' ' + convertirCentenas(r) : '')
  }

  const enteros = Math.floor(num)
  const centavos = Math.round((num - enteros) * 100)
  const centavosStr = centavos.toString().padStart(2, '0')

  if (enteros === 0) return 'CERO CON ' + centavosStr + '/100 SOLES'
  return 'SON ' + convertirMiles(enteros) + ' CON ' + centavosStr + '/100 SOLES'
}

/**
 * Genera la URL de un código QR dinámico usando el estándar SUNAT.
 */
function generarQrUrl(venta, total, isBoleta, isFactura, clientDoc) {
  const tipoDoc = isBoleta ? '03' : (isFactura ? '01' : '00')
  const tipoDocCliente = isFactura ? '6' : '1'
  const serie = venta.serieComprobante || '000'
  const numero = venta.numeroComprobante || '000'
  const fecha = (venta.fecha || '').substring(0, 10)

  const qrData = STORE_INFO.ruc + '|' + tipoDoc + '|' + serie + '|' + numero + '|0.00|' + total.toFixed(2) + '|' + fecha + '|' + tipoDocCliente + '|' + clientDoc
  return 'https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=' + encodeURIComponent(qrData)
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

/**
 * Abre el diálogo de impresión usando un iframe oculto.
 */
function openPrintWindow(bodyHtml, title = 'Ticket Vida Animal') {
  const iframe = document.createElement('iframe')
  iframe.style.position = 'fixed'
  iframe.style.right = '0'
  iframe.style.bottom = '0'
  iframe.style.width = '0'
  iframe.style.height = '0'
  iframe.style.border = '0'
  document.body.appendChild(iframe)

  const doc = iframe.contentWindow.document
  doc.open()
  doc.write(
    '<!DOCTYPE html><html><head><meta charset="UTF-8" /><title>' + title + '</title><style>' + TICKET_CSS + '</style></head><body>' + bodyHtml + '</body></html>'
  )
  doc.close()

  iframe.onload = () => {
    setTimeout(() => {
      iframe.contentWindow.focus()
      iframe.contentWindow.print()
      setTimeout(() => {
        if (document.body.contains(iframe)) document.body.removeChild(iframe)
      }, 5000)
    }, 300)
  }
}

/**
 * Imprime el ticket de una venta (Boleta, Factura o Nota de Venta).
 */
export function imprimirTicketVenta(venta) {
  const isAnulada = venta.estado === 'Anulada'

  const filasProductos = (venta.detalleVentas || []).map(d => {
    const cant = Number(d.cantidad).toFixed(0)
    const pu = Number(d.precioUnitario).toFixed(2)
    const imp = (Number(d.cantidad) * Number(d.precioUnitario)).toFixed(2)
    const nombre = (d.producto?.nombre || 'PRODUCTO').toUpperCase()
    return '<tr>' +
      '<td class="center">' + cant + '</td>' +
      '<td class="center">NIU</td>' +
      '<td class="left prod-name">' + nombre + '</td>' +
      '<td class="right">' + pu + '</td>' +
      '<td class="right">' + imp + '</td>' +
      '</tr>'
  }).join('')

  const subtotal = Number(venta.subTotal || venta.total || 0)
  const descuento = Number(venta.descuento || 0)
  const total = Number(venta.total || 0)

  const isBoleta = venta.serieComprobante?.startsWith('B')
  const isFactura = venta.serieComprobante?.startsWith('F')

  let tipoComprobante = 'NOTA DE VENTA'
  if (isBoleta) tipoComprobante = 'BOLETA DE VENTA ELECTRONICA'
  if (isFactura) tipoComprobante = 'FACTURA ELECTRONICA'

  const docLabel = isFactura ? 'R.U.C.' : 'D.N.I.'
  const clientDoc = venta.cliente?.documento || '00000000'
  const clientName = (venta.cliente?.nombreCompleto || 'PUBLICO GENERAL').toUpperCase()
  const clientDir = venta.cliente?.direccion ? venta.cliente.direccion.toUpperCase() : '-'
  const metodoPago = venta.metodoPago ? venta.metodoPago.toUpperCase() : 'EFECTIVO'
  const cajeroName = venta.cajero ? venta.cajero.toUpperCase() : STORE_INFO.businessName
  const observaciones = venta.observaciones ? venta.observaciones.toUpperCase() : ''
  const sunatHash = venta.sunatHash || ''

  // QR dinámico
  const qrUrl = generarQrUrl(venta, total, isBoleta, isFactura, clientDoc)

  // Serie y número
  const serie = venta.serieComprobante || 'N001'
  const numero = venta.numeroComprobante || ''

  const body =
    // ── Header con logo y datos del negocio ──
    '<div class="store-logo-container">' +
      '<img class="store-logo" src="' + STORE_INFO.logoUrl + '" alt="Logo" onerror="this.style.display=\'none\'" />' +
    '</div>' +

    '<div class="text-center store-info-text">' +
      '<div class="store-name">' + STORE_INFO.businessName + '</div>' +
      '<div>R.U.C: ' + STORE_INFO.ruc + '</div>' +
      '<div>' + STORE_INFO.address + '</div>' +
      '<div>Telf.: ' + STORE_INFO.phone + '</div>' +
    '</div>' +

    // ── Tipo de comprobante y número ──
    '<div class="comp-title-container">' +
      '<div class="comp-title">' + tipoComprobante + '</div>' +
      '<div class="comp-number">' + serie + ' - ' + numero + '</div>' +
    '</div>' +

    // ── Datos del cliente ──
    '<div class="client-info">' +
      '<div class="client-row"><span class="client-label">FECHA DE EMISIÓN:</span><span>' + formatDateTime(venta.fecha) + '</span></div>' +
      '<div class="client-row"><span class="client-label">SEÑOR (ES):</span><span>' + clientName + '</span></div>' +
      '<div class="client-row"><span class="client-label">' + docLabel + ':</span><span>' + clientDoc + '</span></div>' +
      '<div class="client-row"><span class="client-label">DIREC:</span><span>' + clientDir + '</span></div>' +
      '<div class="client-row"><span class="client-label">FORMA DE PAGO:</span><span>CONTADO - ' + metodoPago + '</span></div>' +
    '</div>' +

    // ── Sello de anulada ──
    (isAnulada ? '<div style="text-align:center"><div class="anulada-stamp">⛔ ANULADA ⛔</div></div>' : '') +

    '<div class="section-div"></div>' +

    // ── Tabla de productos ──
    '<table>' +
      '<thead><tr>' +
        '<th class="center">CT.</th>' +
        '<th class="center">U.M</th>' +
        '<th class="left" style="padding-left:4px;">DESCRIPCIÓN</th>' +
        '<th class="right">P.U</th>' +
        '<th class="right">IMP.</th>' +
      '</tr></thead>' +
      '<tbody>' + filasProductos + '</tbody>' +
    '</table>' +

    '<div class="section-div"></div>' +

    // ── Totales ──
    '<div class="totals-container">' +
      (descuento > 0 ? '<div class="totals-row"><span>Descuento:</span><span>S/ ' + descuento.toFixed(2) + '</span></div>' : '') +
      '<div class="totals-row"><span>Exonerado:</span><span>S/ ' + subtotal.toFixed(2) + '</span></div>' +
      '<div class="totals-row"><span>Total a Pagar:</span><span>S/ ' + total.toFixed(2) + '</span></div>' +
    '</div>' +

    '<div class="section-div"></div>' +

    // ── Monto en letras ──
    '<div class="amount-words">' + numeroALetras(total) + '</div>' +

    // ── QR + Observaciones ──
    '<div class="qr-section">' +
      '<img class="qr-code" src="' + qrUrl + '" alt="QR" />' +
      '<div class="obs-box">' +
        '<div class="bold" style="margin-bottom: 2px;">Observación:</div>' +
        '<div>' + observaciones + '</div>' +
      '</div>' +
    '</div>' +

    // ── Pie legal ──
    '<div class="footer-legal">' +
      (isBoleta || isFactura
        ? 'Representación Impresa del Comprobante Electrónico<br>Consulte su Documento en:<br><strong>https://vidaanimal.vercel.app/consultaboleta</strong><br>'
        : ''
      ) +
      '<div style="margin-top: 4px;">HASH: ' + sunatHash + '</div>' +
      '<div style="margin-top: 2px;">VENDEDOR: ' + cajeroName + '</div>' +
    '</div>'

  openPrintWindow(body, 'Ticket ' + serie + '-' + numero)
}

/**
 * Imprime un resumen de cierre de caja.
 */
export function imprimirCierreCaja(ventas, filtros = {}) {
  const ventasActivas = ventas.filter(v => v.estado !== 'Anulada')
  const totalEfectivo = ventasActivas.filter(v => (v.metodoPago || 'Efectivo').toLowerCase() === 'efectivo').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalYape = ventasActivas.filter(v => (v.metodoPago || '').toLowerCase() === 'yape').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalPlin = ventasActivas.filter(v => (v.metodoPago || '').toLowerCase() === 'plin').reduce((s, v) => s + Number(v.total || 0), 0)
  const totalGeneral = ventasActivas.reduce((s, v) => s + Number(v.total || 0), 0)

  const body =
    '<div class="store-logo-container"><img class="store-logo" src="' + STORE_INFO.logoUrl + '" onerror="this.style.display=\'none\'" /></div>' +
    '<div class="text-center store-info-text">' +
      '<div class="store-name">' + STORE_INFO.businessName + '</div>' +
      '<div>R.U.C: ' + STORE_INFO.ruc + '</div>' +
    '</div>' +
    '<div class="comp-title-container">' +
      '<div class="comp-title">CIERRE DE CAJA</div>' +
      '<div class="comp-number">' + formatDateTime(new Date()) + '</div>' +
    '</div>' +
    '<div class="section-div"></div>' +
    '<table style="font-size: 12px; margin: 10px 0;">' +
      '<tr><td class="bold">💵 Efectivo:</td><td class="right bolder">S/ ' + totalEfectivo.toFixed(2) + '</td></tr>' +
      '<tr><td class="bold">📱 Yape:</td><td class="right bolder">S/ ' + totalYape.toFixed(2) + '</td></tr>' +
      '<tr><td class="bold">📱 Plin:</td><td class="right bolder">S/ ' + totalPlin.toFixed(2) + '</td></tr>' +
      '<tr><td colspan="2"><div class="section-div"></div></td></tr>' +
      '<tr><td class="bold" style="font-size: 14px;">TOTAL:</td><td class="right bolder" style="font-size: 14px;">S/ ' + totalGeneral.toFixed(2) + '</td></tr>' +
    '</table>'

  openPrintWindow(body, 'Cierre de Caja - Vida Animal')
}
