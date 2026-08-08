using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.RegularExpressions;
using VidaAnimal.API.Hubs;

namespace VidaAnimal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YapeController : ControllerBase
{
    private readonly IHubContext<YapeHub> _hubContext;

    public YapeController(IHubContext<YapeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Este endpoint lo llamará MacroDroid cada vez que llegue una notificación de Yape
    [AllowAnonymous]
    [HttpPost("Webhook")]
    public async Task<IActionResult> RecibirNotificacion([FromBody] JsonElement body)
    {
        string titulo = "";
        string texto = "";

        try
        {
            if (body.TryGetProperty("title", out var titleProp))
                titulo = titleProp.GetString() ?? "";
            if (body.TryGetProperty("text", out var textProp))
                texto = textProp.GetString() ?? "";
        }
        catch
        {
            return BadRequest("Payload inválido.");
        }

        // Verificar que sea una notificación de Yape con un monto
        string textoCompleto = $"{titulo} {texto}".ToLower();
        if (!textoCompleto.Contains("yape") && !textoCompleto.Contains("yapeo") && !textoCompleto.Contains("yapearon"))
            return Ok(new { mensaje = "No es una notificación de Yape, ignorado." });

        // Extraer el monto con Regex: busca patrones como "S/ 20.50" o "S/20.50" o "s/ 5.00"
        var montoMatch = Regex.Match(texto, @"[Ss]\/\s?(\d+(?:[.,]\d{1,2})?)");
        string monto = montoMatch.Success ? montoMatch.Groups[1].Value.Replace(",", ".") : "0";

        // Extraer el nombre del remitente
        // Patrón: "Juan te yapeó" → extrae "Juan"
        // Patrón: "Te yapearon ... de María Celina." → extrae "María Celina"
        string remitente = "Alguien";
        var remitenteMatch1 = Regex.Match(texto, @"^([A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+(?:\s[A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+)*)\s+te\s+yape[oó]");
        var remitenteMatch2 = Regex.Match(texto, @"de\s+([A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+(?:\s[A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+)*)[\.\!]?$");

        if (remitenteMatch1.Success)
            remitente = remitenteMatch1.Groups[1].Value;
        else if (remitenteMatch2.Success)
            remitente = remitenteMatch2.Groups[1].Value;

        // Enviar en tiempo real a todos los navegadores conectados
        await _hubContext.Clients.All.SendAsync("YapeNotification", new
        {
            remitente = remitente,
            monto = monto,
            textoOriginal = texto
        });

        return Ok(new { mensaje = "Notificación enviada a la pantalla.", remitente, monto });
    }
}
