using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Tesseract;
using UglyToad.PdfPig;
using System;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using System.Linq;
using Google.Apis.Auth.OAuth2;

namespace VidaAnimal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IAController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public IAController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        [HttpPost("AnalizarFactura")]
        public async Task<IActionResult> AnalizarFactura(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { success = false, mensaje = "No se enviÃ³ ningÃºn archivo." });

            string extractedText = "";

            try
            {
                var extension = Path.GetExtension(archivo.FileName).ToLower();

                if (extension == ".pdf")
                {
                    using var stream = archivo.OpenReadStream();
                    using var document = PdfDocument.Open(stream);
                    foreach (var page in document.GetPages())
                    {
                        extractedText += page.Text + "\n";
                    }
                }
                else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                {
                    // Usar Tesseract
                    string tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                    using var engine = new TesseractEngine(tessDataPath, "spa", EngineMode.Default);
                    
                    using var ms = new MemoryStream();
                    await archivo.CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();

                    using var pix = Pix.LoadFromMemory(fileBytes);
                    using var page = engine.Process(pix);
                    extractedText = page.GetText();
                }
                else
                {
                    return BadRequest(new { success = false, mensaje = "Formato no soportado. Sube PDF o ImÃ¡genes." });
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return BadRequest(new { success = false, mensaje = "No se pudo extraer texto del archivo." });
                }

                // Llamar a Gemma 4 para procesar el texto
                var prompt = @"Eres un asistente experto en extraer datos de facturas y boletas. 
Te pasarÃ© el texto crudo extraÃ­do de una factura/boleta mediante OCR o PDF. 
Debes devolver UNICAMENTE un objeto JSON con el siguiente formato, no agregues texto extra ni comillas de cÃ³digo, solo el JSON puro:
{
  ""proveedorNombre"": ""Nombre del proveedor"",
  ""numeroComprobante"": ""F001-XXXX (si lo encuentras)"",
  ""productos"": [
    {
      ""nombre"": ""Nombre del producto"",
      ""cantidad"": 0.0,
      ""precioCostoUnitario"": 0.0
    }
  ]
}

TEXTO EXTRAÃDO:
" + extractedText;

                var payload = new
                {
                    contents = new[]
                    {
                        new { role = "user", parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        responseMimeType = "application/json"
                    }
                };

                var auth = await GetVertexAuthAsync();
                
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, auth.Url);
                requestMsg.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                requestMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

                var response = await _httpClient.SendAsync(requestMsg);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, mensaje = $"Gemini respondiÃ³ con error {response.StatusCode}: {errorDetails}" });
                }

                var resultStr = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonSerializer.Deserialize<JsonElement>(resultStr);
                
                var aiResponse = jsonDoc.GetProperty("candidates")[0]
                                        .GetProperty("content")
                                        .GetProperty("parts")[0]
                                        .GetProperty("text").GetString();

                // Intentar parsear el JSON de la respuesta
                var datos = JsonSerializer.Deserialize<JsonElement>(aiResponse);

                return Ok(new { success = true, datos = datos });

            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error al procesar: " + ex.Message });
            }
        }


        [HttpPost("Chatbot")]
        public async Task<IActionResult> Chatbot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Mensaje))
                return BadRequest(new { success = false, mensaje = "El mensaje no puede estar vacÃ­o." });

            var auth = await GetVertexAuthAsync();

            var esquemaDB = @"
BASE DE DATOS SQL SERVER: VidaAnimalBD
ZONA HORARIA: Los datos de Fecha estan en hora local de Peru (UTC-5). El servidor corre en UTC. Para la hora actual en Peru usa DATEADD(hour,-5,GETDATE()). Para hoy en Peru usa CONVERT(date, DATEADD(hour,-5,GETDATE())). Usa CONVERT(date, Fecha) para comparar fechas del campo Fecha.

TABLAS DISPONIBLES (SOLO LECTURA):

1. Ventas (VentaID, Fecha DATETIME, UsuarioID, SubTotal, Total, Descuento, MetodoPago, Estado, ClienteID, NumeroComprobante)
   - Estado puede ser: 'Completada', 'Anulada'. Solo usa Estado='Completada' para reportes.
   - Total es el monto cobrado al cliente. Descuento es el descuento aplicado.

2. DetalleVentas (DetalleVentaID, VentaID, ProductoID, Cantidad, PrecioUnitario, PrecioCostoUnitario, Ganancia, SubTotal)
   - Ganancia = (PrecioUnitario - PrecioCostoUnitario) * Cantidad
   - SubTotal = Cantidad * PrecioUnitario

3. Productos (ProductoID, Codigo, Nombre, PrecioCosto, PrecioVenta, StockActual, StockMinimo, UnidadMedida, Activo, CategoriaID, ProveedorID)

4. Clientes (ClienteID, NombreCompleto, DocumentoIdentidad, Telefono, FechaRegistro, Activo)

5. Compras (CompraID, ProveedorID, FechaCompra, NumeroComprobante, Total)

6. CompraDetalles (CompraDetalleID, CompraID, ProductoID, Cantidad, PrecioCostoUnitario, SubTotal)

7. Proveedores (ProveedorID, Nombre, Telefono, Email, Activo)

8. MovimientosInventario (MovimientoID, Fecha, Tipo, ProductoID, Cantidad, StockAnterior, StockNuevo, ReferenciaID, Observaciones, UsuarioID)
   - Tipo puede ser: 'Entrada', 'Salida', 'Ajuste'

9. Usuarios (UsuarioID, NombreCompleto, DNI, Correo, Rol, Activo)
";

            try
            {
                // === PASO 1: Gemini genera el SQL ===
                var promptSQL = $@"Eres un experto en SQL Server. Tu Ãºnica tarea es generar una consulta SQL para responder la pregunta del usuario sobre la base de datos de 'Vida Animal' (veterinaria).

REGLAS ESTRICTAS:
1. SOLO puedes generar sentencias SELECT. NUNCA uses INSERT, UPDATE, DELETE, DROP, ALTER, EXEC, o cualquier sentencia que modifique datos.
2. Responde ÃšNICAMENTE con el cÃ³digo SQL puro, sin explicaciones, sin comillas de cÃ³digo, sin bloques markdown, sin etiquetas sql.
3. Si la pregunta NO requiere base de datos (ej. consejos generales, preguntas de ayuda del sistema), responde exactamente con: NO_SQL
4. Limita resultados con TOP 20 cuando sea apropiado para no sobrecargar.
5. Para fechas de ""hoy"", usa CONVERT(date, GETDATE()). Para ""este mes"" usa MONTH(Fecha)=MONTH(GETDATE()) AND YEAR(Fecha)=YEAR(GETDATE()).
6. Usa alias descriptivos en espaÃ±ol para las columnas (ej. AS TotalVentas, AS NombreProducto).

{esquemaDB}

HISTORIAL:
{request.Historial}

PREGUNTA: {request.Mensaje}

SQL:";

                var sqlPayload = new { contents = new[] { new { role = "user", parts = new[] { new { text = promptSQL } } } } };
                var sqlReq = new HttpRequestMessage(HttpMethod.Post, auth.Url) 
                { 
                    Content = new StringContent(JsonSerializer.Serialize(sqlPayload), Encoding.UTF8, "application/json") 
                };
                sqlReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                var sqlResponse = await _httpClient.SendAsync(sqlReq);

                if (!sqlResponse.IsSuccessStatusCode)
                {
                    var geminiError = await sqlResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[GEMINI ERROR] Status: {sqlResponse.StatusCode}, Body: {geminiError}");
                    return BadRequest(new { success = false, mensaje = $"Gemini error {(int)sqlResponse.StatusCode}: {geminiError}" });
                }

                var sqlResultStr = await sqlResponse.Content.ReadAsStringAsync();
                var sqlDoc = JsonSerializer.Deserialize<JsonElement>(sqlResultStr);
                var generatedSQL = sqlDoc.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()?.Trim();

                string resultadoDatos = "";

                // === PASO 2: Ejecutar el SQL si lo generÃ³ ===
                if (!string.IsNullOrEmpty(generatedSQL) && generatedSQL != "NO_SQL")
                {
                    // ValidaciÃ³n de seguridad: rechazar cualquier SQL peligroso
                    var sqlUpper = generatedSQL.ToUpper();
                    var palabrasPeligrosas = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "EXEC", "TRUNCATE", "CREATE", "MERGE" };
                    bool esPeligroso = palabrasPeligrosas.Any(p => sqlUpper.Contains(p));

                    if (!esPeligroso && sqlUpper.Contains("SELECT"))
                    {
                        try
                        {
                            var connectionString = _configuration.GetConnectionString("DefaultConnection");
                            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                            await conn.OpenAsync();
                            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(generatedSQL, conn);
                            cmd.CommandTimeout = 15;
                            using var reader = await cmd.ExecuteReaderAsync();

                            var sb = new System.Text.StringBuilder();
                            // Encabezados
                            var cols = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
                            sb.AppendLine(string.Join(" | ", cols));
                            sb.AppendLine(new string('-', 60));
                            int filas = 0;
                            while (await reader.ReadAsync() && filas < 20)
                            {
                                var valores = cols.Select((_, i) => reader.IsDBNull(i) ? "N/A" : reader.GetValue(i).ToString());
                                sb.AppendLine(string.Join(" | ", valores));
                                filas++;
                            }
                            resultadoDatos = filas > 0 ? sb.ToString() : "La consulta no devolviÃ³ resultados.";
                        }
                        catch (Exception sqlEx)
                        {
                            resultadoDatos = $"[Error al ejecutar la consulta: {sqlEx.Message}]";
                        }
                    }
                }

                // === PASO 3: Gemini redacta la respuesta final en lenguaje natural ===
                var contextoRespuesta = string.IsNullOrEmpty(resultadoDatos)
                    ? "No se necesitÃ³ consultar la base de datos para esta pregunta, o la pregunta fue de tipo general."
                    : $"Resultado de la base de datos:\n{resultadoDatos}";

                var promptFinal = $@"Eres 'Fer', el asistente inteligente y amigable de 'Vida Animal', una veterinaria peruana.
Tu objetivo es responder de forma clara, breve y profesional en espaÃ±ol. Usa moneda peruana (S/) cuando aplique.
Si tienes datos de la base de datos, Ãºsalos para dar una respuesta precisa y Ãºtil. Si no hay datos, responde con tu conocimiento general sobre veterinaria, negocios o el sistema.
MantÃ©n tus respuestas concisas (mÃ¡ximo 3 pÃ¡rrafos). Puedes usar viÃ±etas o negritas con ** para resaltar informaciÃ³n importante.

HISTORIAL DE LA CONVERSACIÃ“N:
{request.Historial}

PREGUNTA DEL USUARIO: {request.Mensaje}

{contextoRespuesta}

RESPUESTA DE FER:";

                var finalPayload = new { contents = new[] { new { role = "user", parts = new[] { new { text = promptFinal } } } } };
                var finalReq = new HttpRequestMessage(HttpMethod.Post, auth.Url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(finalPayload), Encoding.UTF8, "application/json")
                };
                finalReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                var finalResponse = await _httpClient.SendAsync(finalReq);

                var finalStr = await finalResponse.Content.ReadAsStringAsync();
                var finalDoc = JsonSerializer.Deserialize<JsonElement>(finalStr);
                var aiResponse = finalDoc.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                return Ok(new { success = true, respuesta = aiResponse });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensaje = "Error al procesar tu mensaje: " + ex.Message });
            }
        }

        private async Task<(string Url, string Token)> GetVertexAuthAsync()
        {
            string projectId = "ambient-glazing-462000-f4";
            string location = "us-central1"; 
            string model = "gemini-2.5-flash";
            string url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{model}:generateContent";

            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "google-credentials.json");
            using var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            var token = await ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
            return (url, token);
        }
    }

    public class ChatRequest
    {
        public string Mensaje { get; set; }
        public string Historial { get; set; }
    }
}

