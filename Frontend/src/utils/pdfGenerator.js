import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import QRCode from 'qrcode';

// Utilidad para convertir números a letras (para el total)
const numeroALetras = (numero) => {
    let unida = ["", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE", "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE"];
    let dece = ["", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"];
    let cente = ["", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"];
    
    // Función recursiva simplificada (soporta hasta 999,999)
    const convertirGrupo = (n) => {
        let output = "";
        if (n == 100) return "CIEN";
        if (n === 0) return "CERO";
        if (n > 99) {
            output += cente[Math.floor(n / 100)] + " ";
            n = n % 100;
        }
        if (n > 20) {
            output += dece[Math.floor(n / 10)] + (n % 10 > 0 ? " Y " + unida[n % 10] : "");
        } else if (n > 0) {
            output += unida[n];
        }
        return output.trim();
    };

    let integerPart = Math.floor(numero);
    let decimalPart = Math.round((numero - integerPart) * 100).toString().padStart(2, '0');
    
    let result = "";
    if (integerPart > 999) {
        let miles = Math.floor(integerPart / 1000);
        integerPart = integerPart % 1000;
        if (miles === 1) result += "MIL ";
        else result += convertirGrupo(miles) + " MIL ";
    }
    result += convertirGrupo(integerPart);
    
    return `${result} CON ${decimalPart}/100 SOLES`;
};

const formatDate = (fecha) => {
    const d = new Date(fecha);
    return `${d.toLocaleDateString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric' })} / ${d.toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit', hour12: true })}`;
};

export const generateA4PDF = async (venta) => {
    const doc = new jsPDF('p', 'mm', 'a4');
    const width = doc.internal.pageSize.getWidth();
    
    // Configuraciones de empresa
    const EMPRESA = "VIDA ANIMAL";
    const RUC = "10764194883";
    const DIR = "JR. ATAHUALPA N° 291 - AUCAYACU";
    const TEL = "975 418 965";
    
    const docType = venta.serieComprobante?.startsWith('F') ? 'FACTURA' : 'BOLETA';
    const numDoc = `${venta.serieComprobante} - ${venta.numeroComprobante}`;

    // Cabecera Empresa
    doc.setFontSize(18);
    doc.setFont("helvetica", "bold");
    doc.text(EMPRESA, 105, 20, { align: "center" });
    
    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    doc.text(DIR, 105, 26, { align: "center" });
    doc.text(`Telf.: ${TEL}`, 105, 31, { align: "center" });

    // Recuadro RUC (Derecha superior)
    doc.roundedRect(135, 12, 65, 25, 2, 2);
    doc.setFontSize(11);
    doc.setFont("helvetica", "bold");
    doc.text(`R.U.C. ${RUC}`, 167.5, 18, { align: "center" });
    doc.text(`${docType} DE VENTA`, 167.5, 24, { align: "center" });
    doc.text(`ELECTRÓNICA`, 167.5, 29, { align: "center" });
    doc.text(numDoc, 167.5, 34, { align: "center" });

    // Datos Cliente
    doc.setFontSize(9);
    doc.setFont("helvetica", "bold");
    doc.text("CLIENTE:", 15, 45);
    doc.text(docType === "FACTURA" ? "R.U.C.:" : "D.N.I.:", 15, 50);
    doc.text("DIRECCIÓN:", 15, 55);
    doc.text("TIPO PAGO:", 15, 60);

    doc.setFont("helvetica", "normal");
    doc.text(venta.cliente?.nombreCompleto || 'PÚBLICO GENERAL', 40, 45);
    doc.text(venta.cliente?.documentoIdentidad || '---', 40, 50);
    doc.text(venta.cliente?.direccion || '---', 40, 55);
    doc.text(venta.metodoPago || 'CONTADO - EFECTIVO', 40, 60);

    // Tabla Info Adicional (Fecha, moneda)
    autoTable(doc, {
        startY: 65,
        head: [["FECHA DE EMISIÓN", "CONDICIÓN DE PAGO", "TIPO DE MONEDA", "CAJERO"]],
        body: [[formatDate(venta.fecha), venta.metodoPago || 'EFECTIVO', 'Soles', venta.cajero || 'SISTEMA']],
        theme: 'grid',
        headStyles: { fillColor: [240, 240, 240], textColor: [0, 0, 0], fontSize: 8, halign: 'center', fontStyle: 'bold' },
        bodyStyles: { fontSize: 8, halign: 'center' },
        margin: { left: 15, right: 15 }
    });

    // Tabla Productos
    const finalYInfo = doc.lastAutoTable.finalY + 5;
    
    const tableColumn = ["CANT.", "UNID/MED", "DESCRIPCIÓN", "PRECIO U.", "IGV", "IMPORTE"];
    const tableRows = venta.detalleVentas.map(d => {
        const isExonerado = true; // Todo exonerado en este sistema
        const igvRow = isExonerado ? "0.0000" : (d.precioUnitario * 0.18).toFixed(4);
        return [
            d.cantidad.toString(),
            d.unidadVenta || 'UND',
            d.producto?.nombre || 'Producto',
            `S/ ${Number(d.precioUnitario).toFixed(2)}`,
            igvRow,
            `S/ ${(d.cantidad * d.precioUnitario).toFixed(2)}`
        ];
    });

    autoTable(doc, {
        startY: finalYInfo,
        head: [tableColumn],
        body: tableRows,
        theme: 'grid',
        headStyles: { fillColor: [220, 230, 240], textColor: [0, 0, 0], fontSize: 8, halign: 'center', fontStyle: 'bold' },
        bodyStyles: { fontSize: 8 },
        columnStyles: {
            0: { halign: 'center', cellWidth: 15 },
            1: { halign: 'center', cellWidth: 20 },
            3: { halign: 'right', cellWidth: 25 },
            4: { halign: 'right', cellWidth: 20 },
            5: { halign: 'right', cellWidth: 25 }
        },
        margin: { left: 15, right: 15 }
    });

    let finalY = doc.lastAutoTable.finalY;

    // Total en Letras
    doc.setDrawColor(200);
    doc.setFillColor(245, 245, 245);
    doc.rect(15, finalY, width - 30, 6, 'FD');
    doc.setFont("helvetica", "bold");
    doc.setFontSize(8);
    doc.text(`SON: ${numeroALetras(venta.total)}`, 17, finalY + 4);

    finalY += 10;

    // Generar QR
    const qrText = `${RUC}|${docType==='FACTURA'?'01':'03'}|${venta.serieComprobante}|${venta.numeroComprobante}|0.00|${venta.total}|${venta.fecha.split('T')[0]}|${venta.cliente?.documentoIdentidad? '1':'0'}|${venta.cliente?.documentoIdentidad||'-'}`;
    const qrDataUrl = await QRCode.toDataURL(qrText, { margin: 0, width: 35 });
    doc.addImage(qrDataUrl, 'PNG', 15, finalY, 35, 35);

    // Notas y Enlace
    doc.setFont("helvetica", "normal");
    doc.setFontSize(8);
    doc.text("Consulte su documento electrónico en:", 55, finalY + 5);
    doc.setTextColor(0, 0, 255);
    doc.text("https://vidaanimal.vercel.app/consultaboleta", 55, finalY + 10);
    doc.setTextColor(0, 0, 0);
    
    // Recuadro de Resumen
    const resumenX = 135;
    doc.roundedRect(resumenX, finalY, 60, 30, 2, 2);
    
    doc.setFont("helvetica", "normal");
    doc.text("Gravada:", resumenX + 2, finalY + 6);
    doc.text("Exonerada:", resumenX + 2, finalY + 11);
    doc.text("IGV (18.00%):", resumenX + 2, finalY + 16);
    doc.text("Descuento Total:", resumenX + 2, finalY + 21);
    
    doc.setFont("helvetica", "bold");
    doc.text("Total:", resumenX + 2, finalY + 27);

    // Valores (Alineados derecha)
    doc.setFont("helvetica", "normal");
    doc.text("S/ 0.00", resumenX + 55, finalY + 6, { align: "right" });
    doc.text(`S/ ${Number(venta.subTotal).toFixed(2)}`, resumenX + 55, finalY + 11, { align: "right" });
    doc.text("S/ 0.00", resumenX + 55, finalY + 16, { align: "right" });
    doc.text(`S/ ${Number(venta.descuento || 0).toFixed(2)}`, resumenX + 55, finalY + 21, { align: "right" });
    
    doc.setFont("helvetica", "bold");
    doc.text(`S/ ${Number(venta.total).toFixed(2)}`, resumenX + 55, finalY + 27, { align: "right" });

    // Guardar
    doc.save(`Venta_${venta.serieComprobante}-${venta.numeroComprobante}.pdf`);
};

export const generateTicketPDF = (venta) => {
    // Ticket de 80mm de ancho. (80mm es aprox 3.15 pulgadas. usaremos mm)
    const doc = new jsPDF('p', 'mm', [80, 250]); 
    const EMPRESA = "VIDA ANIMAL";
    const RUC = "10764194883";
    const DIR = "JR. ATAHUALPA N° 291 - AUCAYACU";
    const TEL = "Telf.: 975 418 965";
    const docType = venta.serieComprobante?.startsWith('F') ? 'FACTURA' : 'BOLETA';
    const numDoc = `${venta.serieComprobante} - ${venta.numeroComprobante}`;
    
    let y = 10;
    
    doc.setFontSize(12);
    doc.setFont("helvetica", "bold");
    doc.text(EMPRESA, 40, y, { align: "center" });
    y += 5;
    
    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text(`RUC: ${RUC}`, 40, y, { align: "center" });
    y += 5;
    doc.text(DIR, 40, y, { align: "center" });
    y += 5;
    doc.text(TEL, 40, y, { align: "center" });
    y += 8;
    
    doc.setFont("helvetica", "bold");
    doc.text(`${docType} ELECTRÓNICA`, 40, y, { align: "center" });
    y += 5;
    doc.text(numDoc, 40, y, { align: "center" });
    y += 8;
    
    doc.setFontSize(8);
    doc.setFont("helvetica", "normal");
    doc.text(`FECHA: ${formatDate(venta.fecha)}`, 5, y);
    y += 5;
    doc.text(`CLIENTE: ${venta.cliente?.nombreCompleto || 'PÚBLICO GENERAL'}`, 5, y);
    y += 5;
    doc.text(`DOC: ${venta.cliente?.documentoIdentidad || '---'}`, 5, y);
    y += 5;
    doc.text(`CAJERO: ${venta.cajero || 'SISTEMA'}`, 5, y);
    y += 5;
    
    doc.setLineDash([1, 1]);
    doc.line(5, y, 75, y);
    y += 5;
    
    doc.setFont("helvetica", "bold");
    doc.text("CANT DESCRIPCIÓN          TOTAL", 5, y);
    y += 2;
    doc.line(5, y, 75, y);
    y += 5;
    
    doc.setFont("helvetica", "normal");
    venta.detalleVentas.forEach(d => {
        let name = d.producto?.nombre || 'Producto';
        if (name.length > 18) name = name.substring(0, 18) + ".";
        
        doc.text(`${d.cantidad}`, 5, y);
        doc.text(name, 15, y);
        const sub = `S/ ${(d.cantidad * d.precioUnitario).toFixed(2)}`;
        doc.text(sub, 75, y, { align: "right" });
        y += 5;
    });
    
    doc.line(5, y, 75, y);
    y += 5;
    
    doc.text(`Exonerado:`, 30, y);
    doc.text(`S/ ${Number(venta.subTotal).toFixed(2)}`, 75, y, { align: "right" });
    y += 5;
    
    doc.setFont("helvetica", "bold");
    doc.setFontSize(10);
    doc.text(`TOTAL A PAGAR:`, 20, y);
    doc.text(`S/ ${Number(venta.total).toFixed(2)}`, 75, y, { align: "right" });
    y += 10;
    
    doc.setFontSize(7);
    doc.setFont("helvetica", "normal");
    const enLetras = numeroALetras(venta.total);
    const splitLetras = doc.splitTextToSize(`SON: ${enLetras}`, 70);
    doc.text(splitLetras, 40, y, { align: "center" });
    y += 10;
    
    doc.text("Consulte su documento electrónico en:", 40, y, { align: "center" });
    y += 4;
    doc.text("https://vidaanimal.vercel.app/consultaboleta", 40, y, { align: "center" });
    
    // Save with precise content height
    doc.save(`Ticket_${numDoc}.pdf`);
};
