using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Services
{
    public class ApisPeruService : IApisPeruService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private static string? _cachedToken;
        private static DateTime _tokenExpiry = DateTime.MinValue;

        private const string BASE_URL = "https://facturacion.apisperu.com/api/v1";

        public ApisPeruService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task<(string? Token, string? Error)> GetTokenAsync()
        {
            // Si hay un token estático permanente configurado, usarlo directamente
            var staticToken = _config["ApisPeruConfig:Token"];
            if (!string.IsNullOrEmpty(staticToken))
            {
                return (staticToken, null);
            }

            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return (_cachedToken, null);

            var username = _config["ApisPeruConfig:Username"];
            var password = _config["ApisPeruConfig:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (null, "No se han configurado las credenciales ni el Token de APIsPERU en appsettings.json.");

            var payload = JsonSerializer.Serialize(new { username, password });
            var response = await _httpClient.PostAsync(
                $"{BASE_URL}/auth/login",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (null, $"Login APIsPERU HTTP {(int)response.StatusCode}: {responseBody}");

            var json = JsonDocument.Parse(responseBody);
            var token = json.RootElement.GetProperty("token").GetString();
            _cachedToken = token;
            _tokenExpiry = DateTime.UtcNow.AddHours(23);
            return (token, null);
        }

        public async Task<(bool Success, string Message, string? XmlUrl, string? PdfUrl, string? CdrUrl, string? SunatStatus)> EnviarBoletaAsync(Venta venta)
        {
            try
            {
                var (token, tokenError) = await GetTokenAsync();
                if (token == null)
                    return (false, tokenError ?? "Error al obtener token de APIsPERU.", null, null, null, "ERROR_AUTH");

                var ruc = _config["ApisPeruConfig:Ruc"] ?? "";

                // Construir los items de la boleta alineado con OpenAPI de APIsPERU
                var details = venta.VentaDetalles.Select(d => {
                    var valorVenta = (double)Math.Round(d.PrecioVentaUnitario * d.Cantidad, 2);
                    var precioUnitario = (double)Math.Round(d.PrecioVentaUnitario, 2);
                    return new
                    {
                        codProducto = d.ProductoID.ToString(),
                        unidad = "NIU",
                        cantidad = (double)d.Cantidad,
                        descripcion = d.Producto?.Nombre ?? "PRODUCTO",
                        mtoValorUnitario = precioUnitario,
                        mtoPrecioUnitario = precioUnitario,
                        mtoValorVenta = valorVenta,
                        mtoBaseIgv = valorVenta,
                        porcentajeIgv = 0.0,
                        igv = 0.0,
                        totalImpuestos = 0.0,
                        tipAfeIgv = "20" // Exonerado
                    };
                }).ToList();

                // Total de la boleta
                var totalVenta = (double)Math.Round(venta.Total, 2);

                // Datos del cliente (si no tiene, se usa consumidor final)
                object buyerData;
                if (venta.Cliente != null && !string.IsNullOrEmpty(venta.Cliente.DocumentoIdentidad))
                {
                    string doc = venta.Cliente.DocumentoIdentidad.Trim();
                    string tipoDoc = "0"; // Otros por defecto
                    if (doc.Length == 8) tipoDoc = "1"; // DNI
                    else if (doc.Length == 11) tipoDoc = "6"; // RUC

                    buyerData = new
                    {
                        tipoDoc = tipoDoc,
                        numDoc = doc,
                        rznSocial = venta.Cliente.NombreCompleto ?? "CONSUMIDOR FINAL" // rznSocial según OpenAPI
                    };
                }
                else
                {
                    buyerData = new
                    {
                        tipoDoc = "0",
                        numDoc = "00000000",
                        rznSocial = "CONSUMIDOR FINAL" // rznSocial según OpenAPI
                    };
                }

                var boletaPayload = new
                {
                    ublVersion = "2.1",
                    tipoOperacion = "0101", // Venta Interna
                    tipoDoc = "03",        // 03 = Boleta de Venta
                    serie = venta.SerieComprobante ?? "B001",
                    correlativo = venta.NumeroComprobante ?? "1",
                    fechaEmision = venta.Fecha.ToString("yyyy-MM-dd") + "T" + venta.Fecha.ToString("HH:mm:ss") + "-05:00",
                    formaPago = new
                    {
                        moneda = "PEN",
                        tipo = "Contado"
                    },
                    tipoMoneda = "PEN", // tipoMoneda según OpenAPI
                    client = buyerData,
                    company = new
                    {
                        ruc = ruc,
                        razonSocial = "BELITH RETIS BARTOLOME", // Debe coincidir con la razón social del RUC
                        nombreComercial = "Vida Animal",
                        address = new
                        {
                            ubigueo = "150101",
                            departamento = "Huanuco",
                            provincia = "Leoncio Prado",
                            distrito = "Jose Crespo Y Castillo",
                            urbanizacion = "",
                            direccion = "Jr. Atahualpa N° 291"
                        }
                    },
                    mtoOperExoneradas = totalVenta,  // Exonerado del IGV (Nuevo RUS)
                    mtoIGV = 0.0,
                    totalImpuestos = 0.0,
                    valorVenta = totalVenta,
                    subTotal = totalVenta,
                    mtoImpVenta = totalVenta,
                    details = details
                };

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var jsonPayload = JsonSerializer.Serialize(boletaPayload);
                var response = await _httpClient.PostAsync(
                    $"{BASE_URL}/invoice/send",
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, $"Error APIsPERU: {responseBody}", null, null, null, "ERROR");

                var result = JsonDocument.Parse(responseBody);
                string? xmlUrl = null, pdfUrl = null, cdrUrl = null, sunatStatus = "ACEPTADO";

                // Log full response for debugging
                try { System.IO.File.WriteAllText("/var/www/vida-animal/apisperu_response.log", responseBody); } catch { }

                // Obtener serie y correlativo para nombrar los archivos
                var serieCorrelativo = $"{venta.SerieComprobante ?? "B001"}-{(venta.NumeroComprobante ?? "1").PadLeft(8, '0')}";
                var rucVal = _config["ApisPeruConfig:Ruc"] ?? "00000000000";
                var storageDir = "/var/www/vida-animal/comprobantes";
                try { System.IO.Directory.CreateDirectory(storageDir); } catch { }

                // ── 1. Extraer XML firmado ──────────────────────────────────────────
                // ApisPeru free devuelve el XML firmado en el campo "xml" (base64 o texto plano)
                string? xmlContent = null;
                if (result.RootElement.TryGetProperty("xml", out var xmlProp))
                    xmlContent = xmlProp.GetString();
                // También puede venir en "xmlSigned" o "document"
                if (string.IsNullOrEmpty(xmlContent) && result.RootElement.TryGetProperty("xmlSigned", out var xmlSigned))
                    xmlContent = xmlSigned.GetString();
                if (string.IsNullOrEmpty(xmlContent) && result.RootElement.TryGetProperty("document", out var docProp))
                    xmlContent = docProp.GetString();

                if (!string.IsNullOrEmpty(xmlContent))
                {
                    try
                    {
                        // Intentar decodificar base64, si falla guardar como texto
                        byte[] xmlBytes;
                        try { xmlBytes = Convert.FromBase64String(xmlContent); }
                        catch { xmlBytes = Encoding.UTF8.GetBytes(xmlContent); }

                        var xmlFileName = $"{rucVal}-03-{serieCorrelativo}.xml";
                        var xmlPath = System.IO.Path.Combine(storageDir, xmlFileName);
                        System.IO.File.WriteAllBytes(xmlPath, xmlBytes);
                        xmlUrl = $"/api/ventas/descargar/xml/{xmlFileName}";
                    }
                    catch { }
                }

                // ── 2. Extraer CDR (Constancia de Recepción) ───────────────────────
                string? cdrContent = null;
                if (result.RootElement.TryGetProperty("cdr", out var cdrProp))
                    cdrContent = cdrProp.GetString();
                if (string.IsNullOrEmpty(cdrContent) && result.RootElement.TryGetProperty("cdrZip", out var cdrZip))
                    cdrContent = cdrZip.GetString();
                // A veces viene en sunatResponse.cdr
                if (string.IsNullOrEmpty(cdrContent) &&
                    result.RootElement.TryGetProperty("sunatResponse", out var sr))
                {
                    if (sr.TryGetProperty("cdr", out var srCdr))
                        cdrContent = srCdr.GetString();
                    if (string.IsNullOrEmpty(cdrContent) && sr.TryGetProperty("cdrZip", out var srCdrZip))
                        cdrContent = srCdrZip.GetString();
                }

                if (!string.IsNullOrEmpty(cdrContent))
                {
                    try
                    {
                        byte[] cdrBytes;
                        try { cdrBytes = Convert.FromBase64String(cdrContent); }
                        catch { cdrBytes = Encoding.UTF8.GetBytes(cdrContent); }

                        var cdrFileName = $"R-{rucVal}-03-{serieCorrelativo}.zip";
                        var cdrPath = System.IO.Path.Combine(storageDir, cdrFileName);
                        System.IO.File.WriteAllBytes(cdrPath, cdrBytes);
                        cdrUrl = $"/api/ventas/descargar/cdr/{cdrFileName}";
                    }
                    catch { }
                }

                // ── 3. Fallback: buscar URLs directas si ApisPeru las devuelve ─────
                if (string.IsNullOrEmpty(xmlUrl))
                {
                    if (result.RootElement.TryGetProperty("links", out var links))
                        xmlUrl = links.TryGetProperty("xml", out var lx) ? lx.GetString() : null;
                    xmlUrl ??= result.RootElement.TryGetProperty("xmlUrl", out var xu) ? xu.GetString() : null;
                    xmlUrl ??= result.RootElement.TryGetProperty("linkXml", out var lxu) ? lxu.GetString() : null;
                }
                if (string.IsNullOrEmpty(pdfUrl))
                {
                    if (result.RootElement.TryGetProperty("links", out var links2))
                        pdfUrl = links2.TryGetProperty("pdf", out var lp) ? lp.GetString() : null;
                    pdfUrl ??= result.RootElement.TryGetProperty("pdfUrl", out var pu) ? pu.GetString() : null;
                    pdfUrl ??= result.RootElement.TryGetProperty("linkPdf", out var lpu) ? lpu.GetString() : null;
                }
                if (string.IsNullOrEmpty(cdrUrl))
                {
                    if (result.RootElement.TryGetProperty("links", out var links3))
                        cdrUrl = links3.TryGetProperty("cdr", out var lc) ? lc.GetString() : null;
                    cdrUrl ??= result.RootElement.TryGetProperty("cdrUrl", out var cu) ? cu.GetString() : null;
                    cdrUrl ??= result.RootElement.TryGetProperty("linkCdr", out var lcu) ? lcu.GetString() : null;
                }

                // ── 4. Estado SUNAT ────────────────────────────────────────────────
                // Nuevo formato: cdrResponse directo en el root
                if (result.RootElement.TryGetProperty("cdrResponse", out var cdrResponse))
                {
                    var desc = cdrResponse.TryGetProperty("description", out var d) ? d.GetString() : null;
                    var code = cdrResponse.TryGetProperty("code", out var c) ? c.GetString() : null;
                    if (code == "0")
                        sunatStatus = desc ?? "La Boleta ha sido aceptada";
                    else
                        sunatStatus = desc ?? "ACEPTADO";
                }
                else if (result.RootElement.TryGetProperty("sunatResponse", out var sunatRes) &&
                         sunatRes.TryGetProperty("cdrResponse", out var cdrRes))
                {
                    sunatStatus = cdrRes.TryGetProperty("description", out var d) ? d.GetString() : "ACEPTADO";
                }

                return (true, "Boleta enviada exitosamente a SUNAT.", xmlUrl, pdfUrl, cdrUrl, sunatStatus);
            }
            catch (Exception ex)
            {
                return (false, $"Excepción al enviar boleta: {ex.Message}", null, null, null, "ERROR");
            }
        }
    }
}
