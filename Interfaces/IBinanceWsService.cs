using System;
using System.Threading.Tasks;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface IBinanceWsService
    {
        event Action<string, decimal, decimal>? PriceUpdated;
        bool IsConnected { get; }
        Task ConnectAsync();
        Task DisconnectAsync();
    }
}
