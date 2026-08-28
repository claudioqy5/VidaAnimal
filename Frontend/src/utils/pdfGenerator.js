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

import logoUrl from '../assets/logovidaanimal.png';

const loadLogo = async () => {
    return new Promise((resolve) => {
        const img = new Image();
        img.crossOrigin = 'Anonymous';
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.width;
            canvas.height = img.height;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0);
            resolve(canvas.toDataURL('image/png'));
        };
        img.onerror = (e) => {
            console.error("Error cargando el logo", e);
            resolve(null);
        };
        img.src = logoUrl;
    });
};

export const generateA4PDF = async (venta) => {
    const doc = new jsPDF('p', 'mm', 'a4');
    const width = doc.internal.pageSize.getWidth();
    
    // Cargar logo
    const logoBase64 = await loadLogo();

    // Configuraciones de empresa
    const EMPRESA = "VIDA ANIMAL";
    const RUC = "10764194883";
    const DIR = "Jr. Atahualpa N° 291 - Aucayacu";
    const DIR2 = "José Crespo y Castillo, Leoncio Prado, Huánuco";
    const TEL = "975 418 965";
    
    const serie = venta.serieComprobante || '';
    const docType = serie.startsWith('F') ? 'FACTURA' : serie.startsWith('B') ? 'BOLETA' : 'NOTA DE VENTA';
    const isElectronica = docType === 'BOLETA' || docType === 'FACTURA';
    const numDoc = `${venta.serieComprobante} - ${venta.numeroComprobante}`;

    // Dibujar Logo si existe
    if (logoBase64) {
        doc.addImage(logoBase64, 'PNG', 15, 14, 25, 25);
    }

    // Cabecera Empresa
    doc.setFontSize(18);
    doc.setFont("helvetica", "bold");
    doc.text(EMPRESA, 90, 18, { align: "center" });
    
    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text(DIR, 90, 24, { align: "center" });
    doc.text(DIR2, 90, 29, { align: "center" });
    doc.text(`Telf.: ${TEL}`, 90, 34, { align: "center" });

    // Recuadro RUC (Derecha superior)
    doc.roundedRect(135, 12, 60, 25, 2, 2);
    doc.setFontSize(11);
    doc.setFont("helvetica", "bold");
    doc.text(`R.U.C. ${RUC}`, 165, 18, { align: "center" });
    if (isElectronica) {
        doc.text(`${docType} DE VENTA`, 165, 24, { align: "center" });
        doc.text(`ELECTRÓNICA`, 165, 29, { align: "center" });
        doc.text(numDoc, 165, 34, { align: "center" });
    } else {
        doc.text(`${docType}`, 165, 25, { align: "center" });
        doc.text(numDoc, 165, 31, { align: "center" });
    }

    // Datos Cliente
    doc.setFontSize(9);
    doc.setFont("helvetica", "bold");
    doc.text("CLIENTE:", 15, 45);
    doc.text(docType === "FACTURA" ? "R.U.C.:" : "D.N.I./DOC.:", 15, 50);
    doc.text("DIRECCIÓN:", 15, 55);
    doc.text("TIPO PAGO:", 15, 60);

    doc.setFont("helvetica", "normal");
    doc.text(venta.cliente?.nombreCompleto || 'PÚBLICO GENERAL', 40, 45);
    doc.text(venta.cliente?.documentoIdentidad || '---', 40, 50);
    doc.text(venta.cliente?.direccion || '---', 40, 55);
    doc.text(venta.metodoPago || 'CONTADO - EFECTIVO', 40, 60);

    // Tabla Info Adicional (Fecha, moneda)
    const lineColor = [150, 160, 170];
    autoTable(doc, {
        startY: 65,
        head: [["FECHA DE EMISIÓN", "CONDICIÓN DE PAGO", "TIPO DE MONEDA", "CAJERO"]],
        body: [[formatDate(venta.fecha), venta.metodoPago || 'EFECTIVO', 'Soles', (venta.cajero || 'SISTEMA').toUpperCase()]],
        theme: 'grid',
        headStyles: { fillColor: [240, 245, 250], textColor: [0, 0, 0], fontSize: 8, halign: 'center', fontStyle: 'bold', lineColor: lineColor, lineWidth: 0.2 },
        bodyStyles: { fontSize: 8, halign: 'center', lineColor: lineColor, lineWidth: 0.2 },
        margin: { left: 15, right: 15 },
        didDrawPage: function (data) {
            // Draw rounded border safely
            const rx = 2;
            const x = data.settings.margin.left;
            const y = data.pageNumber === 1 ? data.settings.startY : data.settings.margin.top;
            const w = doc.internal.pageSize.getWidth() - data.settings.margin.left - data.settings.margin.right;
            const h = data.cursor.y - y;
            if (!h || isNaN(h)) return;
            doc.setFillColor(255, 255, 255);
            doc.rect(x-0.2, y-0.2, rx+0.2, rx+0.2, 'F'); doc.rect(x + w - rx, y-0.2, rx+0.2, rx+0.2, 'F');
            doc.rect(x-0.2, y + h - rx, rx+0.2, rx+0.2, 'F'); doc.rect(x + w - rx, y + h - rx, rx+0.2, rx+0.2, 'F');
            doc.setDrawColor(lineColor[0], lineColor[1], lineColor[2]);
            doc.setLineWidth(0.2);
            doc.roundedRect(x, y, w, h, rx, rx, 'S');
        }
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
        headStyles: { fillColor: [220, 230, 240], textColor: [0, 0, 0], fontSize: 8, halign: 'center', fontStyle: 'bold', lineColor: lineColor, lineWidth: 0.2 },
        bodyStyles: { fontSize: 8, lineColor: lineColor, lineWidth: 0.2 },
        columnStyles: {
            0: { halign: 'center', cellWidth: 15 },
            1: { halign: 'center', cellWidth: 20 },
            3: { halign: 'right', cellWidth: 25 },
            4: { halign: 'right', cellWidth: 20 },
            5: { halign: 'right', cellWidth: 25 }
        },
        margin: { left: 15, right: 15 },
        didDrawPage: function (data) {
            const rx = 2;
            const x = data.settings.margin.left;
            const y = data.pageNumber === 1 ? data.settings.startY : data.settings.margin.top;
            const w = doc.internal.pageSize.getWidth() - data.settings.margin.left - data.settings.margin.right;
            const h = data.cursor.y - y;
            if (!h || isNaN(h)) return;
            doc.setFillColor(255, 255, 255);
            doc.rect(x-0.2, y-0.2, rx+0.2, rx+0.2, 'F'); doc.rect(x + w - rx, y-0.2, rx+0.2, rx+0.2, 'F');
            doc.rect(x-0.2, y + h - rx, rx+0.2, rx+0.2, 'F'); doc.rect(x + w - rx, y + h - rx, rx+0.2, rx+0.2, 'F');
            doc.setDrawColor(lineColor[0], lineColor[1], lineColor[2]);
            doc.setLineWidth(0.2);
            doc.roundedRect(x, y, w, h, rx, rx, 'S');
        }
    });

    let finalY = doc.lastAutoTable.finalY;

    // Total en Letras (Caja redondeada debajo de la tabla)
    doc.setDrawColor(lineColor[0], lineColor[1], lineColor[2]);
    doc.setFillColor(245, 245, 245);
    doc.setLineWidth(0.2);
    // Para que quede exacto debajo de la tabla
    doc.roundedRect(15, finalY, doc.internal.pageSize.width - 30, 6, 2, 2, 'FD');
    doc.setFont("helvetica", "bold");
    doc.setFontSize(8);
    doc.setTextColor(0, 0, 0);
    doc.text(`SON: ${numeroALetras(venta.total)}`, 17, finalY + 4);

    finalY += 10;

    // Generar QR
    // Para Notas de Venta, el QR no tiene datos SUNAT oficiales, solo es referencial
    const tipoSunat = docType === 'FACTURA' ? '01' : docType === 'BOLETA' ? '03' : 'NV';
    const qrText = `${RUC}|${tipoSunat}|${venta.serieComprobante}|${venta.numeroComprobante}|0.00|${venta.total}|${venta.fecha.split('T')[0]}|${venta.cliente?.documentoIdentidad? '1':'0'}|${venta.cliente?.documentoIdentidad||'-'}`;
    const qrDataUrl = await QRCode.toDataURL(qrText, { margin: 0, width: 35 });
    doc.addImage(qrDataUrl, 'PNG', 15, finalY, 35, 35);

    // Observacion box
    doc.setFont("helvetica", "bold");
    doc.setFontSize(9);
    doc.setTextColor(0, 102, 204); // Blue color for "Observación:"
    doc.text("Observación:", 57, finalY + 5);
    doc.setTextColor(0, 0, 0);
    doc.setDrawColor(0, 102, 204);
    doc.roundedRect(55, finalY + 1, 75, 7, 2, 2);

    // Consulte su documento
    doc.setFont("helvetica", "normal");
    doc.setFontSize(8);
    if (isElectronica) {
        doc.text("Consulte su documento electrónico en:", 55, finalY + 13);
        doc.setTextColor(0, 0, 255);
        doc.text("https://vidaanimal.vercel.app/consultaboleta", 55, finalY + 17);
        doc.setTextColor(0, 0, 0);
    }

    // HASH and VENDEDOR
    doc.setFont("helvetica", "bold");
    doc.text("HASH:", 55, finalY + 22);
    doc.setFont("helvetica", "normal");
    // Si la BD tuviera Hash, lo pondríamos aquí, por defecto dejamos un guion si no hay.
    doc.text(venta.hashSunat || "-", 70, finalY + 22);

    doc.setFont("helvetica", "bold");
    doc.text("VENDEDOR:", 55, finalY + 27);
    doc.setFont("helvetica", "normal");
    doc.text((venta.cajero || "SISTEMA").toUpperCase(), 77, finalY + 27);
    
    // Recuadro de Resumen
    const resumenX = 135;
    doc.setDrawColor(lineColor[0], lineColor[1], lineColor[2]);
    doc.setLineWidth(0.2);
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

export const generateTicketPDF = async (venta) => {
    // Ticket de 80mm de ancho. (80mm es aprox 3.15 pulgadas. usaremos mm)
    const doc = new jsPDF('p', 'mm', [80, 250]); 
    const EMPRESA = "VIDA ANIMAL";
    const RUC = "10764194883";
    const DIR = "Jr. Atahualpa N° 291 - Aucayacu";
    const DIR2 = "José Crespo y Castillo, Huánuco"; // Shorter for ticket
    const TEL = "Telf.: 975 418 965";
    const serie = venta.serieComprobante || '';
    const docType = serie.startsWith('F') ? 'FACTURA' : serie.startsWith('B') ? 'BOLETA' : 'NOTA DE VENTA';
    const isElectronica = docType === 'BOLETA' || docType === 'FACTURA';
    const numDoc = `${venta.serieComprobante} - ${venta.numeroComprobante}`;
    
    let y = 5;

    // Cargar logo
    const logoBase64 = await loadLogo();
    if (logoBase64) {
        doc.addImage(logoBase64, 'PNG', 25, y, 30, 30);
        y += 37;
    } else {
        y += 5;
    }
    
    doc.setFontSize(12);
    doc.setFont("helvetica", "bold");
    doc.text(EMPRESA, 40, y, { align: "center" });
    y += 5;
    
    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.text(`RUC: ${RUC}`, 40, y, { align: "center" });
    y += 5;
    doc.setFontSize(8);
    doc.text(DIR, 40, y, { align: "center" });
    y += 4;
    doc.text(DIR2, 40, y, { align: "center" });
    y += 4;
    doc.text(TEL, 40, y, { align: "center" });
    y += 8;
    
    doc.setFontSize(9);
    
    doc.setFont("helvetica", "bold");
    if (isElectronica) {
        doc.text(`${docType} ELECTRÓNICA`, 40, y, { align: "center" });
    } else {
        doc.text(docType, 40, y, { align: "center" });
    }
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
    
    if (isElectronica) {
        doc.text("Consulte su documento electrónico en:", 40, y, { align: "center" });
        y += 4;
        doc.text("https://vidaanimal.vercel.app/consultaboleta", 40, y, { align: "center" });
    }
    
    // Save with precise content height
    doc.save(`Ticket_${numDoc}.pdf`);
};
