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

        // Verificar que el payload no esté vacío
        if (string.IsNullOrWhiteSpace(titulo) && string.IsNullOrWhiteSpace(texto))
            return BadRequest("Payload vacío.");

        // Extraer el monto con múltiples patrones en cascada
        string textoCompleto = (texto + " " + titulo).Trim();
        string monto = "0";

        // Patrón 1: "S/ 5", "S/5.00", "S/ 20.50"
        var m1 = Regex.Match(textoCompleto, @"S[/\.]\s*(\d+(?:[.,]\d{1,2})?)");
        if (m1.Success)
        {
            monto = m1.Groups[1].Value.Replace(",", ".");
        }
        else
        {
            // Patrón 2: "por 5", "por 20.50"
            var m2 = Regex.Match(textoCompleto, @"\bpor\s+(\d+(?:[.,]\d{1,2})?)");
            if (m2.Success)
            {
                monto = m2.Groups[1].Value.Replace(",", ".");
            }
            else
            {
                // Patrón 3: cualquier número en el texto (último recurso)
                var m3 = Regex.Match(textoCompleto, @"\b(\d+(?:[.,]\d{1,2})?)\b");
                if (m3.Success) monto = m3.Groups[1].Value.Replace(",", ".");
            }
        }

        // Enviar en tiempo real a todos los navegadores conectados
        await _hubContext.Clients.All.SendAsync("YapeNotification", new
        {
            monto = monto,
            textoOriginal = texto
        });

        return Ok(new { mensaje = "OK", monto, rawTitulo = titulo, rawTexto = texto });
    }
}
