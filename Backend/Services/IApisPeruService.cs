using System.Threading.Tasks;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Services
{
    public interface IApisPeruService
    {
        Task<(bool Success, string Message, string? XmlUrl, string? PdfUrl, string? CdrUrl, string? SunatStatus)> EnviarBoletaAsync(Venta venta);
    }
}
