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
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return (_cachedToken, null);

            var username = _config["ApisPeruConfig:Username"];
            var password = _config["ApisPeruConfig:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (null, "No se han configurado las credenciales de APIsPERU en appsettings.json.");

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

                // Construir los items de la boleta
                var details = venta.VentaDetalles.Select(d => new
                {
                    unidad = "NIU",           // Unidad de medida (NIU = Unidad)
                    cantidad = (double)d.Cantidad,
                    descripcion = d.Producto?.Nombre ?? "PRODUCTO",
                    valorUnitario = (double)Math.Round(d.PrecioVentaUnitario, 2),
                    precioUnitario = (double)Math.Round(d.PrecioVentaUnitario, 2),
                    // Código de afectación IGV 20 = Exonerado (para Nuevo RUS)
                    tipAfeIgv = "20",
                    igv = 0.0,
                    totalImpuestos = 0.0,
                    subtotal = (double)Math.Round(d.PrecioVentaUnitario * d.Cantidad, 2),
                    total = (double)Math.Round(d.PrecioVentaUnitario * d.Cantidad, 2),
                    mtoValorVenta = (double)Math.Round(d.PrecioVentaUnitario * d.Cantidad, 2),
                }).ToList();

                // Total de la boleta
                var totalVenta = (double)Math.Round(venta.Total, 2);

                // Datos del cliente (si no tiene, se usa consumidor final)
                object buyerData;
                if (venta.Cliente != null && !string.IsNullOrEmpty(venta.Cliente.DocumentoIdentidad))
                {
                    buyerData = new
                    {
                        tipoDoc = "1",  // 1 = DNI
                        numDoc = venta.Cliente.DocumentoIdentidad,
                        rzSocial = venta.Cliente.NombreCompleto ?? "CONSUMIDOR FINAL"
                    };
                }
                else
                {
                    buyerData = new
                    {
                        tipoDoc = "-",
                        numDoc = "-",
                        rzSocial = "CONSUMIDOR FINAL"
                    };
                }

                var boletaPayload = new
                {
                    ublVersion = "2.1",
                    tipoDoc = "03",        // 03 = Boleta de Venta
                    serie = venta.SerieComprobante ?? "B001",
                    correlativo = venta.NumeroComprobante ?? "1",
                    fechaEmision = venta.Fecha.ToString("yyyy-MM-dd"),
                    moneda = "PEN",
                    client = buyerData,
                    company = new
                    {
                        ruc = ruc,
                        razonSocial = "VIDA ANIMAL",
                        nombreComercial = "Vida Animal",
                        address = new
                        {
                            ubigueo = "150101",  // Lima - reemplazar por Aucayacu si disponible
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
                    details,
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
