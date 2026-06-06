using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface ICoinCapWsService
    {
        event Action<string, decimal>? PriceUpdated;
        bool IsConnected { get; }
        Task ConnectAsync(List<string> assets);
        Task DisconnectAsync();
    }
}
