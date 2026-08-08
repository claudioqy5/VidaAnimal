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

        // Extraer el monto con Regex flexible: "S/ 1", "S/1.00", "S/ 20.50", etc.
        var montoMatch = Regex.Match(texto + " " + titulo, @"S[/\.]?\s*(\d+(?:[.,]\d{1,2})?)");
        string monto = montoMatch.Success ? montoMatch.Groups[1].Value.Replace(",", ".") : "0";

        // Enviar en tiempo real a todos los navegadores conectados
        await _hubContext.Clients.All.SendAsync("YapeNotification", new
        {
            monto = monto,
            textoOriginal = texto
        });

        return Ok(new { mensaje = "Notificación enviada a la pantalla.", monto });
    }
}
