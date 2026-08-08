using Microsoft.AspNetCore.SignalR;

namespace VidaAnimal.API.Hubs;

public class YapeHub : Hub
{
    // Los clientes (navegadores) se conectan a este Hub.
    // El servidor les enviará eventos mediante SendYapeNotification.
}
