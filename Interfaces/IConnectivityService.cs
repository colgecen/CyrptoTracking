using System;
using System.Threading.Tasks;

namespace AvaloniaAppCrypto.Interfaces
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }
        event Action<bool> ConnectivityChanged;
        Task CheckConnectivityAsync();
    }
}
