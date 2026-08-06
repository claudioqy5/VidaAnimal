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
                        codProducto = d.ProductoId.ToString(),
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
                var xmlUrl = result.RootElement.TryGetProperty("xmlUrl", out var x) ? x.GetString() : null;
                var pdfUrl = result.RootElement.TryGetProperty("pdfUrl", out var p) ? p.GetString() : null;
                var cdrUrl = result.RootElement.TryGetProperty("cdrUrl", out var c) ? c.GetString() : null;
                var sunatStatus = result.RootElement.TryGetProperty("sunatDescription", out var s) ? s.GetString() : "ACEPTADO";

                return (true, "Boleta enviada exitosamente a SUNAT.", xmlUrl, pdfUrl, cdrUrl, sunatStatus);
            }
            catch (Exception ex)
            {
                return (false, $"Excepción al enviar boleta: {ex.Message}", null, null, null, "ERROR");
            }
        }
    }
}
